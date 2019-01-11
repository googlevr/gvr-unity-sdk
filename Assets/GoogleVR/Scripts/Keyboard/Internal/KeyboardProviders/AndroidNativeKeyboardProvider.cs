//-----------------------------------------------------------------------
// <copyright file="AndroidNativeKeyboardProvider.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

// This is a Keyboard Subclass that runs on device only. It displays the
// full VR Keyboard.

using UnityEngine;
using System;
using System.Runtime.InteropServices;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif  // UNITY_2017_2_OR_NEWER

/// @cond
namespace Gvr.Internal
{
    public class AndroidNativeKeyboardProvider : IKeyboardProvider
    {
        private IntPtr renderEventFunction;

#if UNITY_ANDROID && !UNITY_EDITOR
        private float currentDistance = 0.0f;
#endif // UNITY_ANDROID && !UNITY_EDITOR

        // Android method names.
        private const string METHOD_NAME_GET_PACKAGE_MANAGER = "getPackageManager";
        private const string METHOD_NAME_GET_PACKAGE_INFO = "getPackageInfo";
        private const string PACKAGE_NAME_VRINPUTMETHOD = "com.google.android.vr.inputmethod";
        private const string FIELD_NAME_VERSION_CODE = "versionCode";

        // Min version for VrInputMethod.
        private const int MIN_VERSION_VRINPUTMETHOD = 170509062;

        // Library name.
        private const string dllName = "gvr_keyboard_shim_unity";

        // Enum gvr_trigger_state.
        private const int TRIGGER_NONE = 0;
        private const int TRIGGER_PRESSED = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct gvr_clock_time_point
        {
            public long monotonic_system_time_nanos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct gvr_recti
        {
            public int left;
            public int right;
            public int bottom;
            public int top;
        }

        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern gvr_clock_time_point gvr_get_time_point_now();

        [DllImport(dllName)]
        private static extern GvrKeyboardInputMode gvr_keyboard_get_input_mode(IntPtr keyboard_context);

        [DllImport(dllName)]
        private static extern void gvr_keyboard_set_input_mode(IntPtr keyboard_context, GvrKeyboardInputMode mode);

#if UNITY_ANDROID
        [DllImport(dllName)]
        private static extern IntPtr gvr_keyboard_initialize(AndroidJavaObject app_context, AndroidJavaObject class_loader);
#endif
        [DllImport(dllName)]
        private static extern IntPtr gvr_keyboard_create(IntPtr closure, GvrKeyboard.KeyboardCallback callback);

        // Gets a recommended world space matrix.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_get_recommended_world_from_keyboard_matrix(float distance_from_eye,
                                                                                           IntPtr matrix);

        // Sets the recommended world space matrix. The matrix may
        // contain a combination of translation/rotation/scaling information.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_set_world_from_keyboard_matrix(IntPtr keyboard_context, IntPtr matrix);

        // Shows the keyboard
        [DllImport(dllName)]
        private static extern void gvr_keyboard_show(IntPtr keyboard_context);

        // Updates the keyboard with the controller's button state.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_update_button_state(IntPtr keyboard_context, int buttonIndex, bool pressed);

        // Updates the controller ray on the keyboard.
        [DllImport(dllName)]
        private static extern bool gvr_keyboard_update_controller_ray(IntPtr keyboard_context, IntPtr vector3Start,
                                                                      IntPtr vector3End, IntPtr vector3Hit);

        // Updates the touch state of the controller.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_update_controller_touch(IntPtr keyboard_context, bool touched, IntPtr vector2Pos);

        // Returns the EditText with for the keyboard.
        [DllImport(dllName)]
        private static extern IntPtr gvr_keyboard_get_text(IntPtr keyboard_context);

        // Sets the edit_text for the keyboard.
        // @return 1 if the edit text could be set. 0 if it cannot be set.
        [DllImport(dllName)]
        private static extern int gvr_keyboard_set_text(IntPtr keyboard_context, IntPtr edit_text);

        // Hides the keyboard.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_hide(IntPtr keyboard_context);

        // Destroys the keyboard. Resources related to the keyboard is released.
        [DllImport(dllName)]
        private static extern void gvr_keyboard_destroy(IntPtr keyboard_context);

        // Called once per frame to set the time index.
        [DllImport(dllName)]
        private static extern void GvrKeyboardSetFrameData(IntPtr keyboard_context, gvr_clock_time_point t);

        // Sets VR eye data in preparation for rendering a single eye's view.
        [DllImport(dllName)]
        private static extern void GvrKeyboardSetEyeData(int eye_type, Matrix4x4 modelview, Matrix4x4 projection, gvr_recti viewport);

        [DllImport(dllName)]
        private static extern IntPtr GetKeyboardRenderEventFunc();

        // Private class data.
        private IntPtr keyboard_context = IntPtr.Zero;

        // Used in the GVR Unity C++ shim layer.
        private const int advanceID = 0x5DAC793B;
        private const int renderLeftID = 0x3CF97A3D;
        private const int renderRightID = 0x3CF97A3E;
        private const string KEYBOARD_JAVA_CLASS = "com.google.vr.keyboard.GvrKeyboardUnity";
        private const long kPredictionTimeWithoutVsyncNanos = 50000000;
        private const int kGvrControllerButtonClick = 1;

        private GvrKeyboardInputMode mode = GvrKeyboardInputMode.DEFAULT;
        private string editorText = string.Empty;
        private Matrix4x4 worldMatrix;
        private bool isValid = false;
        private bool isReady = false;

        public string EditorText
        {
            get
            {
                IntPtr text = gvr_keyboard_get_text(keyboard_context);
                editorText = Marshal.PtrToStringAnsi(text);
                return editorText;
            }

            set
            {
                editorText = value;
                IntPtr text = Marshal.StringToHGlobalAnsi(editorText);
                gvr_keyboard_set_text(keyboard_context, text);
            }
        }

        public void SetInputMode(GvrKeyboardInputMode mode)
        {
            Debug.Log("Calling set input mode: " + mode);
            gvr_keyboard_set_input_mode(keyboard_context, mode);
            this.mode = mode;
        }

        public void OnPause()
        {
        }

        public void OnResume()
        {
        }

        public void ReadState(KeyboardState outState)
        {
            outState.editorText = editorText;
            outState.mode = mode;
            outState.worldMatrix = worldMatrix;
            outState.isValid = isValid;
            outState.isReady = isReady;
        }

        // Initialization function.
        public AndroidNativeKeyboardProvider()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Running on Android device.
            AndroidJavaObject activity = GvrActivityHelper.GetActivity();
            if (activity == null)
            {
                Debug.Log("Failed to get activity for keyboard.");
                return;
            }

            AndroidJavaObject context = GvrActivityHelper.GetApplicationContext(activity);
            if (context == null)
            {
                Debug.Log("Failed to get context for keyboard.");
                return;
            }

            AndroidJavaObject plugin = new AndroidJavaObject(KEYBOARD_JAVA_CLASS);
            if (plugin != null)
            {
                plugin.Call("initializeKeyboard", context);
                isValid = true;
            }

#endif // UNITY_ANDROID && !UNITY_EDITOR
            renderEventFunction = GetKeyboardRenderEventFunc();
        }

        ~AndroidNativeKeyboardProvider()
        {
            if (keyboard_context != IntPtr.Zero)
            {
                gvr_keyboard_destroy(keyboard_context);
            }
        }

        public bool Create(GvrKeyboard.KeyboardCallback keyboardEvent)
        {
            if (!IsVrInputMethodAppMinVersion(keyboardEvent))
            {
                return false;
            }

            keyboard_context = gvr_keyboard_create(IntPtr.Zero, keyboardEvent);
            isReady = keyboard_context != IntPtr.Zero;
            return isReady;
        }

        public void Show(Matrix4x4 userMatrix, bool useRecommended, float distance, Matrix4x4 model)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            currentDistance = distance;
#endif  // UNITY_ANDROID && !UNITY_EDITOR

            if (useRecommended)
            {
                worldMatrix = getRecommendedMatrix(distance);
            }
            else
            {
                // Convert to GVR coordinates.
                worldMatrix = Pose3D.FlipHandedness(userMatrix).transpose;
            }

            Matrix4x4 matToSet = worldMatrix * model.transpose;
            IntPtr mat_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(matToSet));
            Marshal.StructureToPtr(matToSet, mat_ptr, true);
            gvr_keyboard_set_world_from_keyboard_matrix(keyboard_context, mat_ptr);
            gvr_keyboard_show(keyboard_context);
            Marshal.FreeHGlobal(mat_ptr);
        }

        public void UpdateData()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Running on Android device.
            // Update controller state.
            GvrBasePointer pointer = GvrPointerInputModule.Pointer;
            bool isPointerAvailable = pointer != null && pointer.IsAvailable;
            if (isPointerAvailable)
            {
                GvrControllerInputDevice controllerInputDevice = pointer.ControllerInputDevice;
                if (controllerInputDevice != null && controllerInputDevice.State == GvrConnectionState.Connected)
                {
                    bool pressed = controllerInputDevice.GetButton(GvrControllerButton.TouchPadButton);
                    gvr_keyboard_update_button_state(keyboard_context, kGvrControllerButtonClick, pressed);

                    // Update touch state
                    Vector2 touch_pos = controllerInputDevice.TouchPos;
                    IntPtr touch_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(touch_pos));
                    Marshal.StructureToPtr(touch_pos, touch_ptr, true);
                    bool isTouching = controllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch);
                    gvr_keyboard_update_controller_touch(keyboard_context, isTouching, touch_ptr);

                    GvrBasePointer.PointerRay pointerRay = pointer.GetRayForDistance(currentDistance);

                    Vector3 startPoint = pointerRay.ray.origin;

                    // Need to flip Z for native library
                    startPoint.z *= -1;
                    IntPtr start_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(startPoint));
                    Marshal.StructureToPtr(startPoint, start_ptr, true);

                    Vector3 endPoint = pointerRay.ray.GetPoint(pointerRay.distance);

                    // Need to flip Z for native library
                    endPoint.z *= -1;
                    IntPtr end_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(endPoint));
                    Marshal.StructureToPtr(endPoint, end_ptr, true);

                    Vector3 hit = Vector3.one;
                    IntPtr hit_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(Vector3.zero));
                    Marshal.StructureToPtr(Vector3.zero, hit_ptr, true);

                    gvr_keyboard_update_controller_ray(keyboard_context, start_ptr, end_ptr, hit_ptr);
                    hit = (Vector3)Marshal.PtrToStructure(hit_ptr, typeof(Vector3));
                    hit.z *= -1;

                    Marshal.FreeHGlobal(touch_ptr);
                    Marshal.FreeHGlobal(hit_ptr);
                    Marshal.FreeHGlobal(end_ptr);
                    Marshal.FreeHGlobal(start_ptr);
                }
            }
#endif  // UNITY_ANDROID && !UNITY_EDITOR

            // Get time stamp.
            gvr_clock_time_point time = gvr_get_time_point_now();
            time.monotonic_system_time_nanos += kPredictionTimeWithoutVsyncNanos;

            // Update frame data.
            GvrKeyboardSetFrameData(keyboard_context, time);
            GL.IssuePluginEvent(renderEventFunction, advanceID);
        }

        public void Render(int eye, Matrix4x4 modelview, Matrix4x4 projection, Rect viewport)
        {
            gvr_recti rect = new gvr_recti();
            rect.left = (int)viewport.x;
            rect.top = (int)viewport.y + (int)viewport.height;
            rect.right = (int)viewport.x + (int)viewport.width;
            rect.bottom = (int)viewport.y;

            // For the modelview matrix, we need to convert it to a world-to-camera
            // matrix for GVR keyboard, hence the inverse.  We need to convert left
            // handed to right handed, hence the multiply by flipZ.
            // Unity projection matrices are already in a form GVR needs.
            // Unity stores matrices row-major, so both get a final transpose to get
            // them column-major for GVR.
            GvrKeyboardSetEyeData(
                eye,
                (Pose3D.FLIP_Z * modelview.inverse).transpose.inverse,
                projection.transpose,
                rect);
            GL.IssuePluginEvent(renderEventFunction, eye == 0 ? renderLeftID : renderRightID);
        }

        public void Hide()
        {
            gvr_keyboard_hide(keyboard_context);
        }

        // Return the recommended keyboard local to world space
        // matrix given a distance value by the user. This value should
        // be between 1 and 5 and will get clamped to that range.
        private Matrix4x4 getRecommendedMatrix(float inputDistance)
        {
            float distance = Mathf.Clamp(inputDistance, 1.0f, 5.0f);
            Matrix4x4 result = new Matrix4x4();

            IntPtr mat_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(result));
            Marshal.StructureToPtr(result, mat_ptr, true);
            gvr_keyboard_get_recommended_world_from_keyboard_matrix(distance, mat_ptr);

            result = (Matrix4x4)Marshal.PtrToStructure(mat_ptr, typeof(Matrix4x4));
            Marshal.FreeHGlobal(mat_ptr);

            return result;
        }

        // Returns true if the VrInputMethod APK is at least as high as MIN_VERSION_VRINPUTMETHOD.
        private bool IsVrInputMethodAppMinVersion(GvrKeyboard.KeyboardCallback keyboardEvent)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Running on Android device.
            AndroidJavaObject activity = GvrActivityHelper.GetActivity();
            if (activity == null)
            {
                Debug.Log("Failed to get activity for keyboard.");
                return false;
            }

            AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>(METHOD_NAME_GET_PACKAGE_MANAGER);
            if (packageManager == null)
            {
                Debug.Log("Failed to get activity package manager");
                return false;
            }

            AndroidJavaObject info = packageManager.Call<AndroidJavaObject>(METHOD_NAME_GET_PACKAGE_INFO, PACKAGE_NAME_VRINPUTMETHOD, 0);
            if (info == null)
            {
                Debug.Log("Failed to get package info for com.google.android.apps.vr.inputmethod");
                return false;
            }

            int versionCode = info.Get<int>(FIELD_NAME_VERSION_CODE);
            if (versionCode < MIN_VERSION_VRINPUTMETHOD)
            {
                keyboardEvent(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_ERROR_SDK_LOAD_FAILED);
                return false;
            }

            return true;
#else
            return true;
#endif  // UNITY_ANDROID && !UNITY_EDITOR
        }
    }
}

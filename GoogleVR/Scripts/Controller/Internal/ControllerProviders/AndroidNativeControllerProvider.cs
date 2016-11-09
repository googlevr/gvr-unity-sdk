// Copyright 2016 Google Inc. All rights reserved.
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
// See the License for the specific language governing permissioßns and
// limitations under the License.

#if UNITY_HAS_GOOGLEVR && UNITY_ANDROID
using UnityEngine;

using System;
using System.Runtime.InteropServices;

/// @cond
namespace Gvr.Internal {
  /// Controller Provider that uses the native GVR C API to communicate with controllers
  /// via Google VR Services on Android.
  class AndroidNativeControllerProvider : IControllerProvider {
    // Note: keep structs and function signatures in sync with the C header file (gvr_controller.h).
    // GVR controller option flags.
    private const int GVR_CONTROLLER_ENABLE_ORIENTATION = 1 << 0;
    private const int GVR_CONTROLLER_ENABLE_TOUCH = 1 << 1;
    private const int GVR_CONTROLLER_ENABLE_GYRO = 1 << 2;
    private const int GVR_CONTROLLER_ENABLE_ACCEL = 1 << 3;
    private const int GVR_CONTROLLER_ENABLE_GESTURES = 1 << 4;
    private const int GVR_CONTROLLER_ENABLE_POSE_PREDICTION = 1 << 5;

    // enum gvr_controller_button:
    private const int GVR_CONTROLLER_BUTTON_NONE = 0;
    private const int GVR_CONTROLLER_BUTTON_CLICK = 1;
    private const int GVR_CONTROLLER_BUTTON_HOME = 2;
    private const int GVR_CONTROLLER_BUTTON_APP = 3;
    private const int GVR_CONTROLLER_BUTTON_VOLUME_UP = 4;
    private const int GVR_CONTROLLER_BUTTON_VOLUME_DOWN = 5;
    private const int GVR_CONTROLLER_BUTTON_COUNT = 6;

    // enum gvr_controller_connection_state:
    private const int GVR_CONTROLLER_DISCONNECTED = 0;
    private const int GVR_CONTROLLER_SCANNING = 1;
    private const int GVR_CONTROLLER_CONNECTING = 2;
    private const int GVR_CONTROLLER_CONNECTED = 3;

    // enum gvr_controller_api_status
    private const int GVR_CONTROLLER_API_OK = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct gvr_quat {
      internal float x;
      internal float y;
      internal float z;
      internal float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gvr_vec3 {
      internal float x;
      internal float y;
      internal float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gvr_vec2 {
      internal float x;
      internal float y;
    }

    private const string dllName = "gvr";

    [DllImport(dllName)]
    private static extern int gvr_controller_get_default_options();

    [DllImport(dllName)]
    private static extern IntPtr gvr_controller_create_and_init_android(
        IntPtr jniEnv, IntPtr androidContext, IntPtr classLoader,
        int options, IntPtr context);

    [DllImport(dllName)]
    private static extern void gvr_controller_destroy(ref IntPtr api);

    [DllImport(dllName)]
    private static extern void gvr_controller_pause(IntPtr api);

    [DllImport(dllName)]
    private static extern void gvr_controller_resume(IntPtr api);

    [DllImport(dllName)]
    private static extern IntPtr gvr_controller_state_create();

    [DllImport(dllName)]
    private static extern void gvr_controller_state_destroy(ref IntPtr state);

    [DllImport(dllName)]
    private static extern void gvr_controller_state_update(IntPtr api, int flags, IntPtr out_state);

    [DllImport(dllName)]
    private static extern int gvr_controller_state_get_api_status(IntPtr state);

    [DllImport(dllName)]
    private static extern int gvr_controller_state_get_connection_state(IntPtr state);

    [DllImport(dllName)]
    private static extern gvr_quat gvr_controller_state_get_orientation(IntPtr state);

    [DllImport(dllName)]
    private static extern gvr_vec3 gvr_controller_state_get_gyro(IntPtr state);

    [DllImport(dllName)]
    private static extern gvr_vec3 gvr_controller_state_get_accel(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_is_touching(IntPtr state);

    [DllImport(dllName)]
    private static extern gvr_vec2 gvr_controller_state_get_touch_pos(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_touch_down(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_touch_up(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_recentered(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_recentering(IntPtr state);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_button_state(IntPtr state, int button);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_button_down(IntPtr state, int button);

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_button_up(IntPtr state, int button);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_orientation_timestamp(IntPtr state);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_gyro_timestamp(IntPtr state);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_accel_timestamp(IntPtr state);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_touch_timestamp(IntPtr state);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_button_timestamp(IntPtr state);

    private const string UNITY_PLAYER_CLASS = "com.unity3d.player.UnityPlayer";

    private IntPtr api;

    private AndroidJavaObject androidContext;
    private AndroidJavaObject classLoader;

    private bool error;
    private String errorDetails;

    private IntPtr statePtr;

    private MutablePose3D pose3d = new MutablePose3D();

    internal AndroidNativeControllerProvider(bool enableGyro, bool enableAccel) {
      Debug.Log("Initializing Daydream controller API.");

      int options = gvr_controller_get_default_options();
      options |= enableAccel ? GVR_CONTROLLER_ENABLE_ACCEL : 0;
      options |= enableGyro ? GVR_CONTROLLER_ENABLE_GYRO : 0;

      statePtr = gvr_controller_state_create();

      // Get a hold of the activity, context and class loader.
      AndroidJavaObject activity = GetActivity();
      if (activity == null) {
        error = true;
        errorDetails = "Failed to get Activity from Unity Player.";
        return;
      }
      androidContext = GetApplicationContext(activity);
      if (androidContext == null) {
        error = true;
        errorDetails = "Failed to get Android application context from Activity.";
        return;
      }
      classLoader = GetClassLoaderFromActivity(activity);
      if (classLoader == null) {
        error = true;
        errorDetails = "Failed to get class loader from Activity.";
        return;
      }

      // Use IntPtr instead of GetRawObject() so that Unity can shut down gracefully on
      // Application.Quit(). Note that GetRawObject() is not pinned by the receiver so it's not
      // cleaned up appropriately on shutdown, which is a known bug in Unity.
      IntPtr androidContextPtr = AndroidJNI.NewLocalRef(androidContext.GetRawObject());
      IntPtr classLoaderPtr = AndroidJNI.NewLocalRef(classLoader.GetRawObject());
      Debug.Log ("Creating and initializing GVR API controller object.");
      api = gvr_controller_create_and_init_android (IntPtr.Zero, androidContextPtr, classLoaderPtr,
          options, IntPtr.Zero);
      AndroidJNI.DeleteLocalRef(androidContextPtr);
      AndroidJNI.DeleteLocalRef(classLoaderPtr);
      if (IntPtr.Zero == api) {
        Debug.LogError("Error creating/initializing Daydream controller API.");
        error = true;
        errorDetails = "Failed to initialize Daydream controller API.";
        return;
      }



      Debug.Log("Creating and initializing GVR API controller object.");
      api = gvr_controller_create_and_init_android(IntPtr.Zero, androidContext.GetRawObject(),
          classLoader.GetRawObject(), options, IntPtr.Zero);
      if (IntPtr.Zero == api) {
        Debug.LogError("Error creating/initializing Daydream controller API.");
        error = true;
        errorDetails = "Failed to initialize Daydream controller API.";
        return;
      }
      Debug.Log("GVR API successfully initialized. Now resuming it.");
      gvr_controller_resume(api);
      Debug.Log("GVR API resumed.");
    }

    ~AndroidNativeControllerProvider() {
      Debug.Log("Destroying GVR API structures.");
      gvr_controller_state_destroy(ref statePtr);
      Debug.Log("AndroidNativeControllerProvider destroyed.");
    }

    public void ReadState(ControllerState outState) {
      if (error) {
        outState.connectionState = GvrConnectionState.Error;
        outState.errorDetails = errorDetails;
        return;
      }
      gvr_controller_state_update(api, 0, statePtr);

      outState.connectionState = ConvertConnectionState(
          gvr_controller_state_get_connection_state(statePtr));

      gvr_quat rawOri = gvr_controller_state_get_orientation(statePtr);
      gvr_vec3 rawAccel = gvr_controller_state_get_accel(statePtr);
      gvr_vec3 rawGyro = gvr_controller_state_get_gyro(statePtr);

      // Convert GVR API orientation (right-handed) into Unity axis system (left-handed).
      pose3d.Set(Vector3.zero, new Quaternion(rawOri.x, rawOri.y, rawOri.z, rawOri.w));
      pose3d.SetRightHanded(pose3d.Matrix);
      outState.orientation = pose3d.Orientation;

      // For accelerometer, we have to flip Z because the GVR API has Z pointing backwards
      // and Unity has Z pointing forward.
      outState.accel = new Vector3(rawAccel.x, rawAccel.y, -rawAccel.z);

      // Gyro in GVR represents a right-handed angular velocity about each axis (positive means
      // clockwise when sighting along axis). Since Unity uses a left-handed system, we flip the
      // signs to adjust the sign of the rotational velocity (so that positive means
      // counter-clockwise). In addition, since in Unity the Z axis points forward while GVR
      // has Z pointing backwards, we flip the Z axis sign again. So the result is that
      // we should use -X, -Y, +Z:
      outState.gyro = new Vector3(-rawGyro.x, -rawGyro.y, rawGyro.z);

      outState.isTouching = 0 != gvr_controller_state_is_touching(statePtr);

      gvr_vec2 touchPos = gvr_controller_state_get_touch_pos(statePtr);
      outState.touchPos = new Vector2(touchPos.x, touchPos.y);

      outState.touchDown = 0 != gvr_controller_state_get_touch_down(statePtr);
      outState.touchUp = 0 != gvr_controller_state_get_touch_up(statePtr);

      outState.appButtonDown =
        0 != gvr_controller_state_get_button_down(statePtr, GVR_CONTROLLER_BUTTON_APP);
      outState.appButtonState =
        0 != gvr_controller_state_get_button_state(statePtr, GVR_CONTROLLER_BUTTON_APP);
      outState.appButtonUp =
        0 != gvr_controller_state_get_button_up(statePtr, GVR_CONTROLLER_BUTTON_APP);

      outState.clickButtonDown =
        0 != gvr_controller_state_get_button_down(statePtr, GVR_CONTROLLER_BUTTON_CLICK);
      outState.clickButtonState =
        0 != gvr_controller_state_get_button_state(statePtr, GVR_CONTROLLER_BUTTON_CLICK);
      outState.clickButtonUp =
        0 != gvr_controller_state_get_button_up(statePtr, GVR_CONTROLLER_BUTTON_CLICK);

      outState.recentering = 0 != gvr_controller_state_get_recentering(statePtr);
      outState.recentered = 0 != gvr_controller_state_get_recentered(statePtr);
    }

    public void OnPause() {
      if (IntPtr.Zero != api) {
        gvr_controller_pause(api);
      }
    }

    public void OnResume() {
      if (IntPtr.Zero != api) {
        gvr_controller_resume(api);
      }
    }

    private GvrConnectionState ConvertConnectionState(int connectionState) {
      switch (connectionState) {
        case GVR_CONTROLLER_CONNECTED:
          return GvrConnectionState.Connected;
        case GVR_CONTROLLER_CONNECTING:
          return GvrConnectionState.Connecting;
        case GVR_CONTROLLER_SCANNING:
          return GvrConnectionState.Scanning;
        default:
          return GvrConnectionState.Disconnected;
      }
    }

    private static AndroidJavaObject GetActivity() {
      AndroidJavaClass jc = new AndroidJavaClass(UNITY_PLAYER_CLASS);
      if (jc == null) {
        Debug.LogErrorFormat("Failed to get Unity Player class, {0}", UNITY_PLAYER_CLASS);
        return null;
      }
      AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
      if (activity == null) {
        Debug.LogError("Failed to obtain Android Activity from Unity Player class.");
        return null;
      }
      return activity;
    }

    private static AndroidJavaObject GetApplicationContext(AndroidJavaObject activity) {
      AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
      if (context == null) {
        Debug.LogErrorFormat("Failed to get application context from Activity.");
        return null;
      }
      return context;
    }

    private static AndroidJavaObject GetClassLoaderFromActivity(AndroidJavaObject activity) {
      AndroidJavaObject result = activity.Call<AndroidJavaObject>("getClassLoader");
      if (result == null) {
        Debug.LogErrorFormat("Failed to get class loader from Activity.");
        return null;
      }
      return result;
    }
  }
}
/// @endcond

#endif  // UNITY_HAS_GOOGLEVR && UNITY_ANDROID

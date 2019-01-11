//-----------------------------------------------------------------------
// <copyright file="AndroidNativeShimControllerProvider.cs" company="Google Inc.">
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

// This provider is only available on an Android device.
#if UNITY_ANDROID && !UNITY_EDITOR

/// @cond
namespace Gvr.Internal
{
    using UnityEngine;

    using System;
    using System.Runtime.InteropServices;

#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;

#else
    using XRDevice = UnityEngine.VR.VRDevice;

#endif  // UNITY_2017_2_OR_NEWER

    /// Controller Provider that uses the native GVR C API to communicate with controllers
    /// via Google VR Services on Android.
    class AndroidNativeShimControllerProvider : IControllerProvider
    {
        private const int GVR_SHIM_UNITY_API_LEVEL = 1;

        // Shim API return OK value.  Result is error if != OK.
        private const int GVR_SHIM_OK = 1;

        private enum GvrShimUnitySupport
        {
            /// GVRShim is supported.
            Supported = 0,

            /// The GVRShim plugin is not present.  This happens when GVRShim is not included in
            /// the Unity project and the default shim compiled with Unity is used.
            PluginNotPresent = 1,

            /// The expected API level is not supported by GVRShim.
            ApiLevelUnavailable = 2,

            /// The device is not able to run GVRShim.
            DeviceNotSupported = 3,
        }

        private enum GvrShimConnectionStatus
        {
            /// Controller is disconnected.
            Disconnected = 0,

            /// Controller is scanning.
            Scanning = 1,

            /// Controller is connecting.
            Connecting = 2,

            /// Controller is connected.
            Connected = 3,

            /// There was an error connecting to the controller.
            /// This enum subsumes all of the API status errors which are largely obsolete now.
            Error = 100,
        }

        private enum GvrShimTrackedDataAvailableFlags
        {
            PositionAvailable = 1 << 1,
            RotationAvailable = 1 << 2,
            GyroAvailable = 1 << 3,
            AccelerationAvailable = 1 << 4,
        }

        private int lastUpdateFrame = 0;
        private MutablePose3D pose3d = new MutablePose3D();
        private GvrControllerButton[] lastButtonStates = new GvrControllerButton[2];

        public static bool ShimAvailable()
        {
            int supportStatus = (int)GvrShimUnitySupport.ApiLevelUnavailable;
            int retval = 0;
            try
            {
                retval = GvrShimUnity_getGVRShimSupportStatus(GVR_SHIM_UNITY_API_LEVEL, ref supportStatus);
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("GVR Unity shim not found.");
                return false;
            }

            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_getGVRShimSupportStatus returned error.");
                return false;
            }

            if (supportStatus != (int)GvrShimUnitySupport.Supported)
            {
                Debug.LogError("GVR Unity Shim doesn't support shim API level " + GVR_SHIM_UNITY_API_LEVEL);
                return false;
            }

            return true;
        }

        internal AndroidNativeShimControllerProvider()
        {
            int retval = GvrShimUnity_initShimWithContext(XRDevice.GetNativePtr());
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_initShimWithContext returned error.");
            }

            // Start the shim session.
            retval = GvrShimUnity_resumeShim();
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_resumeShim returned error.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Debug.Log("Destroying GVR API structures.");
                int retval = GvrShimUnity_destroyShim();
                if (retval != GVR_SHIM_OK)
                {
                    Debug.LogError("GvrShimUnity_destroyShim returned error.");
                }
            }
        }

        public bool SupportsBatteryStatus
        {
            get { return true; }
        }

        /// Reads the number of controllers the system is configured to use.  This does not
        /// indicate the number of currently connected controllers.
        public int MaxControllerCount
        {
            get
            {
                int count = 0;
                int retval = GvrShimUnity_getControllerCount(ref count);
                if (retval != GVR_SHIM_OK)
                {
                    Debug.LogError("GvrShimUnity_getControllerCount returned error.");
                    return 0;
                }

                return count;
            }
        }

        /// Notifies the controller provider that the application has paused.
        public void OnPause()
        {
            int retval = GvrShimUnity_pauseShim();
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_pauseShim returned error.");
            }
        }

        /// Notifies the controller provider that the application has resumed.
        public void OnResume()
        {
            int retval = GvrShimUnity_resumeShim();
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_resumeShim returned error.");
            }
        }

        private static GvrConnectionState ConvertConnectionState(int connectionState)
        {
            switch (connectionState)
            {
                case (int)GvrShimConnectionStatus.Connected:
                    return GvrConnectionState.Connected;
                case (int)GvrShimConnectionStatus.Connecting:
                    return GvrConnectionState.Connecting;
                case (int)GvrShimConnectionStatus.Scanning:
                    return GvrConnectionState.Scanning;
                case (int)GvrShimConnectionStatus.Disconnected:
                    return GvrConnectionState.Disconnected;
                default:
                    return GvrConnectionState.Error;
            }
        }

        /// Reads the controller's current state and stores it in outState.
        public void ReadState(ControllerState outState, int controller_id)
        {
            int retval = 0;
            if (lastUpdateFrame != Time.frameCount)
            {
                lastUpdateFrame = Time.frameCount;

                retval = GvrShimUnity_updateState();
                if (retval != GVR_SHIM_OK)
                {
                    Debug.LogError("GvrShimUnity_updateState returned error.");
                    return;
                }
            }

            int connStatus = 0;
            retval = GvrShimUnity_getControllerConnectionStatus(controller_id, ref connStatus);
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_getControllerConnectionStatus returned error.");
                return;
            }

            outState.connectionState = ConvertConnectionState(connStatus);

            // Early out if not connected.  No other state is relevant.
            if (outState.connectionState != GvrConnectionState.Connected)
            {
                return;
            }

            GvrShimUnityControllerState aState = new GvrShimUnityControllerState();
            retval = GvrShimUnity_getControllerState(controller_id, ref aState);
            if (retval != GVR_SHIM_OK)
            {
                Debug.LogError("GvrShimUnity_getControllerState returned error.");
                return;
            }

            // Convert GVR API orientation (right-handed) into Unity axis system (left-handed).
            pose3d.Set(aState.position, aState.orientation);
            pose3d.SetRightHanded(pose3d.Matrix);
            outState.orientation = pose3d.Orientation;
            outState.position = pose3d.Position;

            // For accelerometer, we have to flip Z because the GVR API has Z pointing backwards
            // and Unity has Z pointing forward.
            outState.accel = aState.acceleration;
            outState.accel.z *= -1;

            // Gyro in GVR represents a right-handed angular velocity about each axis (positive means
            // clockwise when sighting along axis). Since Unity uses a left-handed system, we flip the
            // signs to adjust the sign of the rotational velocity (so that positive means
            // counter-clockwise). In addition, since in Unity the Z axis points forward while GVR
            // has Z pointing backwards, we flip the Z axis sign again. So the result is that
            // we should use -X, -Y, +Z:
            outState.gyro = aState.gyro;
            outState.gyro.Scale(new Vector3(-1, -1, 1));

            outState.touchPos = aState.touchPos;

            // Shim outputs centered touchpos coordinates, but the ControllerState struct is
            // top-left coordinates. Convert back to top-left.
            outState.touchPos.x = (outState.touchPos.x / 2.0f) + 0.5f;
            outState.touchPos.y = (-outState.touchPos.y / 2.0f) + 0.5f;

            outState.is6DoF = (
                aState.trackedDataAvailable &
                (int)GvrShimTrackedDataAvailableFlags.PositionAvailable) != 0;
            outState.buttonsState = (GvrControllerButton)aState.buttonState;

            // Derive button up/down state from Unity perspective, as it can miss
            // up/downs from platform, like on pause/resume.
            if (controller_id >= 0 && controller_id < lastButtonStates.Length)
            {
                outState.SetButtonsUpDownFromPrevious(lastButtonStates[controller_id]);
                lastButtonStates[controller_id] = outState.buttonsState;
            }

            outState.recentered = (aState.recentered != 0);
            outState.isCharging = (aState.batteryCharging != 0);
            outState.batteryLevel = (GvrControllerBatteryLevel)aState.batteryLevel;
        }

        [StructLayout(LayoutKind.Sequential, Size = 348)]
        private struct GvrShimUnityControllerState
        {
            public Vector3 position;
            public Quaternion orientation;
            public Vector3 gyro;
            public Vector3 acceleration;
            public Vector2 touchPos;
            public int buttonState;
            public int buttonUp;
            public int buttonDown;
            public int recentered;
            public int batteryCharging;
            public int batteryLevel;
            public int connectionState;
            public int trackedDataAvailable;
        }

        private const string shimDllName = GvrActivityHelper.GVR_SHIM_DLL_NAME;

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_getGVRVersion(ref float version);

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_getGVRShimSupportStatus(uint expected_api_level, ref int gvrshim_support_status);

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_initShimWithContext(IntPtr gvr_context);

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_destroyShim();

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_pauseShim();

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_resumeShim();

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_updateState();

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_getControllerConnectionStatus(int device, ref int status);

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_getControllerState(int device, ref GvrShimUnityControllerState state);

        [DllImport(shimDllName)]
        private static extern int GvrShimUnity_getControllerCount(ref int count);
    }
}

/// @endcond
#endif  // UNITY_ANDROID && !UNITY_EDITOR

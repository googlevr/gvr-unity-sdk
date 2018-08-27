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

// This provider is only available on an Android device.
#if UNITY_ANDROID && !UNITY_EDITOR
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
    private const int GVR_CONTROLLER_ENABLE_POSITION = 1 << 6;
    private const int GVR_CONTROLLER_ENABLE_BATTERY = 1 << 7;
    private const int GVR_CONTROLLER_ENABLE_ARM_MODEL = 1 << 8;

    // enum gvr_controller_button:
    private const int GVR_CONTROLLER_BUTTON_NONE = 0;
    private const int GVR_CONTROLLER_BUTTON_CLICK = 1;
    private const int GVR_CONTROLLER_BUTTON_HOME = 2;
    private const int GVR_CONTROLLER_BUTTON_APP = 3;
    private const int GVR_CONTROLLER_BUTTON_VOLUME_UP = 4;
    private const int GVR_CONTROLLER_BUTTON_VOLUME_DOWN = 5;
    private const int GVR_CONTROLLER_BUTTON_RESERVED0 = 6;
    private const int GVR_CONTROLLER_BUTTON_RESERVED1 = 7;
    private const int GVR_CONTROLLER_BUTTON_RESERVED2 = 8;
    private const int GVR_CONTROLLER_BUTTON_COUNT = 9;

    // enum gvr_controller_connection_state:
    private const int GVR_CONTROLLER_DISCONNECTED = 0;
    private const int GVR_CONTROLLER_SCANNING = 1;
    private const int GVR_CONTROLLER_CONNECTING = 2;
    private const int GVR_CONTROLLER_CONNECTED = 3;

    // enum gvr_controller_api_status
    private const int GVR_CONTROLLER_API_OK = 0;
    private const int GVR_CONTROLLER_API_UNSUPPORTED = 1;
    private const int GVR_CONTROLLER_API_NOT_AUTHORIZED = 2;
    private const int GVR_CONTROLLER_API_UNAVAILABLE = 3;
    private const int GVR_CONTROLLER_API_SERVICE_OBSOLETE = 4;
    private const int GVR_CONTROLLER_API_CLIENT_OBSOLETE = 5;
    private const int GVR_CONTROLLER_API_MALFUNCTION = 6;

    // The serialization of button-state used to determine which buttons are being pressed.
    private readonly GvrControllerButton[] GVR_UNITY_BUTTONS = new GvrControllerButton[] {
        GvrControllerButton.App,
        GvrControllerButton.System,
        GvrControllerButton.TouchPadButton,
        GvrControllerButton.Reserved0,
        GvrControllerButton.Reserved1,
        GvrControllerButton.Reserved2
    };
    private readonly int[] GVR_BUTTONS = new int[] {
        GVR_CONTROLLER_BUTTON_APP,
        GVR_CONTROLLER_BUTTON_HOME,
        GVR_CONTROLLER_BUTTON_CLICK,
        GVR_CONTROLLER_BUTTON_RESERVED0,
        GVR_CONTROLLER_BUTTON_RESERVED1,
        GVR_CONTROLLER_BUTTON_RESERVED2
    };

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

    private const string dllName = GvrActivityHelper.GVR_DLL_NAME;

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
    private static extern gvr_vec3 gvr_controller_state_get_position(IntPtr state);

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

    [DllImport(dllName)]
    private static extern byte gvr_controller_state_get_battery_charging(IntPtr state);

    [DllImport(dllName)]
    private static extern int gvr_controller_state_get_battery_level(IntPtr state);

    [DllImport(dllName)]
    private static extern long gvr_controller_state_get_last_battery_timestamp(IntPtr state);

    [DllImport(dllName)]
    private static extern int gvr_controller_get_count(IntPtr api);

    private const string VRCORE_UTILS_CLASS = "com.google.vr.vrcore.base.api.VrCoreUtils";

    private IntPtr api;
    private bool hasBatteryMethods = false;

    private AndroidJavaObject androidContext;
    private AndroidJavaObject classLoader;

    private bool error = false;
    private string errorDetails = string.Empty;

    private IntPtr statePtr;

    private MutablePose3D pose3d = new MutablePose3D();

    private GvrControllerButton[] lastButtonsState = new GvrControllerButton[2];

    public bool SupportsBatteryStatus {
      get { return hasBatteryMethods; }
    }

    public int MaxControllerCount {
      get {
        if (api == IntPtr.Zero) {
          return 0;
        }
        return gvr_controller_get_count(api);
      }
    }

    internal AndroidNativeControllerProvider() {
      // Debug.Log("Initializing Daydream controller API.");

      int options = gvr_controller_get_default_options();
      options |= GVR_CONTROLLER_ENABLE_ACCEL;
      options |= GVR_CONTROLLER_ENABLE_GYRO;
      options |= GVR_CONTROLLER_ENABLE_POSITION;

      statePtr = gvr_controller_state_create();
      // Get a hold of the activity, context and class loader.
      AndroidJavaObject activity = GvrActivityHelper.GetActivity();
      if (activity == null) {
        error = true;
        errorDetails = "Failed to get Activity from Unity Player.";
        return;
      }
      androidContext = GvrActivityHelper.GetApplicationContext(activity);
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

      try {
        gvr_controller_state_get_battery_charging(statePtr);
        gvr_controller_state_get_battery_level(statePtr);
        hasBatteryMethods = true;
      } catch (EntryPointNotFoundException) {
        // Older VrCore version. Does not support battery indicator.
        // Note that controller API is not dynamically loaded as of June 2017 (b/35662043),
        // so we'll need to support this case indefinitely...
      }

      // Debug.Log("GVR API successfully initialized. Now resuming it.");
      gvr_controller_resume(api);
      // Debug.Log("GVR API resumed.");
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        // Debug.Log("Destroying GVR API structures.");
        gvr_controller_state_destroy(ref statePtr);
        gvr_controller_destroy(ref api);
        if (statePtr != IntPtr.Zero) {
          Debug.LogError("gvr_controller_state not zeroed after destroy");
        }
        if (api != IntPtr.Zero) {
          Debug.LogError("gvr_controller_api not zeroed after destroy");
        }
        // Debug.Log("AndroidNativeControllerProvider destroyed.");
      }
    }

    public void ReadState(ControllerState outState, int controller_id) {
      if (error) {
        outState.connectionState = GvrConnectionState.Error;
        outState.apiStatus = GvrControllerApiStatus.Error;
        outState.errorDetails = errorDetails;
        return;
      }
      if (api == IntPtr.Zero || statePtr == IntPtr.Zero) {
        Debug.LogError("AndroidNativeControllerProvider used after dispose.");
        return;
      }
      gvr_controller_state_update(api, controller_id, statePtr);

      outState.connectionState = ConvertConnectionState(
          gvr_controller_state_get_connection_state(statePtr));
      outState.apiStatus = ConvertControllerApiStatus(
          gvr_controller_state_get_api_status(statePtr));

      gvr_quat rawOri = gvr_controller_state_get_orientation(statePtr);
      gvr_vec3 rawAccel = gvr_controller_state_get_accel(statePtr);
      gvr_vec3 rawGyro = gvr_controller_state_get_gyro(statePtr);
      gvr_vec3 rawPos = gvr_controller_state_get_position(statePtr);

      // Convert GVR API orientation (right-handed) into Unity axis system (left-handed).
      pose3d.Set(new Vector3(rawPos.x,rawPos.y,rawPos.z), new Quaternion(rawOri.x, rawOri.y, rawOri.z, rawOri.w));
      pose3d.SetRightHanded(pose3d.Matrix);
      outState.orientation = pose3d.Orientation;
      outState.position = pose3d.Position;

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

      gvr_vec2 touchPos = gvr_controller_state_get_touch_pos(statePtr);
      outState.touchPos = new Vector2(touchPos.x, touchPos.y);

      outState.buttonsState = 0;
      for (int i=0; i<GVR_BUTTONS.Length; i++) {
        if (0 != gvr_controller_state_get_button_state(statePtr, GVR_BUTTONS[i])) {
          outState.buttonsState |= GVR_UNITY_BUTTONS[i];
        }
      }
      if (0 != gvr_controller_state_is_touching(statePtr)) {
        outState.buttonsState |= GvrControllerButton.TouchPadTouch;
      }

      outState.SetButtonsUpDownFromPrevious(lastButtonsState[controller_id]);
      lastButtonsState[controller_id] = outState.buttonsState;

      outState.recentered = 0 != gvr_controller_state_get_recentered(statePtr);
      outState.gvrPtr = statePtr;

      if (hasBatteryMethods) {
        outState.isCharging = 0 != gvr_controller_state_get_battery_charging(statePtr);
        outState.batteryLevel = (GvrControllerBatteryLevel)gvr_controller_state_get_battery_level(statePtr);
      }
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

    private GvrControllerApiStatus ConvertControllerApiStatus(int gvrControllerApiStatus) {
      switch (gvrControllerApiStatus) {
        case GVR_CONTROLLER_API_OK:
          return GvrControllerApiStatus.Ok;
        case GVR_CONTROLLER_API_UNSUPPORTED:
          return GvrControllerApiStatus.Unsupported;
        case GVR_CONTROLLER_API_NOT_AUTHORIZED:
          return GvrControllerApiStatus.NotAuthorized;
        case GVR_CONTROLLER_API_SERVICE_OBSOLETE:
          return GvrControllerApiStatus.ApiServiceObsolete;
        case GVR_CONTROLLER_API_CLIENT_OBSOLETE:
          return GvrControllerApiStatus.ApiClientObsolete;
        case GVR_CONTROLLER_API_MALFUNCTION:
          return GvrControllerApiStatus.ApiMalfunction;
        case GVR_CONTROLLER_API_UNAVAILABLE:
        default:  // Fall through.
          return GvrControllerApiStatus.Unavailable;
      }
    }

    private static void UpdateInputEvents(bool currentState, ref bool previousState, ref bool up, ref bool down) {

      down = !previousState && currentState;
      up = previousState && !currentState;

      previousState = currentState;
    }

    private static AndroidJavaObject GetClassLoaderFromActivity(AndroidJavaObject activity) {
      AndroidJavaObject result = activity.Call<AndroidJavaObject>("getClassLoader");
      if (result == null) {
        Debug.LogErrorFormat("Failed to get class loader from Activity.");
        return null;
      }
      return result;
    }

    private static int GetVrCoreClientApiVersion(AndroidJavaObject activity) {
      try {
        AndroidJavaClass utilsClass = new AndroidJavaClass(VRCORE_UTILS_CLASS);
        int apiVersion = utilsClass.CallStatic<int>("getVrCoreClientApiVersion", activity);
        // Debug.LogFormat("VrCore client API version: " + apiVersion);
        return apiVersion;
      } catch (Exception exc) {
        // Even though a catch-all block is normally frowned upon, in this case we really
        // need it because this method has to be robust to unpredictable circumstances:
        // VrCore might not exist in the device, the Java layer might be broken, etc, etc.
        // None of those should abort the app.
        Debug.LogError("Error obtaining VrCore client API version: " + exc);
        return 0;
      }
    }
  }
}
/// @endcond
#endif  // UNITY_ANDROID && !UNITY_EDITOR

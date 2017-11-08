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

using UnityEngine;
using System;
using System.Collections;

using Gvr.Internal;

/// Represents the controller's current connection state.
/// All values and semantics below (except for Error) are
/// from gvr_types.h in the GVR C API.
public enum GvrConnectionState {
  /// Indicates that an error has occurred.
  Error = -1,

  /// Indicates that the controller is disconnected.
  Disconnected = 0,
  /// Indicates that the device is scanning for controllers.
  Scanning = 1,
  /// Indicates that the device is connecting to a controller.
  Connecting = 2,
  /// Indicates that the device is connected to a controller.
  Connected = 3,
};

/// Represents the API status of the current controller state.
/// Values and semantics from gvr_types.h in the GVR C API.
public enum GvrControllerApiStatus {
  /// A Unity-localized error occurred.
  /// This is the only value that isn't in gvr_types.h.
  Error = -1,

  /// API is happy and healthy. This doesn't mean the controller itself
  /// is connected, it just means that the underlying service is working
  /// properly.
  Ok = 0,

  /// Any other status represents a permanent failure that requires
  /// external action to fix:

  /// API failed because this device does not support controllers (API is too
  /// low, or other required feature not present).
  Unsupported = 1,
  /// This app was not authorized to use the service (e.g., missing permissions,
  /// the app is blacklisted by the underlying service, etc).
  NotAuthorized = 2,
  /// The underlying VR service is not present.
  Unavailable = 3,
  /// The underlying VR service is too old, needs upgrade.
  ApiServiceObsolete = 4,
  /// The underlying VR service is too new, is incompatible with current client.
  ApiClientObsolete = 5,
  /// The underlying VR service is malfunctioning. Try again later.
  ApiMalfunction = 6,
};

/// Represents the controller's current battery level.
/// Values and semantics from gvr_types.h in the GVR C API.
public enum GvrControllerBatteryLevel {
  /// A Unity-localized error occurred.
  /// This is the only value that isn't in gvr_types.h.
  Error = -1,

  /// The battery state is currently unreported
  Unknown = 0,

  /// Equivalent to 1 out of 5 bars on the battery indicator
  CriticalLow = 1,

  /// Equivalent to 2 out of 5 bars on the battery indicator
  Low = 2,

  /// Equivalent to 3 out of 5 bars on the battery indicator
  Medium = 3,

  /// Equivalent to 4 out of 5 bars on the battery indicator
  AlmostFull = 4,

  /// Equivalent to 5 out of 5 bars on the battery indicator
  Full = 5,
};


/// Main entry point for the Daydream controller API.
///
/// To use this API, add this behavior to a GameObject in your scene, or use the
/// GvrControllerMain prefab. There can only be one object with this behavior on your scene.
///
/// This is a singleton object.
///
/// To access the controller state, simply read the static properties of this class. For example,
/// to know the controller's current orientation, use GvrControllerInput.Orientation.
public class GvrControllerInput : MonoBehaviour {
  private static GvrControllerInput instance;
  private static IControllerProvider controllerProvider;

  private ControllerState controllerState = new ControllerState();
  private Vector2 touchPosCentered = Vector2.zero;

  private int lastUpdatedFrameCount = -1;

  /// Event handler for receiving button, touchpad, and IMU updates from the controller.
  /// Use this handler to update app state based on controller input.
  public static event Action OnControllerInputUpdated;

  /// Event handler for receiving a second notification callback, after all
  /// `OnControllerInputUpdated` events have fired.
  public static event Action OnPostControllerInputUpdated;

  /// Event handler for when the connection state of the controller changes.
  public delegate void OnStateChangedEvent(GvrConnectionState state, GvrConnectionState oldState);
  public static event OnStateChangedEvent OnStateChanged;

  public enum EmulatorConnectionMode {
    OFF,
    USB,
    WIFI,
  }
  /// Indicates how we connect to the controller emulator.
  [GvrInfo("Hold Shift to use the Mouse as the controller.\n\n" +
           "Controls:  Shift +\n" +
           "   • Move Mouse = Change Orientation\n" +
           "   • Left Mouse Button = ClickButton\n" +
           "   • Right Mouse Button = AppButton\n" +
           "   • Middle Mouse Button = HomeButton/Recenter\n" +
           "   • Ctrl = IsTouching\n" +
           "   • Ctrl + Move Mouse = Change TouchPos", 8)]
  [Tooltip("How to connect to the emulator: USB cable (recommended) or WIFI.")]
  public EmulatorConnectionMode emulatorConnectionMode = EmulatorConnectionMode.USB;

  /// Returns the controller's current connection state.
  public static GvrConnectionState State {
    get {
      if (instance == null) {
        return GvrConnectionState.Error;
      }
      instance.Update();
      return instance.controllerState.connectionState;
    }
  }

  /// Returns the API status of the current controller state.
  public static GvrControllerApiStatus ApiStatus {
    get {
      if (instance == null) {
        return GvrControllerApiStatus.Error;
      }
      instance.Update();
      return instance.controllerState.apiStatus;
    }
  }

  /// Returns true if battery status is supported.
  public static bool SupportsBatteryStatus {
    get {
      if (controllerProvider == null || instance == null) {
        return false;
      }
      instance.Update();
      return controllerProvider.SupportsBatteryStatus;
    }
  }

  /// Returns the controller's current orientation in space, as a quaternion.
  /// The rotation is provided in 'orientation space' which means the rotation is given relative
  /// to the last time the user recentered their controller. To make a game object in your scene
  /// have the same orientation as the controller, simply assign this quaternion to the object's
  /// `transform.rotation`. To match the relative rotation, use `transform.localRotation` instead.
  public static Quaternion Orientation {
    get {
      if (instance == null) {
        return Quaternion.identity;
      }
      instance.Update();
      return instance.controllerState.orientation;
    }
  }

  /// Returns the controller's current angular speed in radians per second, using the right-hand
  /// rule (positive means a right-hand rotation about the given axis), as measured by the
  /// controller's gyroscope.
  /// The controller's axes are:
  /// - X points to the right,
  /// - Y points perpendicularly up from the controller's top surface
  /// - Z lies along the controller's body, pointing towards the front
  public static Vector3 Gyro {
    get {
      if (instance == null) {
        return Vector3.zero;
      }
      instance.Update();
      return instance.controllerState.gyro;
    }
  }

  /// Returns the controller's current acceleration in meters per second squared.
  /// The controller's axes are:
  /// - X points to the right,
  /// - Y points perpendicularly up from the controller's top surface
  /// - Z lies along the controller's body, pointing towards the front
  /// Note that gravity is indistinguishable from acceleration, so when the controller is resting
  /// on a surface, expect to measure an acceleration of 9.8 m/s^2 on the Y axis. The accelerometer
  /// reading will be zero on all three axes only if the controller is in free fall, or if the user
  /// is in a zero gravity environment like a space station.
  public static Vector3 Accel {
    get {
      if (instance == null) {
        return Vector3.zero;
      }
      instance.Update();
      return instance.controllerState.accel;
    }
  }

  /// Returns true while the user is touching the touchpad.
  public static bool IsTouching {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.isTouching;
    }
  }

  /// Returns true in the frame the user starts touching the touchpad.  Every TouchDown event
  /// is guaranteed to be followed by exactly one TouchUp event in a later frame.
  /// Also, TouchDown and TouchUp will never both be true in the same frame.
  public static bool TouchDown {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.touchDown;
    }
  }

  /// Returns true the frame after the user stops touching the touchpad.  Every TouchUp event
  /// is guaranteed to be preceded by exactly one TouchDown event in an earlier frame.
  /// Also, TouchDown and TouchUp will never both be true in the same frame.
  public static bool TouchUp {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.touchUp;
    }
  }

  /// Position of the current touch, if touching the touchpad.
  /// If not touching, this is the position of the last touch (when the finger left the touchpad).
  /// The X and Y range is from 0 to 1.
  /// (0, 0) is the top left of the touchpad and (1, 1) is the bottom right of the touchpad.
  public static Vector2 TouchPos {
    get {
      if (instance == null) {
        return new Vector2(0.5f,0.5f);
      }
      instance.Update();
      return instance.controllerState.touchPos;
    }
  }

  /// Position of the current touch, if touching the touchpad.
  /// If not touching, this is the position of the last touch (when the finger left the touchpad).
  /// The X and Y range is from -1 to 1.  (-.707,-.707) is bottom left, (.707,.707) is upper right.
  /// (0, 0) is the center of the touchpad.
  /// The magnitude of the touch vector is guaranteed to be <= 1.
  public static Vector2 TouchPosCentered {
    get {
      if (instance == null) {
        return Vector2.zero;
      }
      instance.Update();
      return instance.touchPosCentered;
    }
  }

  [System.Obsolete("Use Recentered to detect when user has completed the recenter gesture.")]
  public static bool Recentering {
    get {
      return false;
    }
  }

  /// Returns true if the user just completed the recenter gesture. The headset and
  /// the controller's orientation are now being reported in the new recentered
  /// coordinate system. This is an event flag (it is true for only one frame
  /// after the event happens, then reverts to false).
  public static bool Recentered {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.recentered;
    }
  }

  /// Returns true while the user holds down the click button (touchpad button).
  public static bool ClickButton {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.clickButtonState;
    }
  }

  /// Returns true in the frame the user starts pressing down the click button.
  /// (touchpad button).  Every ClickButtonDown event is
  /// guaranteed to be followed by exactly one ClickButtonUp event in a later frame.
  /// Also, ClickButtonDown and ClickButtonUp will never both be true in the same frame.
  public static bool ClickButtonDown {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.clickButtonDown;
    }
  }

  /// Returns true the frame after the user stops pressing down the click button.
  /// (touchpad button).  Every ClickButtonUp event
  /// is guaranteed to be preceded by exactly one ClickButtonDown event in an earlier frame.
  /// Also, ClickButtonDown and ClickButtonUp will never both be true in the same frame.
  public static bool ClickButtonUp {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.clickButtonUp;
    }
  }

  /// Returns true while the user holds down the app button.
  public static bool AppButton {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.appButtonState;
    }
  }

  /// Returns true in the frame the user starts pressing down the app button.
  /// Every AppButtonDown event is guaranteed
  /// to be followed by exactly one AppButtonUp event in a later frame.
  /// Also, AppButtonDown and AppButtonUp will never both be true in the same frame.
  public static bool AppButtonDown {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.appButtonDown;
    }
  }

  /// Returns true the frame after the user stops pressing down the app button.
  /// Every AppButtonUp event
  /// is guaranteed to be preceded by exactly one AppButtonDown event in an earlier frame.
  /// Also, AppButtonDown and AppButtonUp will never both be true in the same frame.
  public static bool AppButtonUp {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.appButtonUp;
    }
  }

  /// Returns true in the frame the user starts pressing down the home button.
  public static bool HomeButtonDown {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.homeButtonDown;
    }
  }

  /// Returns true while the user holds down the home button.
  public static bool HomeButtonState {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.homeButtonState;
    }
  }

  /// If State == GvrConnectionState.Error, this contains details about the error.
  public static string ErrorDetails {
    get {
      if (instance != null) {
        instance.Update();
        return instance.controllerState.connectionState == GvrConnectionState.Error ?
          instance.controllerState.errorDetails : "";
      } else {
        return "GvrController instance not found in scene. It may be missing, or it might "
          + "not have initialized yet.";
      }
    }
  }

  // Returns the GVR C library controller state pointer (gvr_controller_state*).
  public static IntPtr StatePtr {
    get {
      if (instance == null) {
        return IntPtr.Zero;
      }
      instance.Update();
      return instance.controllerState.gvrPtr;
    }
  }

  /// Returns true if the controller is currently being charged.
  public static bool IsCharging {
    get {
      if (instance == null) {
        return false;
      }
      instance.Update();
      return instance.controllerState.isCharging;
    }
  }

  /// Returns the controller's current battery charge level.
  public static GvrControllerBatteryLevel BatteryLevel {
    get {
      if (instance == null) {
        return GvrControllerBatteryLevel.Error;
      }
      instance.Update();
      return instance.controllerState.batteryLevel;
    }
  }

  void Awake() {
    if (instance != null) {
      Debug.LogError("More than one GvrController instance was found in your scene. "
        + "Ensure that there is only one GvrControllerInput.");
      this.enabled = false;
      return;
    }
    instance = this;
    if (controllerProvider == null) {
      controllerProvider = ControllerProviderFactory.CreateControllerProvider(this);
    }

    // Keep screen on here, since GvrController must be in any GVR scene in order to enable
    // controller capabilities.
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
  }

  void Update() {
    if(lastUpdatedFrameCount != Time.frameCount){
      // The controller state must be updated prior to any function using the
      // controller API to ensure the state is consistent throughout a frame.
      lastUpdatedFrameCount = Time.frameCount;

      GvrConnectionState oldState = State;

      controllerProvider.ReadState(controllerState);
      UpdateTouchPosCentered();

#if UNITY_EDITOR
      // Make sure the EditorEmulator is updated immediately.
      if (GvrEditorEmulator.Instance != null) {
        GvrEditorEmulator.Instance.UpdateEditorEmulation();
      }
#endif  // UNITY_EDITOR

      if (OnStateChanged != null && State != oldState) {
        OnStateChanged(State, oldState);
      }

      if (OnControllerInputUpdated != null) {
        OnControllerInputUpdated();
      }

      if (OnPostControllerInputUpdated != null) {
        OnPostControllerInputUpdated();
      }
    }
  }

  void OnDestroy() {
    instance = null;
  }

  void OnApplicationPause(bool paused) {
    if (null == controllerProvider) return;
    if (paused) {
      controllerProvider.OnPause();
    } else {
      controllerProvider.OnResume();
    }
  }

  private void UpdateTouchPosCentered() {
    if (instance == null) {
      return;
    }

    touchPosCentered.x = (instance.controllerState.touchPos.x - 0.5f) * 2.0f;
    touchPosCentered.y = -(instance.controllerState.touchPos.y - 0.5f) * 2.0f;

    float magnitude = touchPosCentered.magnitude;
    if(magnitude > 1) {
      touchPosCentered.x /= magnitude;
      touchPosCentered.y /= magnitude;
    }
  }


}

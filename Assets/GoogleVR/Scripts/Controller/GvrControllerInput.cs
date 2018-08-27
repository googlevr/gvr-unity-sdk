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

/// Represents a controller's current connection state.
/// All values and semantics below (except for Error) are
/// from gvr_types.h in the GVR C API.
public enum GvrConnectionState {
  /// Indicates that an error has occurred.
  Error = -1,

  /// Indicates a controller is disconnected.
  Disconnected = 0,
  /// Indicates that the device is scanning for controllers.
  Scanning = 1,
  /// Indicates that the device is connecting to a controller.
  Connecting = 2,
  /// Indicates that the device is connected to a controller.
  Connected = 3,
}

/// Represents the status of the controller API.
/// Values and semantics from gvr_types.h in the GVR C API.
public enum GvrControllerApiStatus {
  /// A Unity-localized error occurred.
  /// This is the only value that isn't in gvr_types.h.
  Error = -1,

  /// API is happy and healthy. This doesn't mean any controllers are
  /// connected, it just means that the underlying service is working
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
}

/// Represents a controller's current battery level.
/// Values and semantics from gvr_types.h in the GVR C API.
public enum GvrControllerBatteryLevel {
  /// A Unity-localized error occurred.
  /// This is the only value that isn't in gvr_types.h.
  Error = -1,

  /// The battery state is currently unreported.
  Unknown = 0,

  /// Equivalent to 1 out of 5 bars on the battery indicator.
  CriticalLow = 1,

  /// Equivalent to 2 out of 5 bars on the battery indicator.
  Low = 2,

  /// Equivalent to 3 out of 5 bars on the battery indicator.
  Medium = 3,

  /// Equivalent to 4 out of 5 bars on the battery indicator.
  AlmostFull = 4,

  /// Equivalent to 5 out of 5 bars on the battery indicator.
  Full = 5,
}

/// Represents controller buttons.
/// Values 0-9 from gvr_types.h in the GVR C API.
/// Value 31 not represented in the C API.
public enum GvrControllerButton {
  /// Button under the touch pad. Formerly known as Click.
  TouchPadButton = 1 << 1,

  /// Touch pad touching indicator.
  TouchPadTouch = 1 << 31,

  /// General application button.
  App = 1 << 3,

  /// System button. Formerly known as Home.
  System = 1 << 2,

  /// Buttons reserved for future use. Subject to name change.
  Reserved0 = 1 << 6,
  Reserved1 = 1 << 7,
  Reserved2 = 1 << 8,

}

/// Represents controller handedness.
public enum GvrControllerHand {
  Right,
  Left,
  Dominant,  // Alias for dominant hand as specified by `GvrSettings.Handedness`.
  NonDominant,  // Alias for non-dominant hand.
}


/// Main entry point for the Daydream controller API.
///
/// To use this API, add this script to a game object in your scene, or use the
/// **GvrControllerMain** prefab.
///
/// This is a singleton object. There can only be one object with this script in your scene.
///
/// To access a controller's state, get a device from `GvrControllerInput.GetDevice` then
/// query it for state. For example, to the dominant controller's current orientation, use
/// `GvrControllerInput.GetDevice(GvrControllerHand.Dominant).Orientation`.
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrControllerInput")]
public class GvrControllerInput : MonoBehaviour {
  private static GvrControllerInputDevice[] instances = new GvrControllerInputDevice[0];
  private static IControllerProvider controllerProvider;
  private static GvrSettings.UserPrefsHandedness handedness;
  private static Action onDevicesChangedInternal;

  /// Event handler for receiving button, touchpad, and IMU updates from the controllers.
  /// Use this handler to update app state based on controller input.
  public static event Action OnControllerInputUpdated;

  /// Event handler for receiving a second notification callback, after all
  /// `OnControllerInputUpdated` events have fired.
  public static event Action OnPostControllerInputUpdated;

  /// Event handler for when the connection state of a controller changes.
  public delegate void OnStateChangedEvent(GvrConnectionState state, GvrConnectionState oldState);

  /// Event handler for when controller devices have changed. Any code that stores a
  /// `GvrControllerInputDevice` should get a new device instance from `GetDevice`.
  /// Existing `GvrControllerInputDevice`s will be marked invalid and will log errors
  /// when used. Event handlers are called immediately when added.
  public static event Action OnDevicesChanged {
    add {
      onDevicesChangedInternal += value;
      value();
    }
    remove {
      onDevicesChangedInternal -= value;
    }
  }

  /// Event handler for when the connection state of the dominant controller changes.
  [System.Obsolete("Replaced by GvrControllerInputDevice.OnStateChangedEvent.")]
  public static event OnStateChangedEvent OnStateChanged {
    add {
      if (instances.Length > 0) {
        instances[0].OnStateChanged += value;
      } else {
        Debug.LogError("GvrControllerInput: Adding OnStateChanged event before instance created.");
      }
    }
    remove {
      if (instances.Length > 0) {
        instances[0].OnStateChanged -= value;
      } else {
        Debug.LogError("GvrControllerInput: Removing OnStateChanged event before instance created.");
      }
    }
  }

  public enum EmulatorConnectionMode {
    OFF,
    USB,
    WIFI,
  }
  /// Indicates how we connect to the controller emulator.
  [GvrInfo("Hold Shift to use the Mouse as the dominant controller.\n\n" +
           "Controls:  Shift +\n" +
           "   • Move Mouse = Change Orientation\n" +
           "   • Left Mouse Button = ClickButton\n" +
           "   • Right Mouse Button = AppButton\n" +
           "   • Middle Mouse Button = HomeButton/Recenter\n" +
           "   • Ctrl = IsTouching\n" +
           "   • Ctrl + Move Mouse = Change TouchPos", 8)]
  [Tooltip("How to connect to the emulator: USB cable (recommended) or WIFI.")]

  public EmulatorConnectionMode emulatorConnectionMode = EmulatorConnectionMode.USB;

  /// Returns a controller device for the specified hand.
  public static GvrControllerInputDevice GetDevice(GvrControllerHand hand) {
    if (instances.Length == 0) {
      return null;
    }
    // Remap Right and Left to Dominant or NonDominant according to settings handedness.
    if (hand == GvrControllerHand.Left || hand == GvrControllerHand.Right) {
      if ((int)hand != (int)handedness) {
        hand = GvrControllerHand.NonDominant;
      } else {
        hand = GvrControllerHand.Dominant;
      }
    }

    if (hand == GvrControllerHand.NonDominant) {
      return instances[1];
    } else {
      // Dominant is always controller 0.
      return instances[0];
    }
  }

  /// Returns the dominant controller's current connection state. Returns
  /// `GvrConnectionState.Error` if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.State.")]
  public static GvrConnectionState State {
    get {
      if (instances.Length == 0) {
        return GvrConnectionState.Error;
      }
      return instances[0].State;
    }
  }

  /// Returns the status of the controller API. Returns
  /// `GvrControllerApiStatus.Error` if `GvrControllerInput` is uninitialized.
  public static GvrControllerApiStatus ApiStatus {
    get {
      if (instances.Length == 0) {
        return GvrControllerApiStatus.Error;
      }
      return instances[0].ApiStatus;
    }
  }

  /// Returns true if battery status is supported. Returns false if
  /// `GvrControllerInput` is uninitialized.
  public static bool SupportsBatteryStatus {
    get {
      if (controllerProvider == null) {
        return false;
      }
      return controllerProvider.SupportsBatteryStatus;
    }
  }

  /// Returns the dominant controller's current orientation in space, as a quaternion.
  /// Returns `Quaternion.identity` if `GvrControllerInput` is uninitialized.
  /// The rotation is provided in 'orientation space' which means the rotation is given relative
  /// to the last time the user recentered their controllers. To make a game object in your scene
  /// have the same orientation as the dominant controller, simply assign this quaternion to the
  /// object's `transform.rotation`. To match the relative rotation, use `transform.localRotation`
  /// instead.
  [System.Obsolete("Replaced by GvrControllerInputDevice.Orientation.")]
  public static Quaternion Orientation {
    get {
      if (instances.Length == 0) {
        return Quaternion.identity;
      }
      return instances[0].Orientation;
    }
  }

  /// Returns the dominant controller's current angular speed in radians per second, using the right-hand
  /// rule (positive means a right-hand rotation about the given axis), as measured by the
  /// controller's gyroscope. Returns `Vector3.zero` if `GvrControllerInput` is uninitialized.
  /// The controller's axes are:
  /// - X points to the right,
  /// - Y points perpendicularly up from the controller's top surface
  /// - Z lies along the controller's body, pointing towards the front
  [System.Obsolete("Replaced by GvrControllerInputDevice.Gyro.")]
  public static Vector3 Gyro {
    get {
      if (instances.Length == 0) {
        return Vector3.zero;
      }
      return instances[0].Gyro;
    }
  }

  /// Returns the dominant controller's current acceleration in meters per second squared.
  /// Returns `Vector3.zero` if `GvrControllerInput` is uninitialized.
  /// The controller's axes are:
  /// - X points to the right,
  /// - Y points perpendicularly up from the controller's top surface
  /// - Z lies along the controller's body, pointing towards the front
  /// Note that gravity is indistinguishable from acceleration, so when the controller is resting
  /// on a surface, expect to measure an acceleration of 9.8 m/s^2 on the Y axis. The accelerometer
  /// reading will be zero on all three axes only if the controller is in free fall, or if the user
  /// is in a zero gravity environment like a space station.
  [System.Obsolete("Replaced by GvrControllerInputDevice.Accel.")]
  public static Vector3 Accel {
    get {
      if (instances.Length == 0) {
        return Vector3.zero;
      }
      return instances[0].Accel;
    }
  }

  /// Returns true while the user is touching the dominant controller's touchpad. Returns
  /// false if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch).")]
  public static bool IsTouching {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButton(GvrControllerButton.TouchPadTouch);
    }
  }

  /// Returns true in the frame the user starts touching the dominant controller's touchpad.
  /// Returns false if `GvrControllerInput` is uninitialized.
  /// Every TouchDown event is guaranteed to be followed by exactly one TouchUp event in a
  /// later frame. Also, TouchDown and TouchUp will never both be true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadTouch).")]
  public static bool TouchDown {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonDown(GvrControllerButton.TouchPadTouch);
    }
  }

  /// Returns true the frame after the user stops touching the dominant controller's touchpad.
  /// Returns false if `GvrControllerInput` is uninitialized.
  /// Every TouchUp event is guaranteed to be preceded by exactly one TouchDown event in an
  /// earlier frame. Also, TouchDown and TouchUp will never both be true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadTouch).")]
  public static bool TouchUp {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonUp(GvrControllerButton.TouchPadTouch);
    }
  }

  /// Position of the dominant controller's current touch, if touching the touchpad.
  /// Returns `Vector2(0.5f, 0.5f)` if `GvrControllerInput` is uninitialized.
  /// If not touching, this is the position of the last touch (when the finger left the touchpad).
  /// The X and Y range is from 0 to 1.
  /// (0, 0) is the top left of the touchpad and (1, 1) is the bottom right of the touchpad.
  [System.Obsolete("Obsolete. Migrate to the center-relative GvrControllerInputDevice.TouchPos.")]
  public static Vector2 TouchPos {
    get {
      if (instances.Length == 0) {
        return new Vector2(0.5f,0.5f);
      }
      Vector2 touchPos = instances[0].TouchPos;
      touchPos.x = (touchPos.x / 2.0f) + 0.5f;
      touchPos.y = (-touchPos.y / 2.0f) + 0.5f;
      return touchPos;
    }
  }

  /// Position of the dominant controller's current touch, if touching the touchpad.
  /// Returns `Vector2.zero` if `GvrControllerInput` is uninitialized.
  /// If not touching, this is the position of the last touch (when the finger left the touchpad).
  /// The X and Y range is from -1 to 1. (-.707,-.707) is bottom left, (.707,.707) is upper right.
  /// (0, 0) is the center of the touchpad.
  /// The magnitude of the touch vector is guaranteed to be <= 1.
  [System.Obsolete("Replaced by GvrControllerInputDevice.TouchPos.")]
  public static Vector2 TouchPosCentered {
    get {
      if (instances.Length == 0) {
        return Vector2.zero;
      }
      return instances[0].TouchPos;
    }
  }

  [System.Obsolete("Use Recentered to detect when user has completed the recenter gesture.")]
  public static bool Recentering {
    get {
      return false;
    }
  }

  /// Returns true if the user just completed the recenter gesture. Returns false if
  /// `GvrControllerInput` is uninitialized. The headset and the dominant controller's
  /// orientation are now being reported in the new recentered coordinate system. This
  /// is an event flag (it is true for only one frame after the event happens, then
  /// reverts to false).
  public static bool Recentered {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].Recentered;
    }
  }

  /// Returns true while the user holds down the dominant controller's touchpad button.
  /// Returns false if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadButton).")]
  public static bool ClickButton {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButton(GvrControllerButton.TouchPadButton);
    }
  }

  /// Returns true in the frame the user starts pressing down the dominant controller's
  /// touchpad button. Returns false if `GvrControllerInput` is uninitialized. Every
  /// ClickButtonDown event is guaranteed to be followed by exactly one ClickButtonUp
  /// event in a later frame. Also, ClickButtonDown and ClickButtonUp will never both be
  /// true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadButton).")]
  public static bool ClickButtonDown {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonDown(GvrControllerButton.TouchPadButton);
    }
  }

  /// Returns true the frame after the user stops pressing down the dominant controller's
  /// touchpad button. Returns false if `GvrControllerInput` is uninitialized. Every
  /// ClickButtonUp event is guaranteed to be preceded by exactly one ClickButtonDown
  /// event in an earlier frame. Also, ClickButtonDown and ClickButtonUp will never both
  /// be true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadButton).")]
  public static bool ClickButtonUp {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonUp(GvrControllerButton.TouchPadButton);
    }
  }

  /// Returns true while the user holds down the dominant controller's app button. Returns
  /// false if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.App).")]
  public static bool AppButton {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButton(GvrControllerButton.App);
    }
  }

  /// Returns true in the frame the user starts pressing down the dominant controller's app button.
  /// Returns false if `GvrControllerInput` is uninitialized. Every AppButtonDown event is
  /// guaranteed to be followed by exactly one AppButtonUp event in a later frame.
  /// Also, AppButtonDown and AppButtonUp will never both be true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.App).")]
  public static bool AppButtonDown {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonDown(GvrControllerButton.App);
    }
  }

  /// Returns true the frame after the user stops pressing down the dominant controller's app button.
  /// Returns false if `GvrControllerInput` is uninitialized. Every AppButtonUp event is guaranteed
  /// to be preceded by exactly one AppButtonDown event in an earlier frame. Also, AppButtonDown
  /// and AppButtonUp will never both be true in the same frame.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.App).")]
  public static bool AppButtonUp {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonUp(GvrControllerButton.App);
    }
  }

  /// Returns true in the frame the user starts pressing down the dominant controller's system button.
  /// Returns false if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.System).")]
  public static bool HomeButtonDown {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButtonDown(GvrControllerButton.System);
    }
  }

  /// Returns true while the user holds down the dominant controller's system button.
  /// Returns false if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.System).")]
  public static bool HomeButtonState {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].GetButton(GvrControllerButton.System);
    }
  }

  /// If the dominant controller's state == GvrConnectionState.Error, this contains details about
  /// the error. If `GvrControllerInput` is uninitialized this returns an error string describing
  /// the uninitialized state.
  [System.Obsolete("Replaced by GvrControllerInputDevice.ErrorDetails.")]
  public static string ErrorDetails {
    get {
      if (instances.Length > 0) {
          return instances[0].ErrorDetails;
      } else {
        return "No GvrControllerInput initialized instance found in scene. It may be missing, or it might "
          + "not have initialized yet.";
      }
    }
  }

  /// Returns the GVR C library controller state pointer (gvr_controller_state*) for the dominant
  /// controller. Returns `IntPtr.Zero` if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.StatePtr.")]
  public static IntPtr StatePtr {
    get {
      if (instances.Length == 0) {
        return IntPtr.Zero;
      }
      return instances[0].StatePtr;
    }
  }

  /// Returns true if the dominant controller is currently being charged. Returns false if
  /// `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.IsCharging.")]
  public static bool IsCharging {
    get {
      if (instances.Length == 0) {
        return false;
      }
      return instances[0].IsCharging;
    }
  }

  /// Returns the dominant controller's current battery charge level. Returns
  /// `GvrControllerBatteryLevel.Error` if `GvrControllerInput` is uninitialized.
  [System.Obsolete("Replaced by GvrControllerInputDevice.BatteryLevel.")]
  public static GvrControllerBatteryLevel BatteryLevel {
    get {
      if (instances.Length == 0) {
        return GvrControllerBatteryLevel.Error;
      }
      return instances[0].BatteryLevel;
    }
  }

  void Awake() {
    if (instances.Length > 0) {
      Debug.LogError("More than one active GvrControllerInput instance was found in your scene. "
        + "Ensure that there is only one GvrControllerInput.");
      this.enabled = false;
      return;
    }
    if (controllerProvider == null) {
      controllerProvider = ControllerProviderFactory.CreateControllerProvider(this);
    }

    handedness = GvrSettings.Handedness;
    int controllerCount = 2;
    instances = new GvrControllerInputDevice[controllerCount];
    for (int i=0; i<controllerCount; i++) {
      instances[i] = new GvrControllerInputDevice(controllerProvider, i);
    }
    if (onDevicesChangedInternal != null) {
      onDevicesChangedInternal();
    }

    // Keep screen on here, since GvrControllerInput must be in any GVR scene in order to enable
    // controller capabilities.
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
  }

  void Update() {
    foreach (var instance in instances) {
      if (instance != null) {
        instance.Update();
      }
    }

    if (OnControllerInputUpdated != null) {
      OnControllerInputUpdated();
    }

    if (OnPostControllerInputUpdated != null) {
      OnPostControllerInputUpdated();
    }
  }

  void OnDestroy() {
    foreach (var instance in instances) {
      // Ensure this device will error if used again.
      instance.Invalidate();
    }
    instances = new GvrControllerInputDevice[0];
    if (onDevicesChangedInternal != null) {
      onDevicesChangedInternal();
    }
  }

  void OnApplicationPause(bool paused) {
    if (null == controllerProvider) return;
    if (paused) {
      controllerProvider.OnPause();
    } else {
      handedness = GvrSettings.Handedness;
      controllerProvider.OnResume();
    }
  }
}

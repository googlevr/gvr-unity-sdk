// Copyright 2018 Google Inc. All rights reserved.
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

/// Device instance of the Daydream controller API.
public class GvrControllerInputDevice {
  private IControllerProvider controllerProvider;
  private int controllerId;

  private ControllerState controllerState = new ControllerState();
  private Vector2 touchPosCentered = Vector2.zero;

  private int lastUpdatedFrameCount = -1;
  private bool valid;

  /// Event handler for when the connection state of the controller changes.
  public event GvrControllerInput.OnStateChangedEvent OnStateChanged;

  internal GvrControllerInputDevice(IControllerProvider provider, int controller_id) {
    controllerProvider = provider;
    controllerId = controller_id;
    valid = true;
  }

  internal void Invalidate() {
    valid = false;
  }

  public bool IsDominantHand {
    get {
      return controllerId == 0;
    }
  }

  public bool IsRightHand {
    [SuppressMemoryAllocationError(IsWarning=true)]
    get {
      if (controllerId == 0) {
        return GvrSettings.Handedness == GvrSettings.UserPrefsHandedness.Right;
      } else {
        return GvrSettings.Handedness == GvrSettings.UserPrefsHandedness.Left;
      }
    }
  }

  /// Returns the controller's current connection state.
  public GvrConnectionState State {
    [SuppressMemoryAllocationError(IsWarning=true)]
    get {
      Update();
      return controllerState.connectionState;
    }
  }

  /// Returns the API status of the current controller state.
  public GvrControllerApiStatus ApiStatus {
    get {
      Update();
      return controllerState.apiStatus;
    }
  }

  // Returns true if the controller can be positionally tracked.
  internal bool SupportsPositionalTracking {
    get {
      return controllerState.is6DoF;
    }
  }

  /// Returns the controller's current orientation in space, as a quaternion.
  /// The rotation is provided in 'orientation space' which means the rotation is given relative
  /// to the last time the user recentered their controller. To make a game object in your scene
  /// have the same orientation as the controller, simply assign this quaternion to the object's
  /// `transform.rotation`. To match the relative rotation, use `transform.localRotation` instead.
  public Quaternion Orientation {
    get {
      Update();
      return controllerState.orientation;
    }
  }

  public Vector3 Position {
    get {
      Update();
      return controllerState.position;
    }
  }

  /// Returns the controller's current angular speed in radians per second, using the right-hand
  /// rule (positive means a right-hand rotation about the given axis), as measured by the
  /// controller's gyroscope.
  /// The controller's axes are:
  /// - X points to the right,
  /// - Y points perpendicularly up from the controller's top surface
  /// - Z lies along the controller's body, pointing towards the front
  public Vector3 Gyro {
    get {
      Update();
      return controllerState.gyro;
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
  public Vector3 Accel {
    get {
      Update();
      return controllerState.accel;
    }
  }

  /// Position of the current touch, if touching the touchpad.
  /// If not touching, this is the position of the last touch (when the finger left the touchpad).
  /// The X and Y range is from -1.0 to 1.0. (0, 0) is the center of the touchpad.
  /// (-.707, -.707) is bottom left, (.707, .707) is upper right.
  /// The magnitude of the touch vector is guaranteed to be <= 1.0.
  public Vector2 TouchPos {
    get {
      Update();
      return touchPosCentered;
    }
  }

  /// Returns true if the user just completed the recenter gesture. The headset and
  /// the controller's orientation are now being reported in the new recentered
  /// coordinate system. This is an event flag (it is true for only one frame
  /// after the event happens, then reverts to false).
  public bool Recentered {
    get {
      Update();
      return controllerState.recentered;
    }
  }

  /// Returns true if the user is holding down any of the buttons specified in `buttons`.
  /// GvrControllerButton types can be OR-ed together to check for multiple buttons at once.
  public bool GetButton(GvrControllerButton buttons) {
    Update();
    return (controllerState.buttonsState & buttons) != 0;
  }

  /// Returns true in the frame the user starts pressing down any of the buttons specified
  /// in `buttons`. For an individual button enum, every ButtonDown event is guaranteed to be
  /// followed by exactly one ButtonUp event in a later frame. Also, ButtonDown and ButtonUp
  /// will never both be true in the same frame for an individual button. Using multiple button
  /// enums OR'ed together can result in multiple ButtonDowns before a ButtonUp.
  public bool GetButtonDown(GvrControllerButton buttons) {
    Update();
    return (controllerState.buttonsDown & buttons) != 0;
  }

  /// Returns true the frame after the user stops pressing down any of the buttons specified
  /// in `buttons`. For an individual button enum, every ButtonUp event is guaranteed to be
  /// preceded by exactly one ButtonDown event in an earlier frame. Also, ButtonDown and
  /// ButtonUp will never both be true in the same frame for an individual button. Using
  /// multiple button enums OR'ed together can result in multiple ButtonUps after multiple
  /// ButtonDowns.
  public bool GetButtonUp(GvrControllerButton buttons) {
    Update();
    return (controllerState.buttonsUp & buttons) != 0;
  }

  /// Returns the bitmask of the buttons that are down in the current frame.
  public GvrControllerButton Buttons {
    get {
      return controllerState.buttonsState;
    }
  }

  /// Returns the bitmask of the buttons that began being pressed in the current frame.
  /// Each individual button enum is guaranteed to be followed by exactly one ButtonsUp
  /// event in a later frame. Also, ButtonsDown and ButtonsUp will never both be true
  /// in the same frame for an individual button.
  public GvrControllerButton ButtonsDown {
    get {
      return controllerState.buttonsDown;
    }
  }

  /// Returns the bitmask of the buttons that ended being pressed in the current frame.
  /// Each individual button enum is guaranteed to be preceded by exactly one ButtonsDown
  /// event in an earlier frame. Also, ButtonsDown and ButtonsUp will never both be true
  /// in the same frame for an individual button.
  public GvrControllerButton ButtonsUp {
    get {
      return controllerState.buttonsUp;
    }
  }

  /// If State == GvrConnectionState.Error, this contains details about the error.
  public string ErrorDetails {
    get {
      Update();
      return controllerState.connectionState == GvrConnectionState.Error ?
        controllerState.errorDetails : "";
    }
  }

  /// Returns the GVR C library controller state pointer (gvr_controller_state*).
  public IntPtr StatePtr {
    get {
      Update();
      return controllerState.gvrPtr;
    }
  }

  /// Returns true if the controller is currently being charged.
  public bool IsCharging {
    get {
      Update();
      return controllerState.isCharging;
    }
  }

  /// Returns the controller's current battery charge level.
  public GvrControllerBatteryLevel BatteryLevel {
    get {
      Update();
      return controllerState.batteryLevel;
    }
  }

  internal void Update() {
    if (lastUpdatedFrameCount != Time.frameCount) {
      if (!valid) {
        Debug.LogError("Using an invalid GvrControllerInputDevice. Please acquire a new one from GvrControllerInput.GetDevice().");
        return;
      }
      // The controller state must be updated prior to any function using the
      // controller API to ensure the state is consistent throughout a frame.
      lastUpdatedFrameCount = Time.frameCount;

      GvrConnectionState oldState = State;

      controllerProvider.ReadState(controllerState, controllerId);
      UpdateTouchPosCentered();

#if UNITY_EDITOR
      if (IsDominantHand) {
        // Make sure the EditorEmulator is updated immediately.
        if (GvrEditorEmulator.Instance != null) {
          GvrEditorEmulator.Instance.UpdateEditorEmulation();
        }
      }
#endif  // UNITY_EDITOR

      if (OnStateChanged != null && State != oldState) {
        OnStateChanged(State, oldState);
      }
    }
  }

  private void UpdateTouchPosCentered() {
    touchPosCentered.x = (controllerState.touchPos.x - 0.5f) * 2.0f;
    touchPosCentered.y = -(controllerState.touchPos.y - 0.5f) * 2.0f;

    float magnitude = touchPosCentered.magnitude;
    if (magnitude > 1) {
      touchPosCentered.x /= magnitude;
      touchPosCentered.y /= magnitude;
    }
  }
}

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
using System.Collections;

/// Represents an object tracked by controller input.
/// Manages the active status of the tracked controller based on controller connection status.
/// Fetches a `GvrControllerInputDevice` for the configured `GvrControllerHand` and propagates
/// the device instance to all `IGvrControllerInputDeviceReceiver`s underneath this object on
/// Start and if the controller handedness changes. If the controller is not positionally
/// tracked, position of the object is updated to approximate arm mechanics by using a
/// `GvrBaseArmModel`.  `GvrBaseArmModel`s are also propagated to all `IGvrArmModelReceiver`s
/// underneath this object.
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrTrackedController")]
public class GvrTrackedController : MonoBehaviour {
  [SerializeField]
  [Tooltip("Arm model used to control the pose (position and rotation) of the object, " +
    "and to propagate to children that implement IGvrArmModelReceiver.")]
  private GvrBaseArmModel armModel;
  private GvrControllerInputDevice controllerInputDevice;

  [SerializeField]
  [Tooltip("Is the object's active status determined by the controller connection status.")]
  private bool isDeactivatedWhenDisconnected = true;

  [SerializeField]
  [Tooltip("Controller Hand")]
  private GvrControllerHand controllerHand = GvrControllerHand.Dominant;

  public GvrControllerInputDevice ControllerInputDevice {
    get {
      return controllerInputDevice;
    }
  }

  public GvrControllerHand ControllerHand {
    get {
      return controllerHand;
    }
    set {
      if (value != controllerHand) {
        controllerHand = value;
        SetupControllerInputDevice();
      }
    }
  }

  /// Arm model used to control the pose (position and rotation) of the object, and to propagate to
  /// children that implement IGvrArmModelReceiver.
  public GvrBaseArmModel ArmModel {
    get {
      return armModel;
    }
    set {
      if (armModel == value) {
        return;
      }

      armModel = value;
      PropagateControllerInputDeviceToArmModel();
      PropagateArmModel();
    }
  }

  /// Is the object's active status determined by the controller connection status.
  public bool IsDeactivatedWhenDisconnected {
    get {
      return isDeactivatedWhenDisconnected;
    }
    set {
      if (isDeactivatedWhenDisconnected == value) {
        return;
      }

      isDeactivatedWhenDisconnected = value;

      if (isDeactivatedWhenDisconnected) {
        OnControllerStateChanged(controllerInputDevice.State, controllerInputDevice.State);
      }
    }
  }

  public void PropagateArmModel() {
    IGvrArmModelReceiver[] receivers =
      GetComponentsInChildren<IGvrArmModelReceiver>(true);

    for (int i = 0; i < receivers.Length; i++) {
      IGvrArmModelReceiver receiver = receivers[i];
      receiver.ArmModel = armModel;
    }
  }

  void Awake() {
    // Adding this event handler calls it immediately.
    GvrControllerInput.OnDevicesChanged += SetupControllerInputDevice;
  }

  void OnEnable() {
    // Print an error to console if no GvrControllerInput is found.
    if (controllerInputDevice.State == GvrConnectionState.Error) {
      Debug.LogWarning(controllerInputDevice.ErrorDetails);
    }

    // Update the position using OnPostControllerInputUpdated.
    // This way, the position and rotation will be correct for the entire frame
    // so that it doesn't matter what order Updates get called in.
    GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;

    /// Force the pose to update immediately in case the controller isn't updated before the next
    /// time a frame is rendered.
    UpdatePose();

    /// Check the controller state immediately whenever this script is enabled.
    OnControllerStateChanged(controllerInputDevice.State, controllerInputDevice.State);
  }

  void OnDisable() {
    GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
  }

  void Start() {
    PropagateArmModel();
    if (controllerInputDevice != null) {
      PropagateControllerInputDevice();
      OnControllerStateChanged(controllerInputDevice.State, controllerInputDevice.State);
    }
  }

  void OnDestroy() {
    GvrControllerInput.OnDevicesChanged -= SetupControllerInputDevice;
    if (controllerInputDevice != null) {
      controllerInputDevice.OnStateChanged -= OnControllerStateChanged;
      controllerInputDevice = null;
      PropagateControllerInputDevice();
    }
  }

  private void PropagateControllerInputDevice() {
    IGvrControllerInputDeviceReceiver[] receivers =
      GetComponentsInChildren<IGvrControllerInputDeviceReceiver>(true);

    foreach (var receiver in receivers) {
      receiver.ControllerInputDevice = controllerInputDevice;
    }
    PropagateControllerInputDeviceToArmModel();
  }

  private void PropagateControllerInputDeviceToArmModel() {
    // Propagate the controller input device to everything in the arm model's object's
    // hierarchy in case it is not a child of the tracked controller.
    if (armModel != null) {
      IGvrControllerInputDeviceReceiver[] receivers =
        armModel.GetComponentsInChildren<IGvrControllerInputDeviceReceiver>(true);

      foreach (var receiver in receivers) {
        receiver.ControllerInputDevice = controllerInputDevice;
      }
    }
  }

  private void SetupControllerInputDevice() {
    GvrControllerInputDevice newDevice = GvrControllerInput.GetDevice(controllerHand);
    if (controllerInputDevice == newDevice) {
      return;
    }
    if (controllerInputDevice != null) {
      controllerInputDevice.OnStateChanged -= OnControllerStateChanged;
      controllerInputDevice = null;
    }

    controllerInputDevice = newDevice;
    if (controllerInputDevice != null) {
      controllerInputDevice.OnStateChanged += OnControllerStateChanged;
      OnControllerStateChanged(controllerInputDevice.State, controllerInputDevice.State);
    } else {
      OnControllerStateChanged(GvrConnectionState.Disconnected, GvrConnectionState.Disconnected);
    }
    PropagateControllerInputDevice();
  }

  private void OnPostControllerInputUpdated() {
    UpdatePose();
  }

  private void OnControllerStateChanged(GvrConnectionState state, GvrConnectionState oldState) {
    if (isDeactivatedWhenDisconnected && enabled) {
      gameObject.SetActive(state == GvrConnectionState.Connected);
    }
  }

  private void UpdatePose() {
    if (controllerInputDevice == null) {
      return;
    }

    // Non-positionally tracked controllers always return Position of Vector3.zero.
    if (controllerInputDevice.Position != Vector3.zero) {
      transform.localPosition = controllerInputDevice.Position;
      transform.localRotation = controllerInputDevice.Orientation;
    } else {
      if (armModel == null || !controllerInputDevice.IsDominantHand) {
        return;
      }

      transform.localPosition = ArmModel.ControllerPositionFromHead;
      transform.localRotation = ArmModel.ControllerRotationFromHead;
    }
  }

#if UNITY_EDITOR
  /// If the "armModel" serialized field is changed while the application is playing
  /// by using the inspector in the editor, then we need to call the PropagateArmModel
  /// to ensure all children IGvrArmModelReceiver are updated.
  /// Outside of the editor, this can't happen because the arm model can only change when
  /// a Setter is called that automatically calls PropagateArmModel.
  void OnValidate() {
    if (Application.isPlaying && isActiveAndEnabled) {
      PropagateArmModel();
      if (controllerInputDevice != null) {
        OnControllerStateChanged(controllerInputDevice.State, controllerInputDevice.State);
      }
    }
  }
#endif  // UNITY_EDITOR

}

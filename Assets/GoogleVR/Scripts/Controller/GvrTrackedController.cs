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
///
/// Updates the position and rotation of an object to approximate the controller by using
/// a _GvrBaseArmModel_ and propagates the _GvrBaseArmModel_ to all _IGvrArmModelReceivers_
/// underneath this object.
///
/// Manages the active status of the tracked controller based on controller connection status.
public class GvrTrackedController : MonoBehaviour {
  [SerializeField]
  [Tooltip("Arm model used to control the pose (position and rotation) of the object, " +
    "and to propagate to children that implement IGvrArmModelReceiver.")]
  private GvrBaseArmModel armModel;

  [SerializeField]
  [Tooltip("Is the object's active status determined by the controller connection status.")]
  private bool isDeactivatedWhenDisconnected = true;

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
        OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
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
    if (isDeactivatedWhenDisconnected) {
      GvrControllerInput.OnStateChanged += OnControllerStateChanged;
    }
  }

  void OnEnable() {
    // Update the position using OnPostControllerInputUpdated.
    // This way, the position and rotation will be correct for the entire frame
    // so that it doesn't matter what order Updates get called in.
    GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;

    /// Force the pose to update immediately in case the controller isn't updated before the next
    /// time a frame is rendered.
    UpdatePose();

    GvrControllerInput.OnStateChanged += OnControllerStateChanged;
    /// Check the controller state immediately whenever this script is enabled.
    OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
  }

  void OnDisable() {
    GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
  }

  void Start() {
    PropagateArmModel();
    OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
  }

  void OnDestroy() {
    GvrControllerInput.OnStateChanged -= OnControllerStateChanged;
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
    if (armModel == null) {
      return;
    }

    transform.localPosition = ArmModel.ControllerPositionFromHead;
    transform.localRotation = ArmModel.ControllerRotationFromHead;
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
      OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
    }
  }
#endif  // UNITY_EDITOR

}

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
///
/// Provides access to the laser and the controller visual.
/// Allows for enabling and disabling the laser/controller independently without
/// causing conflicts with enabling/disabling based on the controller connection status.
///
/// Propogates a _GvrBaseArmModel_ to all _IGvrArmModelReceivers_ underneath this object
/// so that they can follow the pose from the arm model.
public class GvrTrackedController : MonoBehaviour {
  /// Reference to the object that represents the Laser.
  public GvrLaserVisual laserVisual;

  /// Reference to the object that represents the Controller.
  public GvrControllerVisual controllerVisual;

  [SerializeField]
  private GvrBaseArmModel armModel;

  [SerializeField]
  private bool isLaserVisualEnabled = true;

  [SerializeField]
  private bool isControllerVisualEnabled = true;

  [SerializeField]
  private bool isVisibleWhenDisconnected = false;

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

  public bool IsLaserVisualEnabled {
    get {
      return isLaserVisualEnabled;
    }
    set {
      if (isLaserVisualEnabled == value) {
        return;
      }

      isLaserVisualEnabled = value;
      RefreshActiveStatus();
    }
  }

  public bool IsControllerVisualEnabled {
    get {
      return isControllerVisualEnabled;
    }
    set {
      if (isControllerVisualEnabled == value) {
        return;
      }

      isControllerVisualEnabled = value;
      RefreshActiveStatus();
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

  void Start() {
    PropagateArmModel();
    RefreshActiveStatus();
  }

  void Update() {
    RefreshActiveStatus();
  }

  private bool IsControllerConnected() {
    return GvrControllerInput.State == GvrConnectionState.Connected;
  }

  private void RefreshActiveStatus() {
    bool isVisible = isVisibleWhenDisconnected || IsControllerConnected();

    if (laserVisual != null) {
      laserVisual.gameObject.SetActive(IsLaserVisualEnabled && isVisible);
    }

    if (controllerVisual != null) {
      controllerVisual.gameObject.SetActive(IsControllerVisualEnabled && isVisible);
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
    }
  }
#endif  // UNITY_EDITOR

}

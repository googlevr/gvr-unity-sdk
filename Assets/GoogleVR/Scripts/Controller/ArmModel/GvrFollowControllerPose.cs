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

/// This script positions and rotates the transform that it is attached to
/// according to a pose in the arm model. See GvrArmModel.cs for details.
public class GvrFollowControllerPose : MonoBehaviour, IGvrArmModelReceiver {
  public enum Pose {
    ControllerFromHead,
    PointerFromController
  }

  /// Determines which pose to set the position and rotation to.
  public Pose pose;

  public GvrBaseArmModel ArmModel {
    get {
      return armModel;
    } set {
      armModel = value;
    }
  }

  /// Source for the controller's poses.
  [SerializeField]
  private GvrBaseArmModel armModel;

  void OnEnable() {
    // Update the position using OnPostControllerInputUpdated.
    // This way, the position and rotation will be correct for the entire frame
    // so that it doesn't matter what order Updates get called in.
    GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
  }

  void OnDisable() {
    GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
  }

  private void OnPostControllerInputUpdated() {
    if (armModel == null) {
      return;
    }

    Vector3 position;
    Quaternion rotation;

    switch (pose) {
      case Pose.ControllerFromHead:
        position = ArmModel.ControllerPositionFromHead;
        rotation = ArmModel.ControllerRotationFromHead;
        break;
      case Pose.PointerFromController:
        position = ArmModel.PointerPositionFromController;
        rotation = ArmModel.PointerRotationFromController;
        break;
      default:
        throw new System.Exception("Invalid mode.");
    }

    transform.localPosition = position;
    transform.localRotation = rotation;
  }
}

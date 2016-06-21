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
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace GVR.Input {
  /// <summary>
  /// Matches the attached object's local rotation to the controller's
  /// with an available mask for removing/reversing specific axes.
  /// </summary>
  public class MaskedOrientationInput : MonoBehaviour {
    [Tooltip("Multiplied component-wise vs the controller's local rotation")]
    public Vector3 Mask = Vector3.one;

    [Tooltip("If true, the orientation match will occur during LateUpdate")]
    public bool UseLateUpdate;

    void Update() {
      if (!UseLateUpdate) {
        MaskRotation();
      }
    }

    void LateUpdate() {
      if (UseLateUpdate) {
        MaskRotation();
      }
    }

    private void MaskRotation() {
      Vector3 lastRotation = GvrController.Orientation.eulerAngles;
      transform.localRotation = Quaternion.Euler(new Vector3 {
        x = lastRotation.x * Mask.x,
        y = lastRotation.y * Mask.y,
        z = lastRotation.z * Mask.z
      });
    }
  }
}

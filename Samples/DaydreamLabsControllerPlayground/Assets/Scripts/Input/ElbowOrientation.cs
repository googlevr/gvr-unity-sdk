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
  /// Mimic elbow/forearm rotation using controller input.
  /// </summary>
  public class ElbowOrientation : MonoBehaviour {
    [Tooltip("Maximum allowed forward X axis rotation (toward ground)")]
    public float Max = 350f;

    [Tooltip("Minium allowed reverse X axis rotation (toward player)")]
    public float Min = 300f;

    [Tooltip("If true, oreientation will occur during LateUpdate")]
    public bool UseLateUpdate = true;

    [Tooltip("If true, modify local rotation only")]
    public bool UseLocalOrientation = false;

    [Tooltip("Use this vector to remove or reverse specific axes.")]
    public Vector3 Mask = Vector3.one;

    void Update() {
      if (!UseLateUpdate) {
        ClampRotation();
      }
    }

    void LateUpdate() {
      if (UseLateUpdate) {
        ClampRotation();
      }
    }

    private void ClampRotation() {
      if (UseLocalOrientation) {
        transform.localRotation = GvrController.Orientation;
      } else {
        transform.rotation = GvrController.Orientation;
      }
      Vector3 local = transform.localEulerAngles;
      if (local.x < 90f || local.x > Max) {
        local.x = Max;
      }
      if (local.x < Min) {
        local.x = Min;
      }
      transform.localEulerAngles = new Vector3(local.x * Mask.x, local.y * Mask.y, local.z * Mask.z);
    }
  }
}

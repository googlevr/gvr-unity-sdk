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

namespace GVR.Samples.Pancakes {
  /// <summary>
  ///  This component prevents pancakes from flying too close to the HMD
  ///  without an obvious collider. This way, angular momentum is
  ///  preserved.
  /// </summary>
  public class PancakeLimiter : MonoBehaviour {
    [Tooltip("Reference to the pancake's rigidbody")]
    public Rigidbody ActiveRigidbody;

    [Tooltip("Reference to the HMD transform")]
    public Transform PlayerRoot;

    [Tooltip("Minimum distance from the HMD pancakes are allowed to travel")]
    public float MinDistanceXZ;

    void FixedUpdate() {
      if (PlayerRoot == null) {
        return;
      }
      if (Vector3.Distance(Vector3.ProjectOnPlane(transform.position, Vector3.up),
            Vector3.ProjectOnPlane(PlayerRoot.transform.position, Vector3.up)) > MinDistanceXZ) {
        return;
      }
      Vector3 inwardVector =
          Vector3.ProjectOnPlane(PlayerRoot.transform.position - transform.position, Vector3.up).normalized;
      Vector3 projVector = Vector3.Project(ActiveRigidbody.velocity, inwardVector);
      float sign = Vector3.Dot(inwardVector, projVector);
      ActiveRigidbody.velocity += projVector * -sign;
    }
  }
}

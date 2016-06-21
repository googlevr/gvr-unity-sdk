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

namespace GVR.Samples.Adventure {
  /// <summary>
  /// A simple drop shadow. This raycast down to find a position and
  /// orientation to place a shadow object.
  /// </summary>
  public class DropShadow : MonoBehaviour {
    [Tooltip("Max distance to raycast. If nothing is hit in this distance the shadow is disabled.")]
    public float Distance;

    [Tooltip("Transform of the shadow quad. Moved to position and rotation of raycast.")]
    public Transform Quad;

    [Tooltip("Offset from this transform position to start the raycast.")]
    public float RaycastOffset;

    [Tooltip("Amount to move the shadow up off of a contact to prevent z-fighting.")]
    public float Bias = 0.01f;

    [Tooltip("Should this run every frame or just on start?")]
    public bool CalculateEveryFrame;

    void Start() {
      if (!CalculateEveryFrame) {
        SetShadowPosition();
      }
    }

    void Update() {
      if (CalculateEveryFrame) {
        SetShadowPosition();
      }
    }

    public void SetShadowPosition() {
      var down = new Ray(transform.position + (Vector3.up * RaycastOffset), Vector3.down);
      RaycastHit downHit;
      bool didHit = Physics.Raycast(down, out downHit, Distance,
                                    int.MaxValue, QueryTriggerInteraction.Ignore);
      if (didHit) {
        if (!Quad.gameObject.activeInHierarchy) {
          Quad.gameObject.SetActive(true);
        }
        Quad.position = downHit.point + (Vector3.up * Bias);
        Quad.forward = downHit.normal;
      } else {
        if (Quad.gameObject.activeInHierarchy) {
          Quad.gameObject.SetActive(false);
        }
      }
    }

  }
}

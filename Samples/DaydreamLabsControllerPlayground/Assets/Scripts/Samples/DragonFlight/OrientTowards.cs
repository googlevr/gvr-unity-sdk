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

using GVR.GUI;

using UnityEngine;

namespace GVR.Samples.DragonFlight {
  /// <summary>
  /// Rotates the transform that this component is attached to towards a Target world-space
  /// Orientation, represented by a separate transform. For use in the Dragon Flight demo,
  /// this also supports a Position override used to orient towards a target position. The best
  /// example use case for this is in the Dragon Flight demo, where the Dragon is given nearby
  /// "Bite Targets" as Position Overrides, in order to assist the player in aiming the dragon
  /// towards them.
  /// </summary>
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class OrientTowards : MonoBehaviour {
    [Tooltip("A multiplier to the basic SLERP between the transform's current rotation and the " +
             "target rotation. The rotation will exponentially decrease just shy of reaching " +
             "the target rotation, so this value has no meaningful world unit.")]
    public float Rate = 2.0f;

    [Tooltip("The Target orientation of the driving transform. Only the rotation property of " +
             "this transform is relevant.")]
    public Transform TargetOrientation;

    [Tooltip("A positional override to the target orientation, useful for orienting the " +
             "transform towards a target point in the world. This point can be dynamically " +
             "updated, and only the position component of the Transform is used.")]
    public Transform PositionTargetOverride;

    private LevelSelectMenuListener levelSelectMenuListener;

    void Awake() {
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
    }

    void Update() {
      if (levelSelectMenuListener.IsMenuOpen) {
        return;
      }
      if (PositionTargetOverride) {
        Vector3 pos = PositionTargetOverride.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(pos);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Rate * Time.deltaTime);
        if (PositionTargetOverride.gameObject.activeSelf == false) {
          ClearTarget();
        }
      } else {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, TargetOrientation.localRotation, Rate * Time.deltaTime);
      }
    }

    public void SetTarget(Transform newTarget) {
      PositionTargetOverride = newTarget;
    }

    public void ClearTarget() {
      PositionTargetOverride = null;
    }
  }
}

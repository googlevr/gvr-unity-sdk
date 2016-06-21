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
using UnityEngine.Events;

namespace GVR.Input {
  public class DirectionalSlashInput : MonoBehaviour {
    public UnityEvent OnDownSlash;
    public UnityEvent OnUpSlash;
    public UnityEvent OnLeftSlash;
    public UnityEvent OnRightSlash;

    [Tooltip("Radians per second requirement to trigger a slash.")]
    public float SlashThreshold = 10f;

    public float SlashCooldown = 0.5f;

    public bool UseWorldSpaceSlash = true;

    private float currentCooldown = 0f;

    private Vector3 currentAccel = Vector3.zero;

    void Update() {
      currentAccel = GvrController.Gyro;
      if (currentCooldown > 0) {
        currentCooldown -= Time.deltaTime;
      } else {
        CheckSlashTriggers();
      }
    }

    void CheckSlashTriggers() {
      Vector3 transformedAccel = UseWorldSpaceSlash ? GvrController.Orientation * currentAccel
                                                    : currentAccel;
      float Xmag = Mathf.Abs(transformedAccel.x);
      float Ymag = Mathf.Abs(transformedAccel.y);
      if (Xmag > SlashThreshold || Ymag > SlashThreshold) {
        currentCooldown = SlashCooldown;
        if (Xmag > Ymag) {
          //If X is the greatest Magnitude.
          if (transformedAccel.x > 0) {
            OnUpSlash.Invoke();
          } else {
            OnDownSlash.Invoke();
          }
        } else {
          //If Z is the greatest Magnitude.
          if (transformedAccel.z > 0) {
            OnRightSlash.Invoke();
          } else {
            OnLeftSlash.Invoke();
          }
        }
      }
    }
  }
}

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

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Simple controller for the Dragonlings in the Dragon Team demo. Used as a pass-through to
  /// automate simple functions in the Animator, such as an idle variation.
  /// </summary>
  public class DragonlingAnimController : MonoBehaviour {
    [Tooltip("The animator for the Dragonling.")]
    public Animator AnimController;

    [Tooltip("Minimum time between idle variations.")]
    public float MinIdleVarTimer = 2f;

    [Tooltip("Maximum time between idle variations.")]
    public float MaxIdleVarTimer = 5f;

    private float currentIdleCooldown = 0f;

    void Start() {
      ResetIdleTimer();
    }

    void ResetIdleTimer() {
      currentIdleCooldown = Random.Range(MinIdleVarTimer, MaxIdleVarTimer);
    }

    void Update() {
      if (currentIdleCooldown <= 0) {
        AnimController.SetTrigger("IdleVariation");
        ResetIdleTimer();
      } else {
        currentIdleCooldown -= Time.deltaTime;
      }
    }

    public void SetMoveSpeed(float speed) {
      AnimController.SetFloat("MoveSpeed", speed);
    }

    public void Sing() {
      AnimController.SetTrigger("Sing");
    }

    public void SetSleepVisuals(bool sleeping) {
      if (sleeping) {
        AnimController.SetTrigger("Sleep");
      } else {
        AnimController.SetTrigger("Wake");
      }
    }
  }
}

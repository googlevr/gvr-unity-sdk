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

namespace GVR.Samples.Adventure {
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class AdventurePlayerAnimationController: MonoBehaviour, ICharacterAnimatorController {
    public string ForwardMovementParamName = "Rate";
    private int forwardMovementParamID;

    public string AbilityParamName = "Sing";
    private int abilityParamID;

    public Animator Animator;
    public bool InstantRotate;
    public float RotateSpeed = 10.0f;

    private LevelSelectMenuListener levelSelectMenuListener;

    void Start() {
      forwardMovementParamID = Animator.StringToHash(ForwardMovementParamName);
      abilityParamID = Animator.StringToHash(AbilityParamName);

      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
      levelSelectMenuListener.OnLevelSelectMenuClosed.AddListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.AddListener(OnMenuOpened);
    }

    void OnDestroy() {
      levelSelectMenuListener.OnLevelSelectMenuClosed.RemoveListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.RemoveListener(OnMenuOpened);
    }

    public Vector3 GetAnimationDelta() {
      return Animator.deltaPosition;
    }

    public void UseAbility() {
      if (Animator.enabled) {
        Animator.SetTrigger(abilityParamID);
      }
    }

    public void ProcessInput(Vector3 direction, float maxSpeed, float dt) {
      Vector3 idealMove = direction * maxSpeed * Time.deltaTime;

      if (InstantRotate) {
        transform.LookAt(transform.position + idealMove);
      } else {
        transform.rotation = Quaternion.LookRotation(
            Vector3.RotateTowards(transform.forward, idealMove, RotateSpeed * Time.deltaTime, 0.0f));
      }

      if (direction.sqrMagnitude > 0.001f) {
        Animator.SetFloat(forwardMovementParamID, direction.magnitude * maxSpeed);
      } else {
        Animator.SetFloat(forwardMovementParamID, 0.0f);
      }
    }

    #region -- LevelSelectMenuListener Functions --------------------------

    private void OnMenuClosed() {
      Animator.enabled = true;
    }

    private void OnMenuOpened() {
      Animator.enabled = false;
    }

    #endregion -- LevelSelectMenuListener Functions -----------------------
  }
}

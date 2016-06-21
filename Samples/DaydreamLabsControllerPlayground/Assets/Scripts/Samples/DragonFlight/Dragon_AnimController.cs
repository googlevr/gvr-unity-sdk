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
  /// Very simple Stub for an Animation Controller on the Dragon's head in the Dragon Flight demo.
  /// The Snap Method is called by the Hitbox on the Snap Trigger GameObject, nested under the
  /// Dragon's Head Motor.
  /// </summary>
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class Dragon_AnimController : MonoBehaviour {
    public Animator animator;

    private LevelSelectMenuListener levelSelectMenuListener;

    void Awake() {
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
      levelSelectMenuListener.OnLevelSelectMenuClosed.AddListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.AddListener(OnMenuOpened);
    }

    void OnDestroy() {
      levelSelectMenuListener.OnLevelSelectMenuClosed.RemoveListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.RemoveListener(OnMenuOpened);
    }

    public void Snap() {
      if (animator.enabled) {
        animator.SetTrigger("Snap");
      }
    }

    private void OnMenuClosed() {
      animator.enabled = true;
    }

    private void OnMenuOpened() {
      animator.enabled = false;
    }
  }
}

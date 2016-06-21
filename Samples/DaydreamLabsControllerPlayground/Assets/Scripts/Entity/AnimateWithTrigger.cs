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

namespace GVR.Entity {
  /// <summary>
  /// Starts an animation with a trigger.
  /// </summary>
  public class AnimateWithTrigger : ArtPlayerBase<Animator> {
    [Header("Animator Settings")]
    [Tooltip("Name of the trigger controlling the animation")]
    public string TriggerName;

    [Tooltip("Controller driving the animation")]
    public RuntimeAnimatorController Controller;

    protected override void Awake() {
      base.Awake();
      _triggerHash = Animator.StringToHash(TriggerName);
    }

    protected override void OnAfterStart() {
      _triggerHash = Animator.StringToHash(TriggerName);
      if (Controller != null)
        Player.runtimeAnimatorController = Controller;
      else
        Debug.LogWarningFormat("No Animator Controller Specified {0}", name);
    }

    protected override void FireAction() {
      if (Player != null) {
        Player.SetTrigger(_triggerHash);
      }
    }

    public override void Reset() {
      if (Player != null) {
        Player.ResetTrigger(_triggerHash);
      }
    }

    private int _triggerHash;
  }
}

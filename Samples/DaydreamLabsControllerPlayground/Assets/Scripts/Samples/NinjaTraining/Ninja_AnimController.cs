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

namespace GVR.Samples.NinjaTraining {
  /// <summary>
  /// Simple Animator controller for the Ninja character in the GVR Ninja Training prototype.
  /// </summary>
  public class Ninja_AnimController : MonoBehaviour {
    public Animator CharacterAnimator;

    public void SetHorizontal(float horiz) {
      CharacterAnimator.SetFloat("LeftRight", horiz);
    }

    public void SetVertical(float vert) {
      CharacterAnimator.SetFloat("UpDown", vert);
    }

    public void TriggerKnockdown() {
      CharacterAnimator.SetTrigger("Knockdown");
    }

    public void TriggerSlashFromLeft() {
      CharacterAnimator.SetTrigger("Slash_LeftRight");
    }

    public void TriggerSlashFromRight() {
      CharacterAnimator.SetTrigger("Slash_RightLeft");
    }

    public void TriggerSlashFromHigh() {
      CharacterAnimator.SetTrigger("Slash_HighLow");
    }

    public void TriggerSlashFromLow() {
      CharacterAnimator.SetTrigger("Slash_LowHigh");
    }
  }
}

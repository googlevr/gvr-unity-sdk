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
  /// Attach to play a sound when a trigger event occurs.
  /// </summary>
  public class TriggerSoundPlayer : SoundPlayerBase {
    [Tooltip("Play sound on trigger enter")]
    public bool TriggerEnter = true;

    [Tooltip("Play sound on trigger stay")]
    public bool TriggerStay = false;

    [Tooltip("Play sound on trigger exit")]
    public bool TriggerExit = false;

    void OnTriggerEnter(Collider coll) {
      if (TriggerEnter) {
        PlaySound();
      }
    }

    void OnTriggerExit(Collider coll) {
      if (TriggerExit) {
        PlaySound();
      }
    }

    void OnTriggerStay(Collider coll) {
      if (TriggerStay) {
        PlaySound();
      }
    }
  }
}

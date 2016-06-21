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
  /// Plays a group of audio clips in order. After an audio
  /// clip is played, the next one in the list is cycled in.
  /// </summary>
  public class SoundCycle : SoundPlayerBase {
    [Tooltip("All clips available to play, clips will play in order top to bottom.")]
    public AudioClip[] Clips;

    protected override void Start() {
      base.Start();
      if (!HasClips) {
        Debug.LogWarning("No clips attached.");
      }
    }

    /// <summary>
    /// Plays the next sound in the cycle.
    /// </summary>
    public override void PlaySound() {
      if (HasClips) {
        Clip = Clips[_index++ % Clips.Length];
        if (_index >= 10000) {
          // Keep our index tracker from getting unreasonably high
          _index = 0;
        }
      }

      base.PlaySound();
    }

    private bool HasClips {
      get { return Clips != null && Clips.Length > 0; }
    }

    private int _index;
  }
}

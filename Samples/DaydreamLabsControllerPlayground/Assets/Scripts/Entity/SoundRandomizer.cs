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
  /// Takes a SoundEventListener and performs the PlaySound method, randomly using any one of the
  /// sounds in the "Clips" array.
  /// </summary>
  public class SoundRandomizer : MonoBehaviour {
    [Tooltip("The SoundEventListener to play the sound through. The EventListener is used to " +
             "help unify the use of the SoundEventListener as a pass-through for volume settings.")]
    public SoundEventListener Listener;

    [Tooltip("Each time the PickAndPlaySound method is called, one of the sounds in this array " +
             "will be randomly selected and played through the Listener.")]
    public AudioClip[] Clips;

    public bool PreventRepeats = true;

    private int prevID = -1;

    void Awake() {
      Listener = GetComponent<SoundEventListener>();
    }

    public void PickAndPlaySound() {
      if (Clips.Length <= 0) {
        return;
      }
      int id = Random.Range(0, Clips.Length - 1);
      if (Clips.Length > 1 && PreventRepeats) {
        // If we're preventing repeats, Shift the resulting ID up or down one space to prevent the
        // same sound from being selected twice in a row.
        if (id == prevID) {
          if (id + 1 < Clips.Length) {
            id = id++;
          } else if (id - 1 >= 0) {
            id = id--;
          }
        }
        prevID = id;
      }
      Listener.PlaySound(Clips[id]);
    }
  }
}

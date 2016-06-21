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
  /// Provides a generic interface for playing sounds from
  /// fired Unity events. Method listeners are available for
  /// one-shots and audio loops.
  /// </summary>
  public class SoundEventListener : MonoBehaviour {
    public GvrAudioSource Source;

    [Tooltip("Volume of the audio source")]
    [Range(0f, 1f)]
    public float Volume = 1f;

    void Start() {
      if (Source == null) {
        Source = GetComponent<GvrAudioSource>();
      }
    }

    /// <summary>
    /// Plays the specified audio clip as a one shot.
    /// </summary>
    /// <param name="clip">Clip to play</param>
    public void PlaySound(AudioClip clip) {
      if (Source) {
        Source.PlayOneShot(clip, Volume);
      } else {
        Debug.LogWarningFormat("Attempting to play sound clip {0} on {1}, but the Sound Event " +
                               "Listener has no audio source.", clip.name, gameObject);
      }
    }

    /// <summary>
    /// Plays the an audio loop
    /// </summary>
    /// <param name="clip">Clip to play</param>
    public void PlaySoundLoop(AudioClip clip) {
      if (Source) {
        Source.clip = clip;
        Source.loop = true;
        Source.Play();
      } else {
        Debug.LogWarningFormat("Attempting to play sound clip {0} on {1}, but the Sound Event " +
                               "Listener has no audio source.", clip.name, gameObject);
      }
    }

    /// <summary>
    /// Stops a looping audio clip.
    /// </summary>
    public void StopLoop() {
      if (Source) {
        Source.Stop();
      } else {
        Debug.LogWarningFormat("Attempting to stop loop on {0}, but the Sound Event Listener " +
                               "has no audio source.", gameObject);
      }
    }

    /// <summary>
    /// Sets the volume of the audio source.
    /// </summary>
    /// <param name="newVolume">The new volume.</param>
    public void SetVolume(float newVolume) {
      Volume = newVolume;
    }
  }
}

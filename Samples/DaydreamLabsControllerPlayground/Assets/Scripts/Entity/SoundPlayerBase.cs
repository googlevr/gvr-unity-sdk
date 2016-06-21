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

namespace GVR.Entity {
  /// <summary>
  /// Basic class for playing a single sound from an audio source.
  /// An event is provided to chain events from sound playing.
  /// </summary>
  public class SoundPlayerBase : MonoBehaviour {
    [Tooltip("Fired when the sound is played")]
    public UnityEvent OnPlayed;

    [Tooltip("Source playing the clip. If none specified, will attempt to attach from this gameobject")]
    public GvrAudioSource Source;

    [Tooltip("Clip to play")]
    public AudioClip Clip;

    [Tooltip("Play audio clip as an uninterrutpable one-shot")]
    public bool PlayAsOneShot;

    [Range(0f, 1f)]
    public float VolumeScale = 1f;

    /// <summary>
    /// Unity Start message
    /// </summary>
    protected virtual void Start() {
      if (Source == null) {
        Source = GetComponent<GvrAudioSource>();
      }
    }

    /// <summary>
    /// Plays the assigned clip on the specified audio source.
    /// </summary>
    public virtual void PlaySound() {
      if (Source != null && Clip != null) {
        if (PlayAsOneShot) {
          Source.PlayOneShot(Clip, VolumeScale);
        } else {
          Source.Stop();
          Source.clip = Clip;
          Source.Play();
        }
      }
      OnPlayed.Invoke();
    }
  }
}

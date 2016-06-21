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

namespace GVR.Samples.Pottery {
  public class SculptTool : MonoBehaviour {
    [Tooltip("Animator to change tool state.")]
    public Animator Animator;

    [Tooltip("Audio Source to blend to when the tool position is closer to the origin.")]
    public AudioSource LowSource;

    [Tooltip("Audio Source to blend to when the tool position is further from the origin.")]
    public AudioSource HighSource;

    [Tooltip("The minimum x value for this transform position, where the LowSource audio will be used.")]
    public float XLow = 0.2f;

    [Tooltip("The maximum x value for this transform position, where the HighSource audio will be used.")]
    public float XHigh = 1.0f;

    [Tooltip("Amount of time for audio to fade in or out when the tool switches states.")]
    public float VolumeTransitionTime = 0.2f;

    private float distanceRatio;
    private float totalVolume = 0.0f;
    private int animHash;
    private bool inUse;

    void Start() {
      animHash = Animator.StringToHash("Extended");
    }

    public void StartUsing() {
      inUse = true;
      Animator.SetBool(animHash, true);
    }

    public void StopUsing() {
      inUse = false;
      Animator.SetBool(animHash, false);
    }

    public void Update() {
      if (!inUse) {
        distanceRatio = Mathf.Min(distanceRatio + Time.deltaTime / VolumeTransitionTime, 1.0f);
        totalVolume = Mathf.Max(totalVolume - Time.deltaTime / VolumeTransitionTime, 0.0f);
      } else {
        distanceRatio = Mathf.InverseLerp(XLow, XHigh, transform.position.x);
        totalVolume = Mathf.Min(totalVolume + Time.deltaTime / VolumeTransitionTime, 1.0f);
      }
      if (!(distanceRatio > 1.0f || distanceRatio < 0.0f)) {
        HighSource.volume = (1.0f - distanceRatio) * totalVolume;
        LowSource.volume = distanceRatio * totalVolume;
      } else {
        HighSource.volume = 0.0f;
        LowSource.volume = 0.0f;
      }
    }
  }
}

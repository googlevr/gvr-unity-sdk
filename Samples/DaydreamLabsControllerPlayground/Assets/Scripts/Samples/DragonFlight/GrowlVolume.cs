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

namespace GVR.Samples.DragonFlight {
  /// <summary>
  /// This script modifies the volume of an audio source according to an Animation Curve.
  /// The limits of the animation curve define how the curve is evaluated after the initial timeframe.
  /// </summary>
  public class GrowlVolume : MonoBehaviour {
    [Tooltip("The Audio Source whose volume will be adjusted.")]
    public GvrAudioSource Source;

    [Tooltip("Animation Curve used to define the volume over time.")]
    public AnimationCurve VolumeCurve;

    [Tooltip("Multiplier to the X axis of the Animation curve. Leaving this at 1 will cause " +
             "the curve to evaluate over the course of 1 second.")]
    public float TimeFrame = 1f;

    void Update() {
      Source.volume = VolumeCurve.Evaluate(Time.time * TimeFrame);
    }
  }
}

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
  /// Causes the applied Transform to bob up and down in local space along the Y Axis.
  /// The bobbing motion is defined by an Animation curve.
  /// </summary>
  public class FloatMotion : MonoBehaviour {
    [Tooltip("The curve of motion that the object will follow. This is represented as an " +
             "ABSOLUTE offset in the local Y axis from the Transform's initial position.")]
    public AnimationCurve BobbingCurve;

    [Tooltip("The amount of time, in seconds, it takes to evaluate one full sequence of the " +
             "Animation Curve.")]
    public float TimeFrame = 1.0f;

    [Tooltip("Modifier to the amplitude of the curve. This allows us to keep the motion of the " +
             "curve in the 1 to -1 space.")]
    public float Amplitude = 1.0f;

    [Tooltip("If true, the Object will evaluate its position starting at a random point along " +
             "the Bobbing Curve.")]
    public bool RandomlyOffset = true;

    private float currentNormalizedTime = 0f;
    private Vector3 prevOffset = Vector3.zero;
    private Vector3 newOffset = Vector3.zero;

    void Start() {
      if (RandomlyOffset) {
        currentNormalizedTime = Random.Range(0f, 1f);
      }
    }

    void Update() {
      if (currentNormalizedTime > 1f) {
        currentNormalizedTime = 0f;
      } else {
        currentNormalizedTime += Time.deltaTime / TimeFrame;
      }
      newOffset.y = BobbingCurve.Evaluate(currentNormalizedTime) * Amplitude;
      transform.localPosition = transform.localPosition - prevOffset + newOffset;
      prevOffset = newOffset;
    }
  }
}

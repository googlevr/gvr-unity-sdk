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

namespace GVR.Samples.DragonFlight {
  /// <summary>
  /// Triggers a basic event every X seconds. Useful for periodic checks that are too expensive
  /// to evaluate every frame.
  /// </summary>
  public class IntervalTrigger : MonoBehaviour {
    [Tooltip("The amount of time, in seconds, between Event calls.")]
    public float Interval = 2.0f;

    [Tooltip("The UnityEvent triggered every X seconds.")]
    public UnityEvent OnTriggered;

    private float currentTime = 0f;

    void Start() {
      OnTriggered.Invoke();
    }

    void Update() {
      if (currentTime > Interval) {
        currentTime = 0f;
        OnTriggered.Invoke();
      } else {
        currentTime += Time.deltaTime;
      }
    }
  }
}

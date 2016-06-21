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
  /// Simple class to Deactivate an Object automatically after it has been active for a period of time. Useful for briefly flashing objects onscreen.
  /// </summary>
  public class ObjectFlash : MonoBehaviour {
    [Tooltip("The amount of time, in seconds, before the object will automatically deactivate.")]
    public float DeactivateAfterSeconds = 1f;

    private float secondsRemaining = 0f;

    void OnEnable() {
      secondsRemaining = DeactivateAfterSeconds;
    }

    void Update() {
      if (secondsRemaining > 0) {
        secondsRemaining -= Time.deltaTime;
      } else {
        gameObject.SetActive(false);
      }
    }
  }
}

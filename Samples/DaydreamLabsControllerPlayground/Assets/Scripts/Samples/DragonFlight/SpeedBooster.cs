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
  /// For use with the Forward Motor component. Used to cause the Dragon in the Dragon Flight
  /// demo to gain a decaying speed boost for each floating ruin the Dragon consumes.
  /// </summary>
  public class SpeedBooster : MonoBehaviour {
    [Tooltip("The motor to have the speed buff applied to it.")]
    public ForwardMotor motor;

    [Tooltip("The amount of speed lost per second due to natural decay. Only bonus speed can decay this way.")]
    public float RateLossPerSecond = 1.0f;

    private float originalRate;
    private float currentRate;

    void Start() {
      originalRate = motor.Rate;
      currentRate = originalRate;
    }

    public void BoostRate(float boost) {
      currentRate += boost;
    }

    void Update() {
      if (currentRate > originalRate) {
        currentRate -= Time.deltaTime * (RateLossPerSecond + (currentRate - originalRate) / originalRate);
        motor.Rate = currentRate;
      } else if (motor.Rate != originalRate) {
        motor.Rate = originalRate;
      }
    }


  }
}

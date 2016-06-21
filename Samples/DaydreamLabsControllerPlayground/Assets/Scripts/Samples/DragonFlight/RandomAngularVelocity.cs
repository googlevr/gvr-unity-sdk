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
  /// Built around the "Apply Random Torque" method. Used to make the debris in the dragon flight
  /// demo spin chaotically when hit.
  /// </summary>
  public class RandomAngularVelocity : MonoBehaviour {
    [Tooltip("Rigidbody to apply the torque to.")]
    public Rigidbody body;

    [Tooltip("Minimum random angular force to be applied.")]
    public float MinForce = 0.1f;

    [Tooltip("Maximum random angular force to be applied.")]
    public float MaxForce = 10f;

    [Tooltip("How many times should this script apply a random rotation? Doing so twice " +
             "results in more complex movement along several axis at once.")]
    public int NumberOfForces = 2;

    void Start() {
      for (int i = 0; i < NumberOfForces; i++) {
        ApplyRandomTorque();
      }
    }

    public void ApplyRandomTorque() {
      Vector3 torqueVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
      float torqueMag = Random.Range(MinForce, MaxForce);
      body.AddTorque(torqueVector * torqueMag);
    }
  }
}

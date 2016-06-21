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
  /// Use in conjunction with an Explodable to apply
  /// a random torque impulse to a rigidbody.
  /// </summary>
  public class RandomTorqueExplosion : ExplosionForce {
    /// <summary>
    /// Applies the force.
    /// </summary>
    /// <param name="body">The body.</param>
    public override void ApplyForce(Rigidbody body) {
      var torqueVector = Random.insideUnitSphere * Amount;
      body.AddRelativeTorque(torqueVector, ForceMode.Impulse);
    }
  }
}

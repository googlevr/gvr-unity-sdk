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
  /// Applies a random explosion force using the rigidbody ExplosionForce API.
  /// </summary>
  public class RandomExplosionForce : ExplosionForce {
    public Transform ExplosionOrigin;
    public float ExplosionRadius = 2f;
    public float UpwardsModifier = 1f;

    /// <summary>
    /// Applies the force to the specified rigidbody
    /// </summary>
    /// <param name="body">Body receiving the force</param>
    public override void ApplyForce(Rigidbody body) {
      body.AddExplosionForce(Amount, ExplosionOrigin.position, ExplosionRadius, UpwardsModifier,
                             ForceMode.Impulse);
    }
  }
}

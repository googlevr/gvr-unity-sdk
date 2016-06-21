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
  /// Applies a force to a rigidbody when an Explodable instance
  /// signals an explosion has occurred.
  /// </summary>
  /// <seealso cref="UnityEngine.MonoBehaviour" />
  [RequireComponent(typeof(Explodable))]
  public abstract class ExplosionForce : MonoBehaviour {
    [Tooltip("Generic scaling value to apply to an explosive force/")]
    public float Amount = 20f;

    /// <summary>
    /// Applies the force to the specified rigidbody
    /// </summary>
    /// <param name="body">Body receiving the force</param>
    public abstract void ApplyForce(Rigidbody body);
  }
}

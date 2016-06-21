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

using System.Collections.Generic;
using UnityEngine;

namespace GVR.Entity {
  /// <summary>
  /// Used in conjunction with Explodable to create composed explodable
  /// objects. Objects using ExplodablePart need a parent gameobject
  /// with Explodable attached.
  /// </summary>
  [RequireComponent(typeof(Rigidbody))]
  public class ExplodablePart : ResetPosition {
    [Tooltip("If true, unparent from the hierarchy when exploding.")]
    public bool UnParentOnExplode = false;

    void Start() {
      _body = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Attaches this part to a parent object, disabling gravity.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public void Attach(Transform parent) {
      _body.isKinematic = true;
      ResetObject(parent);
    }

    /// <summary>
    /// Releases this instance, apply no force and letting gravity take over.
    /// </summary>
    public void Release() {
      _body.isKinematic = false;
      _body.transform.parent = null;
    }

    /// <summary>
    /// Releases the instance by applying an explosive force.
    /// </summary>
    /// <param name="force">Force to apply</param>
    public void Release(ExplosionForce force) {
      Release();
      if (force != null) {
        force.ApplyForce(_body);
      }
    }

    /// <summary>
    /// Releases the instance by applying explosive forces.
    /// </summary>
    /// <param name="forces">Forces to apply</param>
    public void Release(List<ExplosionForce> forces) {
      Release();
      if (forces != null && forces.Count > 0) {
        forces.ForEach(f => f.ApplyForce(_body));
      }
    }

    private Rigidbody _body;
  }
}

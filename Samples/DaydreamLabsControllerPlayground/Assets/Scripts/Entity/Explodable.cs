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
using UnityEngine.Events;

namespace GVR.Entity {
  /// <summary>
  /// Central class of the Explodable object system. An Explodable object
  /// consists of a simple hierarchy components.
  ///
  /// 1) A parent game object with an Explodable component
  ///    and 1 or more Explosion force components attached
  ///
  /// 2) 1 or more child game objects with ExploadablePart attached
  ///
  /// On start, all forces and and exploadable parts are collected. Calling
  /// the explode method will apply any ExplosionForces to all Explodable parts.
  /// If desired, an Explodable can be set to respawn after a given amount of time.
  ///
  /// Example:
  /// Balloon prefabs in the Boomerang sample use Explodables with
  /// RandomTorqueExplosion
  /// </summary>
  [RequireComponent(typeof(Rigidbody))]
  public class Explodable : ResetPosition {
    public UnityEvent OnExplode;
    public UnityEvent OnRespawn;

    [Tooltip("Can this object respawn after a certain period of time?")]
    public bool CanRespawn;

    [Tooltip("In seconds")]
    public float RespawnTime = 1f;

    [Tooltip("If true, the OnTriggerEnter method in this class will be used to detonate the Explosive.")]
    public bool UseInternalTrigger = true;

    void Start() {
      _forces = new List<ExplosionForce>(GetComponents<ExplosionForce>());
      _pieces = new List<ExplodablePart>(GetComponentsInChildren<ExplodablePart>());
      _parent = transform.parent;
    }

    void OnTriggerEnter(Collider coll) {
      if (!_exploded && UseInternalTrigger) {
        Explode();
      }
    }

    void Update() {
      if (_exploded && CanRespawn) {
        float elapsed = Time.realtimeSinceStartup - _respawnTimer;
        if (elapsed > RespawnTime) {
          ResetObject(_parent);
        }
      }
    }

    /// <summary>
    /// Reset back to the start position and original parent.
    /// </summary>
    public void ResetDefault() {
      ResetObject(_parent);
    }

    /// <summary>
    /// Resets the object to the start position with a new parent.
    /// </summary>
    /// <param name="parent">Parent</param>
    public override void ResetObject(Transform parent) {
      base.ResetObject(parent);
      _exploded = false;
      ReAttachPieces();
      OnRespawn.Invoke();
    }

    /// <summary>
    /// Explodes this instance, applying forces to any known ExplodableParts.
    /// </summary>
    public void Explode() {
      OnExplode.Invoke();
      _exploded = true;
      _respawnTimer = Time.realtimeSinceStartup;
      ExplodePieces();
    }

    private void ExplodePieces() {
      if (HasPieces) {
        _pieces.ForEach(p => p.Release(_forces));
      }
    }

    private void ReAttachPieces() {
      if (HasPieces) {
        _pieces.ForEach(p => p.Attach(transform));
      }
    }

    private bool HasPieces {
      get { return (_pieces != null && _pieces.Count > 0); }
    }

    private bool _exploded;
    private Transform _parent;
    private float _respawnTimer;
    private List<ExplosionForce> _forces;
    private List<ExplodablePart> _pieces;
  }
}

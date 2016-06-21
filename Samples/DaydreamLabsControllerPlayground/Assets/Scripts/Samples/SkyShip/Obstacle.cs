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

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  This component defines the spawn and despawn conditions for obstacles.
  ///  When an obstacle is spawned, it will detect to see if spawned inside
  ///  another obstacle. If that is the case, it will wait until it is clear,
  ///  or the force spawn time has passed. The obstacle will then be marked
  ///  as unblocked, and the obstacle manager can launch it.
  /// </summary>
  public class Obstacle : MonoBehaviour {
    [Tooltip("Reference to star coin if this obstacle has one")]
    public StarCoin StarCoin;

    [Tooltip("Reference to rigidbody")]
    public Rigidbody ActiveRigidbody;

    [Tooltip("When the obstacle reaches this global Z value, it despawns")]
    public float DespawnZValue;

    [Tooltip("Angular velocity is randomly assigned to obstacle where this is the max speed. " +
             "Currently, obstacles with a star coin will not spin.")]
    public float AngularVelocityVariance;

    [Tooltip("This is a hack to fix a bug. Basically, manually poll for obstacle blocked messages. " +
             "If no such message was recieved after this amount of time, activate obstacle.")]
    public float ForceSpawnTimer;

    public bool IsObstacleBlocked { get; private set; }

    public ObstacleCycler ObjectPool { get; set; }

    private List<Collider> colliders = new List<Collider>();
    private List<Renderer> renderers = new List<Renderer>();
    private float timer = 0;
    private bool isActive = false;

    void Start() {
      colliders.AddRange(GetComponents<Collider>());
      colliders.AddRange(GetComponentsInChildren<Collider>());
      renderers.AddRange(GetComponents<Renderer>());
      renderers.AddRange(GetComponentsInChildren<Renderer>());
    }

    void OnTriggerStay(Collider collider) {
      if (timer < 0) {
        IsObstacleBlocked = false;
      } else {
        IsObstacleBlocked = true;
        timer = ForceSpawnTimer;
      }
    }

    void OnTriggerExit(Collider collider) {
      IsObstacleBlocked = false;
    }

    void Update() {
      if (transform.position.z < DespawnZValue) {
        Despawn();
      }

      if (ForceSpawnTimer != 0 && isActive) {
        timer -= Time.deltaTime;
      }
    }

    public void Activate() {
      for (int i = 0; i < colliders.Count; i++) {
        colliders[i].isTrigger = false;
      }
      for (int i = 0; i < renderers.Count; i++) {
        renderers[i].enabled = true;
      }
      if (StarCoin != null) {
        StarCoin.Activate();
      }
      isActive = true;
    }

    public void DeActivate() {
      for (int i = 0; i < renderers.Count; i++) {
        renderers[i].enabled = false;
      }
      if (StarCoin == null) {
        ActiveRigidbody.angularVelocity = AngularVelocityVariance *
            new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
      } else {
        StarCoin.DeActivate();
      }
      isActive = false;
    }

    private void Despawn() {
      for (int i = 0; i < colliders.Count; i++) {
        colliders[i].isTrigger = true;
      }
      ObjectPool.ReturnObject(gameObject);
    }
  }
}

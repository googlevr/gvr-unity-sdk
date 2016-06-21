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

namespace GVR.Samples.Magic {
  /// <summary>
  /// Used to broadcast when this object is collided with. Layers
  /// should be used to filter collisions.
  /// </summary>
  [RequireComponent(typeof(Collider))]
  public class MagicHitBox : MonoBehaviour {
    public MagicHitEvent OnHit = new MagicHitEvent();

    [Tooltip("If true, uses trigger events, otherwise uses collision events.")]
    public bool UsesTrigger = true;

    void OnTriggerEnter(Collider coll) {
      if (UsesTrigger)
        FireHitEvent(coll.gameObject);
    }

    void OnCollisionEnter(Collision coll) {
      if (!UsesTrigger)
        FireHitEvent(coll.gameObject);
    }

    private void FireHitEvent(GameObject obj) {
      var projectile = obj.GetComponent<MagicProjectile>();
      if (projectile != null) {
        OnHit.Invoke(projectile);
      }
    }

    public void Subscribe(UnityAction<MagicProjectile> listener) {
      OnHit.AddListener(listener);
    }
  }
}

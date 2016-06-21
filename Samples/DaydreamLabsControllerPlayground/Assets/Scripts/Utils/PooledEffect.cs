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

namespace GVR.Utils {
  /// <summary>
  ///  This component assumes an EffectPlayer is on a pooled object.
  ///  When the effect is finished, this object will return itself
  ///  to the pool.
  /// </summary>
  [RequireComponent(typeof(EffectPlayer))]
  public class PooledEffect : MonoBehaviour {
    private EffectPlayer effect = null;
    private ObjectPool pool = null;
    private bool isActive = false;

    void Start() {
      effect = GetComponent<EffectPlayer>();
      pool = GetComponentInParent<ObjectPool>();
    }

    void Update() {
      if (isActive) {
        if ((effect.Audio == null || !effect.Audio.isPlaying) &&
            (effect.Particle == null || !effect.Particle.isPlaying)) {
          DeActivate();
        }
      }
    }

    public void Activate(Transform effectTarget) {
      if (effect == null) {
        effect = GetComponent<EffectPlayer>();
      }
      gameObject.transform.position = effectTarget.position;
      gameObject.transform.rotation = effectTarget.rotation;
      effect.Play();
      isActive = true;
    }

    public void DeActivate() {
      if (effect == null) {
        effect = GetComponent<EffectPlayer>();
      }
      if (pool == null) {
        pool = GetComponentInParent<ObjectPool>();
      }
      isActive = false;
      effect.Stop();
      pool.ReturnObject(gameObject);
    }
  }
}

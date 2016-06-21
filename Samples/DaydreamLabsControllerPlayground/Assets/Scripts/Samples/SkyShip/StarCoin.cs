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

using GVR.Utils;
using UnityEngine;

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  This component allows the player to 'collect' this object when
  ///  running into it with the plane.
  /// </summary>
  public class StarCoin : MonoBehaviour {
    public ObjectPool EffectPool;

    void OnTriggerEnter(Collider collider) {
      if (!collider.CompareTag("Player")) {
        return;
      }
      EffectPlayer effect = collider.GetComponent<EffectPlayer>();
      if (effect) {
        effect.Play();
      }
      EffectPool.GetFreeObject().GetComponent<PooledEffect>().Activate(transform);
      DeActivate();
    }

    public void Activate() {
      gameObject.SetActive(true);
      GetComponent<Collider>().isTrigger = true;
    }

    public void DeActivate() {
      gameObject.SetActive(false);
    }
  }
}

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
using GVR.Utils;

namespace GVR.Samples.Archae {
  /// <summary>
  ///  This component plays a dust particle system and then returns
  ///  the object to the pool once the effect is finished.
  /// </summary>
  public class DustSpawn : MonoBehaviour {
    [Tooltip("Reference this object's particle system.")]
    public ParticleSystem ParticleSys;

    [Tooltip("Reference to Object Pool that owns this object.")]
    public ObjectPool Pool;

    private bool activate = false;

    void Update() {
      if (activate && !ParticleSys.isPlaying) {
        activate = false;
        Pool.ReturnObject(gameObject);
      }
    }

    public void Play() {
      ParticleSys.Play();
      activate = true;
    }
  }
}

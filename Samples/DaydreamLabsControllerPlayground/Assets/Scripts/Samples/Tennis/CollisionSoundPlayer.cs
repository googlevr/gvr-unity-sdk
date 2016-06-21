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

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component will have the gameobject play a sound when it collides with something.
  /// </summary>
  [RequireComponent(typeof(Rigidbody))]
  public class CollisionSoundPlayer : MonoBehaviour {
    [Tooltip("The audio source that should play when this object collides.")]
    public GvrAudioSource CollisionAudioSource;

    private Rigidbody cachedRigidBody;

    void Start() {
      cachedRigidBody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter() {
      CollisionAudioSource.volume = Mathf.Min(1f, Mathf.Pow(cachedRigidBody.velocity.magnitude * .225f, 5f));
      CollisionAudioSource.Play();
    }
  }
}

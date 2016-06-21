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

namespace GVR.Samples.Fishing {
  /// <summary>
  /// Handles attaching and dropping a fish. Fish will drop at
  /// a random rotation because fish are floppy creatures.
  /// </summary>
  public class Fish : MonoBehaviour {
    void Awake() {
      _rigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Catches the fish and attaches it to the hook.
    /// Kinematic physics are enabled.
    /// </summary>
    /// <param name="hook">Transform to attach this fish.</param>
    public void Catch(Transform hook) {
      _rigidBody.isKinematic = true;
      transform.SetParent(hook);
      transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Releases the fish by placing it at the specified drop
    /// point and enabling gravity/physics.
    /// </summary>
    /// <param name="dropPoint">The drop point</param>
    public void Release(Transform dropPoint) {
      _rigidBody.isKinematic = false;
      _rigidBody.velocity = Vector3.zero;
      transform.SetParent(dropPoint);
      transform.localPosition = Vector3.zero;
      transform.localRotation = Random.rotation;
    }

    private Rigidbody _rigidBody;
  }
}

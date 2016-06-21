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
  ///  This component artificially applies gravity to each gameobject in an Object Recycler.
  /// </summary>
  [RequireComponent(typeof(ObjectRecycler))]
  public class ObjectRecyclerGravitySim : MonoBehaviour {
    [Tooltip("Reference to the object recycler on this gameobject.")]
    public ObjectRecycler Recycler;

    [Tooltip("The gravity vector to be applied to each rigidbody amongst gameobjects in the recycler.")]
    public Vector3 gravity;

    private Rigidbody[] recyclerRigidbodies;

    void Start() {
      recyclerRigidbodies = new Rigidbody[Recycler.ObjectArray.Length];
      for (int i = 0; i < recyclerRigidbodies.Length; i++) {
        recyclerRigidbodies[i] = ((GameObject)Recycler.ObjectArray[i]).GetComponent<Rigidbody>();
      }
    }

    void FixedUpdate() {
      for (int i = 0; i < recyclerRigidbodies.Length; i++) {
        recyclerRigidbodies[i].AddForce(gravity, ForceMode.Acceleration);
      }
    }
  }
}

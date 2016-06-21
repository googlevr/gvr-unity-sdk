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

namespace GVR.Utils {
  /// <summary>
  ///  This is not a true pool, because it tracks a fixed number of pre-existing objects.
  ///  It does, however, allow for tracked objects to be loaned out and returned.
  /// </summary>
  public class ObjectPool : MonoBehaviour {
    [Tooltip("Array of objects to pool. Likely, these gameobjects should be clones of each other.")]
    public GameObject[] pooledObjects;

    protected KeyValuePair<bool, GameObject>[] availableObjects = null;

    void Start() {
      availableObjects = new KeyValuePair<bool, GameObject>[pooledObjects.Length];
      Initialize();
    }

    public void Initialize() {
      for (int i = 0; i < availableObjects.Length; i++) {
        availableObjects[i] = new KeyValuePair<bool, GameObject>(true, pooledObjects[i]);
        pooledObjects[i].SetActive(false);
      }
    }

    public GameObject GetFreeObject() {
      for (int i = 0; i < availableObjects.Length; i++) {
        if (availableObjects[i].Key) {
          availableObjects[i] = new KeyValuePair<bool, GameObject>(false, availableObjects[i].Value);
          availableObjects[i].Value.SetActive(true);
          return availableObjects[i].Value;
        }
      }
      return null;
    }

    public bool ReturnObject(GameObject gameObj) {
      for (int i = 0; i < availableObjects.Length; i++) {
        if (availableObjects[i].Value.Equals(gameObj)) {
          availableObjects[i] = new KeyValuePair<bool, GameObject>(true, availableObjects[i].Value);
          availableObjects[i].Value.SetActive(false);
          return true;
        }
      }
      return false;
    }
  }
}

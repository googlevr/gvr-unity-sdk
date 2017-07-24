// Copyright 2017 Google Inc. All rights reserved.
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
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

/// Specialized version of Pool specifically built to work with GameObjects.
///
/// GameObjects in the pool are stored underneath a container object in the scene.
/// This container is scaled to (0,0,0) to hide the pooled objects. This is done instead
/// of disabling the pooled objects to prevent an issue where disabling & enabling objects
/// will generate a large amount of memory allocations.
///
/// Additionally, when an object is returned to the pool it isn't reparented to the container
/// object until the end of the frame. This way, if the same object is borrowed again before
/// the end of the frame (a common occurence), it isn't reparented an extra time.
/// Reparenting an object can cause a significant amount of memory allocations and CPU load.
public class GameObjectPool : ObjectPool<GameObject> {
  private GameObject prefab;
  private GameObjectPoolController poolController;

  public GameObjectPool(GameObject prefab, int capacity)
    : this(prefab, capacity, 0) {
  }

  public GameObjectPool(GameObject prefab, int capacity, int preAllocateAmount) {
    Assert.IsNotNull(prefab);
    this.prefab = prefab;

    GameObject poolContainerObject = new GameObject(prefab.name + " Pool");
    poolController = poolContainerObject.AddComponent<GameObjectPoolController>();
    poolController.Initialize(capacity);

    Initialize(capacity, preAllocateAmount);
  }

  public override void Dispose() {
    if (poolController != null) {
      GameObject.Destroy(poolController.gameObject);
    }
  }

  protected override void OnBorrowed(GameObject borrowedObject) {
    poolController.OnBorrowed(borrowedObject);
  }

  protected override void OnPooled(GameObject pooledObject) {
    poolController.OnPooled(pooledObject);
  }

  protected override void OnUnableToReturn(GameObject returnedObject) {
    GameObject.Destroy(returnedObject);
  }

  protected override GameObject AllocateObject() {
    GameObject obj = GameObject.Instantiate(prefab);
    return obj;
  }
}

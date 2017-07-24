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
using System.Collections;
using System.Collections.Generic;

/// Manages a collection of object pools and provides access to them by name.
public class ObjectPoolManager : MonoBehaviour {
  private static ObjectPoolManager instance;

  public static ObjectPoolManager Instance {
    get {
      return instance;
    }
  }

  private Dictionary<string, IObjectPool> pools = new Dictionary<string, IObjectPool>();

  public bool ContainsPool(string poolName) {
    return pools.ContainsKey(poolName);
  }

  public T GetPool<T>(string poolName)
    where T : class, IObjectPool {
    IObjectPool pool;
    if (pools.TryGetValue(poolName, out pool)) {
      T result = pool as T;
      if (result == null) {
        Debug.LogError("Pool " + poolName + " is not of type " + typeof(T));
      }

      return result;
    }

    return null;
  }

  public void AddPool(string poolName, IObjectPool pool) {
    if (ContainsPool(poolName)) {
      Debug.LogError("Cannot add pool " + poolName + " because it already exists.");
      return;
    }

    pools.Add(poolName, pool);
  }

  public void RemovePool(string poolName) {
    IObjectPool pool;
    if (!pools.TryGetValue(poolName, out pool)) {
      return;
    }

    pool.Dispose();
    pools.Remove(poolName);
  }

  public void RemoveAllPools() {
    var enumerator = pools.GetEnumerator();
    while (enumerator.MoveNext()) {
      IObjectPool pool = enumerator.Current.Value;
      pool.Dispose();
    }
    pools.Clear();
  }

  void Awake() {
    if (instance != null) {
      Debug.LogError("Cannot have multiple instances of ObjectPoolManager.");
      Destroy(this);
      return;
    }
    instance = this;
  }

  void OnDestroy() {
    RemoveAllPools();
  }
}

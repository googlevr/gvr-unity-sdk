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
using UnityEngine.Events;

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Takes a Prefab and instantiates a pool of objects from it. This class also handles activating
  /// these prefabs in sequence. This script is used in the Dragon Team demo to allow multiple Song
  /// FX visuals to play overtop of one another from the same dragonling, while still being pooled.
  /// Primarily used via the "Activate()" method.
  /// </summary>
  public class PooledObjectActivator : MonoBehaviour {
    [Tooltip("Designer facing label for this object intantiator. Helps keep tabs on its use.")]
    public string Label = "New Pooled Object Instantiator.";

    [Tooltip("The object prefab to pool.")]
    public GameObject ObjectPrefab;

    [Tooltip("The number of instances to pool.")]
    public int CachedObjectCount = 3;

    [Tooltip("Event called each time a pooled object is activated.")]
    public UnityEvent OnActivate;

    private List<GameObject> instances = new List<GameObject>();
    private int objectIdToActivate = 0;

    public void Activate() {
      if (objectIdToActivate >= instances.Count) {
        objectIdToActivate = 0;
      }
      instances[objectIdToActivate].SetActive(false);
      instances[objectIdToActivate].transform.position = transform.position;
      instances[objectIdToActivate].SetActive(true);
      OnActivate.Invoke();
      objectIdToActivate++;
    }

    void Start() {
      if (ObjectPrefab) {
        for (int i = 0; i < CachedObjectCount; i++) {
          GameObject go = GameObject.Instantiate(ObjectPrefab);
          go.SetActive(false);
          instances.Add(go);
        }
      }
    }
  }
}

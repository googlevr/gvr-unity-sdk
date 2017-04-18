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

/// Used by GameObjectPool to manage the pooled GameObjects within the scene graph.
public class GameObjectPoolController : MonoBehaviour {
  private Stack<GameObject> toReparentStack;

  public void Initialize(int capacity) {
    transform.localScale = Vector3.zero;
    toReparentStack = new Stack<GameObject>(capacity);
  }

  public void OnBorrowed(GameObject borrowedObject) {
    // The borrowed object will always be the most recently pooled object.
    if (toReparentStack.Count > 0) {
      toReparentStack.Pop();
    }
  }

  public void OnPooled(GameObject pooledObject) {
    toReparentStack.Push(pooledObject);
  }

  void LateUpdate() {
    if (toReparentStack.Count > 0) {
      var enumerator = toReparentStack.GetEnumerator();
      while (enumerator.MoveNext()) {
        GameObject obj = enumerator.Current;
        obj.transform.SetParent(transform, false);
      }
      toReparentStack.Clear();
    }
  }
}

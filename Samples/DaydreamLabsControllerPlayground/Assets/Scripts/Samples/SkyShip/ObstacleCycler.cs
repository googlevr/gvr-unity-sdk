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
using GVR.Utils;
using UnityEngine;

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  This component functions similarly to ObjectPool, but attempts to
  ///  grab a random-ish object instead of just the next free one.
  /// </summary>
  public class ObstacleCycler : ObjectPool {
    // Naive random object selector
    public GameObject GetRandomFreeObject() {
      int randomInt = Random.Range(0, availableObjects.Length);
      if (!availableObjects[randomInt].Key) {
        return GetFreeObject();
      }
      var go = availableObjects[randomInt].Value;
      availableObjects[randomInt] = new KeyValuePair<bool, GameObject>(false, go);
      go.SetActive(true);
      return go;
    }
  }
}

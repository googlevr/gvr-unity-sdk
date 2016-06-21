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

namespace GVR.Samples.Fishing {
  /// <summary>
  /// Limits the number of fish that will pile up on your boat.
  /// </summary>
  public class FishCollector : MonoBehaviour {
    public Transform DropPoint;
    public int MaxFish = 10;

    /// <summary>
    /// Drop a fish into the collector.
    /// </summary>
    /// <param name="f">Fish that was dropped</param>
    public void DropFish(Fish f) {
      f.Release(DropPoint);
      CleanupFish(f);
    }

    private void CleanupFish(Fish f) {
      _collected.Enqueue(f.gameObject);
      if (_collected.Count > MaxFish) {
        // Too many fish, kill the oldest
        Destroy(_collected.Dequeue());
      }
    }

    private Queue<GameObject> _collected = new Queue<GameObject>();
  }
}

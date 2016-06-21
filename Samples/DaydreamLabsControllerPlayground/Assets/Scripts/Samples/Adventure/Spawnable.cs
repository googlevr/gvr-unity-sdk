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
using UnityEngine.Events;

namespace GVR.Samples.Adventure {
  /// <summary>
  /// An object that can record and spawn at specified transforms.
  /// </summary>
  public class Spawnable : MonoBehaviour {
    [Tooltip("Additional function to call just before the spawn moves the object's position.")]
    public UnityEvent OnSpawn;

    private Vector3 startPos;
    private Transform spawnPoint;

    void Start() {
      startPos = transform.position;
    }

    /// <summary>
    /// Set a transform as a spawn point. Any calls to Respawn will move
    /// the object to that transform.
    /// </summary>
    /// <param name="point">Target transform.</param>
    public void SetSpawnPoint(Transform point) {
      spawnPoint = point;
    }

    /// <summary>
    /// Move back to the last specifid spawn point (or start point).
    /// </summary>
    public void Respawn() {
      OnSpawn.Invoke();
      if (spawnPoint == null) {
        transform.position = startPos;
      } else {
        transform.position = spawnPoint.position;
      }
    }

  }
}

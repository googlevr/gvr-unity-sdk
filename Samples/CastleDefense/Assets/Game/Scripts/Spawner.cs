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
using System.Collections.Generic;

public class Spawner : MonoBehaviour {
  public List<Transform> spawn_locations_ = new List<Transform>();

  float spawn_timer_ = 5.0f;

  void Start() {
    spawn_timer_ = Random.Range(0.2f, 5.0f);
  }

  void Update() {
    if (spawn_timer_ > 0.0f) {
      spawn_timer_ -= Time.deltaTime;

      if (spawn_timer_ <= 0.0f) {
        spawn_timer_ = Random.Range(2.0f, 5.0f);

        int spawn_pos_index = Random.Range(0, spawn_locations_.Count);
        Transform at = spawn_locations_[spawn_pos_index];

        AndroidPool.Create(at);
      }
    }

  }
}

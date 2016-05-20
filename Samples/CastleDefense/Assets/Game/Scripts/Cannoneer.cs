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

public class Cannoneer : MonoBehaviour {
  public Transform target_;
  public GameObject cannon_;
  public float cooldown_ = 5.0f;
  public float timer_;
  private CannonBehaviour cannon_behaviour_;


  void Start() {
    cannon_behaviour_ = cannon_.GetComponentInChildren<CannonBehaviour>();
    timer_ = cooldown_ + Random.Range(-1.0f, 1.0f);
  }

  void Update() {
    timer_ -= Time.deltaTime;
    if (timer_ <= 0.0f) {
      timer_ = cooldown_ + Random.Range(-1.0f, 1.0f);
      cannon_behaviour_.FireAtTarget(target_.position);
    }
  }

  public void OnExplosion(Vector3 at) {
    enabled = false;
  }
}

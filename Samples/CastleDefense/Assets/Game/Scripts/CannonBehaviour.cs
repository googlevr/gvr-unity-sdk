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

public class CannonBehaviour : MonoBehaviour {
  public Transform fire_position_;
  public GameObject cannon_ball_;
  public bool player_cannon_ = false;
  public static CannonBehaviour player_ = null;

  public void Awake() {
    if (player_cannon_ == true) {
      player_ = this;
    }
  }

  public void FireAtTarget(Vector3 at) {
    GameObject cannon_ball_obj =
      CannonballPool.Create(transform) as GameObject;
    CannonballBehaviour cannon_ball_cmp = cannon_ball_obj.GetComponent<CannonballBehaviour>();

    float angle_rad = cannon_ball_cmp.Fire(at);
    float angle_deg = angle_rad * Mathf.Rad2Deg;

    transform.localRotation = Quaternion.AngleAxis(-(angle_deg + 90.0f), Vector3.right);

    Transform parent_transform = transform.parent;
    Vector3 dir = cannon_ball_cmp.GetFlatDirection(parent_transform.position, at);
    parent_transform.rotation = Quaternion.LookRotation(dir);
  }
}

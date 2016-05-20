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

// This is based on this Wiki page:
// https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
public class Trajectory {
  public float initial_speed_;
  public float start_y_;
  public float target_y_;
  public float total_distance_;
  public float gravity_ = 9.81f;
  public float fire_angle_;

  public Trajectory(float start_y, float target_y, float distance, float speed) {
    initial_speed_ = speed;
    start_y_ = start_y;
    target_y_ = target_y;
    total_distance_ = distance;

    fire_angle_ = CalculateAngle();
  }

  public float CalculateAngle() {
    // https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
    // Angle theta required to hit coordinate (x,y).
    float delta_y = target_y_ - start_y_;

    float v2 = Mathf.Pow(initial_speed_, 2.0f);
    float v4 = Mathf.Pow(initial_speed_, 4.0f);

    float g_x2 = gravity_ * Mathf.Pow(total_distance_, 2.0f);
    float two_y_v2 = 2.0f * delta_y * v2;

    float top_function = v2 + Mathf.Sqrt(v4 - gravity_ * (g_x2 + two_y_v2));
    float gx = gravity_ * total_distance_;

    float tangent = top_function / gx;

    float angle = Mathf.Atan(tangent);

    return angle;
  }

  public float GetHeightAtDistance(float distance) {
    // https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
    // Conditions at an arbitrary distance x (Height at X).
    float g_x2 = gravity_ * Mathf.Pow(distance, 2);
    float v_cos_a_2 = Mathf.Pow(initial_speed_ * Mathf.Cos(fire_angle_), 2);
    float y = start_y_ + distance * Mathf.Tan(fire_angle_) - g_x2 / (2 * v_cos_a_2);

    return y;
  }
}

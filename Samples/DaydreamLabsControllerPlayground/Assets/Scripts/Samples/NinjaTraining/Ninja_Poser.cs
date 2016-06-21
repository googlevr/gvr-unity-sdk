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

namespace GVR.Samples.NinjaTraining {
  /// <summary>
  /// The poser accepts as input a Vector2 Axis representing Horizontal and Vertical thresholds
  /// between -1 and 1. This is given through a Vector2 Event, typically by the Ninja Input Processor.
  /// The purpose of this code is to smooth the noise out of that input by applying a smooth lerp
  /// and a follow-speed, before passing the axis information directly to the Animation Controller.
  /// </summary>
  public class Ninja_Poser : MonoBehaviour {
    [Tooltip("The Animator for the Ninja Character that is driven by this code.")]
    public Ninja_AnimController AnimController;

    [Tooltip("The Rate at which the Ninja's Pose follows the Direct input. " +
             "This helps smooth out jitter from the input.")]
    public float FollowSpeed = 9f;

    float prevHoriz = 0f;
    float prevVert = 0f;

    public void SetBlendPose(Vector2 axis) {
      float drivingHoriz = axis.y;
      float drivingVert = axis.x;

      float horiz = Mathf.Lerp(prevHoriz, drivingHoriz, FollowSpeed * Time.deltaTime);
      float vert = Mathf.Lerp(prevVert, drivingVert, FollowSpeed * Time.deltaTime);

      AnimController.SetVertical(vert);
      AnimController.SetHorizontal(horiz);

      prevHoriz = horiz;
      prevVert = vert;
    }
  }
}

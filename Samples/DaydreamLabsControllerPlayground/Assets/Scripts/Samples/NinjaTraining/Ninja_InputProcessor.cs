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

using GVR.Input;
using UnityEngine;

namespace GVR.Samples.NinjaTraining {
  /// <summary>
  /// This script accepts raw Transform Data from an orientation object rotating in the world.
  /// Based off of a world-space forward Z axis, it re-interprets deflection away from Forward
  /// into Horizontal and vertical Axis with thresholds between -1 and 1 (For interpretation in the
  /// Ninja's Animator.) The purpose of this code is to prove a simulated one-to-one relationship
  /// between the orientation of the Controller and a rigged Character Blend Tree.
  /// </summary>
  public class Ninja_InputProcessor : MonoBehaviour {
    [Tooltip("The transform that represents the current orientation of the Controller. The " +
             "rotation on this transform will be used to generate the Horizontal and Vertical Axis data.")]
    public Transform RawOrientationInput;

    [Tooltip("The Vector2 Event that dispatches the Horizontal and Vertical Axis data. " +
             "Both the X and Y will output values between -1 and 1. X = Vertical, Y = Horizontal.")]
    public Vector2Event ProcessedPoseInput;

    [Tooltip("A multiplier applied to the strength of the deflection from forward Z. " +
             "A higher multiplier increases the rate at which the values cap at minimum or " +
             "maximum values depending on deflection.")]
    public float PoseWeight = 3f;

    float rightPoseStr;
    float leftPoseStr;
    float upPoseStr;
    float downPoseStr;
    float poseMag;
    float horiz;
    float vert;

    void Update() {
      //Sample the overall deflection from Forward Z. This will be used for the absolute magnitude
      // of our deflection. We do it this way to avoid gimbal lock issues that come from separating
      // out the axis of rotation.
      poseMag = (Vector3.Angle(RawOrientationInput.forward, Vector3.forward) / 180) * PoseWeight;

      //Now sample deflection from each of the cardinal directions we care about. Again, we sample
      // all four to avoid math that separates the angles. This is necessary to avoid locking our
      // output in strange rotations.
      rightPoseStr = 1 - Vector3.Angle(RawOrientationInput.forward, Vector3.right) / 180;
      leftPoseStr = 1 - Vector3.Angle(RawOrientationInput.forward, Vector3.left) / 180;
      upPoseStr = 1 - Vector3.Angle(RawOrientationInput.forward, Vector3.up) / 180;
      downPoseStr = 1 - Vector3.Angle(RawOrientationInput.forward, Vector3.down) / 180;

      //Create the horizontal and Vertical thresholds, then multiply them by the overall pose magnitude.
      //Doing it this way ensures that the poses can't "pop" when passing over world space Up.
      horiz = Mathf.Clamp((rightPoseStr - leftPoseStr) * poseMag, -1f, 1f);
      vert = Mathf.Clamp((downPoseStr - upPoseStr) * poseMag, -1f, 1f);

      ProcessedPoseInput.Invoke(new Vector2(vert, horiz));
    }
  }
}

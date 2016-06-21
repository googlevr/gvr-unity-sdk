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

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  This component maps the controller's orientation to the plane's orientation.
  ///  Since the plane is not parented to the controller, its orientation runs a
  ///  physics simulation to turn to the desired heading as fast as it can.
  /// </summary>
  public class SkyShipController : MonoBehaviour {
    [Tooltip("Reference to ship propeller (controls ships movement)")]
    public SkyShipPropeller SkyShip;

    [Tooltip("Max turning acceleration in X or Y direction (steering)")]
    public float AccelerationXY;

    [Tooltip("Max turning acceleration in Z direction (barrel rolls)")]
    public float AccelerationZ;

    [Tooltip("Max turning velocity in X or Y direction (steering)")]
    public float MaxVelocityXY;

    [Tooltip("Max turning velocity in z direction (barrel rolls)")]
    public float MaxVelocityZ;

    [Tooltip("Max facing angle in X direction (value is clamped in pos and neg angle)")]
    public float MaxXDegrees;

    [Tooltip("Max facing angle in Y direction (value is clamped in pos and neg angle)")]
    public float MaxYDegrees;

    private float curVelocityX = 0;
    private float curVelocityY = 0;
    private float curVelocityZ = 0;

    private Quaternion targetRotation = Quaternion.identity;

    void Update() {
      UpdateRemoteOrientation();
    }

    void FixedUpdate() {
      AdjustCourseToRotation();
    }

    private void UpdateRemoteOrientation() {
      targetRotation = GvrController.Orientation;
    }

    private void AdjustCourseToRotation() {
      Vector3 curEuler = SkyShip.transform.eulerAngles;
      Vector3 targetEuler = targetRotation.eulerAngles;

      float diffZ = NormalizeDegree(targetEuler.z - curEuler.z);
      float diffX = NormalizeDegree(targetEuler.x - curEuler.x);
      float diffY = NormalizeDegree(targetEuler.y - curEuler.y);

      float accelZ = diffZ >= 0 ? AccelerationZ : -AccelerationZ;
      float accelX = diffX >= 0 ? AccelerationXY : -AccelerationXY;
      float accelY = diffY >= 0 ? AccelerationXY : -AccelerationXY;

      curVelocityX = Mathf.Clamp(curVelocityX + (accelX * Time.fixedDeltaTime), -MaxVelocityXY, MaxVelocityXY);
      curVelocityY = Mathf.Clamp(curVelocityY + (accelY * Time.fixedDeltaTime), -MaxVelocityXY, MaxVelocityXY);
      curVelocityZ = Mathf.Clamp(curVelocityZ + (accelZ * Time.fixedDeltaTime), -MaxVelocityZ, MaxVelocityZ);

      float deltaZ = Mathf.Min(Mathf.Abs(diffZ), Mathf.Abs(curVelocityZ) * Time.fixedDeltaTime) * (curVelocityZ >= 0 ? 1f : -1f);
      float deltaX = Mathf.Min(Mathf.Abs(diffX), Mathf.Abs(curVelocityX) * Time.fixedDeltaTime) * (curVelocityX >= 0 ? 1f : -1f);
      float deltaY = Mathf.Min(Mathf.Abs(diffY), Mathf.Abs(curVelocityY) * Time.fixedDeltaTime) * (curVelocityY >= 0 ? 1f : -1f);
      Vector3 newEuler = curEuler + new Vector3(deltaX, deltaY, deltaZ);
      newEuler = new Vector3(
          Mathf.Clamp(NormalizeDegree(newEuler.x), -MaxXDegrees, MaxXDegrees),
          Mathf.Clamp(NormalizeDegree(newEuler.y), -MaxYDegrees, MaxYDegrees),
          newEuler.z);
      SkyShip.SteerShip(newEuler);
    }

    private float NormalizeDegree(float degree) {
      if (degree > 180f) {
        degree -= 360f;
      } else if (degree < -180f) {
        degree += 360f;
      }
      return degree;
    }
  }
}

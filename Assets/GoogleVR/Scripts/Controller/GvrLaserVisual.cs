// Copyright 2017 Google Inc. All rights reserved.
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

using System;
using UnityEngine;
using UnityEngine.Assertions;

/// Visualizes a laser and a reticle using a LineRenderer and a Quad.
/// Provides functions for settings the end point of the laser,
/// and clamps the laser and reticle based on max distances.
[RequireComponent(typeof(LineRenderer))]
public class GvrLaserVisual : MonoBehaviour, IGvrArmModelReceiver {
  /// Used to position the reticle at the current position.
  [Tooltip("Used to position the reticle at the current position.")]
  public GvrControllerReticleVisual reticle;

  /// The end point of the visual will not necessarily be along the forward direction of the laser.
  /// This is particularly true in both Camera and Hybrid Raycast Modes. In that case, both the
  /// laser and the controller are rotated to face the end point. This reference is used to control
  /// the rotation of the controller.
  [Tooltip("Used to rotate the controller to face the current position.")]
  public Transform controller;

  /// Color of the laser pointer including alpha transparency.
  [Tooltip("Start color of the laser pointer including alpha transparency.")]
  public Color laserColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

  /// Color of the laser pointer including alpha transparency.
  [Tooltip("End color of the laser pointer including alpha transparency.")]
  public Color laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 0.0f);

  /// Maximum distance of the laser (meters).
  [Tooltip("Maximum distance of the laser (meters).")]
  [Range(0.0f, 20.0f)]
  public float maxLaserDistance = 1.0f;

  /// The rate that the current position moves towards the target position.
  [Tooltip("The rate that the current position moves towards the target position.")]
  public float lerpSpeed = 20.0f;

  /// If the targetPosition is greater than this threshold, then
  /// the position changes immediately instead of lerping.
  [Tooltip("If the target position is greater than this threshold, then the position changes " +
    "immediately instead of lerping.")]
  public float lerpThreshold = 1.0f;

  /// This is primarily used for Hybrid Raycast mode (details in _GvrBasePointer_) to prevent
  /// mismatches between the laser and the reticle when the "camera" component of the ray is used.
  [Tooltip("Determines if the laser will shrink when it isn't facing in the forward direction " +
    "of the transform.")]
  public bool shrinkLaser = true;

  /// Amount to shrink the laser when it is fully shrunk.
  [Range(0.0f, 1.0f)]
  [Tooltip("Amount to shrink the laser when it is fully shrunk.")]
  public float shrunkScale = 0.2f;

  /// Begin shrinking the laser when the angle between transform.forward and the reticle
  /// is greater than this value.
  [Range(0.0f, 15.0f)]
  [Tooltip("Begin shrinking the laser when the angle between transform.forward and the reticle " +
    "is greater than this value.")]
  public float beginShrinkAngleDegrees = 0.0f;

  /// Finish shrinking the laser when the angle between transform.forward and the reticle is
  /// greater than this value.
  [Range(0.0f, 15.0f)]
  [Tooltip("Finish shrinking the laser when the angle between transform.forward and the reticle " +
    "is greater than this value.")]
  public float endShrinkAngleDegrees = 2.0f;

  private const float LERP_CLAMP_THRESHOLD = 0.02f;

  public GvrBaseArmModel ArmModel { get; set; }

  /// Reference to the laser's line renderer.
  public LineRenderer Laser { get; private set; }

  /// Optional delegate for customizing how the currentPosition is calculated based on the distance.
  /// If not set, the currentPosition is determined based on the distance multiplied by the forward
  /// direction of the transform added to the position of the transform.
  public delegate Vector3 GetPointForDistanceDelegate(float distance);

  public GetPointForDistanceDelegate GetPointForDistanceFunction { get; set; }

  protected float shrinkRatio;
  protected float targetDistance;
  protected float currentDistance;
  protected Vector3 currentPosition;
  protected Vector3 currentLocalPosition;
  protected Quaternion currentLocalRotation;

  /// Set the distance of the laser.
  /// Clamps the distance of the laser and reticle.
  ///
  /// **distance** target distance from the pointer to draw the visual at.
  /// **immediate** If true, the distance is changed immediately. Otherwise, it will lerp.
  public virtual void SetDistance(float distance, bool immediate = false) {
    targetDistance = distance;
    if (immediate) {
      currentDistance = targetDistance;
    }

    if (targetDistance > lerpThreshold) {
      currentDistance = targetDistance;
    }
  }

  protected virtual void Awake() {
    Laser = GetComponent<LineRenderer>();
  }

  protected virtual void LateUpdate() {
    UpdateCurrentPosition();
    UpdateControllerOrientation();
    UpdateReticlePosition();
    UpdateLaserEndPoint();
    UpdateLaserAlpha();
  }

  protected virtual void UpdateCurrentPosition() {
    if (currentDistance != targetDistance) {
      float speed = GetSpeed();
      currentDistance = Mathf.Lerp(currentDistance, targetDistance, speed);
      float diff = Mathf.Abs(targetDistance - currentDistance);
      if (diff < LERP_CLAMP_THRESHOLD) {
        currentDistance = targetDistance;
      }
    }

    if (GetPointForDistanceFunction != null) {
      currentPosition = GetPointForDistanceFunction(currentDistance);
    } else {
      Vector3 origin = transform.position;
      currentPosition = origin + (transform.forward * currentDistance);
    }

    currentLocalPosition = transform.InverseTransformPoint(currentPosition);
    currentLocalRotation = Quaternion.FromToRotation(Vector3.forward, currentLocalPosition);
  }

  protected virtual void UpdateControllerOrientation() {
    if (controller == null) {
      return;
    }

    controller.localRotation = currentLocalRotation;
  }

  protected virtual void UpdateReticlePosition() {
    if (reticle == null) {
      return;
    }

    reticle.transform.position = currentPosition;
  }

  protected virtual void UpdateLaserEndPoint() {
    if (Laser == null) {
      return;
    }

    Vector3 laserStartPoint = Vector3.zero;
    Vector3 laserEndPoint;

    if (controller != null) {
      Vector3 worldPosition = transform.position;
      Vector3 rotatedPosition = controller.InverseTransformPoint(worldPosition);
      rotatedPosition = currentLocalRotation * rotatedPosition;
      laserStartPoint = controller.TransformPoint(rotatedPosition);
      laserStartPoint = transform.InverseTransformPoint(laserStartPoint);
    }

    laserEndPoint = Vector3.ClampMagnitude(currentLocalPosition, maxLaserDistance);

    if (shrinkLaser) {
      // Calculate the angle of rotation in degrees.
      float angle = Vector3.Angle(Vector3.forward, currentLocalPosition);

      // Calculate the shrink ratio based on the angle.
      float shrinkAngleDelta = endShrinkAngleDegrees - beginShrinkAngleDegrees;
      float clampedAngle = Mathf.Clamp(angle - beginShrinkAngleDegrees, 0.0f, shrinkAngleDelta);
      shrinkRatio = clampedAngle / shrinkAngleDelta;

      // Calculate the shrink coeff.
      float shrinkCoeff = GvrMathHelpers.EaseOutCubic(shrunkScale, 1.0f, 1.0f - shrinkRatio);

      // Calculate the final distance of the laser.
      Vector3 diff = laserStartPoint - currentLocalPosition;
      Vector3 dir = diff.normalized;
      float dist = Mathf.Min(diff.magnitude, maxLaserDistance) * shrinkCoeff;

      // Update the laser start and end points.
      laserEndPoint = currentLocalPosition;
      laserStartPoint = laserEndPoint + (dir * dist);
    }

    Laser.useWorldSpace = false;
    Laser.SetPosition(0, laserStartPoint);
    Laser.SetPosition(1, laserEndPoint);
  }

  protected virtual void UpdateLaserAlpha() {
    float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;

    Color finalStartColor = Color.Lerp(Color.clear, laserColor, alpha);
    Color finalEndColor = laserColorEnd;

    // If shrinking the laser, the colors are inversed based on the shrink ratio.
    // This is to ensure that the feathering of the laser goes in the right direction.
    if (shrinkLaser) {
      float colorRatio = GvrMathHelpers.EaseOutCubic(0.0f, 1.0f, shrinkRatio);
      finalEndColor = Color.Lerp(finalEndColor, finalStartColor, colorRatio);
      finalStartColor = Color.Lerp(finalStartColor, laserColorEnd, colorRatio);
    }

    Laser.startColor = finalStartColor;
    Laser.endColor = finalEndColor;
  }

  protected virtual float GetSpeed() {
    return lerpSpeed > 0.0f ? lerpSpeed * Time.unscaledDeltaTime : 1.0f;
  }
}

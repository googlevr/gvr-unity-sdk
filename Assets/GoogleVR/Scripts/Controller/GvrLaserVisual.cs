//-----------------------------------------------------------------------
// <copyright file="GvrLaserVisual.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>Visualizes a laser and a reticle using a LineRenderer and a Quad.</summary>
/// <remarks>
/// Provides functions for settings the end point of the laser, and clamps the laser and reticle
/// based on max distances.
/// </remarks>
[RequireComponent(typeof(LineRenderer))]
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrLaserVisual")]
public class GvrLaserVisual : MonoBehaviour, IGvrArmModelReceiver
{
    /// <summary>Used to position the reticle at the current position.</summary>
    [Tooltip("Used to position the reticle at the current position.")]
    public GvrControllerReticleVisual reticle;

    /// <summary>The controller visual's transform.</summary>
    /// <remarks>
    /// The end point of the visual will not necessarily be along the forward direction of the
    /// laser. This is particularly true in both Camera and Hybrid Raycast Modes. In that case, both
    /// the laser and the controller are rotated to face the end point. This reference is used to
    /// control the rotation of the controller.
    /// </remarks>
    [Tooltip("Used to rotate the controller to face the current position.")]
    public Transform controller;

    /// <summary>Color of the laser pointer including alpha transparency.</summary>
    [Tooltip("Start color of the laser pointer including alpha transparency.")]
    public Color laserColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

    /// <summary>Color of the laser pointer including alpha transparency.</summary>
    [Tooltip("End color of the laser pointer including alpha transparency.")]
    public Color laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    /// <summary>Maximum distance of the laser (meters).</summary>
    [Tooltip("Maximum distance of the laser(meters).")]
    [Range(0.0f, 20.0f)]
    public float maxLaserDistance = 1.0f;

    /// <summary>The rate that the current position moves towards the target position.</summary>
    [Tooltip("The rate that the current position moves towards the target position.")]
    public float lerpSpeed = 20.0f;

    /// <summary>The threashold at which position change becomes immediate.</summary>
    /// <remarks>
    /// If the targetPosition is greater than this threshold, then the position changes immediately
    /// instead of lerping.
    /// </remarks>
    [Tooltip("If the target position is greater than this threshold, then the position changes " +
    "immediately instead of lerping.")]
    public float lerpThreshold = 1.0f;

    /// <summary>
    /// This is primarily used for Hybrid Raycast mode (details in _GvrBasePointer_) to prevent
    /// mismatches between the laser and the reticle when the "camera" component of the ray is used.
    /// </summary>
    [Tooltip("Determines if the laser will shrink when it isn't facing in the forward direction " +
    "of the transform.")]
    public bool shrinkLaser = true;

    /// <summary>Amount to shrink the laser when it is fully shrunk.</summary>
    [Range(0.0f, 1.0f)]
    [Tooltip("Amount to shrink the laser when it is fully shrunk.")]
    public float shrunkScale = 0.2f;

    /// <summary>
    /// Begin shrinking the laser when the angle between `transform.forward` and the reticle is
    /// greater than this value.
    /// </summary>
    [Range(0.0f, 15.0f)]
    [Tooltip("Begin shrinking the laser when the angle between transform.forward and the reticle " +
        "is greater than this value.")]
    public float beginShrinkAngleDegrees = 0.0f;

    /// <summary>
    /// Finish shrinking the laser when the angle between `transform.forward` and the reticle is
    /// greater than this value.
    /// </summary>
    [Range(0.0f, 15.0f)]
    [Tooltip("Finish shrinking the laser when the angle between transform.forward and the reticle " +
        "is greater than this value.")]
    public float endShrinkAngleDegrees = 2.0f;

    /// <summary>Ratio to shrink the visual by.</summary>
    protected float shrinkRatio;

    /// <summary>Distance to the target object.</summary>
    protected float targetDistance;

    /// <summary>Current distance to the visual.</summary>
    protected float currentDistance;

    /// <summary>Current world position of the visual.</summary>
    protected Vector3 currentPosition;

    /// <summary>Current local position of the visual.</summary>
    protected Vector3 currentLocalPosition;

    /// <summary>Current local rotation of the visual.</summary>
    protected Quaternion currentLocalRotation;

    private const float LERP_CLAMP_THRESHOLD = 0.02f;

    /// <summary>
    /// Optional delegate for customizing how the currentPosition is calculated based on the
    /// distance.
    /// </summary>
    /// <remarks>
    /// If not set, the `currentPosition` is determined based on the distance multiplied by the
    /// forward direction of the transform added to the position of the transform.
    /// </remarks>
    /// <returns>
    /// Default: The distance mutliplied by the forwrad direction of the transform added to the
    /// position of the transform.
    /// Overridden: An implementation for calculating `currentPosition` from distance.
    /// </returns>
    /// <param name="distance">The distance to use in calculating the `currentPosition`.</param>
    public delegate Vector3 GetPointForDistanceDelegate(float distance);

    /// <summary>Gets or sets the arm model used to control the visual.</summary>
    /// <value>The arm model used to control the visual.</value>
    public GvrBaseArmModel ArmModel { get; set; }

    /// <summary>Gets a reference to the laser's line renderer.</summary>
    /// <value>The laser's line renderer.</value>
    public LineRenderer Laser { get; private set; }

    /// <summary>Gets or sets the function to use for determining the point at a distance.</summary>
    /// <value>The function to use for determining the point at a distance.</value>
    public GetPointForDistanceDelegate GetPointForDistanceFunction { get; set; }

    /// <summary>Gets the current distance to the visual.</summary>
    /// <value>The current distance to the visual.</value>
    public float CurrentDistance
    {
        get { return currentDistance; }
    }

    /// <summary>Set the distance of the laser.</summary>
    /// <remarks>Clamps the distance of the laser and reticle.</remarks>
    /// <param name="distance">Target distance from the pointer to draw the visual at.</param>
    /// <param name="immediate">
    /// If `true`, the distance is changed immediately. Otherwise, it will lerp.
    /// </param>
    public virtual void SetDistance(float distance, bool immediate = false)
    {
        targetDistance = distance;
        if (immediate)
        {
            currentDistance = targetDistance;
        }

        if (targetDistance > lerpThreshold)
        {
            currentDistance = targetDistance;
        }
    }

    /// @cond
    /// <summary>The MonoBehavior's Awake method.</summary>
    protected virtual void Awake()
    {
        Laser = GetComponent<LineRenderer>();
    }

    /// @endcond
    /// @cond
    /// <summary>The MonoBehavior's Awake method.</summary>
    protected virtual void LateUpdate()
    {
        UpdateCurrentPosition();
        UpdateControllerOrientation();
        UpdateReticlePosition();
        UpdateLaserEndPoint();
        UpdateLaserAlpha();
    }

    /// @endcond
    /// <summary>Updates the current position of the visual.</summary>
    protected virtual void UpdateCurrentPosition()
    {
        if (currentDistance != targetDistance)
        {
            float speed = GetSpeed();
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, speed);
            float diff = Mathf.Abs(targetDistance - currentDistance);
            if (diff < LERP_CLAMP_THRESHOLD)
            {
                currentDistance = targetDistance;
            }
        }

        if (GetPointForDistanceFunction != null)
        {
            currentPosition = GetPointForDistanceFunction(currentDistance);
        }
        else
        {
            Vector3 origin = transform.position;
            currentPosition = origin + (transform.forward * currentDistance);
        }

        currentLocalPosition = transform.InverseTransformPoint(currentPosition);
        currentLocalRotation = Quaternion.FromToRotation(Vector3.forward, currentLocalPosition);
    }

    /// <summary>
    /// Updates the rotation of  the controller based on the current local rotation.
    /// </summary>
    protected virtual void UpdateControllerOrientation()
    {
        if (controller == null)
        {
            return;
        }

        controller.localRotation = currentLocalRotation;
    }

    /// <summary> Updates the position of the reticle to the current position.</summary>
    protected virtual void UpdateReticlePosition()
    {
        if (reticle == null)
        {
            return;
        }

        reticle.transform.position = currentPosition;
    }

    /// <summary>Updates the endpoint of the laser based on max distance.</summary>
    protected virtual void UpdateLaserEndPoint()
    {
        if (Laser == null)
        {
            return;
        }

        Vector3 laserStartPoint = Vector3.zero;
        Vector3 laserEndPoint;

        if (controller != null)
        {
            Vector3 worldPosition = transform.position;
            Vector3 rotatedPosition = controller.InverseTransformPoint(worldPosition);
            rotatedPosition = currentLocalRotation * rotatedPosition;
            laserStartPoint = controller.TransformPoint(rotatedPosition);
            laserStartPoint = transform.InverseTransformPoint(laserStartPoint);
        }

        laserEndPoint = Vector3.ClampMagnitude(currentLocalPosition, maxLaserDistance);

        if (shrinkLaser)
        {
            // Calculate the angle of rotation in degrees.
            float angle = Vector3.Angle(Vector3.forward, currentLocalPosition);

            // Calculate the shrink ratio based on the angle.
            float shrinkAngleDelta = endShrinkAngleDegrees - beginShrinkAngleDegrees;
            float clampedAngle =
                Mathf.Clamp(angle - beginShrinkAngleDegrees, 0.0f, shrinkAngleDelta);
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

    /// <summary>Updates the alpha of the laser beam.</summary>
    protected virtual void UpdateLaserAlpha()
    {
        float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;

        Color finalStartColor = Color.Lerp(Color.clear, laserColor, alpha);
        Color finalEndColor = laserColorEnd;

        // If shrinking the laser, the colors are inversed based on the shrink ratio.
        // This is to ensure that the feathering of the laser goes in the right direction.
        if (shrinkLaser)
        {
            float colorRatio = GvrMathHelpers.EaseOutCubic(0.0f, 1.0f, shrinkRatio);
            finalEndColor = Color.Lerp(finalEndColor, finalStartColor, colorRatio);
            finalStartColor = Color.Lerp(finalStartColor, laserColorEnd, colorRatio);
        }

        Laser.startColor = finalStartColor;
        Laser.endColor = finalEndColor;
    }

    /// <summary>Gets the speed of the moving pointer visual.</summary>
    /// <returns>The lerp speed of the moving pointer visual.</returns>
    protected virtual float GetSpeed()
    {
        return lerpSpeed > 0.0f ? lerpSpeed * Time.unscaledDeltaTime : 1.0f;
    }
}

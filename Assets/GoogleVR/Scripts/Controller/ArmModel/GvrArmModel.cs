//-----------------------------------------------------------------------
// <copyright file="GvrArmModel.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;

/// <summary>
/// Standard implementation for a mathematical model to make the virtual controller approximate the
/// physical location of the Daydream controller.
/// </summary>
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrArmModel")]
public class GvrArmModel : GvrBaseArmModel, IGvrControllerInputDeviceReceiver
{
    /// @cond
    /// <summary>The default elbow bend ratio.</summary>
    public const float DEFAULT_ELBOW_BEND_RATIO = 0.6f;

    // Default values for tuning variables:

    /// @endcond
    /// @cond
    /// <summary>The default elbow rest position.</summary>
    public static readonly Vector3 DEFAULT_ELBOW_REST_POSITION = new Vector3(0.195f, -0.5f, 0.005f);

    /// @endcond
    /// @cond
    /// <summary>The default wrist rest position.</summary>
    public static readonly Vector3 DEFAULT_WRIST_REST_POSITION = new Vector3(0.0f, 0.0f, 0.25f);

    /// @endcond
    /// @cond
    /// <summary>The default controller rest position.</summary>
    public static readonly Vector3 DEFAULT_CONTROLLER_REST_POSITION =
        new Vector3(0.0f, 0.0f, 0.05f);

    /// @endcond
    /// @cond
    /// <summary>The default arm extension offset.</summary>
    public static readonly Vector3 DEFAULT_ARM_EXTENSION_OFFSET = new Vector3(-0.13f, 0.14f, 0.08f);

    /// @endcond
    /// <summary>
    /// Position of the elbow joint relative to the head before the arm model is applied.
    /// </summary>
    public Vector3 elbowRestPosition = DEFAULT_ELBOW_REST_POSITION;

    /// <summary>
    /// Position of the wrist joint relative to the elbow before the arm model is applied.
    /// </summary>
    public Vector3 wristRestPosition = DEFAULT_WRIST_REST_POSITION;

    /// <summary>
    /// Position of the controller joint relative to the wrist before the arm model is applied.
    /// </summary>
    public Vector3 controllerRestPosition = DEFAULT_CONTROLLER_REST_POSITION;

    /// <summary>
    /// Offset applied to the elbow position as the controller is rotated upwards.
    /// </summary>
    public Vector3 armExtensionOffset = DEFAULT_ARM_EXTENSION_OFFSET;

    /// <summary>Ratio of the controller's rotation to apply to the rotation of the elbow.</summary>
    /// <remarks>The remaining rotation is applied to the wrist's rotation.</remarks>
    [Range(0.0f, 1.0f)]
    public float elbowBendRatio = DEFAULT_ELBOW_BEND_RATIO;

    /// <summary>
    /// Offset in front of the controller to determine what position to use when determing if the
    /// controller should fade.
    /// </summary>
    /// <remarks>This is useful when objects are attached to the controller.</remarks>
    [Range(0.0f, 0.4f)]
    public float fadeControllerOffset = 0.0f;

    /// <summary>
    /// Controller distance from the front/back of the head after which the controller disappears
    /// (meters).
    /// </summary>
    [Range(0.0f, 0.4f)]
    public float fadeDistanceFromHeadForward = 0.25f;

    /// <summary>
    /// Controller distance from the left/right of the head after which the controller disappears
    /// (meters).
    /// </summary>
    [Range(0.0f, 0.4f)]
    public float fadeDistanceFromHeadSide = 0.15f;

    /// <summary>Controller distance from face after which the tooltips appear (meters).</summary>
    [Range(0.4f, 0.6f)]
    public float tooltipMinDistanceFromFace = 0.45f;

    /// <summary>
    /// The maximum angle in degrees between the controller and head at which to show tooltips.
    /// </summary>
    /// <remarks>
    /// When the angle between the controller and the head is larger than this value, the tooltips
    /// disappear.  If the value is 180, then the tooltips are always shown.  If the value is 90,
    /// the tooltips are only shown when they are facing the camera.
    /// </remarks>
    [Range(0, 180)]
    public int tooltipMaxAngleFromCamera = 80;

    /// <summary>
    /// If `true`, the root of the pose is locked to the local position of the player's neck.
    /// </summary>
    public bool isLockedToNeck = false;

    /// <summary>Increases elbow bending as the controller moves up (unitless).</summary>
    protected const float EXTENSION_WEIGHT = 0.4f;

    /// <summary>Amount of normalized alpha transparency to change per second.</summary>
    protected const float DELTA_ALPHA = 4.0f;

    /// <summary>
    /// Minimum angle in degrees of the controller the for arm extension offset to start.
    /// </summary>
    /// <remarks>
    /// This is the range of controller X-axis values in which the modeled arm rotates with the
    /// controller, outside of which the modeled arm doesn't rotate with the controller, only the
    /// controller rotates.  Below this value, the wrist is primarily responsible for controller
    /// rotation, not the arm.
    /// </remarks>
    protected const float MIN_EXTENSION_ANGLE = 7.0f;

    /// <summary>
    /// Maximum angle in degrees of the controller the for arm extension offset to end.
    /// </summary>
    /// <remarks>
    /// This is the range of controller X-axis values in which the modeled arm rotates with the
    /// controller, outside of which the modeled arm doesn't rotate with the controller, only the
    /// controller rotates.  Above this value, the wrist is primarily responsible for controller
    /// rotation, not the arm.
    /// </remarks>
    protected const float MAX_EXTENSION_ANGLE = 60.0f;

    /// <summary>Rest position for shoulder joint.</summary>
    protected static readonly Vector3 SHOULDER_POSITION = new Vector3(0.17f, -0.2f, -0.03f);

    /// <summary>Neck offset used to apply the inverse neck model when locked to the head.</summary>
    protected static readonly Vector3 NECK_OFFSET = new Vector3(0.0f, 0.075f, 0.08f);

    /// <summary>The neck position based on this arm model.</summary>
    protected Vector3 neckPosition;

    /// <summary>The elbow position based on this arm model.</summary>
    protected Vector3 elbowPosition;

    /// <summary>The elbow rotation based on this arm model.</summary>
    protected Quaternion elbowRotation;

    /// <summary>The wrist position based on this arm model.</summary>
    protected Vector3 wristPosition;

    /// <summary>The wrist rotation based on this arm model.</summary>
    protected Quaternion wristRotation;

    /// <summary>The controller position based on this arm model.</summary>
    protected Vector3 controllerPosition;

    /// <summary>The controller rotation based on this arm model.</summary>
    protected Quaternion controllerRotation;

    /// <summary>The preferred alpha.</summary>
    protected float preferredAlpha;

    /// <summary>The tooltip alpha value.</summary>
    protected float tooltipAlphaValue;

    /// <summary>Multiplier for handedness such that 1 = Right, 0 = Center, -1 = left.</summary>
    protected Vector3 handedMultiplier;

    /// <summary>Forward direction of user's torso.</summary>
    protected Vector3 torsoDirection;

    /// <summary>Orientation of the user's torso.</summary>
    protected Quaternion torsoRotation;

    /// <inheritdoc/>
    public override Vector3 ControllerPositionFromHead
    {
        get { return controllerPosition; }
    }

    /// <inheritdoc/>
    public override Quaternion ControllerRotationFromHead
    {
        get { return controllerRotation; }
    }

    /// <inheritdoc/>
    public override float PreferredAlpha
    {
        get { return preferredAlpha; }
    }

    /// <inheritdoc/>
    public override float TooltipAlphaValue
    {
        get { return tooltipAlphaValue; }
    }

    /// <summary>Gets the neck's position relative to the user's head.</summary>
    /// <remarks>
    /// If `isLockedToNeck` is `true`, this will be the input tracking position of the head node
    /// modified by an inverse neck model to approximate the neck position.  Otherwise, it is always
    /// zero.
    /// </remarks>
    /// <value>The neck position.</value>
    public Vector3 NeckPosition
    {
        get { return neckPosition; }
    }

    /// <summary>Gets the shoulder's position relative to the user's head.</summary>
    /// <remarks>
    /// This is not actually used as part of the arm model calculations, and exists for debugging.
    /// </remarks>
    /// <value>The shoulder position.</value>
    public Vector3 ShoulderPosition
    {
        get
        {
            Vector3 shoulderPosition =
                neckPosition + (torsoRotation * Vector3.Scale(SHOULDER_POSITION, handedMultiplier));

            return shoulderPosition;
        }
    }

    /// <summary>Gets the shoulder's rotation relative to the user's head.</summary>
    /// <remarks>
    /// This is not actually used as part of the arm model calculations, and exists for debugging.
    /// </remarks>
    /// <value>The shoulder rotation.</value>
    public Quaternion ShoulderRotation
    {
        get { return torsoRotation; }
    }

    /// <summary>Gets the elbow's position relative to the user's head.</summary>
    /// <value>The elbow position.</value>
    public Vector3 ElbowPosition
    {
        get { return elbowPosition; }
    }

    /// <summary>Gets the elbow's rotation relative to the user's head.</summary>
    /// <value>The elbow rotation.</value>
    public Quaternion ElbowRotation
    {
        get { return elbowRotation; }
    }

    /// <summary>Gets the wrist's position relative to the user's head.</summary>
    /// <value>The wrist position.</value>
    public Vector3 WristPosition
    {
        get { return wristPosition; }
    }

    /// <summary>Gets the wrist's rotation relative to the user's head.</summary>
    /// <value>The wrist rotation.</value>
    public Quaternion WristRotation
    {
        get { return wristRotation; }
    }

    /// <summary>Gets or sets the controller input device.</summary>
    /// <value>The controller input device.</value>
    public GvrControllerInputDevice ControllerInputDevice { get; set; }

    /// @cond
    /// <summary>The `MonoBehavior`'s `OnEnable` method.</summary>
    protected virtual void OnEnable()
    {
        // Register the controller update listener.
        GvrControllerInput.OnControllerInputUpdated += OnControllerInputUpdated;

        // Force the torso direction to match the gaze direction immediately.
        // Otherwise, the controller will not be positioned correctly if the ArmModel was enabled
        // when the user wasn't facing forward.
        UpdateTorsoDirection(true);

        // Update immediately to avoid a frame delay before the arm model is applied.
        OnControllerInputUpdated();
    }

    /// @endcond
    /// @cond
    /// <summary>The `MonoBehavior`'s `OnDisable` method.</summary>
    protected virtual void OnDisable()
    {
        GvrControllerInput.OnControllerInputUpdated -= OnControllerInputUpdated;
    }

    /// @endcond
    /// @cond
    /// <summary>The `GvrControllerInput`'s `OnControllerInputUpdated` action.</summary>
    protected virtual void OnControllerInputUpdated()
    {
        UpdateHandedness();
        UpdateTorsoDirection(false);
        UpdateNeckPosition();
        ApplyArmModel();
        UpdateTransparency();
    }

    /// @endcond
    /// <summary>Updates the arm model handedness.</summary>
    protected virtual void UpdateHandedness()
    {
        // Update user handedness if the setting has changed.
        if (ControllerInputDevice == null)
        {
            return;
        }

        // Determine handedness multiplier.
        handedMultiplier.Set(0, 1, 1);
        if (ControllerInputDevice.IsRightHand)
        {
            handedMultiplier.x = 1.0f;
        }
        else
        {
            handedMultiplier.x = -1.0f;
        }
    }

    /// <summary>Updates the arm model torso direction.</summary>
    /// <param name="forceImmediate">
    /// If `true`, uses the gaze direction, otherwise uses slerp to update the direction smoothly.
    /// </param>
    protected virtual void UpdateTorsoDirection(bool forceImmediate)
    {
        // Determine the gaze direction horizontally.
        Vector3 gazeDirection = GvrVRHelpers.GetHeadForward();
        gazeDirection.y = 0.0f;
        gazeDirection.Normalize();

        // Use the gaze direction to update the forward direction.
        if (forceImmediate ||
              (ControllerInputDevice != null && ControllerInputDevice.Recentered))
        {
            torsoDirection = gazeDirection;
        }
        else
        {
            float angularVelocity =
                ControllerInputDevice != null ? ControllerInputDevice.Gyro.magnitude : 0;

            float gazeFilterStrength = Mathf.Clamp((angularVelocity - 0.2f) / 45.0f, 0.0f, 0.1f);
            torsoDirection = Vector3.Slerp(torsoDirection, gazeDirection, gazeFilterStrength);
        }

        // Calculate the torso rotation.
        torsoRotation = Quaternion.FromToRotation(Vector3.forward, torsoDirection);
    }

    /// <summary>Updates the neck position in the arm model.</summary>
    protected virtual void UpdateNeckPosition()
    {
        if (isLockedToNeck)
        {
            // Returns the center of the eyes.
            // However, we actually want to lock to the center of the head.
            neckPosition = GvrVRHelpers.GetHeadPosition();

            // Find the approximate neck position by Applying an inverse neck model.
            // This transforms the head position to the center of the head and also accounts
            // for the head's rotation so that the motion feels more natural.
            neckPosition = ApplyInverseNeckModel(neckPosition);
        }
        else
        {
            neckPosition = Vector3.zero;
        }
    }

    /// <summary>Applies the arm model parameters to update the orientation and position.</summary>
    protected virtual void ApplyArmModel()
    {
        // Set the starting positions of the joints before they are transformed by the arm model.
        SetUntransformedJointPositions();

        // Get the controller's orientation.
        Quaternion controllerOrientation;
        Quaternion xyRotation;
        float xAngle;
        GetControllerRotation(out controllerOrientation, out xyRotation, out xAngle);

        // Offset the elbow by the extension offset.
        float extensionRatio = CalculateExtensionRatio(xAngle);
        ApplyExtensionOffset(extensionRatio);

        // Calculate the lerp rotation, which is used to control how much the rotation of the
        // controller impacts each joint.
        Quaternion lerpRotation = CalculateLerpRotation(xyRotation, extensionRatio);

        CalculateFinalJointRotations(controllerOrientation, xyRotation, lerpRotation);
        ApplyRotationToJoints();
    }

    /// <summary>
    /// Set the starting positions of the joints before they are transformed by the arm model.
    /// </summary>
    protected virtual void SetUntransformedJointPositions()
    {
        elbowPosition = Vector3.Scale(elbowRestPosition, handedMultiplier);
        wristPosition = Vector3.Scale(wristRestPosition, handedMultiplier);
        controllerPosition = Vector3.Scale(controllerRestPosition, handedMultiplier);
    }

    /// <summary>
    /// Calculate the extension ratio based on the angle of the controller along the x axis.
    /// </summary>
    /// <returns>The extension ratio of the elbow.</returns>
    /// <param name="xAngle">The X angle of the controller along the x axis.</param>
    protected virtual float CalculateExtensionRatio(float xAngle)
    {
        float normalizedAngle =
            (xAngle - MIN_EXTENSION_ANGLE) / (MAX_EXTENSION_ANGLE - MIN_EXTENSION_ANGLE);

        float extensionRatio = Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);
        return extensionRatio;
    }

    /// <summary>Offset the elbow by the extension offset.</summary>
    /// <param name="extensionRatio">The extension ratio of the elbow to apply.</param>
    protected virtual void ApplyExtensionOffset(float extensionRatio)
    {
        Vector3 extensionOffset = Vector3.Scale(armExtensionOffset, handedMultiplier);
        elbowPosition += extensionOffset * extensionRatio;
    }

    /// <summary>
    /// Calculate the lerp rotation, which is used to control how much the rotation of the
    /// controller impacts each joint.
    /// </summary>
    /// <returns>The lerp rotation.</returns>
    /// <param name="xyRotation">The xy rotation of the controller.</param>
    /// <param name="extensionRatio">The extension ratio of the elbow.</param>
    protected virtual Quaternion CalculateLerpRotation(Quaternion xyRotation, float extensionRatio)
    {
        float totalAngle = Quaternion.Angle(xyRotation, Quaternion.identity);
        float lerpSuppresion = 1.0f - Mathf.Pow(totalAngle / 180.0f, 6.0f);
        float inverseElbowBendRatio = 1.0f - elbowBendRatio;

        float lerpValue =
            inverseElbowBendRatio + (elbowBendRatio * extensionRatio * EXTENSION_WEIGHT);

        lerpValue *= lerpSuppresion;
        return Quaternion.Lerp(Quaternion.identity, xyRotation, lerpValue);
    }

    /// <summary>Determine the final joint rotations relative to the head.</summary>
    /// <param name="controllerOrientation">Controller orientation.</param>
    /// <param name="xyRotation">The xy rotation of the controller.</param>
    /// <param name="lerpRotation">Lerp rotation.</param>
    protected virtual void CalculateFinalJointRotations(Quaternion controllerOrientation,
                                                        Quaternion xyRotation,
                                                        Quaternion lerpRotation)
    {
        elbowRotation = torsoRotation * Quaternion.Inverse(lerpRotation) * xyRotation;
        wristRotation = elbowRotation * lerpRotation;
        controllerRotation = torsoRotation * controllerOrientation;
    }

    /// <summary>
    /// Apply the joint rotations to the positions of the joints to determine the final pose.
    /// </summary>
    protected virtual void ApplyRotationToJoints()
    {
        elbowPosition = neckPosition + (torsoRotation * elbowPosition);
        wristPosition = elbowPosition + (elbowRotation * wristPosition);
        controllerPosition = wristPosition + (wristRotation * controllerPosition);
    }

    /// <summary>Transform the head position into an approximate neck position.</summary>
    /// <returns>The inverse neck model.</returns>
    /// <param name="headPosition">Head position.</param>
    protected virtual Vector3 ApplyInverseNeckModel(Vector3 headPosition)
    {
        Quaternion headRotation = GvrVRHelpers.GetHeadRotation();
        Vector3 rotatedNeckOffset =
            (headRotation * NECK_OFFSET) - (NECK_OFFSET.y * Vector3.up);
        headPosition -= rotatedNeckOffset;

        return headPosition;
    }

    /// <summary>
    /// Controls the transparency of the controller to prevent the controller from clipping through
    /// the user's head.
    /// </summary>
    /// <remarks>
    /// Also controls the transparency of the tooltips so they are only visible when the controller
    /// is held up.
    /// </remarks>
    protected virtual void UpdateTransparency()
    {
        Vector3 controllerForward = controllerRotation * Vector3.forward;

        Vector3 offsetControllerPosition =
            controllerPosition + (controllerForward * fadeControllerOffset);

        Vector3 controllerRelativeToHead = offsetControllerPosition - neckPosition;

        Vector3 headForward = GvrVRHelpers.GetHeadForward();

        float distanceToHeadForward =
            Vector3.Scale(controllerRelativeToHead, headForward).magnitude;

        Vector3 headRight = Vector3.Cross(headForward, Vector3.up);
        float distanceToHeadSide = Vector3.Scale(controllerRelativeToHead, headRight).magnitude;
        float distanceToHeadUp = Mathf.Abs(controllerRelativeToHead.y);

        bool shouldFadeController = distanceToHeadForward < fadeDistanceFromHeadForward
                                    && distanceToHeadUp < fadeDistanceFromHeadForward
                                    && distanceToHeadSide < fadeDistanceFromHeadSide;

        // Determine how vertical the controller is pointing.
        float animationDelta = DELTA_ALPHA * Time.unscaledDeltaTime;
        if (shouldFadeController)
        {
            preferredAlpha = Mathf.Max(0.0f, preferredAlpha - animationDelta);
        }
        else
        {
            preferredAlpha = Mathf.Min(1.0f, preferredAlpha + animationDelta);
        }

        float dot = Vector3.Dot(controllerRotation * Vector3.up,
                                -controllerRelativeToHead.normalized);
        float minDot = (tooltipMaxAngleFromCamera - 90.0f) / -90.0f;
        float distToFace = Vector3.Distance(controllerRelativeToHead, Vector3.zero);
        if (shouldFadeController
              || distToFace > tooltipMinDistanceFromFace
              || dot < minDot)
        {
            tooltipAlphaValue = Mathf.Max(0.0f, tooltipAlphaValue - animationDelta);
        }
        else
        {
            tooltipAlphaValue = Mathf.Min(1.0f, tooltipAlphaValue + animationDelta);
        }
    }

    /// <summary>Get the controller's orientation.</summary>
    /// <param name="rotation">The output rotation which will be written to.</param>
    /// <param name="xyRotation">The output xy-only rotation.</param>
    /// <param name="xAngle">The output angle from the X axis.</param>
    protected void GetControllerRotation(out Quaternion rotation,
                                         out Quaternion xyRotation,
                                         out float xAngle)
    {
        // Find the controller's orientation relative to the player.
        rotation = ControllerInputDevice != null ?
            ControllerInputDevice.Orientation : Quaternion.identity;

        rotation = Quaternion.Inverse(torsoRotation) * rotation;

        // Extract just the x rotation angle.
        Vector3 controllerForward = rotation * Vector3.forward;
        xAngle = 90.0f - Vector3.Angle(controllerForward, Vector3.up);

        // Remove the z rotation from the controller.
        xyRotation = Quaternion.FromToRotation(Vector3.forward, controllerForward);
    }

#if UNITY_EDITOR
    /// <summary>Raises the draw gizmos selected event.</summary>
    protected virtual void OnDrawGizmosSelected()
    {
        if (!enabled)
        {
            return;
        }

        if (transform.parent == null)
        {
            return;
        }

        Vector3 worldShoulder = transform.parent.TransformPoint(ShoulderPosition);
        Vector3 worldElbow = transform.parent.TransformPoint(elbowPosition);
        Vector3 worldwrist = transform.parent.TransformPoint(wristPosition);
        Vector3 worldcontroller = transform.parent.TransformPoint(controllerPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldShoulder, 0.02f);
        Gizmos.DrawLine(worldShoulder, worldElbow);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(worldElbow, 0.02f);
        Gizmos.DrawLine(worldElbow, worldwrist);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(worldwrist, 0.02f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(worldcontroller, 0.02f);
    }
#endif // UNITY_EDITOR
}

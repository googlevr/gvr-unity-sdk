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
using System.Collections;

/// Standard implementation for a mathematical model to make the virtual controller approximate the
/// physical location of the Daydream controller.
public class GvrArmModel : GvrBaseArmModel{
  /// Position of the elbow joint relative to the head before the arm model is applied.
  public Vector3 elbowRestPosition = DEFAULT_ELBOW_REST_POSITION;

  /// Position of the wrist joint relative to the elbow before the arm model is applied.
  public Vector3 wristRestPosition = DEFAULT_WRIST_REST_POSITION;

  /// Position of the controller joint relative to the wrist before the arm model is applied.
  public Vector3 controllerRestPosition = DEFAULT_CONTROLLER_REST_POSITION;

  /// Offset applied to the elbow position as the controller is rotated upwards.
  public Vector3 armExtensionOffset = DEFAULT_ARM_EXTENSION_OFFSET;

  /// Ratio of the controller's rotation to apply to the rotation of the elbow.
  /// The remaining rotation is applied to the wrist's rotation.
  [Range(0.0f, 1.0f)]
  public float elbowBendRatio = DEFAULT_ELBOW_BEND_RATIO;

  /// Offset in front of the controller to determine what position to use when determing if the
  /// controller should fade. This is useful when objects are attached to the controller.
  [Range(0.0f, 0.4f)]
  public float fadeControllerOffset = 0.0f;

  /// Controller distance from the front/back of the head after which the controller disappears (meters).
  [Range(0.0f, 0.4f)]
  public float fadeDistanceFromHeadForward = 0.25f;

  /// Controller distance from the left/right of the head after which the controller disappears (meters).
  [Range(0.0f, 0.4f)]
  public float fadeDistanceFromHeadSide = 0.15f;

  /// Controller distance from face after which the tooltips appear (meters).
  [Range(0.4f, 0.6f)]
  public float tooltipMinDistanceFromFace = 0.45f;

  /// When the angle (degrees) between the controller and the head is larger than
  /// this value, the tooltips disappear.
  /// If the value is 180, then the tooltips are always shown.
  /// If the value is 90, the tooltips are only shown when they are facing the camera.
  [Range(0, 180)]
  public int tooltipMaxAngleFromCamera = 80;

  /// If true, the root of the pose is locked to the local position of the player's neck.
  public bool isLockedToNeck = false;

  /// Represents the controller's position relative to the user's head.
  public override Vector3 ControllerPositionFromHead {
    get {
      return controllerPosition;
    }
  }

  /// Represent the controller's rotation relative to the user's head.
  public override Quaternion ControllerRotationFromHead {
    get {
      return controllerRotation;
    }
  }

  /// The suggested rendering alpha value of the controller.
  /// This is to prevent the controller from intersecting the face.
  /// The range is always 0 - 1.
  public override float PreferredAlpha {
    get {
      return preferredAlpha;
    }
  }

  /// The suggested rendering alpha value of the controller tooltips.
  /// This is to only display the tooltips when the player is looking
  /// at the controller, and also to prevent the tooltips from intersecting the
  /// player's face.
  public override float TooltipAlphaValue {
    get {
      return tooltipAlphaValue;
    }
  }

  /// Represent the neck's position relative to the user's head.
  /// If isLockedToNeck is true, this will be the InputTracking position of the Head node modified
  /// by an inverse neck model to approximate the neck position.
  /// Otherwise, it is always zero.
  public Vector3 NeckPosition {
    get {
      return neckPosition;
    }
  }

  /// Represent the shoulder's position relative to the user's head.
  /// This is not actually used as part of the arm model calculations, and exists for debugging.
  public Vector3 ShoulderPosition {
    get {
      Vector3 shoulderPosition = neckPosition + torsoRotation * Vector3.Scale(SHOULDER_POSITION, handedMultiplier);
      return shoulderPosition;
    }
  }

  /// Represent the shoulder's rotation relative to the user's head.
  /// This is not actually used as part of the arm model calculations, and exists for debugging.
  public Quaternion ShoulderRotation {
    get {
      return torsoRotation;
    }
  }

  /// Represent the elbow's position relative to the user's head.
  public Vector3 ElbowPosition {
    get {
      return elbowPosition;
    }
  }

  /// Represent the elbow's rotation relative to the user's head.
  public Quaternion ElbowRotation {
    get {
      return elbowRotation;
    }
  }

  /// Represent the wrist's position relative to the user's head.
  public Vector3 WristPosition {
    get {
      return wristPosition;
    }
  }

  /// Represent the wrist's rotation relative to the user's head.
  public Quaternion WristRotation {
    get {
      return wristRotation;
    }
  }

  protected Vector3 neckPosition;
  protected Vector3 elbowPosition;
  protected Quaternion elbowRotation;
  protected Vector3 wristPosition;
  protected Quaternion wristRotation;
  protected Vector3 controllerPosition;
  protected Quaternion controllerRotation;
  protected float preferredAlpha;
  protected float tooltipAlphaValue;

  /// Multiplier for handedness such that 1 = Right, 0 = Center, -1 = left.
  protected Vector3 handedMultiplier;

  /// Forward direction of user's torso.
  protected Vector3 torsoDirection;

  /// Orientation of the user's torso.
  protected Quaternion torsoRotation;

  // Default values for tuning variables.
  public static readonly Vector3 DEFAULT_ELBOW_REST_POSITION = new Vector3(0.195f, -0.5f, 0.005f);
  public static readonly Vector3 DEFAULT_WRIST_REST_POSITION = new Vector3(0.0f, 0.0f, 0.25f);
  public static readonly Vector3 DEFAULT_CONTROLLER_REST_POSITION = new Vector3(0.0f, 0.0f, 0.05f);
  public static readonly Vector3 DEFAULT_ARM_EXTENSION_OFFSET = new Vector3(-0.13f, 0.14f, 0.08f);
  public const float DEFAULT_ELBOW_BEND_RATIO = 0.6f;

  /// Increases elbow bending as the controller moves up (unitless).
  protected const float EXTENSION_WEIGHT = 0.4f;

  /// Rest position for shoulder joint.
  protected static readonly Vector3 SHOULDER_POSITION = new Vector3(0.17f, -0.2f, -0.03f);

  /// Neck offset used to apply the inverse neck model when locked to the head.
  protected static readonly Vector3 NECK_OFFSET = new Vector3(0.0f, 0.075f, 0.08f);

  /// Amount of normalized alpha transparency to change per second.
  protected const float DELTA_ALPHA = 4.0f;

  /// Angle ranges the for arm extension offset to start and end (degrees).
  protected const float MIN_EXTENSION_ANGLE = 7.0f;
  protected const float MAX_EXTENSION_ANGLE = 60.0f;

  protected virtual void OnEnable() {
    // Register the controller update listener.
    GvrControllerInput.OnControllerInputUpdated += OnControllerInputUpdated;

    // Force the torso direction to match the gaze direction immediately.
    // Otherwise, the controller will not be positioned correctly if the ArmModel was enabled
    // when the user wasn't facing forward.
    UpdateTorsoDirection(true);

    // Update immediately to avoid a frame delay before the arm model is applied.
    OnControllerInputUpdated();
  }

  protected virtual void OnDisable() {
    GvrControllerInput.OnControllerInputUpdated -= OnControllerInputUpdated;
  }

  protected virtual void OnControllerInputUpdated() {
    UpdateHandedness();
    UpdateTorsoDirection(false);
    UpdateNeckPosition();
    ApplyArmModel();
    UpdateTransparency();
  }

  protected virtual void UpdateHandedness() {
    // Update user handedness if the setting has changed
    GvrSettings.UserPrefsHandedness handedness = GvrSettings.Handedness;

    // Determine handedness multiplier.
    handedMultiplier.Set(0, 1, 1);
    if (handedness == GvrSettings.UserPrefsHandedness.Right) {
      handedMultiplier.x = 1.0f;
    } else if (handedness == GvrSettings.UserPrefsHandedness.Left) {
      handedMultiplier.x = -1.0f;
    }
  }

  protected virtual void UpdateTorsoDirection(bool forceImmediate) {
    // Determine the gaze direction horizontally.
    Vector3 gazeDirection = GvrVRHelpers.GetHeadForward();
    gazeDirection.y = 0.0f;
    gazeDirection.Normalize();

    // Use the gaze direction to update the forward direction.
    if (GvrControllerInput.Recentered || forceImmediate) {
      torsoDirection = gazeDirection;
    } else {
      float angularVelocity = GvrControllerInput.Gyro.magnitude;
      float gazeFilterStrength = Mathf.Clamp((angularVelocity - 0.2f) / 45.0f, 0.0f, 0.1f);
      torsoDirection = Vector3.Slerp(torsoDirection, gazeDirection, gazeFilterStrength);
    }

    // Calculate the torso rotation.
    torsoRotation = Quaternion.FromToRotation(Vector3.forward, torsoDirection);
  }

  protected virtual void UpdateNeckPosition() {
    if (isLockedToNeck) {
      // Returns the center of the eyes.
      // However, we actually want to lock to the center of the head.
      neckPosition = GvrVRHelpers.GetHeadPosition();

      // Find the approximate neck position by Applying an inverse neck model.
      // This transforms the head position to the center of the head and also accounts
      // for the head's rotation so that the motion feels more natural.
      neckPosition = ApplyInverseNeckModel(neckPosition);
    } else {
      neckPosition = Vector3.zero;
    }
  }

  protected virtual void ApplyArmModel() {
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

  /// Set the starting positions of the joints before they are transformed by the arm model.
  protected virtual void SetUntransformedJointPositions() {
    elbowPosition = Vector3.Scale(elbowRestPosition, handedMultiplier);
    wristPosition = Vector3.Scale(wristRestPosition, handedMultiplier);
    controllerPosition = Vector3.Scale(controllerRestPosition, handedMultiplier);
  }

  /// Calculate the extension ratio based on the angle of the controller along the x axis.
  protected virtual float CalculateExtensionRatio(float xAngle) {
    float normalizedAngle = (xAngle - MIN_EXTENSION_ANGLE) / (MAX_EXTENSION_ANGLE - MIN_EXTENSION_ANGLE);
    float extensionRatio = Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);
    return extensionRatio;
  }

  /// Offset the elbow by the extension offset.
  protected virtual void ApplyExtensionOffset(float extensionRatio) {
    Vector3 extensionOffset = Vector3.Scale(armExtensionOffset, handedMultiplier);
    elbowPosition += extensionOffset * extensionRatio;
  }

  /// Calculate the lerp rotation, which is used to control how much the rotation of the
  /// controller impacts each joint.
  protected virtual Quaternion CalculateLerpRotation(Quaternion xyRotation, float extensionRatio) {
    float totalAngle = Quaternion.Angle(xyRotation, Quaternion.identity);
    float lerpSuppresion = 1.0f - Mathf.Pow(totalAngle / 180.0f, 6.0f);
    float inverseElbowBendRatio = 1.0f - elbowBendRatio;
    float lerpValue = inverseElbowBendRatio + elbowBendRatio * extensionRatio * EXTENSION_WEIGHT;
    lerpValue *= lerpSuppresion;
    return Quaternion.Lerp(Quaternion.identity, xyRotation, lerpValue);
  }

  /// Determine the final joint rotations relative to the head.
  protected virtual void CalculateFinalJointRotations(Quaternion controllerOrientation, Quaternion xyRotation, Quaternion lerpRotation) {
    elbowRotation = torsoRotation * Quaternion.Inverse(lerpRotation) * xyRotation;
    wristRotation = elbowRotation * lerpRotation;
    controllerRotation = torsoRotation * controllerOrientation;
  }

  /// Apply the joint rotations to the positions of the joints to determine the final pose.
  protected virtual void ApplyRotationToJoints() {
    elbowPosition = neckPosition + torsoRotation * elbowPosition;
    wristPosition = elbowPosition + elbowRotation * wristPosition;
    controllerPosition = wristPosition + wristRotation * controllerPosition;
  }

  /// Transform the head position into an approximate neck position.
  protected virtual Vector3 ApplyInverseNeckModel(Vector3 headPosition) {
    Quaternion headRotation = GvrVRHelpers.GetHeadRotation();
    Vector3 rotatedNeckOffset =
      headRotation * NECK_OFFSET - NECK_OFFSET.y * Vector3.up;
    headPosition -= rotatedNeckOffset;

    return headPosition;
  }

  /// Controls the transparency of the controller to prevent the controller from clipping through
  /// the user's head. Also, controls the transparency of the tooltips so they are only visible
  /// when the controller is held up.
  protected virtual void UpdateTransparency() {
    Vector3 controllerForward = controllerRotation * Vector3.forward;
    Vector3 offsetControllerPosition = controllerPosition + (controllerForward * fadeControllerOffset);
    Vector3 controllerRelativeToHead = offsetControllerPosition - neckPosition;

    Vector3 headForward = GvrVRHelpers.GetHeadForward();
    float distanceToHeadForward = Vector3.Scale(controllerRelativeToHead, headForward).magnitude;
    Vector3 headRight = Vector3.Cross(headForward, Vector3.up);
    float distanceToHeadSide = Vector3.Scale(controllerRelativeToHead, headRight).magnitude;
    float distanceToHeadUp = Mathf.Abs(controllerRelativeToHead.y);

    bool shouldFadeController = distanceToHeadForward < fadeDistanceFromHeadForward
      && distanceToHeadUp < fadeDistanceFromHeadForward
      && distanceToHeadSide < fadeDistanceFromHeadSide;

    // Determine how vertical the controller is pointing.
    float animationDelta = DELTA_ALPHA * Time.unscaledDeltaTime;
    if (shouldFadeController) {
      preferredAlpha = Mathf.Max(0.0f, preferredAlpha - animationDelta);
    } else {
      preferredAlpha = Mathf.Min(1.0f, preferredAlpha + animationDelta);
    }

    float dot = Vector3.Dot(controllerRotation * Vector3.up, -controllerRelativeToHead.normalized);
    float minDot = (tooltipMaxAngleFromCamera - 90.0f) / -90.0f;
    float distToFace = Vector3.Distance(controllerRelativeToHead, Vector3.zero);
    if (shouldFadeController
      || distToFace > tooltipMinDistanceFromFace
      || dot < minDot) {
      tooltipAlphaValue = Mathf.Max(0.0f, tooltipAlphaValue - animationDelta);
    } else {
      tooltipAlphaValue = Mathf.Min(1.0f, tooltipAlphaValue + animationDelta);
    }
  }

  /// Get the controller's orientation.
  protected void GetControllerRotation(out Quaternion rotation, out Quaternion xyRotation, out float xAngle) {
    // Find the controller's orientation relative to the player.
    rotation = GvrControllerInput.Orientation;
    rotation = Quaternion.Inverse(torsoRotation) * rotation;

    // Extract just the x rotation angle.
    Vector3 controllerForward = rotation * Vector3.forward;
    xAngle = 90.0f - Vector3.Angle(controllerForward, Vector3.up);

    // Remove the z rotation from the controller.
    xyRotation = Quaternion.FromToRotation(Vector3.forward, controllerForward);
  }

#if UNITY_EDITOR
  protected virtual void OnDrawGizmosSelected() {
    if (!enabled) {
      return;
    }

    if (transform.parent == null) {
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

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
using UnityEngine.VR;
using System.Collections;

/// The GvrArmModel is a standard interface to interact with a scene with the controller.
/// It is responsible for:
/// -  Determining the orientation and location of the controller.
/// -  Predict the location of the shoulder, elbow, wrist, and pointer.
public class GvrArmModel : GvrBaseArmModel{
  /// Represents when gaze-following behavior should occur.
  public enum GazeBehavior {
    Never,        /// The shoulder will never follow the gaze.
    DuringMotion, /// The shoulder will follow the gaze during controller motion.
    Always        /// The shoulder will always follow the gaze.
  }

  /// Height of the elbow  (m).
  [Range(0.0f, 0.2f)]
  public float addedElbowHeight = 0.0f;

  /// Depth of the elbow  (m).
  [Range(0.0f, 0.2f)]
  public float addedElbowDepth = 0.0f;

  /// The Downward tilt or pitch of the laser pointer relative to the controller (degrees).
  [Range(0.0f, 30.0f)]
  public float pointerTiltAngle = 15.0f;

  /// Controller distance from the face after which the controller disappears (meters).
  [Range(0.0f, 0.4f)]
  public float fadeDistanceFromFace = 0.32f;

  /// Controller distance from face after which the tooltips appear (meters).
  [Range(0.4f, 0.6f)]
  public float tooltipMinDistanceFromFace = 0.45f;

  /// When the angle (degrees) between the controller and the head is larger than
  /// this value, the tooltips disappear.
  /// If the value is 180, then the tooltips are always shown.
  /// If the value is 90, the tooltips are only shown when they are facing the camera.
  [Range(0, 180)]
  public int tooltipMaxAngleFromCamera = 80;

  /// Determines if the shoulder should follow the gaze
  public GazeBehavior followGaze = GazeBehavior.DuringMotion;

  /// Determines if the accelerometer should be used.
  public bool useAccelerometer = false;

  [Tooltip("If true, the root of the pose is locked to the local position of the player's head.")]
  public bool isLockedToHead = false;

  /// Vector to represent the controller's location relative to
  /// the user's head position.
  public override Vector3 ControllerPositionFromHead {
    get {
      return wristPosition;
    }
  }

  /// Quaternion to represent the controller's rotation relative to
  /// the user's head position.
  public override Quaternion ControllerRotationFromHead {
    get {
      return wristRotation;
    }
  }

  /// Vector to represent the pointer's location relative to
  /// the controller.
  public override Vector3 PointerPositionFromController {
    get {
      return POINTER_OFFSET;
    }
  }

  /// Quaternion to represent the pointer's rotation relative to
  /// the controller.
  public override Quaternion PointerRotationFromController {
    get {
      return Quaternion.AngleAxis(pointerTiltAngle, Vector3.right);
    }
  }

  /// The suggested rendering alpha value of the controller.
  /// This is to prevent the controller from intersecting the face.
  /// The range is always 0 - 1 but can be scaled by individual
  /// objects when using the GvrBaseControllerVisual script.
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

  /// Vector to represent the elbow's location.
  /// NOTE: This is in meatspace coordinates.
  public Vector3 ElbowPosition {
    get {
      return elbowPosition;
    }
  }

  /// Quaternion to represent the elbow's rotation.
  /// NOTE: This is in meatspace coordinates.
  public Quaternion ElbowRotation {
    get {
      return elbowRotation;
    }
  }

  /// Quaternion to represent the shoulder's rotation.
  /// NOTE: This is in meatspace coordinates.
  public Quaternion ShoulderRotation {
    get {
      return shoulderRotation;
    }
  }

  /// Vector to represent the head's location.
  /// NOTE: This is in meatspace coordinates.
  public Vector3 HeadPosition {
    get {
      return headPosition;
    }
  }

  /// Offset of the elbow due to the accelerometer
  private Vector3 elbowOffset;

  /// Forward direction of the arm model.
  private Vector3 torsoDirection;

  /// Filtered velocity of the controller.
  private Vector3 filteredVelocity;

  /// Filtered acceleration of the controller.
  private Vector3 filteredAccel;

  /// Used to calibrate the ambient gravitational force.
  private Vector3 zeroAccel;

  /// Indicates if this is the first frame to receive new IMU measurements.
  private bool firstUpdate;

  /// Multiplier for handedness such that 1 = Right, 0 = Center, -1 = left.
  private Vector3 handedMultiplier;

  /// Vector to represent the wrist's location.
  /// NOTE: This is in meatspace coordinates.
  private Vector3 wristPosition;

  /// Quaternion to represent the wrist's rotation.
  /// NOTE: This is in meatspace coordinates.
  private Quaternion wristRotation;

  /// Vector to represent the elbow's location.
  /// NOTE: This is in meatspace coordinates.
  private Vector3 elbowPosition;

  /// Quaternion to represent the elbow's rotation.
  /// NOTE: This is in meatspace coordinates.
  private Quaternion elbowRotation;

  /// Quaternion to represent the shoulder's rotation.
  /// NOTE: This is in meatspace coordinates.
  private Quaternion shoulderRotation;

  /// Vector to represent the head's location.
  /// NOTE: This is in meatspace coordinates.
  private Vector3 headPosition;

  /// Backing variable for the PreferredAlpha property.
  private float preferredAlpha;

  /// Backing variable for the TooltipAlphaValue property.
  private float tooltipAlphaValue;

  /// Increases elbow bending as the controller moves up (unitless).
  private const float EXTENSION_WEIGHT = 0.4f;

  /// The range of movement from the elbow position due to accelerometer (meters).
  private static readonly Vector3 ELBOW_MIN_RANGE = new Vector3(-0.05f, -0.1f, 0.0f);
  private static readonly Vector3 ELBOW_MAX_RANGE = new Vector3(0.05f, 0.1f, 0.2f);

  /// Offset of the laser pointer origin relative to the wrist (meters)
  private static readonly Vector3 POINTER_OFFSET = new Vector3(0.0f, -0.009f, 0.099f);

  /// Rest position parameters for arm model (meters).
  private static readonly Vector3 ELBOW_POSITION = new Vector3(0.195f, -0.5f, -0.075f);
  private static readonly Vector3 WRIST_POSITION = new Vector3(0.0f, 0.0f, 0.25f);
  private static readonly Vector3 ARM_EXTENSION_OFFSET = new Vector3(-0.13f, 0.14f, 0.08f);

  /// Neck offset used to apply the inverse neck model when locked to the head.
  private static readonly Vector3 NECK_OFFSET = new Vector3(0, 0.075f, 0.08f);

  /// Strength of the acceleration filter (unitless).
  private const float GRAVITY_CALIB_STRENGTH = 0.999f;

  /// Strength of the velocity suppression (unitless).
  private const float VELOCITY_FILTER_SUPPRESS = 0.99f;

  /// Strength of the velocity suppression during low acceleration (unitless).
  private const float LOW_ACCEL_VELOCITY_SUPPRESS = 0.9f;

  /// Strength of the acceleration suppression during low velocity (unitless).
  private const float LOW_VELOCITY_ACCEL_SUPPRESS = 0.5f;

  /// The minimum allowable accelerometer reading before zeroing (m/s^2).
  private const float MIN_ACCEL = 1.0f;

  /// The expected force of gravity (m/s^2).
  private const float GRAVITY_FORCE = 9.807f;

  /// Amount of normalized alpha transparency to change per second.
  private const float DELTA_ALPHA = 4.0f;

  /// Angle ranges the for arm extension offset to start and end (degrees).
  private const float MIN_EXTENSION_ANGLE = 7.0f;
  private const float MAX_EXTENSION_ANGLE = 60.0f;

  void OnEnable() {
    UpdateHandedness();

    // Register the controller update listener.
    GvrControllerInput.OnControllerInputUpdated += OnControllerInputUpdated;

    // Reset other relevant state.
    firstUpdate = true;
    elbowOffset = Vector3.zero;
    zeroAccel.Set(0, GRAVITY_FORCE, 0);
  }

  void OnDisable() {
    GvrControllerInput.OnControllerInputUpdated -= OnControllerInputUpdated;
  }

  private void OnControllerInputUpdated() {
    if (GvrControllerInput.Recentered) {
      ResetState();
    }

    UpdateHandedness();
    UpdateTorsoDirection();
    if (GvrControllerInput.State == GvrConnectionState.Connected) {
      UpdateFromController();
    } else {
      ResetState();
    }
    if (useAccelerometer) {
      UpdateVelocity();
      TransformElbow();
    } else {
      elbowOffset = Vector3.zero;
    }
    ApplyArmModel();
    UpdateTransparency();

    firstUpdate = false;
  }

  private void UpdateHandedness() {
    // Update user handedness if the setting has changed
    GvrSettings.UserPrefsHandedness handedness = GvrSettings.Handedness;

    // Determine handedness multiplier.
    handedMultiplier.Set(0, 1, 1);
    if (handedness == GvrSettings.UserPrefsHandedness.Right) {
      handedMultiplier.x = 1.0f;
    } else if (handedness == GvrSettings.UserPrefsHandedness.Left) {
      handedMultiplier.x = -1.0f;
    }

    // Place the shoulder in anatomical positions based on the height and handedness.
    shoulderRotation = Quaternion.identity;
  }

  private Vector3 GetHeadForward() {
    return GetHeadRotation() * Vector3.forward;
  }

  private Quaternion GetHeadRotation() {
#if UNITY_EDITOR
    return GvrEditorEmulator.HeadRotation;
#else
    return InputTracking.GetLocalRotation(VRNode.Head);
#endif // UNITY_EDITOR
  }

  private Vector3 GetHeadPosition() {
#if UNITY_EDITOR
    return GvrEditorEmulator.HeadPosition;
#else
    return InputTracking.GetLocalPosition(VRNode.Head);
#endif // UNITY_EDITOR
  }

  private void UpdateTorsoDirection() {
    // Ignore updates here if requested.
    if (followGaze == GazeBehavior.Never) {
      return;
    }

    // Determine the gaze direction horizontally.
    Vector3 gazeDirection = GetHeadForward();
    gazeDirection.y = 0.0f;
    gazeDirection.Normalize();

    // Use the gaze direction to update the forward direction.
    if (followGaze == GazeBehavior.Always || firstUpdate) {
      torsoDirection = gazeDirection;
    } else if (followGaze == GazeBehavior.DuringMotion) {
      float angularVelocity = GvrControllerInput.Gyro.magnitude;
      float gazeFilterStrength = Mathf.Clamp((angularVelocity - 0.2f) / 45.0f, 0.0f, 0.1f);
      torsoDirection = Vector3.Slerp(torsoDirection, gazeDirection, gazeFilterStrength);
    }

    // Rotate the fixed joints.
    Quaternion gazeRotation = Quaternion.FromToRotation(Vector3.forward, torsoDirection);
    shoulderRotation = gazeRotation;
  }

  private void UpdateFromController() {
    // Get the orientation-adjusted acceleration.
    Vector3 accel = GvrControllerInput.Orientation * GvrControllerInput.Accel;

    // Very slowly calibrate gravity force out of acceleration.
    zeroAccel = zeroAccel * GRAVITY_CALIB_STRENGTH + accel * (1.0f - GRAVITY_CALIB_STRENGTH);
    filteredAccel = accel - zeroAccel;

    // If no tracking history, reset the velocity.
    if (firstUpdate) {
      filteredVelocity = Vector3.zero;
    }

    // IMPORTANT: The accelerometer is not reliable at these low magnitudes
    // so ignore it to prevent drift.
    if (filteredAccel.magnitude < MIN_ACCEL) {
      // Suppress the acceleration.
      filteredAccel = Vector3.zero;
      filteredVelocity *= LOW_ACCEL_VELOCITY_SUPPRESS;
    } else {
      // If the velocity is decreasing, prevent snap-back by reducing deceleration.
      Vector3 newVelocity = filteredVelocity + filteredAccel * Time.deltaTime;
      if (newVelocity.sqrMagnitude < filteredVelocity.sqrMagnitude) {
        filteredAccel *= LOW_VELOCITY_ACCEL_SUPPRESS;
      }
    }
  }

  private void UpdateVelocity() {
    // Update the filtered velocity.
    filteredVelocity += filteredAccel * Time.deltaTime;
    filteredVelocity *= VELOCITY_FILTER_SUPPRESS;
  }

  private void ResetState() {
    // We've lost contact, quickly reset the state.
    filteredVelocity *= 0.5f;
    filteredAccel *= 0.5f;
    firstUpdate = true;
  }

  private void TransformElbow() {
    // Apply the filtered velocity to update the elbow offset position.
    if (useAccelerometer) {
      elbowOffset += filteredVelocity * Time.deltaTime;
      elbowOffset.x = Mathf.Clamp(elbowOffset.x, ELBOW_MIN_RANGE.x, ELBOW_MAX_RANGE.x);
      elbowOffset.y = Mathf.Clamp(elbowOffset.y, ELBOW_MIN_RANGE.y, ELBOW_MAX_RANGE.y);
      elbowOffset.z = Mathf.Clamp(elbowOffset.z, ELBOW_MIN_RANGE.z, ELBOW_MAX_RANGE.z);
    }
  }

  private void ApplyArmModel() {
    // Find the controller's orientation relative to the player.
    Quaternion controllerOrientation = GvrControllerInput.Orientation;
    controllerOrientation = Quaternion.Inverse(shoulderRotation) * controllerOrientation;

    // Get the relative positions of the joints.
    elbowPosition = ELBOW_POSITION + new Vector3(0.0f, addedElbowHeight, addedElbowDepth);
    elbowPosition = Vector3.Scale(elbowPosition, handedMultiplier) + elbowOffset;
    wristPosition = Vector3.Scale(WRIST_POSITION, handedMultiplier);
    Vector3 armExtensionOffset = Vector3.Scale(ARM_EXTENSION_OFFSET, handedMultiplier);

    // Extract just the x rotation angle.
    Vector3 controllerForward = controllerOrientation * Vector3.forward;
    float xAngle = 90.0f - Vector3.Angle(controllerForward, Vector3.up);

    // Remove the z rotation from the controller.
    Quaternion xyRotation = Quaternion.FromToRotation(Vector3.forward, controllerForward);

    // Offset the elbow by the extension.
    float normalizedAngle = (xAngle - MIN_EXTENSION_ANGLE) / (MAX_EXTENSION_ANGLE - MIN_EXTENSION_ANGLE);
    float extensionRatio = Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);
    if (!useAccelerometer) {
      elbowPosition += armExtensionOffset * extensionRatio;
    }

    // Calculate the lerp interpolation factor.
    float totalAngle = Quaternion.Angle(xyRotation, Quaternion.identity);
    float lerpSuppresion = 1.0f - Mathf.Pow(totalAngle / 180.0f, 6);
    float lerpValue = lerpSuppresion * (0.4f + 0.6f * extensionRatio * EXTENSION_WEIGHT);

    // Apply the absolute rotations to the joints.
    Quaternion lerpRotation = Quaternion.Lerp(Quaternion.identity, xyRotation, lerpValue);
    elbowRotation = shoulderRotation * Quaternion.Inverse(lerpRotation) * controllerOrientation;
    wristRotation = shoulderRotation * controllerOrientation;

    // Get the head position.
    if (isLockedToHead) {
      // Returns the center of the eyes.
      // However, we actually want to lock to the center of the head.
      headPosition = GetHeadPosition();

      // Apply inverse neck model to both transform the head position to
      // the center of the head and account for the head's rotation
      // so that the motion feels more natural.
      headPosition = ApplyInverseNeckModel(headPosition);
    } else {
      headPosition = Vector3.zero;
    }

    // Determine the relative positions.
    elbowPosition = headPosition + shoulderRotation * elbowPosition;
    wristPosition = elbowPosition + elbowRotation * wristPosition;
  }

  private Vector3 ApplyInverseNeckModel(Vector3 headPosition) {
    Quaternion headRotation = GetHeadRotation();
    Vector3 rotatedNeckOffset =
      headRotation * NECK_OFFSET - NECK_OFFSET.y * Vector3.up;
    headPosition -= rotatedNeckOffset;

    return headPosition;
  }

  private void UpdateTransparency() {
    Vector3 wristRelativeToHead = wristPosition - headPosition;

    // Determine how vertical the controller is pointing.
    float animationDelta = DELTA_ALPHA * Time.deltaTime;
    float distToFace = Vector3.Distance(wristRelativeToHead, Vector3.zero);
    if (distToFace < fadeDistanceFromFace) {
      preferredAlpha = Mathf.Max(0.0f, preferredAlpha - animationDelta);
    } else {
      preferredAlpha = Mathf.Min(1.0f, preferredAlpha + animationDelta);
    }

    float dot = Vector3.Dot(wristRotation * Vector3.up, -wristRelativeToHead.normalized);
    float minDot = (tooltipMaxAngleFromCamera - 90.0f) / -90.0f;
    if (distToFace < fadeDistanceFromFace
        || distToFace > tooltipMinDistanceFromFace
        || dot < minDot) {
      tooltipAlphaValue = Mathf.Max(0.0f, tooltipAlphaValue - animationDelta);
    } else {
      tooltipAlphaValue = Mathf.Min(1.0f, tooltipAlphaValue + animationDelta);
    }
  }
}

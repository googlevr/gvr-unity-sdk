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
  [Serializable]
  public struct DoesReticleFaceCameraData {
    public bool alongXAxis;
    public bool alongYAxis;
    public bool alongZAxis;

    public bool IsAnyAxisOff {
      get {
        return !alongXAxis || !alongYAxis || !alongZAxis;
      }
    }

    public DoesReticleFaceCameraData(bool startEnabled) {
      alongXAxis = startEnabled;
      alongYAxis = startEnabled;
      alongZAxis = startEnabled;
    }
  }

  /// Final size of the reticle in meters when it is 1 meter from the camera.
  /// The reticle will be scaled based on the size of the mesh so that it's size
  /// matches this size.
  public const float RETICLE_SIZE_METERS = 0.1f;

  [Tooltip("Color of the laser pointer including alpha transparency")]
  public Color laserColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

  [Tooltip("Color of the laser pointer including alpha transparency")]
  public Color laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 0.0f);

  [Tooltip("Maximum distance of the laser (meters).")]
  [Range(0.0f, 10.0f)]
  public float maxLaserDistance = 0.75f;

  /// When using RayCastMode's other than Direct, it is possible
  /// that the currentPosition will not be directly in front of the laser.
  /// If this is set to true, then the visual will rotate to face the currentPosition.
  [Tooltip("Determines if the visual is allowed to rotate toward the final reticle position.")]
  public bool allowRotation = true;

  [Tooltip("References the reticle that will be positioned.")]
  [SerializeField]
  private Transform reticle;

  [Tooltip("Determines if the reticle will always face the camera and along what axes.")]
  public DoesReticleFaceCameraData doesReticleFaceCamera = new DoesReticleFaceCameraData(true);

  /// If allowRotation is true, then this is used to rotate the controller
  /// to face the reticle as well.
  [Tooltip("References to the controller visual.")]
  public Transform controller;

  [Tooltip("The rate that the currentPosition changes.")]
  public float lerpSpeed = 20.0f;

  /// If the targetPosition is greater than this threshold, then
  /// the position changes immediately instead of lerping.
  [Tooltip("Determines if lerping is used.")]
  public float lerpThreshold = 1.5f;

  /// Sorting order to use for the reticle's renderer.
  /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
  [Range(-32767, 32767)]
  public int reticleSortingOrder = 0;

  private const float LERP_CLAMP_THRESHOLD = 0.02f;

  public GvrBaseArmModel ArmModel { get; set; }

  public Transform Reticle {
    get {
      return reticle;
    }
    set {
      reticle = value;
      ReticleMeshSizeMeters = 1.0f;
      ReticleMeshSizeRatio = 1.0f;

      if (reticle != null) {
        MeshFilter meshFilter = reticle.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null) {
          ReticleMeshSizeMeters = meshFilter.mesh.bounds.size.x;
          if (ReticleMeshSizeMeters != 0.0f) {
            ReticleMeshSizeRatio = 1.0f / ReticleMeshSizeMeters;
          }
        }
      }
    }
  }

  /// The size of the reticle's mesh in meters.
  public float ReticleMeshSizeMeters { get; private set; }

  /// The ratio of the reticleMeshSizeMeters to 1 meter.
  /// If reticleMeshSizeMeters is 10, then reticleMeshSizeRatio is 0.1.
  public float ReticleMeshSizeRatio { get; private set; }

  /// Reference to the laser's line renderer.
  public LineRenderer Laser { get; private set; }

  /// Optional delegate for customizing how the currentPosition is calculated based on the distance.
  /// If not set, the currentPosition is determined based on the distance multiplied by the forward
  /// direction of the transform added to the position of the transform.
  public delegate Vector3 GetPointForDistanceDelegate(float distance);
  public GetPointForDistanceDelegate GetPointForDistanceFunction { get; set; }

  private float targetDistance;
  private float currentDistance;
  private Vector3 currentPosition;
  private Vector3 currentLocalPosition;
  private Quaternion currentLocalRotation;

  /// Set the distance of the laser.
  /// Clamps the distance of the laser and reticle.
  ///
  /// **distance** target distance from the pointer to draw the visual at.
  /// **immediate** If true, the distance is changed immediately. Otherwise, it will lerp.
  public void SetDistance(float distance, bool immediate=false) {
    targetDistance = distance;
    if (immediate) {
      currentDistance = targetDistance;
    }

    if (targetDistance > lerpThreshold) {
      currentDistance = targetDistance;
    }
  }

  void Awake() {
    Laser = GetComponent<LineRenderer>();

    // Will calculate reticleMeshSizeMeters and reticleMeshSizeRatio
    Reticle = reticle;

    // Set the reticle sorting order.
    if (reticle != null) {
      Renderer reticleRenderer = reticle.GetComponent<Renderer>();
      Assert.IsNotNull(reticleRenderer);
      reticleRenderer.sortingOrder = reticleSortingOrder;
    }
  }

  void LateUpdate() {
    UpdateCurrentPosition();
    UpdateControllerOrientation();
    UpdateReticlePosition();
    UpdateLaserEndPoint();
    UpdateLaserAlpha();
  }

  void OnWillRenderObject() {
    Camera camera = Camera.current;
    UpdateReticleSize(camera);
    UpdateReticleOrientation(camera);
  }

  private void UpdateCurrentPosition() {
    if (currentDistance != targetDistance) {
      float speed = GetSpeed();
      currentDistance = Mathf.Lerp(currentDistance, targetDistance, speed);
      float diff =  Mathf.Abs(targetDistance - currentDistance);
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

    if (allowRotation) {
      currentLocalRotation =
        Quaternion.FromToRotation(Vector3.forward, currentLocalPosition);
    } else {
      currentLocalRotation = Quaternion.identity;
    }
  }

  private void UpdateControllerOrientation() {
    if (controller == null) {
      return;
    }

    controller.localRotation = currentLocalRotation;
  }

  private void UpdateReticlePosition() {
    if (reticle == null) {
      return;
    }

    reticle.position = currentPosition;
  }

  private void UpdateLaserEndPoint() {
    if (Laser == null) {
      return;
    }

    Vector3 laserStartPoint = Vector3.zero;
    Vector3 laserEndPoint;

    if (allowRotation) {
      if (controller != null) {
        Vector3 worldPosition = transform.position;
        Vector3 rotatedPosition = controller.InverseTransformPoint(worldPosition);
        rotatedPosition = currentLocalRotation * rotatedPosition;
        laserStartPoint = controller.TransformPoint(rotatedPosition);
        laserStartPoint = transform.InverseTransformPoint(laserStartPoint);
      }

      laserEndPoint = Vector3.ClampMagnitude(currentLocalPosition, maxLaserDistance);
    } else {
      Vector3 projected = Vector3.Project(currentLocalPosition, Vector3.forward);
      laserEndPoint = Vector3.ClampMagnitude(projected, maxLaserDistance);
    }

    Laser.useWorldSpace = false;
    Laser.SetPosition(0, laserStartPoint);
    Laser.SetPosition(1, laserEndPoint);
  }

  private void UpdateLaserAlpha() {
    float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;
#if UNITY_5_6_OR_NEWER
    Laser.startColor = Color.Lerp(Color.clear, laserColor, alpha);
    Laser.endColor = laserColorEnd;
#else
    Laser.SetColors(Color.Lerp(Color.clear, laserColor, alpha), laserColorEnd);
#endif  // UNITY_5_6_OR_NEWER
  }

  private void UpdateReticleSize(Camera camera) {
    if (reticle == null) {
      return;
    }

    if (camera == null) {
      return;
    }

    float reticleDistanceFromCamera = (reticle.position - camera.transform.position).magnitude;
    float scale = RETICLE_SIZE_METERS * ReticleMeshSizeRatio * reticleDistanceFromCamera;
    reticle.localScale = new Vector3(scale, scale, scale);
  }

  private void UpdateReticleOrientation(Camera camera) {
    if (reticle == null) {
      return;
    }

    if (camera == null) {
      return;
    }

    Vector3 direction = reticle.position - camera.transform.position;
    reticle.rotation = Quaternion.LookRotation(direction, Vector3.up);

    if (doesReticleFaceCamera.IsAnyAxisOff) {
      Vector3 euler = reticle.localEulerAngles;
      if (!doesReticleFaceCamera.alongXAxis) {
        euler.x = 0.0f;
      }

      if (!doesReticleFaceCamera.alongYAxis) {
        euler.y = 0.0f;
      }

      if (!doesReticleFaceCamera.alongZAxis) {
        euler.z = 0.0f;
      }

      reticle.localEulerAngles = euler;
    }
  }

  private float GetSpeed() {
    return lerpSpeed > 0.0f ? lerpSpeed * Time.deltaTime : 1.0f;
  }

  void OnValidate() {
    // If the "reticle" serialized field is changed while the application is playing
    // by using the inspector in the editor, then we need to call the Reticle setter to
    // ensure that ReticleMeshSizeMeters and ReticleMeshSizeRatio are updated.
    // Outside of the editor, this can't happen because only the Reticle setter is publicly
    // accessible.  The Laser null check excludes cases where OnValidate is invoked before Awake.
    if (Application.isPlaying && Laser != null) {
      Reticle = reticle;
    }
  }
}

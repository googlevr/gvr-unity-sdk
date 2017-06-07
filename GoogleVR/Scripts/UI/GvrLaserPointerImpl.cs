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

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using UnityEngine.EventSystems;

/// Implementation of GvrBasePointer for a laser pointer visual.
/// This script should be attached to the controller object.
/// The laser visual is important to help users locate their cursor
/// when its not directly in their field of view.
public class GvrLaserPointerImpl : GvrBasePointer {
  /// Small offset to prevent z-fighting of the reticle (meters).
  private const float Z_OFFSET_EPSILON = 0.1f;

  /// Final size of the reticle in meters when it is 1 meter from the camera.
  /// The reticle will be scaled based on the size of the mesh so that it's size
  /// matches this size.
  private const float RETICLE_SIZE_METERS = 0.1f;

  /// The percentage of the reticle mesh that shows the reticle.
  /// The rest of the reticle mesh is transparent.
  private const float RETICLE_VISUAL_RATIO = 0.1f;

  public Camera MainCamera { private get; set; }

  public Color LaserColor { private get; set; }

  public LineRenderer LaserLineRenderer { get; set; }

  public float MaxLaserDistance { private get; set; }

  public float MaxReticleDistance { private get; set; }

  private GameObject reticle;
  public GameObject Reticle {
    get {
      return reticle;
    }
    set {
      reticle = value;
      reticleMeshSizeMeters = 1.0f;
      reticleMeshSizeRatio = 1.0f;

      if (reticle != null) {
        MeshFilter meshFilter = reticle.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null) {
          reticleMeshSizeMeters = meshFilter.mesh.bounds.size.x;
          if (reticleMeshSizeMeters != 0.0f) {
            reticleMeshSizeRatio = 1.0f / reticleMeshSizeMeters;
          }
        }
      }
    }
  }

  // Properties exposed for testing purposes.
  public Vector3 PointerIntersection { get; private set; }

  public bool IsPointerIntersecting { get; private set; }

  public Ray PointerIntersectionRay { get; private set; }

  // The size of the reticle's mesh in meters.
  private float reticleMeshSizeMeters;

  // The ratio of the reticleMeshSizeMeters to 1 meter.
  // If reticleMeshSizeMeters is 10, then reticleMeshSizeRatio is 0.1.
  private float reticleMeshSizeRatio;

  private Vector3 lineEndPoint = Vector3.zero;
  public override Vector3 LineEndPoint { get { return lineEndPoint; } }

  public override float MaxPointerDistance {
    get {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
      return MaxReticleDistance;
#else
      return 0;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    }
  }

  public GvrLaserPointerImpl() {
    MaxLaserDistance = 0.75f;
    MaxReticleDistance = 2.5f;
  }

#if !(UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR))
  public override void OnStart() {
    // Don't call base.Start() so that this pointer isn't activated when
    // the editor doesn't have UNITY_HAS_GOOGLE_VR.
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

  public override void OnInputModuleEnabled() {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    if (LaserLineRenderer != null) {
      LaserLineRenderer.enabled = true;
    }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnInputModuleDisabled() {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    if (LaserLineRenderer != null) {
      LaserLineRenderer.enabled = false;
    }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerEnter(RaycastResult rayastResult, Ray ray,
    bool isInteractive) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    PointerIntersection = rayastResult.worldPosition;
    PointerIntersectionRay = ray;
    IsPointerIntersecting = true;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerHover(RaycastResult rayastResult, Ray ray,
    bool isInteractive) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    PointerIntersection = rayastResult.worldPosition;
    PointerIntersectionRay = ray;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerExit(GameObject previousObject) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    PointerIntersection = Vector3.zero;
    PointerIntersectionRay = new Ray();
    IsPointerIntersecting = false;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerClickDown() {
    // User has performed a click on the target.  In a derived class, you could
    // handle visual feedback such as laser or cursor color changes here.
  }

  public override void OnPointerClickUp() {
    // User has released a click from the target.  In a derived class, you could
    // handle visual feedback such as laser or cursor color changes here.
  }

  public override void GetPointerRadius(out float enterRadius, out float exitRadius) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    if (Reticle != null) {
      float reticleScale = Reticle.transform.localScale.x;

      // Fixed size for enter radius to avoid flickering.
      // This will cause some slight variability based on the distance of the object
      // from the camera, and is optimized for the average case.
      enterRadius = RETICLE_SIZE_METERS * 0.5f * RETICLE_VISUAL_RATIO;

      // Dynamic size for exit radius.
      // Always correct because we know the intersection point of the object and can
      // therefore use the correct radius based on the object's distance from the camera.
      exitRadius = reticleScale * reticleMeshSizeMeters * RETICLE_VISUAL_RATIO;
    } else {
      enterRadius = 0.0f;
      exitRadius = 0.0f;
    }
#else
    enterRadius = 0.0f;
    exitRadius = 0.0f;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  public void OnUpdate() {
    // Set the reticle's position and scale
    if (Reticle != null) {
      if (IsPointerIntersecting) {
        Vector3 controllerDiff = PointerTransform.position - PointerIntersectionRay.origin;
        Vector3 proj = Vector3.Project(controllerDiff, PointerIntersectionRay.direction);
        Vector3 controllerAlongRay = PointerIntersectionRay.origin + proj;
        Vector3 difference = PointerIntersection - controllerAlongRay;
        Vector3 clampedDifference = Vector3.ClampMagnitude(difference, MaxReticleDistance);
        Vector3 clampedPosition = controllerAlongRay + clampedDifference;
        Reticle.transform.position = clampedPosition;
      } else {
        Reticle.transform.localPosition = new Vector3(0, 0, MaxReticleDistance);
      }

      float reticleDistanceFromCamera =
        (Reticle.transform.position - MainCamera.transform.position).magnitude;
      float scale = RETICLE_SIZE_METERS * reticleMeshSizeRatio * reticleDistanceFromCamera;
      Reticle.transform.localScale = new Vector3(scale, scale, scale);
    }

    if (LaserLineRenderer == null) {
      Debug.LogWarning("Line renderer is null, returning");
      return;
    }

    // Set the line renderer positions.
    if (IsPointerIntersecting) {
      Vector3 laserDiff = PointerIntersection - base.PointerTransform.position;
      float intersectionDistance = laserDiff.magnitude;
      Vector3 direction = laserDiff.normalized;
      float laserDistance = intersectionDistance > MaxLaserDistance ? MaxLaserDistance : intersectionDistance;
      lineEndPoint = base.PointerTransform.position + (direction * laserDistance);
    } else {
      lineEndPoint = base.PointerTransform.position + (base.PointerTransform.forward * MaxLaserDistance);
    }
    LaserLineRenderer.SetPosition(0,base.PointerTransform.position);
    LaserLineRenderer.SetPosition(1,lineEndPoint);

    // Adjust transparency
    float alpha = GvrArmModel.Instance.preferredAlpha;
#if UNITY_5_6_OR_NEWER
    LaserLineRenderer.startColor = Color.Lerp(Color.clear, LaserColor, alpha);
    LaserLineRenderer.endColor = Color.clear;
#else
    LaserLineRenderer.SetColors(Color.Lerp(Color.clear, LaserColor, alpha), Color.clear);
#endif  // UNITY_5_6_OR_NEWER
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

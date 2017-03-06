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

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using System.Collections;

/// Implementation of GvrBasePointer for a laser pointer visual.
/// This script should be attached to the controller object.
/// The laser visual is important to help users locate their cursor
/// when its not directly in their field of view.
public class GvrLaserPointerImpl : GvrBasePointer {
  /// Small offset to prevent z-fighting of the reticle (meters).
  private const float Z_OFFSET_EPSILON = 0.1f;

  /// Size of the reticle in meters as seen from 1 meter.
  private const float RETICLE_SIZE = 0.01f;

  public Camera MainCamera { private get; set; }

  public Color LaserColor { private get; set; }

  public LineRenderer LaserLineRenderer { get; set; }

  public GameObject Reticle { get; set; }

  public float MaxLaserDistance { private get; set; }

  public float MaxReticleDistance { private get; set; }

  // Properties exposed for testing purposes.
  public Vector3 PointerIntersection { get; private set; }

  public bool IsPointerIntersecting { get; private set; }

  public Ray PointerIntersectionRay { get; private set; }

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

  public override void OnPointerEnter(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    PointerIntersection = intersectionPosition;
    PointerIntersectionRay = intersectionRay;
    IsPointerIntersecting = true;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerHover(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    PointerIntersection = intersectionPosition;
    PointerIntersectionRay = intersectionRay;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerExit(GameObject targetObject) {
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
      enterRadius = RETICLE_SIZE * 0.5f;

      // Dynamic size for exit radius.
      // Always correct because we know the intersection point of the object and can
      // therefore use the correct radius based on the object's distance from the camera.
      exitRadius = reticleScale;
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
        Vector3 difference = PointerIntersection - PointerIntersectionRay.origin;
        Vector3 clampedDifference = Vector3.ClampMagnitude(difference, MaxReticleDistance);
        Vector3 clampedPosition = PointerIntersectionRay.origin + clampedDifference;
        Reticle.transform.position = clampedPosition;
      } else {
        Reticle.transform.localPosition = new Vector3(0, 0, MaxReticleDistance);
      }

      float reticleDistanceFromCamera =
        (Reticle.transform.position - MainCamera.transform.position).magnitude;
      float scale = RETICLE_SIZE * reticleDistanceFromCamera;
      Reticle.transform.localScale = new Vector3(scale, scale, scale);
    }

    if (LaserLineRenderer == null) {
      Debug.LogWarning("Line renderer is null, returning");
      return;
    }

    // Set the line renderer positions.
    Vector3 lineEndPoint;
    if (IsPointerIntersecting) {
      Vector3 laserDiff = PointerIntersection - base.PointerTransform.position;
      float intersectionDistance = laserDiff.magnitude;
      Vector3 direction = laserDiff.normalized;
      float laserDistance = intersectionDistance > MaxLaserDistance ? MaxLaserDistance : intersectionDistance;
      lineEndPoint = base.PointerTransform.position + (direction * laserDistance);
    } else {
      lineEndPoint = base.PointerTransform.position + (base.PointerTransform.forward * MaxLaserDistance);
    }
    LaserLineRenderer.SetPositions(new Vector3[] {base.PointerTransform.position, lineEndPoint});

    // Adjust transparency
    float alpha = GvrControllerVisual.AlphaValue;
    LaserLineRenderer.SetColors(Color.Lerp(Color.clear, LaserColor, alpha), Color.clear);
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

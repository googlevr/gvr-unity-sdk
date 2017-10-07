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
[RequireComponent(typeof(GvrLaserVisual))]
public class GvrLaserPointer : GvrBasePointer {
  [Tooltip("Distance from the pointer that raycast hits will be detected.")]
  public float maxPointerDistance = 20.0f;

  [Tooltip("Distance from the pointer that the reticle will be drawn at when hitting nothing.")]
  public float defaultReticleDistance = 20.0f;

  [Tooltip("By default, the length of the laser is used as the CameraRayIntersectionDistance. " +
    "Set this field to a non-zero value to override it.")]
  public float overrideCameraRayIntersectionDistance;

  /// The percentage of the reticle mesh that shows the reticle.
  /// The rest of the reticle mesh is transparent.
  private const float RETICLE_VISUAL_RATIO = 0.1f;

  public GvrLaserVisual LaserVisual { get; private set; }

  private bool isHittingTarget;

  public override float MaxPointerDistance {
    get {
      return maxPointerDistance;
    }
  }

  public override float CameraRayIntersectionDistance {
    get {
      if (overrideCameraRayIntersectionDistance != 0.0f) {
        return overrideCameraRayIntersectionDistance;
      }

      return LaserVisual != null ?
        LaserVisual.maxLaserDistance : overrideCameraRayIntersectionDistance;
    }
  }

  public override void OnPointerEnter(RaycastResult raycastResult, bool isInteractive) {
    LaserVisual.SetDistance(raycastResult.distance);
    isHittingTarget = true;
  }

  public override void OnPointerHover(RaycastResult raycastResult, bool isInteractive) {
    LaserVisual.SetDistance(raycastResult.distance);
    isHittingTarget = true;
  }

  public override void OnPointerExit(GameObject previousObject) {
    // Don't set the distance immediately.
    // If we exit/enter an object on the same frame, then SetDistance
    // will be called twice which could cause an issue with lerping the reticle.
    // If we don't re-enter a new object, the distance will be set in Update.
    isHittingTarget = false;
  }

  public override void OnPointerClickDown() {
  }

  public override void OnPointerClickUp() {
  }

  public override void GetPointerRadius(out float enterRadius, out float exitRadius) {
    if (LaserVisual.reticle != null) {
      float reticleScale = LaserVisual.reticle.transform.localScale.x;

      // Fixed size for enter radius to avoid flickering.
      // This will cause some slight variability based on the distance of the object
      // from the camera, and is optimized for the average case.
      enterRadius = LaserVisual.reticle.sizeMeters * 0.5f * RETICLE_VISUAL_RATIO;

      // Dynamic size for exit radius.
      // Always correct because we know the intersection point of the object and can
      // therefore use the correct radius based on the object's distance from the camera.
      exitRadius = reticleScale * LaserVisual.reticle.ReticleMeshSizeMeters * RETICLE_VISUAL_RATIO;
    } else {
      enterRadius = 0.0f;
      exitRadius = 0.0f;
    }
  }

  void Awake() {
    LaserVisual = GetComponent<GvrLaserVisual>();
  }

  protected override void Start() {
    base.Start();
    LaserVisual.GetPointForDistanceFunction = GetPointAlongPointer;
    LaserVisual.SetDistance(defaultReticleDistance, true);
  }

  void Update() {
    if (isHittingTarget) {
      return;
    }

    LaserVisual.SetDistance(defaultReticleDistance);
  }
}

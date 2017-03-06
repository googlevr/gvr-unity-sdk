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

/// This laser pointer visual should be attached to the controller object.
/// The laser visual is important to help users locate their cursor
/// when its not directly in their field of view.
[RequireComponent(typeof(LineRenderer))]
public class GvrLaserPointer : MonoBehaviour {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private GvrLaserPointerImpl laserPointerImpl;

  /// Color of the laser pointer including alpha transparency
  public Color laserColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

  /// Maximum distance of the pointer (meters).
  [Range(0.0f, 10.0f)]
  public float maxLaserDistance = 0.75f;

  /// Maximum distance of the reticle (meters).
  [Range(0.4f, 10.0f)]
  public float maxReticleDistance = 2.5f;

  public GameObject reticle;

  /// Sorting order to use for the reticle's renderer.
  /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
  [Range(-32767, 32767)]
  public int reticleSortingOrder = 32767;

  void Awake() {
    laserPointerImpl = new GvrLaserPointerImpl();
    laserPointerImpl.LaserLineRenderer = gameObject.GetComponent<LineRenderer>();

    if (reticle != null) {
      Renderer reticleRenderer = reticle.GetComponent<Renderer>();
      reticleRenderer.sortingOrder = reticleSortingOrder;
    }
  }

  void Start() {
    laserPointerImpl.OnStart();
    laserPointerImpl.MainCamera = Camera.main;
    UpdateLaserPointerProperties();
  }

  void LateUpdate() {
    UpdateLaserPointerProperties();
    laserPointerImpl.OnUpdate();
  }

  public void SetAsMainPointer() {
    GvrPointerManager.Pointer = laserPointerImpl;
  }

  private void UpdateLaserPointerProperties() {
    if (laserPointerImpl == null) {
      return;
    }
    laserPointerImpl.LaserColor = laserColor;
    laserPointerImpl.Reticle = reticle;
    laserPointerImpl.MaxLaserDistance = maxLaserDistance;
    laserPointerImpl.MaxReticleDistance = maxReticleDistance;
    laserPointerImpl.PointerTransform = transform;
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

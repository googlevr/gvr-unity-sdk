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

/// Visualizes a reticle using a Quad.
/// Provides tuning options to control how the reticle scales and rotates based
/// on distance from the camera.
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GvrControllerReticleVisual : MonoBehaviour {
  [Serializable]
  public struct FaceCameraData {
    public bool alongXAxis;
    public bool alongYAxis;
    public bool alongZAxis;

    public bool IsAnyAxisOff {
      get {
        return !alongXAxis || !alongYAxis || !alongZAxis;
      }
    }

    public FaceCameraData(bool startEnabled) {
      alongXAxis = startEnabled;
      alongYAxis = startEnabled;
      alongZAxis = startEnabled;
    }
  }

  /// If set to false, the scale is simply set to the sizeMeters value.
  [Tooltip("Determines if the size of the reticle is based on the distance from the camera.")]
  public bool isSizeBasedOnCameraDistance = true;

  /// The reticle will be scaled based on the size of the mesh so that it's size matches this size.
  [Tooltip("Final size of the reticle in meters when it is 1 meter from the camera.")]
  public float sizeMeters = 0.1f;

  [Tooltip("Determines if the reticle will always face the camera and along what axes.")]
  public FaceCameraData doesReticleFaceCamera = new FaceCameraData(true);

  /// Sorting order to use for the reticle's renderer.
  /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
  [Range(-32767, 32767)]
  public int sortingOrder = 0;

  /// The size of the reticle's mesh in meters.
  public float ReticleMeshSizeMeters { get; private set; }

  /// The ratio of the reticleMeshSizeMeters to 1 meter.
  /// If reticleMeshSizeMeters is 10, then reticleMeshSizeRatio is 0.1.
  public float ReticleMeshSizeRatio { get; private set; }

  protected MeshRenderer meshRenderer;
  protected MeshFilter meshFilter;

  private Vector3 preRenderLocalScale;
  private Quaternion preRenderLocalRotation;

  public void RefreshMesh() {
    ReticleMeshSizeMeters = 1.0f;
    ReticleMeshSizeRatio = 1.0f;

    if (meshFilter != null && meshFilter.mesh != null) {
      ReticleMeshSizeMeters = meshFilter.mesh.bounds.size.x;
      if (ReticleMeshSizeMeters != 0.0f) {
        ReticleMeshSizeRatio = 1.0f / ReticleMeshSizeMeters;
      }
    }

    if (meshRenderer != null) {
      meshRenderer.sortingOrder = sortingOrder;
    }
  }

  protected virtual void Awake() {
    meshRenderer = GetComponent<MeshRenderer>();
    meshFilter = GetComponent<MeshFilter>();
  }

  protected virtual void OnEnable() {
    RefreshMesh();
  }

  protected virtual void OnWillRenderObject() {
    preRenderLocalScale = transform.localScale;
    preRenderLocalRotation = transform.localRotation;

    Camera camera = Camera.current;
    UpdateReticleSize(camera);
    UpdateReticleOrientation(camera);
  }

  protected virtual void OnRenderObject() {
    // It is possible for paired calls to OnWillRenderObject/OnRenderObject to be nested if
    // Camera.Render is explicitly called for any special effects. To avoid the reticle being
    // rotated/scaled incorrectly in that case, the reticle is reset to it's pre-OnWillRenderObject
    // after a render has finished.
    transform.localScale = preRenderLocalScale;
    transform.localRotation = preRenderLocalRotation;
  }

  protected virtual void UpdateReticleSize(Camera camera) {
    if (camera == null) {
      return;
    }

    float scale = sizeMeters;

    if (isSizeBasedOnCameraDistance) {
      float reticleDistanceFromCamera = (transform.position - camera.transform.position).magnitude;
      scale *= ReticleMeshSizeRatio * reticleDistanceFromCamera;
    }

    transform.localScale = new Vector3(scale, scale, scale);
  }

  protected virtual void UpdateReticleOrientation(Camera camera) {
    if (camera == null) {
      return;
    }

    Vector3 direction = transform.position - camera.transform.position;
    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

    if (doesReticleFaceCamera.IsAnyAxisOff) {
      Vector3 euler = transform.localEulerAngles;
      if (!doesReticleFaceCamera.alongXAxis) {
        euler.x = 0.0f;
      }

      if (!doesReticleFaceCamera.alongYAxis) {
        euler.y = 0.0f;
      }

      if (!doesReticleFaceCamera.alongZAxis) {
        euler.z = 0.0f;
      }

      transform.localEulerAngles = euler;
    }
  }

  protected virtual void OnValidate() {
    if (Application.isPlaying && isActiveAndEnabled) {
      RefreshMesh();
    }
  }
}

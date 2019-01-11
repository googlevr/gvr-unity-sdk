//-----------------------------------------------------------------------
// <copyright file="GvrControllerReticleVisual.cs" company="Google Inc.">
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
using Gvr.Internal;

/// <summary>Visualizes a reticle using a Quad.</summary>
/// <remarks>Provides tuning options to control how the reticle scales and rotates based
/// on distance from the camera.</remarks>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrControllerReticleVisual")]
public class GvrControllerReticleVisual : MonoBehaviour
{
    /// <summary>Camera facing positioning data.</summary>
    [Serializable]
    public struct FaceCameraData
    {
        /// <summary>True if aligned on X axis.</summary>
        public bool alongXAxis;

        /// <summary>True if aligned on Y axis.</summary>
        public bool alongYAxis;

        /// <summary>True if aligned on Z axis.</summary>
        public bool alongZAxis;

        /// <summary>Returns true if not along any axis.</summary>
        public bool IsAnyAxisOff
        {
            get
            {
                return !alongXAxis || !alongYAxis || !alongZAxis;
            }
        }

        /// <summary>Constructs a new FaceCameraData object.</summary>
        public FaceCameraData(bool startEnabled)
        {
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

    /// <summary>Determines if the reticle will always face the camera and along what axes.</summary>
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

    /// <summary>The mesh renderer for the reticle.</summary>
    protected MeshRenderer meshRenderer;

    /// <summary>The mesh filter for the reticle.</summary>
    protected MeshFilter meshFilter;

    private Vector3 preRenderLocalScale;
    private Quaternion preRenderLocalRotation;

    /// <summary>Updates the mesh dimensions.</summary>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public void RefreshMesh()
    {
        ReticleMeshSizeMeters = 1.0f;
        ReticleMeshSizeRatio = 1.0f;

        if (meshFilter != null && meshFilter.mesh != null)
        {
            ReticleMeshSizeMeters = meshFilter.mesh.bounds.size.x;
            if (ReticleMeshSizeMeters != 0.0f)
            {
                ReticleMeshSizeRatio = 1.0f / ReticleMeshSizeMeters;
            }
        }

        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }
    }

    /// @cond
    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    /// @endcond

    /// @cond
    protected virtual void OnEnable()
    {
        RefreshMesh();
    }

    /// @endcond

    /// @cond
    protected virtual void OnWillRenderObject()
    {
        preRenderLocalScale = transform.localScale;
        preRenderLocalRotation = transform.localRotation;

        Camera camera = Camera.current;
        UpdateReticleSize(camera);
        UpdateReticleOrientation(camera);
    }

    /// @endcond

    /// @cond
    protected virtual void OnRenderObject()
    {
        // It is possible for paired calls to OnWillRenderObject/OnRenderObject to be nested if
        // Camera.Render is explicitly called for any special effects. To avoid the reticle being
        // rotated/scaled incorrectly in that case, the reticle is reset to it's pre-OnWillRenderObject
        // after a render has finished.
        transform.localScale = preRenderLocalScale;
        transform.localRotation = preRenderLocalRotation;
    }

    /// @endcond

    /// <summary>Update the recticle size based on the distance.</summary>
    protected virtual void UpdateReticleSize(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        float scale = sizeMeters;

        if (isSizeBasedOnCameraDistance)
        {
            float reticleDistanceFromCamera = (transform.position - camera.transform.position).magnitude;
            scale *= ReticleMeshSizeRatio * reticleDistanceFromCamera;
        }

        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>Updates the reticle position and orientation based on the camera.</summary>
    protected virtual void UpdateReticleOrientation(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        Vector3 direction = transform.position - camera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (doesReticleFaceCamera.IsAnyAxisOff)
        {
            Vector3 euler = transform.localEulerAngles;
            if (!doesReticleFaceCamera.alongXAxis)
            {
                euler.x = 0.0f;
            }

            if (!doesReticleFaceCamera.alongYAxis)
            {
                euler.y = 0.0f;
            }

            if (!doesReticleFaceCamera.alongZAxis)
            {
                euler.z = 0.0f;
            }

            transform.localEulerAngles = euler;
        }
    }

    /// @cond
    protected virtual void OnValidate()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            RefreshMesh();
        }
    }

    /// @endcond
}

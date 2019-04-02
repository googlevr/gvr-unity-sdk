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
using Gvr.Internal;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>Visualizes a reticle using a Quad.</summary>
/// <remarks>
/// Provides tuning options to control how the reticle scales and rotates based on distance from the
/// camera.
/// </remarks>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrControllerReticleVisual")]
public class GvrControllerReticleVisual : MonoBehaviour
{
    /// <summary>
    /// If `true`, the scale is based on Camera Distance.  If `false`, the scale is set to the
    /// `sizeMeters` value.
    /// </summary>
    [Tooltip("Determines if the size of the reticle is based on the distance from the camera.")]
    public bool isSizeBasedOnCameraDistance = true;

    /// <summary>
    /// The reticle will be scaled based on the size of the mesh so that its size matches this size.
    /// </summary>
    /// <remarks>Final size of the reticle in meters when it is 1 meter from the camera.</remarks>
    [Tooltip("Final size of the reticle in meters when it is 1 meter from the camera.")]
    public float sizeMeters = 0.1f;

    /// <summary>
    /// Determines if the reticle will always face the camera and along which axes.
    /// </summary>
    [Tooltip("Determines if the reticle will always face the camera and along what axes.")]
    public FaceCameraData doesReticleFaceCamera = new FaceCameraData(true);

    /// <summary>Sorting order to use for the reticle's renderer.</summary>
    /// <remarks>
    /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
    /// </remarks>
    [Range(-32767, 32767)]
    public int sortingOrder = 0;

    /// <summary>The mesh renderer for the reticle.</summary>
    protected MeshRenderer meshRenderer;

    /// <summary>The mesh filter for the reticle.</summary>
    protected MeshFilter meshFilter;

    private Vector3 preRenderLocalScale;
    private Quaternion preRenderLocalRotation;

    /// <summary>Gets the size of the reticle's mesh in meters.</summary>
    /// <value>The reticle mesh size in meters.</value>
    public float ReticleMeshSizeMeters { get; private set; }

    /// <summary>Gets the ratio of the reticleMeshSizeMeters compared to 1 meter.</summary>
    /// <remarks>If reticleMeshSizeMeters is 10, then reticleMeshSizeRatio is 0.1.</remarks>
    /// <value>The reticle mesh size ratio.</value>
    public float ReticleMeshSizeRatio { get; private set; }

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
    /// <summary>The MonoBehavior's Awake method.</summary>
    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    /// @endcond
    /// @cond
    /// <summary>The MonoBehavior's OnEnable method.</summary>
    protected virtual void OnEnable()
    {
        RefreshMesh();
    }

    /// @endcond
    /// @cond
    /// <summary>The MonoBehavior's OnWillRenderObject method.</summary>
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
    /// <summary>The MonoBehavior's OnRenderObject method.</summary>
    protected virtual void OnRenderObject()
    {
        // It is possible for paired calls to OnWillRenderObject/OnRenderObject to be nested if
        // Camera.Render is explicitly called for any special effects. To avoid the reticle being
        // rotated/scaled incorrectly in that case, the reticle is reset to its
        // pre-OnWillRenderObject after a render has finished.
        transform.localScale = preRenderLocalScale;
        transform.localRotation = preRenderLocalRotation;
    }

    /// @endcond
    /// <summary>Update the recticle size based on the distance.</summary>
    /// <param name="camera">The camera to update size relative to.</param>
    protected virtual void UpdateReticleSize(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        float scale = sizeMeters;

        if (isSizeBasedOnCameraDistance)
        {
            float reticleDistanceFromCamera =
                (transform.position - camera.transform.position).magnitude;
            scale *= ReticleMeshSizeRatio * reticleDistanceFromCamera;
        }

        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>Updates the reticle position and orientation based on the camera.</summary>
    /// <remarks>Locks orientation angles according to `along*Axis` fields.</remarks>
    /// <param name="camera">The camera to update orientation to match.</param>
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
    /// <summary>Called by the `validate` event.</summary>
    protected virtual void OnValidate()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            RefreshMesh();
        }
    }

    /// @endcond
    /// <summary>Camera facing positioning data.</summary>
    /// <remarks>
    /// These are parameters for restricting recenters along given euler angle axes.
    /// </remarks>
    [Serializable]
    public struct FaceCameraData
    {
        /// <summary>Value `true` if aligned on X axis.</summary>
        /// <remarks>
        /// If `true`, the `eulerAngle.x` is set to 0 (relative to the designated Camera) after a
        /// recenter. Otherwise, `eulerAngle.x` is determined by relative look-positon from the
        /// Camera.
        /// </remarks>
        public bool alongXAxis;

        /// <summary>Value `true` if aligned on Y axis.</summary>
        /// <remarks>
        /// If `true`, the `eulerAngle.y` is set to 0 (relative to the designated Camera) after a
        /// recenter. Otherwise, `eulerAngle.y` is determined by relative look-positon from the
        /// Camera.
        /// </remarks>
        public bool alongYAxis;

        /// <summary>Value `true` if aligned on Z axis.</summary>
        /// <remarks>
        /// If `true`, the `eulerAngle.z` is set to 0 (relative to the designated Camera) after a
        /// recenter. Otherwise, `eulerAngle.z` is determined by relative look-positon from the
        /// Camera.
        /// </remarks>
        public bool alongZAxis;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceCameraData" /> struct.
        /// </summary>
        /// <param name="startEnabled">Whether the axes should start enabled.</param>
        public FaceCameraData(bool startEnabled)
        {
            alongXAxis = startEnabled;
            alongYAxis = startEnabled;
            alongZAxis = startEnabled;
        }

        /// <summary>Gets a value indicating whether any axis is off.</summary>
        /// <value>Value `false` if along any axis, `true` otherwise.</value>
        public bool IsAnyAxisOff
        {
            get
            {
                return !alongXAxis || !alongYAxis || !alongZAxis;
            }
        }
    }
}

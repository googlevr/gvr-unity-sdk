//-----------------------------------------------------------------------
// <copyright file="GvrPointerGraphicRaycaster.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>This script provides a raycaster for use with the `GvrPointerInputModule`.</summary>
/// <remarks><para>
/// This behaves similarly to the standards Graphic raycaster, except that it utilize raycast
/// modes specifically for Gvr.
/// </para><para>
/// See `GvrBasePointerRaycaster.cs` and `GvrPointerInputModule.cs` for more details.
/// </para></remarks>
[AddComponentMenu("GoogleVR/GvrPointerGraphicRaycaster")]
[RequireComponent(typeof(Canvas))]
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrPointerGraphicRaycaster")]
public class GvrPointerGraphicRaycaster : GvrBasePointerRaycaster
{
    /// <summary>Flag for ignoring reversed graphics direction.</summary>
    public bool ignoreReversedGraphics = true;

    /// <summary>The type of objects which can block raycasts.</summary>
    public BlockingObjects blockingObjects = BlockingObjects.ThreeD;

    /// <summary>The blocking layer mask to use when raycasting.</summary>
    public LayerMask blockingMask = NO_EVENT_MASK_SET;

    private const int NO_EVENT_MASK_SET = -1;

    private static List<Graphic> sortedGraphics = new List<Graphic>();

    private Canvas targetCanvas;
    private List<Graphic> raycastResults = new List<Graphic>();
    private Camera cachedPointerEventCamera;

    /// <summary>
    /// Initializes a new instance of the <see cref="GvrPointerGraphicRaycaster" /> class.
    /// </summary>
    protected GvrPointerGraphicRaycaster()
    {
    }

    /// <summary>Types of blocking objects this object's raycasts can hit.</summary>
    public enum BlockingObjects
    {
        /// <summary>This cannot hit any objects.</summary>
        None = 0,

        /// <summary>This can hit only 2D objects.</summary>
        TwoD = 1,

        /// <summary>This can hit only 3D objects.</summary>
        ThreeD = 2,

        /// <summary>This can hit all objects.</summary>
        All = 3,
    }

    /// <summary>Gets the event Camera used for gaze-based raycasts.</summary>
    /// <value>The event camera.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.LegacyGvrStyleRules",
        "VR1001:AccessibleNonConstantPropertiesMustBeUpperCamelCase",
        Justification = "Legacy Public API.")]
    public override Camera eventCamera
    {
        [SuppressMemoryAllocationError(IsWarning = true,
                                       Reason = "A getter for a Camera should not allocate.")]
        get
        {
            GvrBasePointer pointer = GvrPointerInputModule.Pointer;
            if (pointer == null)
            {
                return null;
            }

            if (pointer.raycastMode == GvrBasePointer.RaycastMode.Hybrid)
            {
                return GetCameraForRaycastMode(pointer, CurrentRaycastModeForHybrid);
            }
            else
            {
                return GetCameraForRaycastMode(pointer, pointer.raycastMode);
            }
        }
    }

    /// <summary>Perform raycast on the scene.</summary>
    /// <param name="pointerRay">The ray to use for the operation.</param>
    /// <param name="radius">The radius of the ray to use when testing for hits.</param>
    /// <param name="eventData">The event data triggered by any resultant Raycast hits.</param>
    /// <param name="resultAppendList">The results are appended to this list.</param>
    /// <returns>Returns `true` if the Raycast has at least one hit, `false` otherwise.</returns>
    protected override bool PerformRaycast(GvrBasePointer.PointerRay pointerRay,
                                           float radius,
                                           PointerEventData eventData,
                                           List<RaycastResult> resultAppendList)
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponent<Canvas>();
            if (targetCanvas == null)
            {
                return false;
            }
        }

        if (eventCamera == null)
        {
            return false;
        }

        if (targetCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError("GvrPointerGraphicRaycaster requires that the canvas renderMode is set "
                           + "to WorldSpace.");
            return false;
        }

        float hitDistance = float.MaxValue;

        if (blockingObjects != BlockingObjects.None)
        {
            float dist = pointerRay.distance;

            if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
            {
                RaycastHit hit;
                if (Physics.Raycast(pointerRay.ray, out hit, dist, blockingMask))
                {
                    hitDistance = hit.distance;
                }
            }

            if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
            {
                RaycastHit2D hit = Physics2D.Raycast(pointerRay.ray.origin,
                                                     pointerRay.ray.direction,
                                                     dist,
                                                     blockingMask);

                if (hit.collider != null)
                {
                    hitDistance = hit.fraction * dist;
                }
            }
        }

        raycastResults.Clear();
        Ray finalRay;
        Raycast(targetCanvas,
                pointerRay.ray,
                eventCamera,
                pointerRay.distance,
                raycastResults,
                out finalRay);

        bool foundHit = false;

        for (int index = 0; index < raycastResults.Count; index++)
        {
            GameObject go = raycastResults[index].gameObject;
            bool appendGraphic = true;

            if (ignoreReversedGraphics)
            {
                // If we have a camera compare the direction against the cameras forward.
                Vector3 cameraFoward = eventCamera.transform.rotation * Vector3.forward;
                Vector3 dir = go.transform.rotation * Vector3.forward;
                appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
            }

            if (appendGraphic)
            {
                float resultDistance = 0;

                Transform trans = go.transform;
                Vector3 transForward = trans.forward;

                // http://geomalgorithms.com/a06-_intersect-2.html
                float transDot = Vector3.Dot(transForward, trans.position - pointerRay.ray.origin);
                float rayDot = Vector3.Dot(transForward, pointerRay.ray.direction);
                resultDistance = transDot / rayDot;
                Vector3 hitPosition =
                    pointerRay.ray.origin + (pointerRay.ray.direction * resultDistance);

                // Check to see if the go is behind the camera.
                if (resultDistance < 0 ||
                    resultDistance >= hitDistance ||
                    resultDistance > pointerRay.distance)
                {
                    continue;
                }

                resultDistance = resultDistance + pointerRay.distanceFromStart;
                Transform pointerTransform =
                    GvrPointerInputModule.Pointer.PointerTransform;
                float delta = (hitPosition - pointerTransform.position).magnitude;
                if (delta < pointerRay.distanceFromStart)
                {
                    continue;
                }

                RaycastResult castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = resultDistance,
                    worldPosition = hitPosition,
                    screenPosition = eventCamera.WorldToScreenPoint(hitPosition),
                    index = resultAppendList.Count,
                    depth = raycastResults[index].depth,
                    sortingLayer = targetCanvas.sortingLayerID,
                    sortingOrder = targetCanvas.sortingOrder
                };

                resultAppendList.Add(castResult);
                foundHit = true;
            }
        }

        return foundHit;
    }

    // Perform a raycast into the screen and collect all graphics underneath it.
    private static void Raycast(Canvas canvas, Ray ray, Camera cam, float distance,
                                 List<Graphic> results, out Ray finalRay)
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(ray.GetPoint(distance));
        finalRay = cam.ScreenPointToRay(screenPoint);

        // Necessary for the event system
        IList<Graphic> foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
        for (int i = 0; i < foundGraphics.Count; ++i)
        {
            Graphic graphic = foundGraphics[i];

            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
            if (graphic.depth == -1 || !graphic.raycastTarget)
            {
                continue;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform,
                                                                   screenPoint,
                                                                   cam))
            {
                continue;
            }

            if (graphic.Raycast(screenPoint, cam))
            {
                sortedGraphics.Add(graphic);
            }
        }

        sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));

        for (int i = 0; i < sortedGraphics.Count; ++i)
        {
            results.Add(sortedGraphics[i]);
        }

        sortedGraphics.Clear();
    }

    private Camera GetCameraForRaycastMode(GvrBasePointer pointer, GvrBasePointer.RaycastMode mode)
    {
        switch (mode)
        {
            case GvrBasePointer.RaycastMode.Direct:
                return GetCameraForRaycastModeDirect(pointer);
            case GvrBasePointer.RaycastMode.Camera:
            default:
                return pointer.PointerCamera;
        }
    }

    private Camera GetCameraForRaycastModeDirect(GvrBasePointer pointer)
    {
        // Clear cachedPointerEventCamera if the pointer has changed.
        if (cachedPointerEventCamera != null &&
            cachedPointerEventCamera.gameObject !=
                GvrPointerInputModule.Pointer.PointerTransform.gameObject)
        {
            cachedPointerEventCamera = null;
        }

        // Get and cache the pointer's camera component.
        if (cachedPointerEventCamera == null)
        {
            Transform pointerTransform = GvrPointerInputModule.Pointer.PointerTransform;
            cachedPointerEventCamera = pointerTransform.GetComponent<Camera>();
        }

        // Still no pointer camera?  Add a dummy one.
        if (cachedPointerEventCamera == null)
        {
            cachedPointerEventCamera = AddDummyCameraToPointer(pointer);
        }

        return cachedPointerEventCamera;
    }

    private Camera AddDummyCameraToPointer(GvrBasePointer pointer)
    {
        Camera camera = pointer.PointerTransform.gameObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.nearClipPlane = 0.01f; // Minimum Near Clip Plane.
        return camera;
    }
}

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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// This script provides a raycaster for use with the GvrPointerInputModule.
/// It behaves similarly to the standards Graphic raycaster, except that it utilize raycast
/// modes specifically for Gvr.
///
/// View GvrBasePointerRaycaster.cs and GvrPointerInputModule.cs for more details.
[AddComponentMenu("GoogleVR/GvrPointerGraphicRaycaster")]
[RequireComponent(typeof(Canvas))]
public class GvrPointerGraphicRaycaster : GvrBasePointerRaycaster {
  public enum BlockingObjects {
    None = 0,
    TwoD = 1,
    ThreeD = 2,
    All = 3,
  }

  private const int NO_EVENT_MASK_SET = -1;

  public bool ignoreReversedGraphics = true;
  public BlockingObjects blockingObjects = BlockingObjects.None;
  public LayerMask blockingMask = NO_EVENT_MASK_SET;

  private Canvas targetCanvas;
  private List<Graphic> raycastResults = new List<Graphic>();
  private Camera cachedPointerEventCamera;

  private static readonly List<Graphic> sortedGraphics = new List<Graphic>();

  public override Camera eventCamera {
    get {
      GvrBasePointer pointer = GvrPointerInputModule.Pointer;
      if (pointer == null) {
        return null;
      }

      if (pointer.raycastMode == GvrBasePointer.RaycastMode.Hybrid) {
        return GetCameraForRaycastMode(pointer, CurrentRaycastModeForHybrid);
      } else {
        return GetCameraForRaycastMode(pointer, pointer.raycastMode);
      }
    }
  }

  private Canvas canvas {
    get {
      if (targetCanvas != null)
        return targetCanvas;

      targetCanvas = GetComponent<Canvas>();
      return targetCanvas;
    }
  }

  protected GvrPointerGraphicRaycaster() {
  }

  protected override bool PerformRaycast(GvrBasePointer.PointerRay pointerRay, float radius,
    PointerEventData eventData, List<RaycastResult> resultAppendList) {
    if (canvas == null) {
      return false;
    }

    if (eventCamera == null) {
      return false;
    }

    if (canvas.renderMode != RenderMode.WorldSpace) {
      Debug.LogError("GvrPointerGraphicRaycaster requires that the canvas renderMode is set to WorldSpace.");
      return false;
    }

    float hitDistance = float.MaxValue;

    if (blockingObjects != BlockingObjects.None) {
      float dist = pointerRay.distance;

      if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All) {
        RaycastHit hit;
        if (Physics.Raycast(pointerRay.ray, out hit, dist, blockingMask)) {
          hitDistance = hit.distance;
        }
      }

      if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All) {
        RaycastHit2D hit = Physics2D.Raycast(pointerRay.ray.origin, pointerRay.ray.direction, dist, blockingMask);

        if (hit.collider != null) {
          hitDistance = hit.fraction * dist;
        }
      }
    }

    raycastResults.Clear();
    Ray finalRay;
    Raycast(canvas, pointerRay.ray, eventCamera, pointerRay.distance, raycastResults, out finalRay);

    bool foundHit = false;

    for (int index = 0; index < raycastResults.Count; index++) {
      GameObject go = raycastResults[index].gameObject;
      bool appendGraphic = true;

      if (ignoreReversedGraphics) {
        // If we have a camera compare the direction against the cameras forward.
        Vector3 cameraFoward = eventCamera.transform.rotation * Vector3.forward;
        Vector3 dir = go.transform.rotation * Vector3.forward;
        appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
      }

      if (appendGraphic) {
        float resultDistance = 0;

        Transform trans = go.transform;
        Vector3 transForward = trans.forward;
        // http://geomalgorithms.com/a06-_intersect-2.html
        float transDot = Vector3.Dot(transForward, trans.position - pointerRay.ray.origin);
        float rayDot = Vector3.Dot(transForward, pointerRay.ray.direction);
        resultDistance = transDot / rayDot;
        Vector3 hitPosition = pointerRay.ray.origin + (pointerRay.ray.direction * resultDistance);
        resultDistance = resultDistance + pointerRay.distanceFromStart;

        // Check to see if the go is behind the camera.
        if (resultDistance < 0 || resultDistance >= hitDistance || resultDistance > pointerRay.distance) {
          continue;
        }

        Transform pointerTransform =
          GvrPointerInputModule.Pointer.PointerTransform;
        float delta = (hitPosition - pointerTransform.position).magnitude;
        if (delta < pointerRay.distanceFromStart) {
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
          sortingLayer = canvas.sortingLayerID,
          sortingOrder = canvas.sortingOrder
        };

        resultAppendList.Add(castResult);
        foundHit = true;
      }
    }

    return foundHit;
  }

  private Camera GetCameraForRaycastMode(GvrBasePointer pointer, GvrBasePointer.RaycastMode mode) {
    switch (mode) {
      case GvrBasePointer.RaycastMode.Direct:
        if (cachedPointerEventCamera == null) {
          Transform pointerTransform = GvrPointerInputModule.Pointer.PointerTransform;
          cachedPointerEventCamera = pointerTransform.GetComponent<Camera>();
        }

        if (cachedPointerEventCamera == null) {
          cachedPointerEventCamera = AddDummyCameraToPointer(pointer);
          return null;
        }

        return cachedPointerEventCamera;
      case GvrBasePointer.RaycastMode.Camera:
      default:
        return pointer.PointerCamera;
    }
  }

  private Camera AddDummyCameraToPointer(GvrBasePointer pointer) {
    Camera camera = pointer.PointerTransform.gameObject.AddComponent<Camera>();
    camera.enabled = false;
    camera.nearClipPlane = 0.01f; // Minimum Near Clip Plane.
    return camera;
  }

  /// Perform a raycast into the screen and collect all graphics underneath it.
  private static void Raycast(Canvas canvas, Ray ray, Camera cam, float distance,
                              List<Graphic> results, out Ray finalRay) {
    Vector3 screenPoint = cam.WorldToScreenPoint(ray.GetPoint(distance));
    finalRay = cam.ScreenPointToRay(screenPoint);

    // Necessary for the event system
    IList<Graphic> foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
    for (int i = 0; i < foundGraphics.Count; ++i) {
      Graphic graphic = foundGraphics[i];

      // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
      if (graphic.depth == -1 || !graphic.raycastTarget) {
        continue;
      }

      if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPoint, cam)) {
        continue;
      }

      if (graphic.Raycast(screenPoint, cam)) {
        sortedGraphics.Add(graphic);
      }
    }

    sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));

    for (int i = 0; i < sortedGraphics.Count; ++i) {
      results.Add(sortedGraphics[i]);
    }

    sortedGraphics.Clear();
  }
}

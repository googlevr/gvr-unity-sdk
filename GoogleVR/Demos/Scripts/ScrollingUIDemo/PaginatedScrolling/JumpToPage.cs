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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// Jumps to a specified page in a PagedScrollRect when it is clicked on.
public class JumpToPage : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  [Tooltip("Destination page.")]
  public RectTransform page;

  [Tooltip("The transform to modify when the pointer is hovering over this script.")]
  public RectTransform hoverTransform;

  [Range(0.01f, 0.5f)]
  [Tooltip("Tile forward distance when the pointer over the tile.")]
  public float hoverPositionZMeters = 0.225f;

  [Range(1.0f, 10.0f)]
  [Tooltip("Speed used for lerping the rotation/scale/position of the tile.")]
  public float interpolationSpeed = 8.0f;

  private Graphic graphic;
  private float desiredPositionZ;

  /// The scroll rect that owns the destination page.
  public PagedScrollRect PageOwnerScrollRect {
    get {
      if (cachedPagedScrollRect != null) {
        return cachedPagedScrollRect;
      }

      if (page != null) {
        cachedPagedScrollRect = page.GetComponentInParent<PagedScrollRect>();
      }

      return cachedPagedScrollRect;
    }
  }
  private PagedScrollRect cachedPagedScrollRect;

  public bool CanClick {
    get {
      if (PageOwnerScrollRect != null) {
        bool isActivePage = PageOwnerScrollRect.ActivePage == page;
        return !PageOwnerScrollRect.IsMoving && !isActivePage;
      }

      return false;
    }
  }

  void Awake() {
    graphic = GetComponent<Graphic>();
    if (graphic == null) {
      Debug.LogWarning("Graphic is null, won't be able to click on JumpToPage.");
    }
  }

  void OnEnable() {
    cachedPagedScrollRect = null;
  }

  void OnDisable() {
    cachedPagedScrollRect = null;
  }

  void Update() {
    if (graphic != null) {
      graphic.raycastTarget = CanClick;
    }

    float finalDesiredPositionZ = desiredPositionZ;
    if (!CanClick) {
      finalDesiredPositionZ = 0.0f;
    }

    if (hoverTransform != null && finalDesiredPositionZ != hoverTransform.localPosition.z) {
      Vector3 localPosition = hoverTransform.localPosition;
      Vector3 desiredPosition = localPosition;
      desiredPosition.z = finalDesiredPositionZ;
      localPosition = Vector3.Lerp(localPosition, desiredPosition, Time.deltaTime * interpolationSpeed);
      hoverTransform.localPosition = localPosition;
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  public void OnPointerEnter(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    // Since canvas graphics render facing the negative Z direction,
    // negative z is the forward direction for a canvas element.
    float metersToCanvasScale = GvrUIHelpers.GetMetersToCanvasScale(page);
    desiredPositionZ = -hoverPositionZMeters / metersToCanvasScale;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public void OnPointerExit(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    desiredPositionZ = 0.0f;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public void OnPointerClick(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    if (CanClick) {
      PageOwnerScrollRect.SnapToVisiblePage(page);
    }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }
}

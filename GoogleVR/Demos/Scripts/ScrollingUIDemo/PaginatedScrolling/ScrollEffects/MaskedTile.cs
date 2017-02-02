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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

// This class is an implementation of Basetile in which the image on each tile
// zooms inward and scrolls along with the controller retical as it is hovering
// over the tile. The edges of the image are masked off if they go beyond the
// bounding rectangle of the tile.
public class MaskedTile : BaseTile {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private const string OBJ_NAME_MASKED_IMAGE = "MaskedImage";

  private const float PARENT_CHANGE_THRESHOLD_PERCENT = 0.33f;

  private Image maskedImage;
  private GameObject maskedImageObject;

  private Vector3 originalMaskedPosition; // Start position when pointer is not on tile.
  private Vector3 maskedScrollOffset;
  private Vector2 originalImageSize;
  private Vector2 enlargedImageSize;
  private float desiredPositionZ;

  [Range(0.1f, 0.5f)]
  [Tooltip("Image scroll amount when the pointer over the tile.")]
  public float movementWeight = 0.15f;

  [Range(1.1f, 2.0f)]
  [Tooltip("Image scale amount when the pointer over the tile.")]
  public float scaleWeight = 1.4f;

  [Range(0.01f, 0.2f)]
  [Tooltip("Tile forward distance when the pointer over the tile.")]
  public float hoverPositionZMeters = 0.125f;

  [Range(0.1f, 10.0f)]
  [Tooltip("Speed used for lerping the rotation/scale/position of the tile.")]
  public float interpolationSpeed = 8.0f;

  protected override void Awake() {
    base.Awake();

    // Get current image.
    Image image = GetComponent<Image>();

    // Create mask.
    gameObject.AddComponent<RectMask2D>();

    // Save size data.
    originalImageSize = image.rectTransform.sizeDelta;
    enlargedImageSize = originalImageSize;
    enlargedImageSize.x *= scaleWeight;
    enlargedImageSize.y *= scaleWeight;

    // Save position data.
    originalMaskedPosition = new Vector3(originalImageSize.x / 2.0f, -originalImageSize.y / 2.0f, 0);

    // Set data that varies.
    maskedScrollOffset = Vector3.zero;

    // Create game object for masked image.
    maskedImageObject = new GameObject(OBJ_NAME_MASKED_IMAGE);
    maskedImageObject.transform.SetParent(transform); // Set as child of this game object.

    // Create maskedImage as component of child game object and initialize to base image.
    maskedImage = maskedImageObject.AddComponent<Image> ();
    maskedImage.sprite = image.sprite;
    maskedImage.color = image.color;
    maskedImage.material = image.material;
    image.sprite = null;

    // If this object has a selectable referencing the original image,
    // then we set the selectable to the masked image.
    Selectable selectable = GetComponent<Selectable>();
    if (selectable != null && selectable.image == image) {
      selectable.image = maskedImage;
    }

    // Set size, scale, rotation and position.
    maskedImage.rectTransform.sizeDelta = originalImageSize;
    maskedImage.rectTransform.localScale = Vector3.one;
    maskedImage.rectTransform.localRotation = Quaternion.identity;
    maskedImage.rectTransform.anchoredPosition3D = originalMaskedPosition;

    // Set masked image alignment to top-left.
    Vector2 anchor = new Vector2(0, 1);
    maskedImage.rectTransform.anchorMin = anchor;
    maskedImage.rectTransform.anchorMax = anchor;
    maskedImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
  }

  void Update() {
    // Make sure to always ignore raycasts.
    // This may be set back to true by BaseTile when the Tile becomes interactable.
    maskedImage.raycastTarget = false;

    UpdateScrollPosition();
    UpdateFloatPosition();
    UpdateScale();
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

  public override void OnPointerEnter(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = true;
    desiredPositionZ = -hoverPositionZMeters / GetMetersToCanvasScale();
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerExit(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = false;
    maskedScrollOffset = Vector3.zero;
    desiredPositionZ = 0.0f;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnGvrPointerHover(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = true;
    Vector3 pos = eventData.pointerCurrentRaycast.worldPosition;

    RectTransform rectTransform = null;
    if (maskedImageObject) {
      rectTransform = maskedImageObject.GetComponent<RectTransform>();
    }

    if (!rectTransform || !isInteractable) {
      return;
    }

    Rect rect = rectTransform.rect;
    Vector3 localCenter = rect.center;
    Vector3 worldCenter = maskedImageObject.transform.TransformPoint(localCenter);

    Vector3 localMin = new Vector3(rect.min.x, rect.min.y, 0.0f);
    Vector3 worldMin = maskedImageObject.transform.TransformPoint(localMin);

    worldCenter -= worldMin;
    pos -= worldMin;

    Vector3 direction = pos - worldCenter;
    maskedScrollOffset.x = movementWeight * enlargedImageSize.x * direction.x;
    maskedScrollOffset.y = movementWeight * enlargedImageSize.y * direction.y;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private void UpdateScrollPosition() {
    Vector3 desiredPosition = originalMaskedPosition;

    if (isInteractable && isHovering) {
      desiredPosition.x += maskedScrollOffset.x;
      desiredPosition.y += maskedScrollOffset.y;
    }

    Vector3 position = maskedImage.rectTransform.anchoredPosition3D;
    position = Vector3.Lerp(position, desiredPosition, Time.deltaTime * interpolationSpeed);
    maskedImage.rectTransform.anchoredPosition3D = position;
  }

  private void UpdateFloatPosition() {
    float finalDesiredPositionZ = desiredPositionZ;

    if (!isInteractable) {
      finalDesiredPositionZ = 0.0f;
    }

    if (finalDesiredPositionZ != transform.localPosition.z) {
      Vector3 localPosition = transform.localPosition;
      Vector3 desiredPosition = localPosition;
      desiredPosition.z = finalDesiredPositionZ;
      localPosition = Vector3.Lerp(localPosition, desiredPosition, Time.deltaTime * interpolationSpeed);
      transform.localPosition = localPosition;

      TiledPage page = GetPage();
      if (page != null) {
        float diff = Mathf.Abs(localPosition.z);

        if (diff < ((PARENT_CHANGE_THRESHOLD_PERCENT * hoverPositionZMeters) / GetMetersToCanvasScale()) &&
          transform.parent == page.transform) {
          transform.SetParent(originalParent, true);
          transform.SetAsLastSibling();
        } else if (isHovering && diff >= 0 && transform.parent == originalParent) {
          transform.SetParent(page.transform, true);
        }
      }
    }
  }

  private void UpdateScale() {
    Vector2 currentSize = maskedImage.rectTransform.sizeDelta;
    Vector2 desiredSize;

    if (IsInteractable && isHovering) {
      desiredSize = enlargedImageSize;
    } else {
      desiredSize = originalImageSize;
    }

    currentSize = Vector2.Lerp(currentSize, desiredSize, Time.deltaTime * interpolationSpeed);
    maskedImage.rectTransform.sizeDelta = currentSize;
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

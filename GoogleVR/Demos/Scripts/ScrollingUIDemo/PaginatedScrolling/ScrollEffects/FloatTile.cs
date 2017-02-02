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
using UnityEngine.Events;

// This class is an implementation of Basetile in which tiles float forward along
// the z-axis and tilt towards the camera when the gvr controller pointer is
// hovering over them.
public class FloatTile : BaseTile {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private const float PARENT_CHANGE_THRESHOLD_PERCENT = 0.33f;
  private const float _360_DEGREES = 360.0f;
  private const float _180_DEGREES = 180.0f;

  private Quaternion desiredRotation = Quaternion.identity;
  private float desiredPositionZ;
  private Vector3 desiredScale = Vector3.one;

  [Range(1.0f, 2.0f)]
  [Tooltip("Tile scale when the pointer over the tile.")]
  public float hoverScale = 1.2f;

  [Range(0.01f, 0.5f)]
  [Tooltip("Tile forward distance when the pointer over the tile.")]
  public float hoverPositionZMeters = 0.225f;

  [Range(0.0f, 30.0f)]
  [Tooltip("Maximum tile rotation towards the camera.")]
  public float maximumRotationDegreesCamera = 15.0f;

  [Range(0.0f, 5.0f)]
  [Tooltip("Maximum tile rotation towards the pointer.")]
  public float maximumRotationDegreesPointer = 3.0f;

  [Range(1.0f, 10.0f)]
  [Tooltip("Speed used for lerping the rotation/scale/position of the tile.")]
  public float interpolationSpeed = 8.0f;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

  public override void OnPointerEnter(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = true;

    // Since canvas graphics render facing the negative Z direction,
    // negative z is the forward direction for a canvas element.
    desiredPositionZ = -hoverPositionZMeters / GetMetersToCanvasScale();
    desiredScale = new Vector3(hoverScale, hoverScale, hoverScale);
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnPointerExit(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = false;

    desiredRotation = Quaternion.identity;
    desiredPositionZ = 0.0f;
    desiredScale = Vector3.one;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

  public override void OnGvrPointerHover(PointerEventData eventData) {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isHovering = true;
    UpdateDesiredRotation(eventData.pointerCurrentRaycast.worldPosition);
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  }

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  void Update() {
    UpdateRotation();
    UpdateFloatPosition();
    UpdateScale();
  }

  private void UpdateRotation() {
    Quaternion finalDesiredRotation = desiredRotation;
    if (!isInteractable) {
      finalDesiredRotation = Quaternion.identity;
    }

    if (finalDesiredRotation != transform.localRotation) {
      Quaternion localRotation = transform.localRotation;
      localRotation = Quaternion.Lerp(localRotation, finalDesiredRotation, Time.deltaTime * interpolationSpeed);
      transform.localRotation = localRotation;
    }
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
    Vector3 finalDesiredScale = desiredScale;
    if (!isInteractable) {
      finalDesiredScale = Vector3.one;
    }

    if (finalDesiredScale != transform.localScale) {
      Vector3 localScale = transform.localScale;
      localScale = Vector3.Lerp(localScale, finalDesiredScale, Time.deltaTime * interpolationSpeed);
      transform.localScale = localScale;
    }
  }

  private void UpdateDesiredRotation(Vector3 pointerIntersectionWorldPosition) {
    Vector3 localCenter = CalculateLocalCenter();
    Vector3 worldCenter = transform.TransformPoint(localCenter);
    Vector2 localSize = CalculateLocalSize();

    Vector3 pointerLocalPositionOnTile = transform.InverseTransformPoint(pointerIntersectionWorldPosition);

    Vector3 pointerDiffFromCenter = pointerLocalPositionOnTile - localCenter;
    float pointerRatioX = pointerDiffFromCenter.x / localSize.x;
    float pointerRatioY = pointerDiffFromCenter.y / localSize.y;
    Vector2 pointerRatioFromCenter = new Vector2(pointerRatioX, pointerRatioY);

    float axisCoeff = maximumRotationDegreesPointer * 2.0f;

    Vector3 worldDirection = worldCenter - Camera.main.transform.position;
    Vector3 localDirection = transform.parent.InverseTransformDirection(worldDirection);
    Quaternion lookRotation = Quaternion.LookRotation(localDirection, Vector3.up);
    Vector3 lookEuler = clampEuler(lookRotation.eulerAngles, maximumRotationDegreesCamera);
    float eulerX = lookEuler.x - pointerRatioFromCenter.y * axisCoeff;
    float eulerY = lookEuler.y + pointerRatioFromCenter.x * axisCoeff;
    desiredRotation = Quaternion.Euler(eulerX, eulerY, lookEuler.z);
  }

  private Vector2 CalculateLocalSize() {
    RectTransform rectTransform = GetComponent<RectTransform>();
    if (rectTransform) {
      Vector3 localMax = rectTransform.rect.max;
      Vector3 localMin = rectTransform.rect.min;
      return localMax - localMin;
    }
    return Vector2.zero;
  }

  protected Vector3 CalculateLocalCenter() {
    RectTransform rectTransform = GetComponent<RectTransform>();
    if (rectTransform) {
      Vector3 localCenter = rectTransform.rect.center;
      return localCenter;
    }
    return Vector3.zero;
  }

  private Vector3 clampEuler(Vector3 rotation, float maxDegrees) {
    rotation.x = clampDegrees(rotation.x, maxDegrees);
    rotation.y = clampDegrees(rotation.y, maxDegrees);
    rotation.z = clampDegrees(rotation.z, maxDegrees);
    return rotation;
  }

  private float clampDegrees(float degrees, float maxDegrees) {
    if (degrees > _180_DEGREES) {
      degrees -= _360_DEGREES;
    }

    return Mathf.Clamp(degrees, -maxDegrees, maxDegrees);
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

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

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IGvrPointerHoverHandler {
  private const float MAX_ROTATION_FACE_CAMERA_DEGREES = 15.0f;
  private const float MAX_ROTATION_POINTER_DEGREES = 3.0f;
  private const float LERP_SPEED = 8.0f;

  private const float HOVER_POSITION_Z_METERS = 0.225f;
  private const float HOVER_DESIRED_SCALE = 1.2f;
  private const float PARENT_CHANGE_THRESHOLD_PERCENT = 0.33f;

  private bool isHovering = false;
  private bool isInteractable = true;

  private Transform originalParent;
  private TiledPage page;

  private Quaternion desiredRotation = Quaternion.identity;
  private float desiredPositionZ;
  private Vector3 desiredScale = Vector3.one;

  public RectTransform Cell {
    get {
      return originalParent.GetComponent<RectTransform>();
    }
  }

  public bool IsInteractable {
    get {
      return isInteractable;
    }
    set {
      if (isInteractable == value) {
        return;
      }

      isInteractable = value;

      EventTrigger[] triggers = GetComponentsInChildren<EventTrigger>();
      foreach (EventTrigger trigger in triggers) {
        trigger.enabled = isInteractable;
      }

      Selectable[] selectables = GetComponentsInChildren<Selectable>();
      foreach (Selectable selectable in selectables) {
        selectable.interactable = isInteractable;
      }
    }
  }

  void Awake() {
    originalParent = transform.parent;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    isHovering = true;

    desiredPositionZ = -HOVER_POSITION_Z_METERS / GetMetersToCanvasScale();
    desiredScale = new Vector3(HOVER_DESIRED_SCALE, HOVER_DESIRED_SCALE, HOVER_DESIRED_SCALE);
  }

  public void OnPointerExit(PointerEventData eventData) {
    isHovering = false;

    desiredRotation = Quaternion.identity;
    desiredPositionZ = 0.0f;
    desiredScale = Vector3.one;
  }

  public void OnGvrPointerHover(PointerEventData eventData) {
    UpdateDesiredRotation(eventData.pointerCurrentRaycast.worldPosition);
  }

  void Update () {
    Quaternion finalDesiredRotation = desiredRotation;
    float finalDesiredPositionZ = desiredPositionZ;
    Vector3 finalDesiredScale = desiredScale;

    // While the tile isn't interactable, it will not display the hover effect.
    if (!isInteractable) {
      finalDesiredRotation = Quaternion.identity;
      finalDesiredPositionZ = 0.0f;
      finalDesiredScale = Vector3.one;
    }

    if (finalDesiredRotation != transform.localRotation) {
      Quaternion localRotation = transform.localRotation;
      localRotation = Quaternion.Lerp(localRotation, finalDesiredRotation, Time.deltaTime * LERP_SPEED);
      transform.localRotation = localRotation;
    }

    if (finalDesiredPositionZ != transform.localPosition.z) {
      Vector3 localPosition = transform.localPosition;
      Vector3 desiredPosition = localPosition;
      desiredPosition.z = finalDesiredPositionZ;
      localPosition = Vector3.Lerp(localPosition, desiredPosition, Time.deltaTime * LERP_SPEED);
      transform.localPosition = localPosition;

      TiledPage page = GetPage();
      if (page != null) {
        float diff = Mathf.Abs(localPosition.z);

        if (diff < ((PARENT_CHANGE_THRESHOLD_PERCENT * HOVER_POSITION_Z_METERS) / GetMetersToCanvasScale()) && transform.parent == page.transform) {
          transform.SetParent(originalParent, true);
          transform.SetAsLastSibling();
        } else if (isHovering && diff >= 0 && transform.parent == originalParent) {
          transform.SetParent(page.transform, true);
        }
      }
    }

    if (finalDesiredScale != transform.localScale) {
      Vector3 localScale = transform.localScale;
      localScale = Vector3.Lerp(localScale, finalDesiredScale, Time.deltaTime * LERP_SPEED);
      transform.localScale = localScale;
    }
  }

  private void UpdateDesiredRotation(Vector3 pointerIntersectionWorldPosition) {
    Vector3 localCenter = CalculateLocalCenter();
    Vector3 worldCenter = transform.TransformPoint(localCenter);
    Vector2 localSize = CalculateLocalSize();

    Vector3 pointerLocalPositionOnTile = transform.InverseTransformPoint(pointerIntersectionWorldPosition);

    Vector3 pointerDiffFromCenter = pointerLocalPositionOnTile - localCenter;
    Vector2 pointerRatioFromCenter = new Vector2(pointerDiffFromCenter.x / localSize.x, pointerDiffFromCenter.y / localSize.y);

    float axisCoeff = MAX_ROTATION_POINTER_DEGREES * 2.0f;

    Quaternion lookRotation = Quaternion.LookRotation(worldCenter - Camera.main.transform.position, Vector3.up);
    Vector3 lookEuler = clampEuler(lookRotation.eulerAngles, MAX_ROTATION_FACE_CAMERA_DEGREES);
    desiredRotation = Quaternion.Euler(lookEuler.x - pointerRatioFromCenter.y * axisCoeff, lookEuler.y + pointerRatioFromCenter.x * axisCoeff, lookEuler.z);
  }

  private Vector2 CalculateLocalSize() {
    RectTransform rectTransform = GetComponent<RectTransform>();
    Vector3 localMax = rectTransform.rect.max;
    Vector3 localMin = rectTransform.rect.min;
    return localMax - localMin;
  }

  private Vector3 CalculateLocalCenter() {
    RectTransform rectTransform = GetComponent<RectTransform>();
    Vector3 localCenter = rectTransform.rect.center;
    return localCenter;
  }

  private TiledPage GetPage() {
    if (page == null) {
      page = GetComponentInParent<TiledPage>();
    }

    return page;
  }

  private float GetMetersToCanvasScale() {
    Canvas canvas = GetComponentInParent<Canvas>();
    float metersToCanvasScale = canvas.transform.lossyScale.x;
    return metersToCanvasScale;
  }

  private Vector3 clampEuler(Vector3 rotation, float maxDegrees) {
    rotation.x = clampDegrees(rotation.x, maxDegrees);
    rotation.y = clampDegrees(rotation.y, maxDegrees);
    rotation.z = clampDegrees(rotation.z, maxDegrees);
    return rotation;
  }

  private float clampDegrees(float degrees, float maxDegrees) {
    if (degrees > 180.0f) {
      degrees -= 360.0f;
    }

    return Mathf.Clamp(degrees, -maxDegrees, maxDegrees);
  }
}

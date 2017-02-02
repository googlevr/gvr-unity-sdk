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

// Used inside a scrolling page view. It contains abstract functions for handling
// interactions between itself and the gvr controller.
public abstract class BaseTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IGvrPointerHoverHandler {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  protected Transform originalParent;
  protected Quaternion originalRotation;
  protected Vector3 originalPosition;
  protected Vector3 originalScale;
  protected TiledPage page;

  protected bool isInteractable = true;
  protected bool isHovering = false;

  /// Returns the tile's outer container's rect transform.
  public RectTransform Cell {
    get {
      return originalParent ? originalParent.GetComponent<RectTransform>() : null;
    }
  }

  protected virtual void Awake() {
    originalParent = transform.parent;
    originalRotation = transform.localRotation;
    originalPosition = transform.localPosition;
    originalScale = transform.localScale;
  }

  protected virtual void OnEnable() {
    OnPointerExit(null);

    transform.SetParent(originalParent, true);
    transform.localRotation = originalRotation;
    transform.localPosition = originalPosition;
    transform.localScale = originalScale;
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

  public abstract void OnPointerEnter(PointerEventData eventData);

  public abstract void OnPointerExit(PointerEventData eventData);

  public abstract void OnGvrPointerHover(PointerEventData eventData);

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  public bool IsInteractable {
    get {
      return isInteractable;
    }
    set {
      if (isInteractable == value) {
        return;
      }

      isInteractable = value;
      SetEventTriggersInteractable(isInteractable);
      SetSelectablesInteractable(isInteractable);
      SetGraphicsRaycastTarget(isInteractable);
    }
  }


  protected TiledPage GetPage() {
    if (page == null) {
      page = GetComponentInParent<TiledPage>();
    }
    return page ? page : null;
  }

  protected float GetMetersToCanvasScale() {
    return GvrUIHelpers.GetMetersToCanvasScale(transform);
  }

  private void SetEventTriggersInteractable(bool interactable) {
    EventTrigger[] triggers = GetComponentsInChildren<EventTrigger>();
    if (triggers == null) {
      return;
    }

    int numTriggers = triggers.Length;
    for (int i = 0; i < numTriggers; i++) {
      EventTrigger trigger = triggers[i];
      trigger.enabled = interactable;
    }
  }

  private void SetSelectablesInteractable(bool interactable) {
    Selectable[] selectables = GetComponentsInChildren<Selectable>();
    if (selectables == null) {
      return;
    }

    int numSelectables = selectables.Length;
    for (int i = 0; i < numSelectables; i++) {
      Selectable selectable = selectables[i];
      selectable.interactable = interactable;
    }
  }

  private void SetGraphicsRaycastTarget(bool isRaycastTarget) {
    Graphic[] graphics = GetComponentsInChildren<Graphic>();
    if (graphics == null) {
      return;
    }

    int numGraphics = graphics.Length;
    for (int i = 0; i < numGraphics; i++) {
      Graphic graphic = graphics[i];
      graphic.raycastTarget = isRaycastTarget;
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

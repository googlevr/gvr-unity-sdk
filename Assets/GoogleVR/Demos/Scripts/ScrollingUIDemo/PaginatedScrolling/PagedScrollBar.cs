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
using System.Collections;

/// Extension of Unity's built-in Scrollbar that integrates with PagedScrollRect.
/// Dragging the scrollbar will control the PagedScrollRect.
/// The Scrollbar will also automatically update when the PagedScrollRect
/// is scrolled directly.
public class PagedScrollBar : Scrollbar {
  public const string PAGED_SCROLL_RECT_PROP_NAME = "pagedScrollRect";

  [SerializeField]
  private PagedScrollRect pagedScrollRect;

  private bool isDragging = false;

  private const float LERP_SPEED = 12.0f;

  private bool IsDragging {
    get {
      return isDragging;
    }
    set {
      if (isDragging == value) {
        return;
      }

      isDragging = value;

      if (!isDragging && pagedScrollRect != null) {
        pagedScrollRect.SetScrollOffsetOverride(null);
      }
    }
  }

  void Update() {
    if (pagedScrollRect == null) {
      Debug.LogWarning("PagedScrollRect must be set.");
      return;
    }


    // Update the size of the handle in case the PageCount has changed.
    float desiredSize = 1.0f / pagedScrollRect.PageCount;
    if (size != desiredSize) {
      size = desiredSize;
    }

    if (IsDragging) {
      float offset = value * (pagedScrollRect.PageCount - 1) * pagedScrollRect.PageSpacing;
      pagedScrollRect.SetScrollOffsetOverride(offset);
    } else {
      // If the PageCount is 1 make sure we don't divide by zero by just setting the value to 0 directly.
      if (pagedScrollRect.PageCount == 1) {
        value = 0.0f;
      } else {
        // Calculate the desired a value of the scrollbar.
        float desiredValue = (float) pagedScrollRect.ActivePageIndex / (pagedScrollRect.PageCount - 1);

        // Animate towards the desired value.
        value = Mathf.Lerp(value, desiredValue, Time.deltaTime * LERP_SPEED);
      }
    }
  }

  public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {
    base.OnPointerDown(eventData);
    IsDragging = true;
  }

  public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {
    base.OnPointerUp(eventData);
    IsDragging = false;
  }
}

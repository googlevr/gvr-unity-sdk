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
using System.Collections;
using System.Collections.Generic;

/// Provides pages to a PagedScrollRect.
///
/// Treats each child of the scroll rect as a page. The pages are ordered
/// by their sibling index in the scene hierarchy.
///
/// Instead of allocating/deallocating pages, they are added and removed simply by
/// setting them active/inactive.
///
public class ChildrenPageProvider : MonoBehaviour, IPageProvider {
  /// The pages, in order.
  /// The active page is moved to be the last sibling after the scroll rect
  /// is initialized, so we need to store the pages in
  /// a seprate list to maintain the correct order.
  private List<Transform> pages = new List<Transform>();

  /// The spacing between pages in local coordinates.
  [Tooltip("The spacing between pages.")]
  public float spacing = 2000.0f;

  public float GetSpacing() {
    return spacing;
  }

  public int GetNumberOfPages() {
    return pages.Count;
  }

  public RectTransform ProvidePage(int index) {
    Transform pageTransform = pages[index];
    RectTransform page = pageTransform.GetComponent<RectTransform>();

    Vector2 middleAnchor = new Vector2(0.5f, 0.5f);
    page.anchorMax = middleAnchor;
    page.anchorMin = middleAnchor;

    pageTransform.gameObject.SetActive(true);

    return page;
  }

  public void RemovePage(int index, RectTransform page) {
    page.gameObject.SetActive(false);
  }

  void Awake() {
    /// Disable all the pages to make sure
    /// none of them are visible initially before
    /// scrolling.
    foreach (Transform page in transform) {
      page.gameObject.SetActive(false);
      pages.Add(page);
    }
  }
}

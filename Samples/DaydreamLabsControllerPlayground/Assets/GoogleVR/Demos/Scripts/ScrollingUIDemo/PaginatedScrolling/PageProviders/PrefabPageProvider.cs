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

public class PrefabPageProvider : MonoBehaviour, IPageProvider {
  /// The prefabs for each page.
  /// The pages are ordered based on the order of this array.
  [Tooltip("The prefabs for each page.")]
  public GameObject[] prefabs;

  /// The spacing between pages in local coordinates.
  [Tooltip("The spacing between pages.")]
  public float spacing = 2000.0f;

  public float GetSpacing() {
    return spacing;
  }

  public int GetNumberOfPages() {
    return prefabs.Length;
  }

  public RectTransform ProvidePage(int index) {
    GameObject pageTransform = GameObject.Instantiate(prefabs[index]);
    RectTransform page = pageTransform.GetComponent<RectTransform>();

    Vector2 middleAnchor = new Vector2(0.5f, 0.5f);
    page.anchorMax = middleAnchor;
    page.anchorMin = middleAnchor;

    return page;
  }

  public void RemovePage(int index, RectTransform page) {
    GameObject.Destroy(page.gameObject);
  }
}

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

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
[RequireComponent(typeof(Scrollbar))]
public class PagedScrollBar : MonoBehaviour {
  [SerializeField]
  private PagedScrollRect pagedScrollRect;

  private Scrollbar scrollbar;

  private const float kLerpSpeed = 12.0f;

  void Awake() {
    scrollbar = GetComponent<Scrollbar>();
  }

  void Update() {
    if (pagedScrollRect == null) {
      return;
    }

    if (scrollbar.interactable) {
      Debug.LogWarning("The Scrollbar associated with a PagedScrollBar must not be interactable.");
      scrollbar.interactable = false;
    }

    // Update the size of the handle in case the
    // PageCount has changed.
    float size = 1.0f / pagedScrollRect.PageCount;
    scrollbar.size = size;

    // Calculate the desired a value of the scrollbar.
    float desiredValue = (float)pagedScrollRect.ActivePageIndex / (pagedScrollRect.PageCount - 1);

    // Animate towards the desired value.
    scrollbar.value = Mathf.Lerp(scrollbar.value, desiredValue, Time.deltaTime * kLerpSpeed);
  }
}
#endif  // UNITY_HAS_GOOGLEVR &&(UNITY_ANDROID || UNITY_EDITOR

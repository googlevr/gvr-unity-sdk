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

/// Class that will translate the tiles of a page
/// in a PagedScrollRect based on the page's offset.
/// This creates a visual effect where the tiles will animate
/// in a staggered fashion relative to the page.
/// Requires the pages to have a TiledPage script.
public class TileScrollEffect : BaseScrollEffect {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  public override void ApplyEffect(BaseScrollEffect.UpdateData updateData) {
    TiledPage tiledPage = updateData.page.GetComponent<TiledPage>();

    if (tiledPage == null) {
      Debug.LogError("Page (" + updateData.page.name + ") does not have TiledPage. " +
        "Cannot apply TileScrollEffect.");
        return;
    }

    /// Calculate the distance between the scroll position and this page.
    float difference = updateData.scrollOffset - updateData.pageOffset;
    float clampedDifference = Mathf.Clamp(difference, -updateData.spacing, updateData.spacing);

    tiledPage.ApplyScrollEffect(clampedDifference, updateData.spacing, updateData.isInteractable);
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

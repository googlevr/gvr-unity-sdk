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

/// Class that can fade the pages of a PagedScrollRect based on the page's offset.
public class FadeScrollEffect : BaseScrollEffect {
  [Range(0.0f, 1.0f)]
  [Tooltip("The alpha of the page when it is one page-length away.")]
  public float minAlpha = 0.0f;

  public override void ApplyEffect(BaseScrollEffect.UpdateData updateData) {
    CanvasGroup pageCanvasGroup = updateData.page.GetComponent<CanvasGroup>();

    /// All pages require a CanvasGroup for manipulating Alpha.
    if (pageCanvasGroup == null) {
      Debug.LogError("Cannot adjust alpha for page " + updateData.page.name + ", missing CanvasGroup");
      return;
    }

    // Calculate the difference
    float difference = updateData.scrollOffset - updateData.pageOffset;

    /// Calculate the alpha for this page.
    float alpha = 1.0f - (Mathf.Abs(difference) / updateData.spacing);
    alpha = (alpha * (1.0f - minAlpha)) + minAlpha;
    alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);

    /// If this is the last page or the first page,
    /// Then we clamp the alpha to 1 when dragging past the edge
    /// Of the scrolling region.
    if (!updateData.looping) {
      if (updateData.pageIndex == 0 && difference < 0) {
        alpha = 1.0f;
      } else if (updateData.pageIndex == updateData.pageCount - 1 && difference > 0) {
        alpha = 1.0f;
      }
    }

    pageCanvasGroup.alpha = alpha;
  }
}

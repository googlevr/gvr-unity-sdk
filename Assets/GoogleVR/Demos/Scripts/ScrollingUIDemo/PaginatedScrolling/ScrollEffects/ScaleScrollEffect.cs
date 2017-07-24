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

/// Class that can scale the pages of a PagedScrollRect based on the page's offset.
public class ScaleScrollEffect : BaseScrollEffect {
  [Range(0.0f, 1.0f)]
  [Tooltip("The scale of the page when it is one page-length away.")]
  public float minScale;

  public override void ApplyEffect(BaseScrollEffect.UpdateData updateData) {
    // Calculate the difference.
    float difference = updateData.scrollOffset - updateData.pageOffset;

    // Calculate the scale for this page.
    float ratioScrolled = Mathf.Abs(difference) / updateData.spacing;
    float scale = ((1.0f - ratioScrolled) * (1.0f - minScale)) + minScale;
    scale = Mathf.Clamp(scale, 0.0f, 1.0f);

    // Update the scale.
    updateData.page.localScale = new Vector3(scale, scale, scale);
  }
}

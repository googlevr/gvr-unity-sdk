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

/// Class that will translate the pages of a PagedScrollRect based on the page's offset.
public class TranslateScrollEffect : BaseScrollEffect {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  [Tooltip("Determines the percentage of the page's offset that is applied to each axis.")]
  public Vector3 Weights = new Vector3(1.0f, 0.0f, 0.0f);

  [Tooltip("Determines if the absolute offset will be used for the X axis.")]
  public bool mirrorX;

  [Tooltip("Determines if the absolute offset will be used for the Y axis.")]
  public bool mirrorY;

  [Tooltip("Determines if the absolute offset will be used for the Z axis.")]
  public bool mirrorZ;

  public override void ApplyEffect(BaseScrollEffect.UpdateData updateData) {
    float distance = updateData.pageOffset - updateData.scrollOffset;
    float absDistance = Mathf.Abs(distance);
    updateData.page.anchoredPosition3D = new Vector3(
      (mirrorX ? absDistance : distance) * Weights.x,
      (mirrorY ? absDistance : distance) * Weights.y,
      (mirrorZ ? absDistance : distance) * Weights.z);
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

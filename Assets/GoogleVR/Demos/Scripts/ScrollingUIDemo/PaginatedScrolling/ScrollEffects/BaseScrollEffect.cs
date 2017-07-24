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

/// This script implements effects for visualizing
/// the scrolling of a PagedScrollRect.
///
/// Scroll effects must be placed on the same object as the PagedScrollRect.
/// Multiple scroll effects can be mixed together. They will be applied in the order
/// of the components on the object.
///
/// Three example implementations are included:
/// TranslateScrollEffect - Change the position of the page linearly based on the scroll offset.
/// FadeScrollEffect - Change the opacity of the page linearly based on the scroll offset.
/// ScaleScrollEffect - Change the scale of the page linearly based on the scroll offset.
public abstract class BaseScrollEffect : MonoBehaviour {
  public struct UpdateData {
    public RectTransform page;
    public int pageIndex;
    public int pageCount;
    public float pageOffset;
    public float scrollOffset;
    public float spacing;
    public bool looping;
    public bool isInteractable;
    public float moveDistance;
  }

  public abstract void ApplyEffect(UpdateData updateData);
}

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

/// This script is an in interface that provides content pages
/// for a PagedScrollRect (Paginated Scrolling)
///
/// The derived class must inherit from MonoBehaviour and be placed on the
/// same object as PagedScrollRect
///
/// Two generic implementations are included:
///
/// ChildrenPageProvider - This implementation automatically uses the children of the
/// PagedScrollRect as the pages. The pages are in order of their SiblingIndex in the scene.
/// This is the simplest way to do PaginatedScrolling.
///
/// PrefabPageProvider - This implementation takes a serialized list of prefabs that are
/// dynamically instantiated/destroyed as the user scrolls through the ScrollRect.
///
/// Here are some example use cases for a custom implementation:
/// 1. Page content is provided asynchronously by a network call.
/// 2. Page content utilizes pooling/object re-use to optimize memory/allocations.
/// 3. Page content could be data-driven by ScriptableObjects or some other data file.
///
public interface IPageProvider {
  /// Returns a float that represents the amount of space between pages
  /// in coordinates local to the PagedScrollRect.
  float GetSpacing();

  /// Returns the total number of pages.
  int GetNumberOfPages();

  /// Returns the appropriate page to display at the index passed in.
  /// This could be implemented by allocating the page, or by just showing it.
  RectTransform ProvidePage(int index);

  /// Removes the page passed in, as it has been scrolled out of view.
  /// This could be implemented by destroying the page, or by just hiding it.
  void RemovePage(int index, RectTransform page);
}

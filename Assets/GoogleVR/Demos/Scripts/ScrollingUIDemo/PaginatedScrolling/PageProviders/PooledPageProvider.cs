// Copyright 2017 Google Inc. All rights reserved.
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
using UnityEngine.Assertions;
using System.Collections;

/// Example page provider that shows how to use Object Pooling to
/// re-use pages instead of re-allocating them as the player scrolls.
/// A full description of Object Pooling can be found at https://en.wikipedia.org/wiki/Object_pool_pattern.
/// Doing this will significantly improve performance by preventing garbage collection and
/// reducing time spent allocating memory.
public class PooledPageProvider : MonoBehaviour, IPageProvider {
  [Tooltip("The prefab for each page.")]
  public GameObject pagePrefab;

  /// The spacing between pages in local coordinates.
  [Tooltip("The spacing between pages.")]
  public float spacing = 2000.0f;

  [SerializeField]
  [Tooltip("The number of pages.")]
  [Range(1, 200)]
  private int NumPages = 100;

  private string prefabName;

  private GameObjectPool Pool {
    get {
      ObjectPoolManager poolManager = ObjectPoolManager.Instance;
      Assert.IsNotNull(poolManager);

      GameObjectPool pool = poolManager.GetPool<GameObjectPool>(prefabName);

      if (pool == null) {
        pool = new GameObjectPool(pagePrefab, 2);
        poolManager.AddPool(prefabName, pool);
      }

      return pool;
    }
  }

  void Awake() {
    Assert.IsNotNull(pagePrefab);
    prefabName = pagePrefab.name;
  }

  public float GetSpacing() {
    return spacing;
  }

  public int GetNumberOfPages() {
    return NumPages;
  }

  public RectTransform ProvidePage(int index) {
    GameObject pageTransform = Pool.Borrow();
    RectTransform page = pageTransform.GetComponent<RectTransform>();

    Vector2 middleAnchor = new Vector2(0.5f, 0.5f);
    page.anchorMax = middleAnchor;
    page.anchorMin = middleAnchor;

    return page;
  }

  public void RemovePage(int index, RectTransform page) {
    Pool.Return(page.gameObject);
  }
}

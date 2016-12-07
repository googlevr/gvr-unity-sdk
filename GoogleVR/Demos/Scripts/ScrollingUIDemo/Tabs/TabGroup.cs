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
using System.Linq;

/// This script is used to manage a group of Tabs.
///
/// Tabs are automatically associated with this group based on
/// the ToggleGroup. Each Tab is required to be a Toggle.
///
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
[RequireComponent(typeof(ToggleGroup))]
public class TabGroup : MonoBehaviour {
  /// This Tab will be the tab that starts open
  /// when the scene is initially loaded.
  [Tooltip("The tab that should start open.")]
  [SerializeField]
  private Tab startingTab;

  /// This transform represents the parent that all tab pages
  /// will be added under when the tab is opened.
  [Tooltip("The parent for all of the tab's pages.")]
  [SerializeField]
  private Transform tabPageParent;

  private ToggleGroup toggleGroup;

  /// Returns the parent of all tab pages.
  public Transform TabPageParent {
    get {
      return tabPageParent;
    }
  }

  /// Returns the currently open Tab.
  public Tab OpenTab {
    get {
      Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
      if (toggle == null) {
        return null;
      }

      return toggle.GetComponent<Tab>();
    }
  }

  /// Returns the currently open page.
  public GameObject OpenTabPage {
    get {
      Tab tab = OpenTab;
      if (tab == null) {
        return null;
      }

      return tab.Page;
    }
  }

  void Awake() {
    toggleGroup = GetComponent<ToggleGroup>();
  }

  void Start() {
    startingTab.Open();
  }
}
#endif  // UNITY_HAS_GOOGLEVR &&(UNITY_ANDROID || UNITY_EDITOR

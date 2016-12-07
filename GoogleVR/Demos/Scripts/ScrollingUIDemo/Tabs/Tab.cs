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
using UnityEngine.EventSystems;

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
[RequireComponent(typeof(Toggle))]
public class Tab : MonoBehaviour {
  /// The prefab to use for this tab's page.
  [Tooltip("The prefab for this tab's page.")]
  [SerializeField]
  private GameObject pagePrefab;

  /// When the page is cached, it will only be instantiated the first
  /// time the tab is opened. On subsequent times it will just be
  /// activated/deactivated.
  [Tooltip("Cache the page when the tab is closed.")]
  [SerializeField]
  private bool cachePage;

  private Toggle toggle;

  /// Represents the tab's page.
  public GameObject Page { get; private set; }

  /// Returns true if the tab is open.
  public bool IsOpen { get; private set; }

  void Awake() {
    toggle = GetComponent<Toggle>();
    toggle.onValueChanged.AddListener(OnValueChanged);
    OnValueChanged(toggle.isOn);
  }

  void OnDestroy() {
    toggle.onValueChanged.RemoveListener(OnValueChanged);

    if (Page != null) {
      GameObject.Destroy(Page);
    }
  }

  void OnValidate() {
    // Awake probably hasn't been called yet, so set this here.
    toggle = GetComponent<Toggle>();

    // Make sure that this tab is part of a ToggleGroup.
    if (toggle.group == null) {
      Debug.LogError("Tab (" + gameObject.name + ") must be part of a ToggleGroup.");
    }

    // Make sure that the ToggleGroup has a TabGroup.
    TabGroup tabGroup = FindTabGroup();
    if (tabGroup == null) {
      Debug.LogError("Tab (" + gameObject.name + ")'s ToggleGroup must have a TabGroup.");
    }
  }

  /// Call this function to open this tab.
  /// When called, the currently open tab in the
  /// TabGroup will automatically be closed.
  /// At least one Tab in the TabGroup must be open at all times.
  public void Open() {
    SetOpen(true);
  }

  private void SetOpen(bool open) {
    if (IsOpen == open) {
      return;
    }

    if (open) {
      EnablePage();

      // Transition In
      IUITransition transition = FindTransition();
      if (transition != null) {
        transition.TransitionIn(Page.transform, null, null);
      }

    } else {
      // Transition Out
      IUITransition transition = FindTransition();
      if (transition != null) {
        transition.TransitionOut(Page.transform, () => {
          DisablePage();
        }, null);
      } else {
        DisablePage();
      }
    }

    IsOpen = open;

    // Make sure the toggle is in the correct state
    //  in case SetOpen was called directly.
    toggle.isOn = open;

    // Toggle shouldn't be interactble when it is on.
    toggle.interactable = !open;

    EventTrigger eventTrigger = GetComponent<EventTrigger>();
    if (eventTrigger != null) {
      eventTrigger.enabled = !open;
    }
  }

  private void EnablePage() {
    // If the page already exists, just activate it,
    // otherwise create it.
    if (Page != null) {
      Page.SetActive(true);
    } else {
      Page = GameObject.Instantiate(pagePrefab);
      TabGroup tabGroup = FindTabGroup();
      Page.transform.SetParent(tabGroup.TabPageParent, false);
    }
  }

  private void DisablePage() {
    // If we are caching the page, then
    // just deactivate it. Otherwise, destroy it.
    if (cachePage) {
      Page.SetActive(false);
    }
    else {
      GameObject.Destroy(Page);
      Page = null;
    }
  }

  private void OnValueChanged(bool isOn) {
    SetOpen(isOn);
  }

  private TabGroup FindTabGroup() {
    // The TabGroup is expected to be on the same object as the ToggleGroup.
    ToggleGroup toggleGroup = toggle.group;
    TabGroup tabGroup = toggleGroup.GetComponent<TabGroup>();
    return tabGroup;
  }

  private IUITransition FindTransition() {
    return GetComponent<IUITransition>();
  }
}
#endif  // UNITY_HAS_GOOGLEVR &&(UNITY_ANDROID || UNITY_EDITOR

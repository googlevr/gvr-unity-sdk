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

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Basic Selectable component used with the SelectionManager script. Any object tagged with this
  /// component will be considered Selectable by the Selection system.
  /// </summary>
  public class Selectable : MonoBehaviour {
    [Tooltip("Prefab Visual for this object's selected state. One instance will be pooled on " +
             "start, and turned on or off to correspond with the object's selected state.")]
    public GameObject SelectionPrefab;

    [Tooltip("Prefab Visual for this object's highlighted state. One instance will be pooled on " +
             "start, and turned on or off to correspond with the object's highlighted state.")]
    public GameObject HighlightPrefab;

    [Tooltip("Whether or not this object is currently selected. Disable to prevent the player " +
             "from being able to select this object.")]
    public bool IsSelectable = true;

    private GameObject selectionVisual;
    private GameObject highlightVisual;

    private bool isSelected = false;
    public bool IsSelected { get { return isSelected; } }

    private bool isHighlighted = false;
    public bool IsHighlighted { get { return isHighlighted; } }

    void Start() {
      selectionVisual = InstantiatePrefab(SelectionPrefab);
      highlightVisual = InstantiatePrefab(HighlightPrefab);
    }

    GameObject InstantiatePrefab(GameObject prefab) {
      GameObject instance = GameObject.Instantiate(prefab);
      instance.transform.SetParent(transform);
      instance.transform.localPosition = Vector3.zero;
      instance.transform.localEulerAngles = Vector3.zero;
      instance.SetActive(false);
      return instance;
    }

    public void SetSelectionStatus(bool active) {
      if (IsSelectable || active == false) {
        isSelected = active;
        selectionVisual.SetActive(active);
      }
    }

    public void SetHighlightStatus(bool highlight) {
      if (IsSelectable || highlight == false) {
        isHighlighted = highlight;
        highlightVisual.SetActive(highlight);
      }
    }
  }
}

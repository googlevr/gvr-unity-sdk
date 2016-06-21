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

using System.Collections.Generic;
using UnityEngine;

namespace GVR.Samples.DragonTeam {
  public class SelectionManager : MonoBehaviour {
    [HideInInspector]
    public static SelectionManager current;

    private List<Selectable> selected = new List<Selectable>();

    public bool IsSelected(Selectable selectable) {
      for (int i = 0; i < selected.Count; i++) {
        if (selectable == selected[i]) {
          return true;
        }
      }
      return false;
    }

    public Selectable[] GetSelected() {
      return selected.ToArray();
    }

    public void SetSelected(Selectable newSelected) {
      if (newSelected.IsSelectable) {
        ClearSelection();
        AddToSelected(newSelected);
      }
    }

    public void AddToSelected(Selectable newSelected) {
      if (!IsSelected(newSelected) && newSelected.IsSelectable) {
        selected.Add(newSelected);
        newSelected.SetSelectionStatus(true);
      }
    }

    public void RemoveSelected(Selectable newSelected) {
      selected.Remove(newSelected);
      newSelected.SetSelectionStatus(false);
    }

    public void ClearSelection() {
      for (int i = 0; i < selected.Count; i++) {
        selected[i].SetSelectionStatus(false);
      }
      selected.Clear();
    }

    void Awake() {
      current = this;
    }
  }
}

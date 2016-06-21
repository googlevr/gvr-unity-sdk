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
using UnityEngine.Events;
using UnityEngine.UI;

namespace GVR.Entity {
  /// <summary>
  /// A simple Counter tracker. Tracks a current integer score and displays it to a Text field.
  /// </summary>
  public class Counter : MonoBehaviour {
    [Tooltip("Event thrown when the score is added to.")]
    public UnityEvent OnPositiveChange;

    [Tooltip("Event thrown when the score is subtracted from.")]
    public UnityEvent OnNegativeChange;

    [Tooltip("Text field that the current Score is output to.")]
    public Text CountVisual;

    [Tooltip("The maximum value to track against.")]
    public int MaxCount = 15;

    [Tooltip("The value that the score starts at.")]
    public int InitialCount = 0;

    private int currentCount = 0;

    void Start() {
      currentCount = InitialCount;
      UpdateVisual();
    }

    private void UpdateVisual() {
      CountVisual.text = currentCount + " / " + MaxCount;
    }

    public void SetCount(int newCount) {
      currentCount = newCount;
      Mathf.Clamp(currentCount, 0, MaxCount);
      UpdateVisual();
      if (newCount > currentCount) {
        OnPositiveChange.Invoke();
      } else if (newCount < currentCount) {
        OnNegativeChange.Invoke();
      }
    }

    public void ModifyCount(int change) {
      currentCount += change;
      Mathf.Clamp(currentCount, 0, MaxCount);
      UpdateVisual();
      if (change > 0) {
        OnPositiveChange.Invoke();
      } else if (change <= 0) {
        OnNegativeChange.Invoke();
      }
    }

    public int GetCurrentCount() {
      return currentCount;
    }
  }
}

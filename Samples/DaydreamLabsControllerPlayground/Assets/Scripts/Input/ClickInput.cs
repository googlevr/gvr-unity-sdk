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

namespace GVR.Input {
  /// <summary>
  /// Provides controller touchpad click events through UnityEvents
  /// </summary>
  public class ClickInput : MonoBehaviour {
    [Tooltip("If set to true, touch input will be interpreted as a click. Useful for debugging.")]
    public bool TouchToClick = false;

    public ButtonEvent OnClickUp;
    public ButtonEvent OnClickHeld;
    public ButtonEvent OnClickDown;

    void Update() {
      if (TouchToClick == false) {
        if (GvrController.ClickButtonUp)
          OnClickUp.Invoke();

        if (GvrController.ClickButtonDown)
          OnClickDown.Invoke();

        if (GvrController.ClickButton)
          OnClickHeld.Invoke();
      } else {
        if (GvrController.TouchUp)
          OnClickUp.Invoke();

        if (GvrController.TouchDown)
          OnClickDown.Invoke();

        if (GvrController.IsTouching)
          OnClickHeld.Invoke();
      }
    }
  }
}

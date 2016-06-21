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
  /// Exposes events for controller touchpad input
  /// </summary>
  public class TouchInput : MonoBehaviour {
    public TouchPadEvent OnTouchDown;
    public TouchPadEvent OnTouchUp;

    void Update() {
      // We don't need to check if the controller is connected or not because
      // we can only get TouchDown and TouchUp if it's connected.
      if (GvrController.TouchDown) {
        OnTouchDown.Invoke();
      }
      if (GvrController.TouchUp) {
        OnTouchUp.Invoke();
      }
    }
  }
}

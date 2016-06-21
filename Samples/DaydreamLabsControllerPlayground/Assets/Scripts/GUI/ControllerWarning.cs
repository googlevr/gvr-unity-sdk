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

using GVR.Input;
using UnityEngine;

namespace GVR.GUI {
  /// <summary>
  /// Manages a warning that is displayed when there is no controller
  /// connected. This will pause the game until a controller is found.
  /// </summary>
  public class ControllerWarning : MonoBehaviour {
    [Tooltip("Object that renders on top of the environment to dim it.")]
    public GameObject DimmingObject;

    [Tooltip("Object containing the warning text.")]
    public GameObject Warning;

    [Tooltip("The menu that will be used to trigger events telling objects to react to the " +
             "opening of a popup.")]
    public LevelSelectMenu Menu;

    [Tooltip("Time to wait after starting before warning. This provides a grace period to " +
             "connect before displaying a warning.")]
    public float StartupDelay = 1.5f;

    private bool displayingWarning;

    void Update() {
      if (Time.realtimeSinceStartup <= StartupDelay) {
        return;
      }
      if (GvrController.State != GvrConnectionState.Connected) {
        if (!displayingWarning) {
          Time.timeScale = 0.0f;
          Warning.gameObject.layer = 0;
          Menu.enabled = true;
          Menu.TriggerOpened();
          DimmingObject.gameObject.layer = 0;
          displayingWarning = true;
        }
      } else {
        if (displayingWarning) {
          Menu.TriggerWillClose();
          Time.timeScale = 1.0f;
          Warning.gameObject.layer = 20;
          Menu.enabled = false;
          DimmingObject.gameObject.layer = 20;
          displayingWarning = false;
          Menu.TriggerClosed();
        }
      }
    }
  }
}

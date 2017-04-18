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

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using System.Collections;

/// Provides visual feedback for the daydream controller.
[RequireComponent(typeof(Renderer))]
public class GvrControllerVisual : GvrBaseControllerVisual {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private Renderer controllerRenderer;

  public Material material_idle;
  public Material material_app;
  public Material material_system;
  public Material material_touchpad;

  void Awake() {
    controllerRenderer = GetComponent<Renderer>();
  }

  public override void OnVisualUpdate() {
    // Choose the appropriate material to render based on button states.
    if (GvrController.ClickButton) {
      controllerRenderer.material = material_touchpad;
    } else {
      // Change material to reflect button presses.
      if (GvrController.AppButton) {
        controllerRenderer.material = material_app;
      } else if (GvrController.Recentering) {
        controllerRenderer.material = material_system;
      } else {
        controllerRenderer.material = material_idle;
      }
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

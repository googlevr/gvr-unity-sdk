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

// This script is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using System.Collections;

/// Manages the battery indicator visual on the controller.
[RequireComponent(typeof(Renderer))]
public class GvrBatteryIndicator : GvrBaseControllerVisual {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private Renderer indicatorRenderer;

  /// Materials to show for each battery level.
  public Material matUnknown;
  public Material matCharging;
  public Material matFull;
  public Material matAlmostFull;
  public Material matMedium;
  public Material matLow;
  public Material matCriticalLow;

  void Awake() {
    indicatorRenderer = GetComponent<Renderer>();
  }

  /// Change the material when the battery state changes.
  public override void OnVisualUpdate() {
    if (GvrController.IsCharging) {
      indicatorRenderer.material = matCharging;
    } else {
      switch (GvrController.BatteryLevel) {
        case GvrControllerBatteryLevel.Full:
          indicatorRenderer.material = matFull;
          break;
        case GvrControllerBatteryLevel.AlmostFull:
          indicatorRenderer.material = matAlmostFull;
          break;
        case GvrControllerBatteryLevel.Medium:
          indicatorRenderer.material = matMedium;
          break;
        case GvrControllerBatteryLevel.Low:
          indicatorRenderer.material = matLow;
          break;
        case GvrControllerBatteryLevel.CriticalLow:
          indicatorRenderer.material = matCriticalLow;
          break;
        default:
          indicatorRenderer.material = matUnknown;
          break;
      }
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

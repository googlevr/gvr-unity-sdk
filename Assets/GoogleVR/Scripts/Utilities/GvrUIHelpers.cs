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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class GvrUIHelpers {
  /// Finds the meters scale for the local coordinate system
  /// of the root canvas that contains the canvasObject passed in.
  public static float GetMetersToCanvasScale(Transform canvasObject) {
    Canvas canvas = canvasObject.GetComponentInParent<Canvas>();
    if (canvas == null) {
      return 0.0f;
    }

    if (!canvas.isRootCanvas) {
      canvas = canvas.rootCanvas;
    }

    float metersToCanvasScale = canvas.transform.localScale.x;
    return metersToCanvasScale;
  }
}

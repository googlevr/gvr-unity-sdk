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

/// This script visualizes the location of the thumb on the touchpad
/// and controls the animation of the point as it moves.
[RequireComponent(typeof(Renderer))]
public class GvrTouchPoint : GvrBaseControllerVisual {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  /// Units are in meters.
  private static readonly Vector3 TOUCHPAD_POINT_DIMENSIONS = new Vector3(0.01f, 0.0004f, 0.01f);
  private const float TOUCHPAD_RADIUS = 0.012f;
  private const float TOUCHPAD_POINT_Y_OFFSET = 0.035f;
  private const float TOUCHPAD_POINT_ELEVATION = 0.0025f;
  private const float TOUCHPAD_POINT_SCALE_DURATION_SECONDS = 0.15f;

  private Renderer touchRenderer;
  private float elapsedScaleTimeSeconds;
  private bool wasTouching;

  [Tooltip("Material to use when the alpha is below one.")]
  public Material touchTransparent;
  [Tooltip("Material to use when the alpha is exactly one.")]
  public Material touchOpaque;

  void Awake() {
    touchRenderer = GetComponent<Renderer>();

    // Initialized the touchPoint at the correct scale.
    elapsedScaleTimeSeconds = TOUCHPAD_POINT_SCALE_DURATION_SECONDS;
  }

  public override void OnVisualUpdate() {
    // Determine if the touch point should be active.
    if (GvrController.ClickButton) {
      touchRenderer.enabled = false;
    } else {
      touchRenderer.enabled = true;
    }

    // Adjust material depending on transparency.
    if (GvrArmModel.Instance.preferredAlpha >= 1.0f) {
      touchRenderer.material = touchOpaque;
    } else {
      touchRenderer.material = touchTransparent;
    }

    if (GvrController.IsTouching) {
      // Reset the elapsedScaleTime when we start touching.
      // This flag is necessary because
      // GvrController.TouchDown sometimes becomes true a frame after GvrController.Istouching
      if (!wasTouching) {
        wasTouching = true;
        elapsedScaleTimeSeconds = 0.0f;
      }

      float x = (GvrController.TouchPos.x - 0.5f) * 2.0f * TOUCHPAD_RADIUS;
      float y = (GvrController.TouchPos.y - 0.5f) * 2.0f * TOUCHPAD_RADIUS;
      Vector3 scale = Vector3.Lerp(Vector3.zero,
                                   TOUCHPAD_POINT_DIMENSIONS,
                                   elapsedScaleTimeSeconds / TOUCHPAD_POINT_SCALE_DURATION_SECONDS);

      transform.localScale = scale;
      transform.localPosition = new Vector3(-x, TOUCHPAD_POINT_Y_OFFSET - y, TOUCHPAD_POINT_ELEVATION);
    } else {
      // Reset the elapsedScaleTime when we stop touching.
      // This flag is necessary because
      // GvrController.TouchDown sometimes becomes true a frame after GvrController.Istouching
      if (wasTouching) {
        wasTouching = false;
        elapsedScaleTimeSeconds = 0.0f;
      }

      Vector3 scale = Vector3.Lerp(TOUCHPAD_POINT_DIMENSIONS,
                                   Vector3.zero,
                                   elapsedScaleTimeSeconds / TOUCHPAD_POINT_SCALE_DURATION_SECONDS);

      transform.localScale = scale;
    }

    elapsedScaleTimeSeconds += Time.deltaTime;
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

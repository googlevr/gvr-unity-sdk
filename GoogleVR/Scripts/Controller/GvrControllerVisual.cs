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
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;
using System.Collections;

/// Provides visual feedback for the daydream controller.
[RequireComponent(typeof(Renderer))]
public class GvrControllerVisual : MonoBehaviour {

  /// Units are in meters.
  private static readonly Vector3 TOUCHPAD_POINT_DIMENSIONS = new Vector3(0.01f, 0.0004f, 0.01f);
  private const float TOUCHPAD_RADIUS = 0.012f;
  private const float TOUCHPAD_POINT_Y_OFFSET = 0.035f;
  private const float TOUCHPAD_POINT_ELEVATION = 0.0025f;
  private const float TOUCHPAD_POINT_SCALE_DURATION_SECONDS = 0.15f;

  private Renderer controllerRenderer;
  private Renderer touchRenderer;
  private float elapsedScaleTimeSeconds;
  private bool wasTouching;
  private MaterialPropertyBlock materialPropertyBlock;
  private int colorId;


  public GameObject touchPoint;
  public Material material_idle;
  public Material material_app;
  public Material material_system;
  public Material material_touchpad;
  public Material touchTransparent;
  public Material touchOpaque;

  void Awake() {
    controllerRenderer = GetComponent<Renderer>();
    touchRenderer = touchPoint.GetComponent<Renderer>();
    materialPropertyBlock = new MaterialPropertyBlock();
    colorId = Shader.PropertyToID("_Color");
  }

  void Update() {
    // Choose the appropriate material to render based on button states.
    if (GvrController.ClickButton) {
      controllerRenderer.material = material_touchpad;
      touchPoint.SetActive(false);
    } else {
      // Change material to reflect button presses.
      if (GvrController.AppButton) {
        controllerRenderer.material = material_app;
      } else if (GvrController.Recentering) {
        controllerRenderer.material = material_system;
      } else {
        controllerRenderer.material = material_idle;
      }

      // Draw the touch point and animate the scale change.
      touchPoint.SetActive(true);
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

        touchPoint.transform.localScale = scale;
        touchPoint.transform.localPosition = new Vector3(-x, TOUCHPAD_POINT_Y_OFFSET - y, TOUCHPAD_POINT_ELEVATION);
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

        touchPoint.transform.localScale = scale;
      }

      elapsedScaleTimeSeconds += Time.deltaTime;
    }

    // Adjust transparency.
    float alpha = GvrArmModel.Instance.alphaValue;
    Color color = new Color(1.0f, 1.0f, 1.0f, alpha);
    controllerRenderer.GetPropertyBlock(materialPropertyBlock);
    materialPropertyBlock.SetColor(colorId, color);
    controllerRenderer.SetPropertyBlock(materialPropertyBlock);
    if (alpha < 1.0f) {
      touchRenderer.material = touchTransparent;
      touchRenderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(colorId, color);
      touchRenderer.SetPropertyBlock(materialPropertyBlock);
    } else {
      touchRenderer.material = touchOpaque;
    }
  }
}

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

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
using System.Collections;

/// Adjusts the material's alpha value according to the value suggested
/// by the arm model.
[RequireComponent(typeof(Renderer))]
public abstract class GvrBaseControllerVisual : MonoBehaviour {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private Renderer materialRenderer;
  private MaterialPropertyBlock materialPropertyBlock;
  private int colorId;

  /// This is the preferred, maximum alpha value the object should have
  /// when it is a comfortable distance from the head.
  [Range(0.0f, 1.0f)]
  public float maximumAlpha = 1.0f;

  protected virtual void Start() {
    // Setup and cache material properties.
    materialRenderer = GetComponent<Renderer>();
    materialPropertyBlock = new MaterialPropertyBlock();
    colorId = Shader.PropertyToID("_Color");

    // Register the arm model updates.
    if (GvrArmModel.Instance != null) {
      GvrArmModel.Instance.OnArmModelUpdate += OnArmModelUpdate;
    } else {
      Debug.LogError("Unable to find GvrArmModel.");
    }
  }

  protected virtual void OnDestroy() {
    // Unregister the arm model updates.
    if (GvrArmModel.Instance != null) {
      GvrArmModel.Instance.OnArmModelUpdate -= OnArmModelUpdate;
    }
  }

  /// Override this method to update materials and other visual changes
  /// that need to happen every frame.
  public abstract void OnVisualUpdate();

  private void OnArmModelUpdate() {
    OnVisualUpdate();
    AlphaUpdate();
  }

  private void AlphaUpdate() {
    if (GvrArmModel.Instance != null &&
        materialRenderer.sharedMaterial.HasProperty(colorId)) {
      // Set the material's alpha to the multiplied preferred alpha.
      Color color = materialRenderer.sharedMaterial.color;
      color.a = maximumAlpha * GvrArmModel.Instance.preferredAlpha;
      materialRenderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(colorId, color);
      materialRenderer.SetPropertyBlock(materialPropertyBlock);
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

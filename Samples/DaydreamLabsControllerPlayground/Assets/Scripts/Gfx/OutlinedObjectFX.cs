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

namespace GVR.Gfx {
  public class OutlinedObjectFX : ImageFXBase {
    [Header("Output Properties")]
    [SerializeField]
    [Tooltip("The Material the resulting OffscreenBuffer will output to.")]
    Material outputMaterial;

    [SerializeField]
    float outlineThickness = 0.0035f;  // Normalized Screen Coordinates.

    [SerializeField]
    Color outlineColor = Color.white;

    [SerializeField]
    ScreenSpaceQuad[] quads;

    [SerializeField]
    OutlinedObject defaultObject;

    private OutlinedObject activeObject;
    private int id_Thickness;
    private int id_Color;
    private int id_Texture;
    private Material meshMaterial;

    /// <summary>
    /// Sets the active Outlined Object and passes its world-space Bounds to all the
    /// ScreenSpaceQuad objects in the quads array.
    /// </summary>
    /// <param name="obj"></param>
    public void SetActiveObject(OutlinedObject obj) {
      activeObject = obj;
      RenderTexture tmp = RenderTexture.active;
      RenderTexture.active = bufferObj.Buffer;
      GL.Clear(true, true, clearColor, 1f);
      RenderTexture.active = tmp;
    }

    public OutlinedObject GetActiveObject() {
      return activeObject;
    }

    protected override void Awake() {
      base.Awake();
      id_Color = Shader.PropertyToID(ShaderLib.Variables.VECTOR_OUTLINE_COLOR);
      id_Thickness = Shader.PropertyToID(ShaderLib.Variables.FLOAT_OUTLINE_THICKNESS);
      id_Texture = Shader.PropertyToID(ShaderLib.Variables.SAMPLER2D_MAINTEX);
      meshMaterial = new Material(replacementShader);
    }

    protected override void  OnEnable() {
      base.OnEnable();
      outputMaterial.SetTexture(id_Texture, bufferObj.Buffer);
      SetActiveObject(defaultObject); // setup a default
    }

    protected override void  OnDisable() {
      base.OnDisable();
    }

    void LateUpdate() {
      outputMaterial.SetColor(id_Color, outlineColor);
      outputMaterial.SetFloat(id_Thickness, outlineThickness);
      for (int i = 0; i < quads.Length; i++) {
        quads[i].targetBounds = activeObject == null ? new Bounds() : activeObject.Bounds;
      }
    }

    public override void Render(Camera cam) {
      if (activeObject != null) {
        MeshBlit.Render(activeObject.Mesh, activeObject.TRS, cam, bufferObj.Buffer,
                        clearColor, meshMaterial);
      }
    }
  }
}

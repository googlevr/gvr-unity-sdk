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
using UnityEditor;

namespace GVR.Gfx {
  /// <summary>
  /// If the texture is assigned, the keyword 'keywordName_ON' (specified in attribute parameter)
  /// will be enabled on this material. Useful for optional textures to reduce TLU instructions.
  /// EXAMPLE: one could use this to turn normal maps on/off to save on fill costs for a single
  /// shader based on whether or not a normal map is present.
  /// NOTE: This is intended to be used with '#pragma shader_feature' in the Shader.
  /// </summary>
  public class OptionalTextureDrawer : MaterialPropertyDrawer {
    private string keywordName;

    public OptionalTextureDrawer(string keyword)
        : base() { // Note: this gets called on import of the Shader, NOT on creation of the Inspector
      this.keywordName = keyword;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label,
                               UnityEditor.MaterialEditor editor) {
      EditorGUI.BeginChangeCheck();
      prop.textureValue = editor.TextureProperty(prop, label);
      if (EditorGUI.EndChangeCheck()) {
        if (prop.textureValue == null) {
          (editor.target as Material).DisableKeyword(keywordName + "_ON");
        } else {
          (editor.target as Material).EnableKeyword(keywordName + "_ON");
        }
      }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label,
                                            UnityEditor.MaterialEditor editor) {
      return 0;
    }
  }
}

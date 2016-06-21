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
  [CustomEditor(typeof(Material))]
  public class MaterialEditor : UnityEditor.MaterialEditor {
    const string FORMAT_WARNING = "A Custom Render Queue of {0} is Defined.";

    Material m { get { return (Material)target; } }

    protected override void OnHeaderGUI() {
      base.OnHeaderGUI();
      serializedObject.Update();
      SerializedProperty customRenderQueue = serializedObject.FindProperty("m_CustomRenderQueue");
      if (customRenderQueue.intValue != -1 && customRenderQueue.intValue != m.shader.renderQueue) {
        EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
          EditorGUILayout.HelpBox(string.Format(FORMAT_WARNING, customRenderQueue.intValue),
                                  MessageType.Warning, true);
          EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(customRenderQueue);
            if (GUILayout.Button("Clear", GUILayout.Width(50))) {
              customRenderQueue.intValue = -1;
            }
          EditorGUILayout.EndHorizontal();
          serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
      }
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
      base.OnPreviewGUI(r, background);
      serializedObject.Update();
      SerializedProperty customRenderQueue = serializedObject.FindProperty("m_CustomRenderQueue");
      if (customRenderQueue.intValue != -1 && customRenderQueue.intValue != m.shader.renderQueue) {
        Rect rect = new Rect(new Vector2(r.x - 5, r.y), new Vector2(20, 20));
        GUIContent gc = EditorGUIUtility.IconContent("console.warnicon",
                                                     string.Format(FORMAT_WARNING,
                                                                   customRenderQueue.intValue));
        UnityEngine.GUI.Label(rect, gc);
      }
    }
  }
}

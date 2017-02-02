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
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(PagedScrollBar))]
public class PagedScrollbarEditor : ScrollbarEditor {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private SerializedProperty pagedScrollRect;

  protected override void OnEnable() {
    base.OnEnable();
    pagedScrollRect = serializedObject.FindProperty(PagedScrollBar.PAGED_SCROLL_RECT_PROP_NAME);
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();
    EditorGUILayout.PropertyField(pagedScrollRect);
    serializedObject.ApplyModifiedProperties();

    base.OnInspectorGUI();
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

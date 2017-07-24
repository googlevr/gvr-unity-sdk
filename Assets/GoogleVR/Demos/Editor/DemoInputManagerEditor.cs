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

[CustomEditor(typeof(DemoInputManager))]
public class DemoInputManagerEditor : Editor {
  SerializedProperty emulatedPlatformTypeProp;
  SerializedProperty gvrControllerMainProp;
  SerializedProperty gvrControllerPointerProp;
  SerializedProperty gvrReticlePointerProp;

  void OnEnable () {
    gvrControllerMainProp =
      serializedObject.FindProperty(DemoInputManager.CONTROLLER_MAIN_PROP_NAME);
    gvrControllerPointerProp =
      serializedObject.FindProperty(DemoInputManager.CONTROLLER_POINTER_PROP_NAME);
    gvrReticlePointerProp =
      serializedObject.FindProperty(DemoInputManager.RETICLE_POINTER_PROP_NAME);

    emulatedPlatformTypeProp =
      serializedObject.FindProperty(DemoInputManager.EMULATED_PLATFORM_PROP_NAME);
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();

    EditorGUILayout.PropertyField(gvrControllerMainProp);
    EditorGUILayout.PropertyField(gvrControllerPointerProp);
    EditorGUILayout.PropertyField(gvrReticlePointerProp);

    if (DemoInputManager.playerSettingsHasCardboard() ==
        DemoInputManager.playerSettingsHasDaydream()) {
      // Show the platform emulation dropdown only if both or neither VR SDK selected in
      // Player Settings > Virtual Reality supported,
      EditorGUILayout.PropertyField(emulatedPlatformTypeProp);
    }

    serializedObject.ApplyModifiedProperties();
  }
}

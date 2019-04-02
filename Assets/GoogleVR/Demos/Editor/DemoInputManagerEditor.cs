//-----------------------------------------------------------------------
// <copyright file="DemoInputManagerEditor.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleVR.Demos
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Allows DemoInputManager fields to be accessed through the Unity Editor.</summary>
    [CustomEditor(typeof(DemoInputManager))]
    public class DemoInputManagerEditor : Editor
    {
        private SerializedProperty emulatedPlatformTypeProp;
        private SerializedProperty gvrControllerMainProp;
        private SerializedProperty gvrControllerPointerProp;
        private SerializedProperty gvrReticlePointerProp;

        /// @cond
        /// <summary>The Editor's OnInspectorGUI method.</summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Add clickable script field, as would have been provided by DrawDefaultInspector()
            MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(gvrControllerMainProp);
            EditorGUILayout.PropertyField(gvrControllerPointerProp, true);
            EditorGUILayout.PropertyField(gvrReticlePointerProp);

            if (DemoInputManager.playerSettingsHasCardboard() ==
                DemoInputManager.playerSettingsHasDaydream())
            {
                // Show the platform emulation dropdown only if both or neither VR SDK selected in
                // Player Settings > Virtual Reality supported,
                EditorGUILayout.PropertyField(emulatedPlatformTypeProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// @endcond
        /// <summary>The MonoBehavior's OnEnable behavior.</summary>
        private void OnEnable()
        {
            gvrControllerMainProp =
                serializedObject.FindProperty(DemoInputManager.CONTROLLER_MAIN_PROP_NAME);
            gvrControllerPointerProp =
                serializedObject.FindProperty(DemoInputManager.CONTROLLER_POINTER_PROP_NAME);
            gvrReticlePointerProp =
                serializedObject.FindProperty(DemoInputManager.RETICLE_POINTER_PROP_NAME);

            emulatedPlatformTypeProp =
                serializedObject.FindProperty(DemoInputManager.EMULATED_PLATFORM_PROP_NAME);
        }
    }
}

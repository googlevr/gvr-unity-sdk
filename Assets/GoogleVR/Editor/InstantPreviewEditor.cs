//-----------------------------------------------------------------------
// <copyright file="InstantPreviewEditor.cs" company="Google LLC.">
// Copyright 2019 Google LLC. All rights reserved.
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

namespace Gvr.Internal
{
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

    /// <summary>Custom editor for `InstantPreview`.</summary>
    /// <remarks>
    /// Enhances the visualization of the `overrideUserPreferencesValues` and ensures that it can
    /// only be edited if the application isn't playing or if `loadDaydreamUserPrefs` is turned off.
    /// </remarks>
    [CustomEditor(typeof(InstantPreview)), CanEditMultipleObjects]
    public class InstantPreviewEditor : Editor
    {
        private string[] userPrefsHandednessNames =
        {
            "Error",
            "Right",
            "Left",
        };

        /// @cond
        /// <summary>A builtin method of the `Editor` class.</summary>
        /// <remarks>Implement this function to make a custom inspector.</remarks>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            InstantPreview ip = target as InstantPreview;
            if (ip.overrideDeviceUserPrefs)
            {
                EditorGUI.indentLevel++;
                ip.editorUserPrefs.handedness = fromIndex(EditorGUILayout.Popup(
                        "Handedness",
                        toIndex(ip.editorUserPrefs.handedness),
                        userPrefsHandednessNames));
                EditorGUI.indentLevel--;
            }
        }

        private static int toIndex(GvrSettings.UserPrefsHandedness value)
        {
            switch (value)
            {
                case GvrSettings.UserPrefsHandedness.Error:
                    return 0;
                case GvrSettings.UserPrefsHandedness.Right:
                    return 1;
                case GvrSettings.UserPrefsHandedness.Left:
                    return 2;
                default:
                    return 0;
            }
        }

        private static GvrSettings.UserPrefsHandedness fromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return GvrSettings.UserPrefsHandedness.Error;
                case 1:
                    return GvrSettings.UserPrefsHandedness.Right;
                case 2:
                    return GvrSettings.UserPrefsHandedness.Left;
                default:
                    return GvrSettings.UserPrefsHandedness.Error;
            }
        }
    }
}

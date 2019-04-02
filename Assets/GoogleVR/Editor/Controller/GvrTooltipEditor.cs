//-----------------------------------------------------------------------
// <copyright file="GvrTooltipEditor.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>A custom editor for the `GvrTooltip` script.</summary>
/// <remarks>
/// This exists to surface to the user that the tooltip changes based on handedness, and to make it
/// easy to preview the handedness settings.
/// </remarks>
[CustomEditor(typeof(GvrTooltip)), CanEditMultipleObjects]
public class GvrTooltipEditor : Editor
{
    /// @cond
    /// <summary>A builtin method of the `Editor` class.</summary>
    /// <remarks>Implement this function to make a custom inspector.</remarks>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Current Handedness",
                                   GvrSettings.Handedness.ToString(),
                                   EditorStyles.boldLabel);
        if (GUILayout.Button("Change Handedness"))
        {
            EditorWindow.GetWindow(typeof(GvrEditorSettings));
        }
    }

    /// @endcond
}

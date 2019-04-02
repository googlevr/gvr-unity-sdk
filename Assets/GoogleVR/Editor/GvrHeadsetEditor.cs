//-----------------------------------------------------------------------
// <copyright file="GvrHeadsetEditor.cs" company="Google LLC.">
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

using UnityEditor;
using UnityEngine;

/// <summary>An Editor window modification utility for GvrHeadset.</summary>
[CustomEditor(typeof(GvrHeadset)), CanEditMultipleObjects]
public class GvrHeadsetEditor : Editor
{
#if UNITY_EDITOR
    private Rect rect;
    private float infoHeight;
    private string infoText;
    private int numInfoLines = 2;

    /// <summary>This Editor's OnInspectorGUI behavior.</summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CreateSupportsPositionalHeadTrackingInfoBox();
        if (GvrHeadset.editorSupportsPositionalHeadTracking)
        {
            if (GUILayout.Button("Disable Positional Tracking Support"))
            {
                GvrHeadset.editorSupportsPositionalHeadTracking = false;
            }
        }
        else
        {
            if (GUILayout.Button("Enable Positional Tracking Support"))
            {
                GvrHeadset.editorSupportsPositionalHeadTracking = true;
            }
        }
    }

    private void CreateSupportsPositionalHeadTrackingInfoBox()
    {
        if (GvrHeadset.editorSupportsPositionalHeadTracking)
        {
            infoText = "Editor supports Positional Tracking.";
            numInfoLines = 2;
        }
        else
        {
            infoText = "Editor does not support Positional Tracking.";
            numInfoLines = 2;
        }

        infoHeight = GvrInfoDrawer.GetHeightForLines(numInfoLines);
        Rect rect = EditorGUILayout.GetControlRect(false, infoHeight);
        GvrInfoDrawer.Draw(rect, infoText, MessageType.None);
    }
#endif // UNITY_EDITOR
}

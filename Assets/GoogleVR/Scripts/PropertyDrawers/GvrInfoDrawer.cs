//-----------------------------------------------------------------------
// <copyright file="GvrInfoDrawer.cs" company="Google Inc.">
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

#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>Use to draw a `GvrInfo` in the inspector.</summary>
[CustomPropertyDrawer(typeof(GvrInfo))]
public class GvrInfoDrawer : DecoratorDrawer
{
    /// <summary>Calculates the height for an Info box based on number of lines.</summary>
    /// <param name="numLines">The number of lines.</param>
    /// <returns>A height.</returns>
    public static float GetHeightForLines(int numLines)
    {
        return EditorGUIUtility.singleLineHeight * numLines;
    }

    /// @cond
    /// <summary>A DecoratorDrawer builtin to draw an Info box.</summary>
    /// <param name="position">The position to draw the Info box.</param>
    /// <param name="text">The text to write in the Info box.</param>
    /// <param name="messageType">The message type of the Info box.</param>
    public static void Draw(Rect position, string text, MessageType messageType)
    {
        position.height -= EditorGUIUtility.standardVerticalSpacing;

        int oldFontSize = EditorStyles.helpBox.fontSize;
        EditorStyles.helpBox.fontSize = 11;
        FontStyle oldFontStyle = EditorStyles.helpBox.fontStyle;
        EditorStyles.helpBox.fontStyle = FontStyle.Bold;
        bool oldWordWrap = EditorStyles.helpBox.wordWrap;
        EditorStyles.helpBox.wordWrap = false;

        EditorGUI.HelpBox(position, text, messageType);

        EditorStyles.helpBox.fontSize = oldFontSize;
        EditorStyles.helpBox.fontStyle = oldFontStyle;
        EditorStyles.helpBox.wordWrap = oldWordWrap;
    }

    /// @endcond
    /// @cond
    /// <inheritdoc/>
    public override float GetHeight()
    {
        return GetHeightForLines((attribute as GvrInfo).numLines);
    }

    /// @endcond
    /// @cond
    /// <summary>A MonoBehavior builtin to draw the Info box on GUI render.</summary>
    /// <param name="position">The position of the Info box.</param>
    public override void OnGUI(Rect position)
    {
        Draw(position, (attribute as GvrInfo).text, (attribute as GvrInfo).messageType);
    }

    /// @endcond
}
#endif  // UNITY_EDITOR

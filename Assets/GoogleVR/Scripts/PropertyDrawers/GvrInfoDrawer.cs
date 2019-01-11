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
using UnityEditor;
using UnityEngine;
using System.Collections;
using System;

/// Use to display an Info box in the inspector for a Monobehaviour or ScriptableObject.
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class GvrInfo : PropertyAttribute
{
    public string text;
    public int numLines;
    public MessageType messageType;

    public GvrInfo(string text, int numLines, MessageType messageType)
    {
        this.text = text;
        this.numLines = numLines;
        this.messageType = messageType;
    }
}

[CustomPropertyDrawer(typeof(GvrInfo))]
public class GvrInfoDrawer : DecoratorDrawer
{
    GvrInfo info
    {
        get { return ((GvrInfo)attribute); }
    }

    public override float GetHeight()
    {
        return GetHeightForLines(info.numLines);
    }

    public override void OnGUI(Rect position)
    {
        Draw(position, info.text, info.messageType);
    }

    public static float GetHeightForLines(int numLines)
    {
        return EditorGUIUtility.singleLineHeight * numLines;
    }

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
}
#endif  // UNITY_EDITOR

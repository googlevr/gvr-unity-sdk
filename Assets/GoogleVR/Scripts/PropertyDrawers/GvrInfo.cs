//-----------------------------------------------------------------------
// <copyright file="GvrInfo.cs" company="Google Inc.">
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

/// <summary>
/// Use to display an Info box in the inspector for a Monobehaviour or ScriptableObject.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class GvrInfo : PropertyAttribute
{
    /// <summary>The text which should be displayed in the Info box.</summary>
    public string text;

    /// <summary>The number of lines the Info box should display.</summary>
    public int numLines;

    /// <summary>The message type of this info.</summary>
    /// <remarks>A Unity builtin, e.g. Info, Warning, Error.</remarks>
    public MessageType messageType;

    /// <summary>Initializes a new instance of the <see cref="GvrInfo" /> class.</summary>
    /// <param name="text">The text.</param>
    /// <param name="numLines">The number of lines.</param>
    /// <param name="messageType">The message type.</param>
    public GvrInfo(string text, int numLines, MessageType messageType)
    {
        this.text = text;
        this.numLines = numLines;
        this.messageType = messageType;
    }
}

#endif  // UNITY_EDITOR

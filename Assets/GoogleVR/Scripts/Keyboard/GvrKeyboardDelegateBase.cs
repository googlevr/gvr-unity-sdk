//-----------------------------------------------------------------------
// <copyright file="GvrKeyboardDelegateBase.cs" company="Google Inc.">
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

using UnityEngine;
using System;

/// <summary>An abstract class instead of an interface so that it can be exposed in Unity's
/// editor. It inherits from MonoBehaviour so that it can be directly used as a game object.
/// </summary>
public abstract class GvrKeyboardDelegateBase : MonoBehaviour
{
    /// <summary>Called to show the keyboard.</summary>
    public abstract void OnKeyboardShow();

    /// <summary>Called to hide the keyboard.</summary>
    public abstract void OnKeyboardHide();

    /// <summary>Called to update the keyboard.</summary>
    /// <param name="edit_text">The current text for the keyboard.</param>
    public abstract void OnKeyboardUpdate(string edit_text);

    /// <summary>Called when the ENTER key is pressed on the keyboard.</summary>
    /// <param name="edit_text">The current text for the keyboard.</param>
    public abstract void OnKeyboardEnterPressed(string edit_text);

    /// <summary>Called when there is an error with the keyboard.</summary>
    /// <param name="errorCode">The code of the error encountered.</param>
    public abstract void OnKeyboardError(GvrKeyboardError errorCode);

    /// <summary>Event for the keyboard being hidden.</summary>
    public abstract event EventHandler KeyboardHidden;

    /// <summary>Event for the keyboard being shown.</summary>
    public abstract event EventHandler KeyboardShown;
}

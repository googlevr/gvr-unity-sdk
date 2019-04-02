//-----------------------------------------------------------------------
// <copyright file="GvrBaseArmModel.cs" company="Google Inc.">
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Interface for a mathematical Arm model for 3DoF controllers.</summary>
/// <remarks>
/// Uses the orientation and location of the physical controller, and predicts the location of the
/// controller and pointer to determine where to place the controller model within the scene.
/// </remarks>
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrBaseArmModel")]
public abstract class GvrBaseArmModel : MonoBehaviour
{
    /// <summary>
    /// Gets a Vector to represent the controller's location relative to the player's head position.
    /// </summary>
    /// <value>A Vector to represent the controller's location.</value>
    public abstract Vector3 ControllerPositionFromHead { get; }

    /// <summary>
    /// Gets a Quaternion to represent the controller's rotation relative to the player's head
    /// position.
    /// </summary>
    /// <value>A Quaternion to represent the controller's rotation.</value>
    public abstract Quaternion ControllerRotationFromHead { get; }

    /// <summary>Gets the suggested rendering alpha value of the controller.</summary>
    /// <remarks>
    /// This is to prevent the controller from intersecting the player's face.
    /// <para>
    /// The range is always 0 - 1.
    /// </para></remarks>
    /// <value>The suggested rendering alpha value of the controller.</value>
    public abstract float PreferredAlpha { get; }

    /// <summary>Gets the suggested rendering alpha value of the controller tooltips.</summary>
    /// <remarks>
    /// This is to only display the tooltips when the player is looking at the controller, and also
    /// to prevent the tooltips from intersecting the player's face.
    /// </remarks>
    /// <value>The suggested rendering alpha value of the controller tooltips.</value>
    public abstract float TooltipAlphaValue { get; }
}

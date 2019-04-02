//-----------------------------------------------------------------------
// <copyright file="IGvrInputModuleController.cs" company="Google Inc.">
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
using UnityEngine.EventSystems;

/// <summary>
/// Interface for manipulating an input module used by `GvrPointerInputModuleImpl`.
/// </summary>
public interface IGvrInputModuleController
{
    /// <summary>Gets a reference to the event system.</summary>
    /// <value>A reference to the event system.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.LegacyGvrStyleRules",
        "VR1001:AccessibleNonConstantPropertiesMustBeUpperCamelCase",
        Justification = "Legacy Public API.")]
    EventSystem eventSystem { get; }

    /// <summary>Gets the raycast result cache list.</summary>
    /// <value>The raycast result cache list.</value>
    List<RaycastResult> RaycastResultCache { get; }

    /// <summary>Whether the controller should be activated.</summary>
    /// <returns>Returns `true` if the controller should be activated, `false` otherwise.</returns>
    bool ShouldActivate();

    /// <summary>Deactivate the controller.</summary>
    void Deactivate();

    /// <summary>
    /// Given two game objects, return a common root game object, or null if there is no common
    /// root.
    /// </summary>
    /// <param name="g1">The first `GameObject`.</param>
    /// <param name="g2">The second `GameObject`.</param>
    /// <returns>The common root.</returns>
    GameObject FindCommonRoot(GameObject g1, GameObject g2);

    /// <summary>Gets a `BaseEventData` that can be used by the `EventSystem`.</summary>
    /// <returns>A `BaseEventData` that can be used by the `EventSystem`.</returns>
    BaseEventData GetBaseEventData();

    /// <summary>Return the first valid raycast result.</summary>
    /// <param name="candidates">
    /// The list of `RaycastResults` to search for the first Raycast.
    /// </param>
    /// <returns>The first raycast.</returns>
    RaycastResult FindFirstRaycast(List<RaycastResult> candidates);
}

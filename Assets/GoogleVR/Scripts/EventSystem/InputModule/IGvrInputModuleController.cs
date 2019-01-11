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
/// Interface for manipulating an InputModule used by _GvrPointerInputModuleImpl_.
/// </summary>
public interface IGvrInputModuleController
{
    /// <summary>Reference to EventSystem.</summary>
    EventSystem eventSystem { get; }

    /// <summary>Raycast result cache list.</summary>
    List<RaycastResult> RaycastResultCache { get; }

    /// <summary>Should the controller be activated.</summary>
    bool ShouldActivate();

    /// <summary>Deactivate the controller.</summary>
    void Deactivate();

    /// <summary>Given two game objects, return a common root game object,
    /// or null if there is no common root.</summary>
    GameObject FindCommonRoot(GameObject g1, GameObject g2);

    /// <summary>Generate a BaseEventData that can be used by the EventSystem.
    /// </summary>
    BaseEventData GetBaseEventData();

    /// <summary>Return the first valid raycast result.</summary>
    RaycastResult FindFirstRaycast(List<RaycastResult> candidates);
}

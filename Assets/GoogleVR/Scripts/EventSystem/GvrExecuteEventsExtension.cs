//-----------------------------------------------------------------------
// <copyright file="GvrExecuteEventsExtension.cs" company="Google Inc.">
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
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script extends the standard Unity `EventSystem` events with GVR-specific events.
/// </summary>
public static class GvrExecuteEventsExtension
{
    /// <summary>Gets a handler for hover events.</summary>
    /// <value>A handler for hover events.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.LegacyGvrStyleRules",
        "VR1001:AccessibleNonConstantPropertiesMustBeUpperCamelCase",
        Justification = "Legacy Public API.")]
    public static ExecuteEvents.EventFunction<IGvrPointerHoverHandler> pointerHoverHandler
    {
        get { return Execute; }
    }

    private static void Execute(IGvrPointerHoverHandler handler, BaseEventData eventData)
    {
        handler.OnGvrPointerHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
    }
}

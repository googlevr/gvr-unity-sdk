//-----------------------------------------------------------------------
// <copyright file="GvrPointerEventDataExtension.cs" company="Google LLC.">
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

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class extends the Unity `PointerEventData` struct with GoogleVR data accessors.
/// </summary>
public static class GvrPointerEventDataExtension
{
    /// <summary>
    /// Returns the `GvrControllerButton` mask of buttons that went down to trigger the event.
    /// </summary>
    /// <returns>
    /// The `GvrControllerButton` mask of buttons that went down to trigger the event.
    /// </returns>
    /// <param name="pointerEventData">Pointer event data.</param>
    public static GvrControllerButton GvrGetButtonsDown(this PointerEventData pointerEventData)
    {
        GvrPointerEventData gped = pointerEventData as GvrPointerEventData;
        if (gped == null)
        {
            return 0;
        }

        return gped.gvrButtonsDown;
    }

    /// <summary>Returns the `GvrControllerInputDevice` that triggered the event.</summary>
    /// <returns>The get controller input device.</returns>
    /// <param name="pointerEventData">Pointer event data.</param>
    public static GvrControllerInputDevice GvrGetControllerInputDevice(
        this PointerEventData pointerEventData)
    {
        return GvrControllerInput.GetDevice((GvrControllerHand)pointerEventData.pointerId);
    }
}

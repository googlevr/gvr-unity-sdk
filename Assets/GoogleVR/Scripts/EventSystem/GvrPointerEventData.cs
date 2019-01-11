//-----------------------------------------------------------------------
// <copyright file="GvrPointerEventData.cs" company="Google Inc.">
// Copyright 2018 Google Inc. All rights reserved.
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
using UnityEngine.EventSystems;
using System.Collections;

/// This script extends the Unity PointerEventData struct with GoogleVR
/// specific data.
public class GvrPointerEventData : PointerEventData
{
    /// <summary> Constructs a new instance of GvrPointerEventData.</summary>
    /// <param name="eventSystem">The event system associated with this event.</param>
    public GvrPointerEventData(EventSystem eventSystem) : base(eventSystem)
    {
    }

    /// <summary>The mask of buttons that are currently down.</summary>
    public GvrControllerButton gvrButtonsDown;
}

/// This class extends the Unity PointerEventData struct with GoogleVR
/// data accessors.
public static class GvrPointerEventDataExtension
{
    /// Returns the `GvrControllerButton` mask of buttons that went down to trigger the event.
    public static GvrControllerButton GvrGetButtonsDown(this PointerEventData pointerEventData)
    {
        GvrPointerEventData gped = pointerEventData as GvrPointerEventData;
        if (gped == null)
        {
            return 0;
        }

        return gped.gvrButtonsDown;
    }

    /// Returns the `GvrControllerInputDevice` that triggered the event.
    public static GvrControllerInputDevice GvrGetControllerInputDevice(this PointerEventData pointerEventData)
    {
        return GvrControllerInput.GetDevice((GvrControllerHand)pointerEventData.pointerId);
    }
}

//-----------------------------------------------------------------------
// <copyright file="GvrScrollSettings.cs" company="Google Inc.">
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
using System.Collections;

/// Used to override the global scroll settings in _GvrPointerScrollInput_
/// for the GameObject that this script is attached to.
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrScrollSettings")]
public class GvrScrollSettings : MonoBehaviour, IGvrScrollSettings
{
    /// Override the Inertia property in _GvrPointerScrollInput_ for this object.
    ///
    /// Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the decelerationRate.
    [Tooltip("Determines if movement inertia is enabled.")]
    public bool inertiaOverride = true;

    /// The deceleration rate is the speed reduction per second.
    /// A value of 0.5 halves the speed each second. The default is 0.05.
    /// The deceleration rate is only used when inertia is enabled.
    [Tooltip("The rate at which movement slows down.")]
    public float decelerationRateOverride = 0.05f;

    /// <summary>Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the decelerationRate.
    /// </summary>
    public bool InertiaOverride
    {
        get { return inertiaOverride; }
    }

    /// <summary>The deceleration rate is the speed reduction per second.
    /// A value of 0.5 halves the speed each second.
    /// </summary>
    /// <remarks>The default is 0.05.
    /// The deceleration rate is only used when inertia is enabled.
    /// </remarks>
    public float DecelerationRateOverride
    {
        get { return decelerationRateOverride; }
    }
}

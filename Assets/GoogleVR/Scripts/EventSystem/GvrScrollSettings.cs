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

using System.Collections;
using UnityEngine;

/// <summary>
/// Used to override the global scroll settings in `GvrPointerScrollInput` for the `GameObject` that
/// this script is attached to.
/// </summary>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrScrollSettings")]
public class GvrScrollSettings : MonoBehaviour, IGvrScrollSettings
{
    /// <summary>Override the `Inertia` property in `GvrPointerScrollInput` for this object.</summary>
    /// <remarks>
    /// Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the `decelerationRate`.
    /// </remarks>
    [Tooltip("Determines if movement inertia is enabled.")]
    public bool inertiaOverride = true;

    /// <summary>The deceleration rate is the speed reduction per second.</summary>
    /// <remarks>
    /// A value of 0.5 halves the speed each second. The default is 0.05.  The deceleration rate is
    /// only used when `inertia` is `true`.
    /// </remarks>
    [Tooltip("The rate at which movement slows down.")]
    public float decelerationRateOverride = 0.05f;

    /// <summary>
    /// Gets a value indicating whether the overridden value for `interia` is enabled.
    /// </summary>
    /// <remarks><para>
    /// This value will override the Inertia property in `GvrPointerScrollInput` for this object.
    /// </para><para>
    /// Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the `decelerationRate`.
    /// </para></remarks>
    /// <value>
    /// Gets the overridden value for whether to use `interia` for the `GvrPointerScrollInput`.
    /// </value>
    public bool InertiaOverride
    {
        get { return inertiaOverride; }
    }

    /// <summary>Gets the deceleration rate override value.</summary>
    /// <remarks><para>
    /// This value will override the deceleration rate in `GvrPointerScrollInput` for this
    /// object.
    /// </para><para>
    /// The deceleration rate is the speed reduction per second.
    /// </para><para>
    /// A value of 0.5 halves the speed each second.
    /// </para><para>
    /// The deceleration rate is only used when `inertia` is `true`.
    /// </para></remarks>
    /// <value>Gets the deceleration rate override value for the `GvrPointerScrollInput`.</value>
    public float DecelerationRateOverride
    {
        get { return decelerationRateOverride; }
    }
}

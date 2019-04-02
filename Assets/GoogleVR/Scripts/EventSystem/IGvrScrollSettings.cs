//-----------------------------------------------------------------------
// <copyright file="IGvrScrollSettings.cs" company="Google Inc.">
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
/// Interface to implement to override the global scroll settings in `GvrPointerScrollInput` for an
/// object.
/// </summary>
/// <remarks><para>
/// Must be implmented by a component. It will override the scroll settings for the
/// `GameObject` that the component is attached to.
/// </para><para>
/// Can use `GvrScrollSettings` to override scroll settings for any existing UI type,
/// or a custom UI component can implement this directly to override the scroll settings
/// for the UI component's use case.
/// </para></remarks>
public interface IGvrScrollSettings
{
    /// <summary>
    /// Gets a value indicating whether the interia is enabled via its override value.
    /// </summary>
    /// <remarks><para>
    /// This value will override the Inertia property in `GvrPointerScrollInput` for this object.
    /// </para><para>
    /// Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the deceleration rate.
    /// </para></remarks>
    /// <value>
    /// Gets whether the interia is enabled via its override value for the `GvrPointerScrollInput`.
    /// </value>
    bool InertiaOverride { get; }

    /// <summary>Gets the deceleration rate override value.</summary>
    /// <remarks><para>
    /// This value will override the deceleration rate in `GvrPointerScrollInput` for this object.
    /// </para><para>
    /// The deceleration rate is the speed reduction per second.  A value of 0.5 halves the speed
    /// each second.  The deceleration rate is only used when `inertia` is `true`.
    /// </para></remarks>
    /// <value>Gets the default deceleration rate for the `GvrPointerScrollInput`.</value>
    float DecelerationRateOverride { get; }
}

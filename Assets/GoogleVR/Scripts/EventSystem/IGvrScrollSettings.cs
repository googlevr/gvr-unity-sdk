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

using UnityEngine;
using System.Collections;

/// Interface to implement to override the global scroll settings
/// in _GvrPointerScrollInput_ for an object.
///
/// Must be implmented by a component. It will override the scroll settings for the
/// GameObject that the component is attached to.
///
/// Can use _GvrScrollSettings_ To override scroll settings for any existing UI type,
/// or a custom UI component can implement this directly to override the scroll settings
/// for the UI component's use case.
public interface IGvrScrollSettings
{
    /// Override the Inertia property in _GvrPointerScrollInput_ for this object.
    ///
    /// Inertia means that scroll events will continue for a while after the user stops
    /// touching the touchpad. It gradually slows down according to the decelerationRate.
    bool InertiaOverride { get; }

    /// Override the DecelerationRate property in _GvrPointerScrollInput_ for this object.
    ///
    /// The deceleration rate is the speed reduction per second.
    /// A value of 0.5 halves the speed each second.
    /// The deceleration rate is only used when inertia is enabled.
    float DecelerationRateOverride { get; }
}

//-----------------------------------------------------------------------
// <copyright file="GvrBetaSettings.cs" company="Google LLC">
// Copyright 2019 Google LLC All rights reserved.
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

/// <summary>Daydream Beta API.</summary>
/// <remarks>
/// This API surface is for experimental purposes and may change or be removed in any future release
/// without forewarning.
/// </remarks>
namespace GoogleVR.Beta
{
    using System;
    using System.Runtime.InteropServices;
    using Gvr.Internal;
    using UnityEngine;

    /// <summary>
    /// Types of Daydream features that the user can enable or disable at runtime.
    /// </summary>
    /// <remarks>Matches the C API enum `gvr_runtime_feature`.</remarks>
    public enum GvrBetaFeature
    {
        /// <summary>The see-through feature.</summary>
        SeeThrough = 1001,
    }

    /// <summary>Daydream beta settings API.</summary>
    public static class GvrBetaSettings
    {
        /// <summary>
        /// Queries whether a particular GVR feature is supported by the underlying platform.
        /// </summary>
        /// <param name="feature">The `GvrBetaFeature` being queried.</param>
        /// <returns>Returns `true` if the feature is supported, `false` otherwise.</returns>
        public static bool IsFeatureSupported(GvrBetaFeature feature)
        {
            return GvrBetaSettingsProvider.IsFeatureSupported(feature);
        }

        /// <summary>
        /// Queries whether a particular GVR feature has been enabled by the user.
        /// </summary>
        /// <param name="feature">The `GvrBetaFeature` being queried.</param>
        /// <returns>Returns `true` if the feature is enabled, `false` otherwise.</returns>
        public static bool IsFeatureEnabled(GvrBetaFeature feature)
        {
            return GvrBetaSettingsProvider.IsFeatureEnabled(feature);
        }

        /// <summary>
        /// Asks the user to enable one or more features. This API will return
        /// immediately and will asynchronously ask the user to enable features using
        /// a separate Activity.
        /// </summary>
        /// <param name="requiredFeatures">A list of required `GvrBetaFeature`s. The
        /// user will not be returned to the app if they decline a required feature.
        /// This can be null if there are no required features.</param>
        /// <param name="optionalFeatures">A list of optional `GvrBetaFeature`s.
        /// This can be null if there are no required features.</param>
        public static void RequestFeatures(GvrBetaFeature[] requiredFeatures,
                                           GvrBetaFeature[] optionalFeatures)
        {
            GvrBetaSettingsProvider.RequestFeatures(requiredFeatures, optionalFeatures);
        }
    }
}

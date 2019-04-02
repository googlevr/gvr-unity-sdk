//-----------------------------------------------------------------------
// <copyright file="GvrControllerInputDeviceExtension.cs" company="Google LLC">
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

/// <summary>Daydream Beta API.</summary>
/// <remarks>
/// This API surface is for experimental purposes and may change or be removed in any future release
/// without forewarning.
/// </remarks>
namespace GoogleVR.Beta
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Class extension for `GvrControllerInputDevice` to add beta tracking status getter.
    /// </summary>
    public static class GvrControllerInputDeviceExtension
    {
        /// <summary>Gets a controller's configuration type.</summary>
        /// <remarks>Controller configuration will only change while the app is paused.</remarks>
        /// <param name="device">A controller input device to get the configuration for.</param>
        /// <returns>The controller configuration (3DoF or 6DoF).</returns>
        public static GvrBetaControllerInput.Configuration
            GetConfigurationType(this GvrControllerInputDevice device)
        {
            return GvrBetaControllerInput.GetConfigurationType(device.IsDominantHand ? 0 : 1);
        }

        /// <summary>Gets a controller's tracking status.</summary>
        /// <remarks>
        /// Although `TrackingStatusFlags` values are in practice currently mutually exclusive,
        /// returned values should be tested using bitwise tests.
        /// </remarks>
        /// <param name="device">A controller input device to get the tracking status for.</param>
        /// <returns>A bitwise series of flags representing tracking status.</returns>
        public static GvrBetaControllerInput.TrackingStatusFlags
            GetTrackingStatusFlags(this GvrControllerInputDevice device)
        {
            return GvrBetaControllerInput.GetTrackingStatusFlags(device.IsDominantHand ? 0 : 1);
        }
    }
}

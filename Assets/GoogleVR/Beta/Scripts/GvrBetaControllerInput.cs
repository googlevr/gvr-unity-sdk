//-----------------------------------------------------------------------
// <copyright file="GvrBetaControllerInput.cs" company="Google LLC">
// Copyright 2018 Google LLC. All rights reserved.
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

    /// <summary>Daydream controller beta API.</summary>
    public class GvrBetaControllerInput
    {
        /// <summary>Daydream Controller configurations.</summary>
        /// <remarks>Matches the C API enum `gvr_beta_controller_configuration_type`.</remarks>
        public enum Configuration
        {
            /// <summary>Used when controller configuration is unknown.</summary>
            Unknown = 0,

            /// <summary>Daydream (3DoF) controller.</summary>
            Is3DoF = 1,

            /// <summary>Daydream 6DoF controller.</summary>
            Is6DoF = 2,
        }

        /// <summary>Tracking status flags for Daydream 6DoF controllers.</summary>
        /// <remarks><para>
        /// Although enum values are in practice currently mutually exclusive, returned values
        /// should be tested using bitwise tests.
        /// </para><para>
        /// Matches the C API enum `gvr_beta_controller_tracking_status_flags`.
        /// </para></remarks>
        public enum TrackingStatusFlags
        {
            /// <summary>The controller's tracking status is unknown.</summary>
            Unknown = (1 << 0),

            /// <summary>The controller is tracking in 6DoF mode.</summary>
            Nominal = (1 << 1),

            /// <summary>The 6DoF controller is occluded.</summary>
            /// <remarks>
            /// 6DoF controllers report 3DoF pose and last-known position in this case.
            /// </remarks>
            Occluded = (1 << 2),

            /// <summary>The controller is out of field of view.</summary>
            /// <remarks>
            /// 6DoF controllers report 3DoF pose and last-known position in this case.
            /// </remarks>
            OutOfFov = (1 << 3),
        }

        /// <summary>Gets the current controller configuration.</summary>
        /// <remarks>Controller configuration will only change while the app is paused.</remarks>
        /// <param name="device">A controller input device to get the configuration for.</param>
        /// <returns>The controller configuration (3DoF or 6DoF).</returns>
        internal static Configuration GetConfigurationType(int device)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GvrBetaAndroidNativeControllerInputProvider.GetConfigurationType(device);
#elif UNITY_ANDROID && UNITY_EDITOR
            return GvrBetaEditorControllerInputProvider.GetConfigurationType(device);
#else
            return Configuration.Is3DoF;
#endif  // UNITY_ANDROID && !UNITY_EDITOR
        }

        /// <summary>Gets the tracking status flags for the given controller.</summary>
        /// <param name="device">A controller input device to get the tracking status for.</param>
        /// <returns>A bitwise series of flags representing tracking status.</returns>
        internal static TrackingStatusFlags GetTrackingStatusFlags(int device)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GvrBetaAndroidNativeControllerInputProvider.GetTrackingStatusFlags(device);
#elif UNITY_ANDROID && UNITY_EDITOR
            return GvrBetaEditorControllerInputProvider.GetTrackingStatusFlags(device);
#else
            return TrackingStatusFlags.Nominal;
#endif  // UNITY_ANDROID && !UNITY_EDITOR
        }
    }
}

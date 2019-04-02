//-----------------------------------------------------------------------
// <copyright file="GvrBetaEditorControllerInputProvider.cs" company="Google LLC">
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

/// <summary>Daydream Beta API helper for Editor (Instant Preview).</summary>
/// <remarks>
/// Should be called only from `GvrBetaControllerInput`.  This API surface is for experimental
/// purposes and may change or be removed in any future release without forewarning.
/// </remarks>
namespace GoogleVR.Beta
{
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>Daydream controller provider for Editor impl of the Beta API.</summary>
    internal class GvrBetaEditorControllerInputProvider
    {
#if UNITY_ANDROID && UNITY_EDITOR
        /// <summary>Gets the given controller's configuration.</summary>
        /// <param name="device">The controller to fetch the configuration for.</param>
        /// <returns>The controller's configuration.</returns>
        internal static GvrBetaControllerInput.Configuration GetConfigurationType(int device)
        {
            return (GvrBetaControllerInput.Configuration)
                GetBetaControllerState(device).betaConfigurationType;
        }

        /// <summary>Gets the tracking status flags for the given controller.</summary>
        /// <param name="device">The controller to fetch the tracking status for.</param>
        /// <returns>The controller's tracking status flags.</returns>
        internal static GvrBetaControllerInput.TrackingStatusFlags
            GetTrackingStatusFlags(int device)
        {
            return (GvrBetaControllerInput.TrackingStatusFlags)
                GetBetaControllerState(device).betaTrackingStatusFlags;
        }

        static GvrBetaControllerState GetBetaControllerState(int device)
        {
            GvrBetaControllerState betaOutState = new GvrBetaControllerState();
            if (Gvr.Internal.InstantPreview.IsActive
                && !Gvr.Internal.EmulatorManager.Instance.Connected)
            {
                // Uses Instant Preview to get controller state if connected.
                Gvr.Internal.EditorControllerProvider.instantPreviewControllerProvider
                    .ReadBetaState(betaOutState, device);
            }

            return betaOutState;
        }
#endif // UNITY_ANDROID && UNITY_EDITOR
    }
}

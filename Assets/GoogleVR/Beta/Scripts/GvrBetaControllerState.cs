//-----------------------------------------------------------------------
// <copyright file="GvrBetaControllerState.cs" company="Google LLC">
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

/// @cond
namespace GoogleVR.Beta
{
    using System;
    using Gvr;
    using UnityEngine;

    /// <summary>
    /// Internal representation of the Beta features for controller's current state.
    /// </summary>
    /// <remarks>
    /// The fields in this class have identical meanings to their correspondents in the GVR C API,
    /// so they are not redundantly documented here.
    /// </remarks>
    internal class GvrBetaControllerState
    {
        internal GvrBetaControllerInput.Configuration betaConfigurationType
            = GvrBetaControllerInput.Configuration.Unknown;

        internal GvrBetaControllerInput.TrackingStatusFlags betaTrackingStatusFlags
            = GvrBetaControllerInput.TrackingStatusFlags.Unknown;

        public void CopyFrom(GvrBetaControllerState other)
        {
            betaConfigurationType = other.betaConfigurationType;
            betaTrackingStatusFlags = other.betaTrackingStatusFlags;
        }
    }
}

/// @endcond

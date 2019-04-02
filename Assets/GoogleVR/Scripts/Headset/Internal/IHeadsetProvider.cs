//-----------------------------------------------------------------------
// <copyright file="IHeadsetProvider.cs" company="Google Inc.">
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

/// @cond
namespace Gvr.Internal
{
    interface IHeadsetProvider
    {
        /// <summary>
        /// Gets a value indicating whether the current headset supports positionally tracked,
        /// 6DoF head poses.
        /// </summary>
        /// <value>
        /// Value `true` if the current headset supports positionally tracked, 6DoF head poses,
        /// `false` otherwise.
        /// </value>
        bool SupportsPositionalTracking { get; }

        /// <summary>Polls for GVR headset events.</summary>
        void PollEventState(ref HeadsetState outState);

        /// <summary>
        /// Populates `floorHeight` with the detected height, if one is available.
        /// </summary>
        /// <remarks>This may be unavailable if the underlying GVR API call fails.</remarks>
        /// <returns>
        /// Returns `true` if value retrieval was successful, `false` otherwise.
        /// </returns>
        /// <param name="floorHeight">
        /// If this call returns `true`, this value is set to the retrieved `floorHeight`.
        /// Otherwise leaves the value unchanged.
        /// </param>
        bool TryGetFloorHeight(ref float floorHeight);

        /// <summary>
        /// Populates position and rotation with the last recenter transform, if one is available.
        /// </summary>
        /// <remarks>This may be unavailable if the underlying GVR API call fails.</remarks>
        /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
        /// <param name="position">
        /// If this call returns `true`, this value is set to the retrieved position.
        /// </param>
        /// <param name="rotation">
        /// If this call returns `true`, this value is set to the retrieved rotation.
        /// </param>
        bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation);

        /// <summary>
        /// Populates `safetyType` with the safety region type, if one is available.
        /// </summary>
        /// <remarks>
        /// Populates `safetyType` with the available safety region feature on the currently-running
        /// device.  This may be unavailable if the underlying GVR API call fails.
        /// </remarks>
        /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
        /// <param name="safetyType">
        /// If this call returns `true`, this value is set to the retrieved `safetyType`.
        /// </param>
        bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType);

        /// <summary>
        /// Populates `innerRadius` with the safety cylinder inner radius, if one is available.
        /// </summary>
        /// <remarks>
        /// If the safety region is of type `GvrSafetyRegionType.Cylinder`, populates `innerRadius`
        /// with the inner radius size of the safety cylinder in meters.  Before using, confirm that
        /// the safety region type is `GvrSafetyRegionType.Cylinder`.  This may be unavailable if
        /// the underlying GVR API call fails.
        /// <para>
        /// This is the radius at which safety management (e.g. safety fog) may cease taking effect.
        /// </para></remarks>
        /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
        /// <param name="innerRadius">
        /// If this call returns `true`, this value is set to the retrieved `innerRadius`.
        /// </param>
        bool TryGetSafetyCylinderInnerRadius(ref float innerRadius);

        /// <summary>
        /// Populates `outerRadius` with the safety cylinder outer radius, if one is available.
        /// </summary>
        /// <remarks>
        /// If the safety region is of type `GvrSafetyRegionType.Cylinder`, populates `outerRadius`
        /// with the outer radius size of the safety cylinder in meters.  Before using, confirm that
        /// the safety region type is `GvrSafetyRegionType.Cylinder`.  This may be unavailable if
        /// the underlying GVR API call fails.
        /// <para>
        /// This is the radius at which safety management (e.g. safety fog) may start to take
        /// effect.
        /// </para></remarks>
        /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
        /// <param name="outerRadius">
        /// If this call returns `true`, this value is set to the retrieved `outerRadius`.
        /// </param>
        bool TryGetSafetyCylinderOuterRadius(ref float outerRadius);
    }
}

/// @endcond

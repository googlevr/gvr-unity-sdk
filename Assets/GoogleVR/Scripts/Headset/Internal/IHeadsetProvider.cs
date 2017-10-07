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

using UnityEngine;

/// @cond
namespace Gvr.Internal {
  interface IHeadsetProvider {
    /// Returns whether the current headset supports positionally tracked, 6DOF head poses.
    bool SupportsPositionalTracking { get; }

    /// Polls for GVR standalone events.
    void PollEventState(ref HeadsetState outState);

    /// If a floor is found, populates floorHeight with the detected height.
    /// Otherwise, leaves the value unchanged.
    /// Returns true if value retrieval was successful, false otherwise (depends on tracking state).
    bool TryGetFloorHeight(ref float floorHeight);

    /// If the last recentering transform is available, populates position and rotation with that
    /// transform.
    /// Returns true if value retrieval was successful, false otherwise (unlikely).
    bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation);

    /// Populates safetyType with the available safety region feature on the
    /// currently-running device.
    /// Returns true if value retrieval was successful, false otherwise (unlikely).
    bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType);

    /// If the safety region is of type GvrSafetyRegionType.Cylinder, populates innerRadius with the
    /// inner radius size (where fog starts appearing) of the safety cylinder in meters.
    /// Assumes the safety region type has been previously checked by the caller.
    /// Returns true if value retrieval was successful, false otherwise (if region type is
    /// GvrSafetyRegionType.Invalid).
    bool TryGetSafetyCylinderInnerRadius(ref float innerRadius);

    /// If the safety region is of type GvrSafetyRegionType.Cylinder, populates outerRadius with the
    /// outer radius size (where fog is 100% opaque) of the safety cylinder in meters.
    /// Assumes the safety region type has been previously checked by the caller.
    /// Returns true if value retrieval was successful, false otherwise (if region type is
    /// GvrSafetyRegionType.Invalid).
    bool TryGetSafetyCylinderOuterRadius(ref float outerRadius);
  }
}
/// @endcond

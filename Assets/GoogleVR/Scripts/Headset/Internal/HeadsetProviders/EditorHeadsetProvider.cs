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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using Gvr;
using UnityEngine;

/// @cond
namespace Gvr.Internal {
  class EditorHeadsetProvider : IHeadsetProvider {
    private HeadsetState dummyState;

    public bool SupportsPositionalTracking { get { return true; } }

    public void PollEventState(ref HeadsetState state) {
      // Emulation coming soon.
    }

    public bool TryGetFloorHeight(ref float floorHeight) {
      floorHeight = -1.6f;
      return true;
    }

    public bool TryGetRecenterTransform(
        ref Vector3 position, ref Quaternion rotation) {
      return true;
    }

    public bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType) {
      safetyType = GvrSafetyRegionType.Cylinder;
      return true;
    }

    public bool TryGetSafetyCylinderInnerRadius(ref float innerRadius) {
      innerRadius = 0.6f;
      return true;
    }

    public bool TryGetSafetyCylinderOuterRadius(ref float outerRadius) {
      outerRadius = 0.7f;
      return true;
    }
  }
}
/// @endcond

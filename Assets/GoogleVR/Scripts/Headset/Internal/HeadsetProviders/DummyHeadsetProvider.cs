//-----------------------------------------------------------------------
// <copyright file="DummyHeadsetProvider.cs" company="Google Inc.">
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

using Gvr;
using UnityEngine;

/// Used for platforms that do not support the GoogleVR 6DoF headset.
/// @cond
namespace Gvr.Internal
{
    class DummyHeadsetProvider : IHeadsetProvider
    {
        private HeadsetState dummyState;

        public bool SupportsPositionalTracking
        {
            get { return false; }
        }

        public void PollEventState(ref HeadsetState state)
        {
        }

        public bool TryGetFloorHeight(ref float floorHeight)
        {
            return false;
        }

        public bool TryGetRecenterTransform(
            ref Vector3 position, ref Quaternion rotation)
        {
            return false;
        }

        public bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType)
        {
            return false;
        }

        public bool TryGetSafetyCylinderInnerRadius(ref float innerRadius)
        {
            return false;
        }

        public bool TryGetSafetyCylinderOuterRadius(ref float outerRadius)
        {
            return false;
        }
    }
}

/// @endcond

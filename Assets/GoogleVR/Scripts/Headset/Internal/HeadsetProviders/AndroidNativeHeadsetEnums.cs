//-----------------------------------------------------------------------
// <copyright file="AndroidNativeHeadsetEnums.cs" company="Google Inc.">
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
using System;

/// @cond
namespace Gvr.Internal
{
    /// <summary>Maps to  gvr_feature in the C API.</summary>
    internal enum gvr_feature
    {
        HeadPose6dof = 3,
    }

    /// <summary>Maps to gvr_property_type in the C API.</summary>
    internal enum gvr_property_type
    {
        /// <summary>float; `GVR_PROPERTY_TRACKING_FLOOR_HEIGHT`</summary>
        TrackingFloorHeight = 1,

        /// <summary>gvr_mat4f, `GVR_PROPERTY_RECENTER_TRANSFORM`</summary>
        RecenterTransform = 2,

        /// <summary>int (`gvr_safety_region_type`), `GVR_PROPERTY_SAFETY_REGION`</summary>
        SafetyRegion = 3,

        /// <summary>float, `GVR_PROPERTY_SAFETY_CYLINDER_INNER_RADIUS`</summary>
        SafetyCylinderInnerRadius = 4,

        /// <summary>float, `GVR_PROPERTY_SAFETY_CYLINDER_OUTER_RADIUS`</summary>
        SafetyCylinderOuterRadius = 5,
    }

    /// <summary>Maps to gvr_value_type in the C API.</summary>
    internal enum gvr_value_type
    {
        /// <summary>A default enum indicating with no value or type.<summary>
        None = 0,

        /// <summary>The float type.<summary>
        Float = 1,

        /// <summary>The double type.<summary>
        Double = 2,

        /// <summary>The integer type.<summary>
        Int = 3,

        /// <summary>The long integer type.<summary>
        Int64 = 4,

        /// <summary>An array of boolean flags cast as an integer.<summary>
        Flags = 5,

        /// <summary>An unsigned integer type indicating size.<summary>
        Sizei = 6,

        /// <summary>Four integers representing the edges of a rectangle.<summary>
        Recti = 7,

        /// <summary>Four floats representing the edges of a rectangle.<summary>
        Rectf = 8,

        /// <summary>Two floats representing a Vector2.<summary>
        Vec2f = 9,

        /// <summary>Three floats representing a Vector3.<summary>
        Vec3f = 10,

        /// <summary>Four floats representing a Quaternion.<summary>
        Quat = 11,

        /// <summary>A four-by-four array of floats representing a 4x4 Matrix.<summary>
        Mat4f = 12,

        /// <summary>A long integer representing a timestamp.<summary>
        ClockTimePoint = 13,
    }

    /// <summary>Flags indicating recenter event properties.<summary>
    internal enum gvr_recenter_flags
    {
        /// <summary>A default value with no associated property.<summary>
        None = 0,
    }
}

/// @endcond

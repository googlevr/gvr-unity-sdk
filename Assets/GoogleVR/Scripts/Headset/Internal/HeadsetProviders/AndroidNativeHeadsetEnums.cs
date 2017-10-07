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
using System;

/// @cond
namespace Gvr.Internal {
  /// Maps to  gvr_feature in the C API.
  internal enum gvr_feature {
    HeadPose6dof = 3,
  };

  /// Maps to gvr_property_type in the C API.
  internal enum gvr_property_type {
    TrackingFloorHeight = 1,   // float; GVR_PROPERTY_TRACKING_FLOOR_HEIGHT
    RecenterTransform = 2,  // gvr_mat4f, GVR_PROPERTY_RECENTER_TRANSFORM
    SafetyRegion = 3,  // int (gvr_safety_region_type), GVR_PROPERTY_SAFETY_REGION
    SafetyCylinderInnerRadius = 4,  // float, GVR_PROPERTY_SAFETY_CYLINDER_INNER_RADIUS
    SafetyCylinderOuterRadius = 5,  // float, GVR_PROPERTY_SAFETY_CYLINDER_OUTER_RADIUS
  };

  /// Maps to gvr_value_type in the C API.
  internal enum gvr_value_type {
    None = 0,
    Float = 1,
    Double = 2,
    Int = 3,
    Int64 = 4,
    Flags = 5,
    Sizei = 6,
    Recti = 7,
    Rectf = 8,
    Vec2f = 9,
    Vec3f = 10,
    Quat = 11,
    Mat4f = 12,
    ClockTimePoint = 13,
  };

  internal enum gvr_recenter_flags {
    None = 0,
  };
}
/// @endcond

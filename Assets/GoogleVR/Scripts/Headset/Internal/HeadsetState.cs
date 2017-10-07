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

using Gvr;

/// @cond
namespace Gvr.Internal {
  // Internal representation of state for the headset.
  struct HeadsetState {
    internal GvrEventType eventType;
    internal int eventFlags;
    internal long eventTimestampNs;  // Maps to gvr_clock_time_point monotonic_systemtime_nanos.

    // Recenter event data.
    internal GvrRecenterEventType recenterEventType;
    internal uint recenterEventFlags;
    internal Vector3 recenteredPosition;
    internal Quaternion recenteredRotation;

    public void Initialize() {
      eventType = GvrEventType.Invalid;
      eventFlags = 0;
      eventTimestampNs = 0;

      recenterEventType = GvrRecenterEventType.Invalid;
      recenterEventFlags = 0;
      recenteredPosition = Vector3.zero;
      recenteredRotation = Quaternion.identity;
    }

  }
}  // namespace Gvr.Internal
/// @endcond

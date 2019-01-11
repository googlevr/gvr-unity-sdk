//-----------------------------------------------------------------------
// <copyright file="GvrHeadsetEnums.cs" company="Google Inc.">
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

// Maps to gvr_event_type in the C API.
public enum GvrEventType
{
    // Not in the C API.
    Invalid = -1,
    Recenter = 1,
    SafetyRegionExit = 2,
    SafetyRegionEnter = 3,
}

// Maps to gvr_recenter_event_type in the C API.
public enum GvrRecenterEventType
{
    // Not in the C API.
    Invalid = -1,

    // Headset removal / re-attach recenter.
    RecenterEventRestart = 1,

    // Controller-initiated recenter.
    RecenterEventAligned = 2,
}

// Placeholder.  No C spec for recenter flags yet.
public enum GvrRecenterFlags
{
    None = 0,
}

// Maps to gvr_error in the C API.
public enum GvrErrorType
{
    None = 0,
    ControllerCreateFailed = 2,
    NoFrameAavilable = 3,
    NoEventAvailable = 1000000,
    NoPropertyAvailable = 1000001,
}

// Maps to gvr_safety_region_type in the C API.
public enum GvrSafetyRegionType
{
    None = 0,
    Cylinder = 1,
}

//-----------------------------------------------------------------------
// <copyright file="GvrEventType.cs" company="Google LLC.">
// Copyright 2018 Google LLC. All rights reserved.
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

/// <summary>Maps to `gvr_event_type` in the C API.</summary>
public enum GvrEventType
{
    /// <summary>Not in the C API.</summary>
    Invalid = -1,

    /// <summary>Indicates that the recenter event is occurring.</summary>
    Recenter = 1,

    /// <summary>Indicates that the headset has left the safety region.</summary>
    SafetyRegionExit = 2,

    /// <summary>Indicates that the headset has reentered the safety region.</summary>
    SafetyRegionEnter = 3,
}

//-----------------------------------------------------------------------
// <copyright file="GvrErrorType.cs" company="Google Inc.">
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

/// <summary>Maps to `gvr_error` in the C API.</summary>
public enum GvrErrorType
{
    /// <summary>A default value indicating that no error has occurred.</summary>
    None = 0,

    /// <summary>Indicates that a controller could not be created.</summary>
    ControllerCreateFailed = 2,

    /// <summary>Indicates that a frame is unavailable for render.</summary>
    NoFrameAavilable = 3,

    /// <summary>Indicates that no events have been received.</summary>
    NoEventAvailable = 1000000,

    /// <summary>Indicates that no properties have been received.</summary>
    NoPropertyAvailable = 1000001,
}

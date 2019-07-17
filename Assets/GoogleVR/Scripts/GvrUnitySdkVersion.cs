//-----------------------------------------------------------------------
// <copyright file="GvrUnitySdkVersion.cs" company="Google Inc.">
// Copyright 2016 Google Inc. All rights reserved.
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

/// <summary>Provides and logs versioning information for the GVR SDK for Unity.</summary>
public class GvrUnitySdkVersion
{
    /// <summary>The version of the SDK.</summary>
    public const string GVR_SDK_VERSION = "1.200.1";

// Google VR SDK supports Unity 5.6 or newer.
#if !UNITY_5_6_OR_NEWER
#error Google VR SDK requires Unity version 5.6 or newer.
#endif  // !UNITY_5_6_OR_NEWER

// Only log GVR SDK version when running on an Android or iOS device.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private const string VERSION_HEADER = "GVR SDK for Unity version: ";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LogGvrUnitySdkVersion()
    {
        Debug.Log(VERSION_HEADER + GVR_SDK_VERSION);
    }
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
}

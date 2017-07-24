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

using UnityEngine;

/// <summary>
/// Provides and logs versioning information for the GVR Unity SDK.
/// </summary>
public class GvrUnitySdkVersion {
  public const string GVR_SDK_VERSION = "1.70.0";

// Google VR SDK supports Unity 5.6 or later.
#if !UNITY_5_6_OR_NEWER
// Allow deprecated tech preview Unity releases (incl. 5.4.2f2-GVR13) for now.
#if UNITY_5_4_2 && UNITY_HAS_GOOGLEVR
  #warning Google VR SDK: Please upgrade to Unity 5.6 or newer.
#else
  // Not running a tech preview release, so require Unity 5.6 or newer.
  #error Google VR SDK requires Unity version 5.6 or newer.
#endif  // UNITY_5_4_2 && UNITY_HAS_GOOGLEVR
#endif  // !UNITY_5_6_OR_NEWER

// Only log GVR SDK version when running on an Android or iOS device.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
  private const string VERSION_HEADER = "GVR Unity SDK Version: ";

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void LogGvrUnitySdkVersion() {
    Debug.Log(VERSION_HEADER + GVR_SDK_VERSION);
  }
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
}

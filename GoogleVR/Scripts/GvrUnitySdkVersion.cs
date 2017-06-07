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
  public const string GVR_SDK_VERSION = "1.60.0";

// Only log GVR SDK version when the current build platform is Android or iOS.
#if UNITY_ANDROID || UNITY_IOS
  private const string VERSION_HEADER = "GVR Unity SDK Version: ";

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void LogGvrUnitySdkVersion() {
    Debug.Log(VERSION_HEADER + GVR_SDK_VERSION);
  }
#endif  // UNITY_ANDROID || UNITY_IOS
}

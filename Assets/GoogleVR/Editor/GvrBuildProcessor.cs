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

// Only invoke custom build processor when building for Android or iOS.
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Linq;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

// Notifies users if they build for Android or iOS without Cardboard or Daydream enabled.
class GvrBuildProcessor : IPreprocessBuild {
  private const string VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE_FORMAT =
    "To use the Google VR SDK on {0}, 'Player Settings > Virtual Reality Supported' setting must be checked.\n" +
    "Please fix this setting and rebuild your app.";
  private const string IOS_MISSING_GVR_SDK_ERROR_MESSAGE =
    "To use the Google VR SDK on iOS, 'Player Settings > Virtual Reality SDKs' must include 'Cardboard'.\n" +
    "Please fix this setting and rebuild your app.";
  private const string ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE =
    "To use the Google VR SDK on Android, 'Player Settings > Virtual Reality SDKs' must include 'Daydream' or 'Cardboard'.\n" +
    "Please fix this setting and rebuild your app.";

  public int callbackOrder {
    get { return 0; }
  }

  public void OnPreprocessBuild (BuildTarget target, string path)
  {
    if (target != BuildTarget.Android && target != BuildTarget.iOS) {
      // Do nothing when not building for Android or iOS.
      return;
    }

    // 'Player Settings > Virtual Reality Supported' must be enabled.
    if (!IsVRSupportEnabled()) {
      Debug.LogErrorFormat(VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE_FORMAT, target);
    }

    if (target == BuildTarget.Android) {
      // When building for Android at least one VR SDK must be included.
      // For Google VR valid VR SDKs are 'Daydream' and/or 'Cardboard'.
      if (!IsSDKOtherThanNoneIncluded()) {
        Debug.LogError(ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE);
      }
    }

    if (target == BuildTarget.iOS) {
      // When building for iOS at least one VR SDK must be included.
      // For Google VR only 'Cardboard' is supported.
      if (!IsSDKOtherThanNoneIncluded()) {
        Debug.LogError(IOS_MISSING_GVR_SDK_ERROR_MESSAGE);
      }
    }
  }

  // 'Player Settings > Virtual Reality Supported' enabled?
  private bool IsVRSupportEnabled() {
    return PlayerSettings.virtualRealitySupported;
  }

  // 'Player Settings > Virtual Reality SDKs' includes any VR SDK other than 'None'?
  private bool IsSDKOtherThanNoneIncluded() {
    bool containsNone = XRSettings.supportedDevices.Contains(GvrSettings.VR_SDK_NONE);
    int numSdks = XRSettings.supportedDevices.Length;
    return containsNone ? numSdks > 1 : numSdks > 0;
  }
}
#endif  // UNITY_ANDROID || UNITY_IOS

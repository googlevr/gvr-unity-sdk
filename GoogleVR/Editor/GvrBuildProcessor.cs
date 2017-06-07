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

#if UNITY_5_6_OR_NEWER && (UNITY_ANDROID || UNITY_IOS)
using UnityEngine;
using UnityEngine.VR;
using UnityEditor;
using UnityEditor.Build;
using System.Linq;

// Notifes users if they build for Android or iOS without Cardboard or Daydream enabled.
class GvrBuildProcessor : IPreprocessBuild {
  private const string VR_SDK_DAYDREAM = "daydream";
  private const string VR_SDK_CARDBOARD = "cardboard";
  private const string OK_BUTTON_TEXT = "OK";
  private const string DISPLAY_DIALOG_TITLE = "Google VR SDK";
  private const string VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE =
    "Please enable the 'Virtual Reality Supported' checkbox in 'Player Settings'.";
  private const string IOS_MISSING_GVR_SDK_ERROR_MESSAGE =
    "Please add 'Cardboard' in 'Player Settings > Virtual Reality SDKs'.";
  private const string ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE =
    "Please add 'Daydream' or 'Cardboard' in 'Player Settings > Virtual Reality SDKs'.";

  public int callbackOrder {
    get { return 0; }
  }

  public void OnPreprocessBuild (BuildTarget target, string path)
  {
    // 'Player Settings > Virtual Reality Supported' must be enabled.
    if (!IsVRSupportEnabled()) {
        EditorUtility.DisplayDialog (DISPLAY_DIALOG_TITLE,
            VR_SETTINGS_NOT_ENABLED_ERROR_MESSAGE, OK_BUTTON_TEXT);
        return;
    }

    if (target == BuildTarget.Android) {
      // On Android VR SDKs must include 'Daydream' and/or 'Cardboard'.
      if (!IsDaydreamSDKIncluded() && !IsCardboardSDKIncluded()) {
        EditorUtility.DisplayDialog(DISPLAY_DIALOG_TITLE,
            ANDROID_MISSING_GVR_SDK_ERROR_MESSAGE, OK_BUTTON_TEXT);
        return;
      }
    }

    if (target == BuildTarget.iOS) {
      // On iOS VR SDKs must include 'Cardboard'.
      if (!IsCardboardSDKIncluded()) {
        EditorUtility.DisplayDialog(DISPLAY_DIALOG_TITLE,
            IOS_MISSING_GVR_SDK_ERROR_MESSAGE, OK_BUTTON_TEXT);
        return;
      }
    }
  }

  // 'Player Settings > Virtual Reality Supported' enabled?
  private bool IsVRSupportEnabled() {
    return PlayerSettings.virtualRealitySupported;
  }

  // 'Player Settings > Virtual Reality SDKs' includes 'Daydream'?
  private bool IsDaydreamSDKIncluded() {
    return VRSettings.supportedDevices.Contains(VR_SDK_DAYDREAM);
  }

  // 'Player Settings > Virtual Reality SDKs' includes 'Cardboard'?
  private bool IsCardboardSDKIncluded() {
    return VRSettings.supportedDevices.Contains(VR_SDK_CARDBOARD);
  }
}
#endif  // UNITY_5_6_OR_NEWER && (UNITY_ANDROID || UNITY_IOS)

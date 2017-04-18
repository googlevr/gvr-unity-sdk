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
  private const string IOS_ERROR_MESSAGE =
    "Please enable Cardboard in Player Settings > Virtual Reality Supported.";
  private const string ANDROID_ERROR_MESSAGE =
    "Please enable Daydream or Cardboard in Player Settings > Virtual Reality Supported.";

  public int callbackOrder {
    get { return 0; }
  }
  public void OnPreprocessBuild(BuildTarget target, string path) {
    bool isAndroid = (target == BuildTarget.Android);
    if (VRSettings.supportedDevices.Length == 0 || VRSettings.supportedDevices.Contains("None")) {
      EditorUtility.DisplayDialog(isAndroid ? ANDROID_ERROR_MESSAGE : IOS_ERROR_MESSAGE,
          null, "OK");
    }
  }
}
#endif  // UNITY_5_6_OR_NEWER && (UNITY_ANDROID || UNITY_IOS)

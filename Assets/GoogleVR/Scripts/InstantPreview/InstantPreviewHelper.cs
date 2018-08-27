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

using System.IO;
using System.Runtime.InteropServices;
using Gvr.Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
[HelpURL("https://developers.google.com/vr/unity/reference/class/InstantPreviewHelper")]
public class InstantPreviewHelper : MonoBehaviour {
  public static string AdbPath;

#if UNITY_HAS_GOOGLEVR && UNITY_EDITOR
  [DllImport(InstantPreview.dllName)]
  private static extern bool SetAdbPathAndStart(string adbPath);

  void Awake() {
    // Gets android SDK root from preferences.
    var sdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
    if (string.IsNullOrEmpty(sdkRoot)) {
      Debug.LogError("Instant Preview requires your Unity Android SDK path to be set. Please set it under Preferences/External Tools/Android. You may need to install the Android SDK first.");
      return;
    }

    // Gets adb path from known directory.
    AdbPath = Path.Combine(Path.GetFullPath(sdkRoot), "platform-tools" + Path.DirectorySeparatorChar + "adb");
#if UNITY_EDITOR_WIN
    AdbPath = Path.ChangeExtension(AdbPath, "exe");
#endif // UNITY_EDITOR_WIN

    if (!File.Exists(AdbPath)) {
      Debug.LogErrorFormat("adb not found at \"{0}\". Please add adb to your SDK path and restart the Unity editor.", AdbPath);
      return;
    }

    // Tries to start server.
    var started = SetAdbPathAndStart(AdbPath);
    if (!started) {
      Debug.LogErrorFormat("Couldn't start Instant Preview server with adb path: {0}.", AdbPath);
    }
  }

#elif UNITY_EDITOR
  void Awake() {
    Debug.LogWarning("Instant Preview is disabled; set target platform to Android to use it.");
  }

#endif
}

#if !UNITY_HAS_GOOGLEVR && UNITY_EDITOR
[CustomEditor(typeof(InstantPreviewHelper))]
public class InstantPreviewHelperEditor : Editor {
  public override void OnInspectorGUI() {
    EditorGUILayout.LabelField("Instant Preview is disabled; set target platform to Android to use it.");
  }
}
#endif

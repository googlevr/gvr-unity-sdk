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

using UnityEngine;
using System;
using System.Runtime.InteropServices;

// Simple static class to abstract out several jni calls that need to be shared
// between different classes.
public static class GvrActivityHelper {
    private const string UNITY_PLAYER_CLASS = "com.unity3d.player.UnityPlayer";

#if UNITY_ANDROID && !UNITY_EDITOR
  /// Returns the Android Activity used by the Unity device player. The caller is
  /// responsible for memory-managing the returned AndroidJavaObject.
  public static AndroidJavaObject GetActivity() {
    AndroidJavaClass jc = new AndroidJavaClass(UNITY_PLAYER_CLASS);
    if (jc == null) {
      Debug.LogErrorFormat("Failed to get class {0}", UNITY_PLAYER_CLASS);
      return null;
    }
    AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
    if (activity == null) {
      Debug.LogError("Failed to obtain current Android activity.");
      return null;
    }
    return activity;
  }

  /// Returns the application context of the current Android Activity.
  public static AndroidJavaObject GetApplicationContext(AndroidJavaObject activity) {
    AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
    if (context == null) {
      Debug.LogError("Failed to get application context from Activity.");
      return null;
    }
    return context;
  }
#endif  // UNITY_ANDROID && !UNITY_EDITOR
}

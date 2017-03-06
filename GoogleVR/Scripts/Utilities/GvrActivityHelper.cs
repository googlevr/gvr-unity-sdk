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
public static class GvrActivityHelper
{
#if UNITY_ANDROID && !UNITY_EDITOR
  public static AndroidJavaObject GetActivity(string javaClass) {
    AndroidJavaClass jc = new AndroidJavaClass(javaClass);
    if (jc == null) {
      Debug.LogErrorFormat("Failed to get Unity Player class, {0}", javaClass);
      return null;
    }
    AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
    if (activity == null) {
      Debug.LogError("Failed to obtain Android Activity from Unity Player class.");
      return null;
    }
    return activity;
  }

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

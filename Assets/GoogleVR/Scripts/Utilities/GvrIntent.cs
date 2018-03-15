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

/// <summary>
/// Provides information about the Android Intent that started the current Activity.
/// </summary>
public static class GvrIntent {

  private const string METHOD_GET_INTENT = "getIntent";
  private const string METHOD_HASH_CODE = "hashCode";
  private const string METHOD_INTENT_GET_DATA_STRING = "getDataString";
  private const string METHOD_INTENT_GET_BOOLEAN_EXTRA = "getBooleanExtra";

  private const string EXTRA_VR_LAUNCH = "android.intent.extra.VR_LAUNCH";

  // Returns the string representation of the data URI on which this activity's intent is
  // operating. See Intent.getDataString() in the Android documentation.
  public static string GetData() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return null;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return null;
    }
    return androidIntent.Call<string>(METHOD_INTENT_GET_DATA_STRING);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

  // Returns true if the intent category contains "android.intent.extra.VR_LAUNCH".
  public static bool IsLaunchedFromVr() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return false;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return false;
    }
    return androidIntent.Call<bool>(METHOD_INTENT_GET_BOOLEAN_EXTRA, EXTRA_VR_LAUNCH, false);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

  // Returns the hash code of the Java intent object.  Useful for discerning whether
  // you have a new intent on un-pause.
  public static int GetIntentHashCode() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return 0;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return 0;
    }
    return androidIntent.Call<int>(METHOD_HASH_CODE);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

#if !UNITY_EDITOR && UNITY_ANDROID
  private static AndroidJavaObject GetIntent() {
    AndroidJavaObject androidActivity = null;
    try {
      androidActivity = GvrActivityHelper.GetActivity();
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception while connecting to the Activity: " + e);
      return null;
    }
    return androidActivity.Call<AndroidJavaObject>(METHOD_GET_INTENT);
  }
#endif  // !UNITY_EDITOR && UNITY_ANDROID
}

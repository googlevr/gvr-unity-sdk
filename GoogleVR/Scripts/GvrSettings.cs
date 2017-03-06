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


/// <summary>
/// Accesses and configures Daydream settings.
/// </summary>

// This class is defined only for versions of Unity with the GVR native integration.
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
using UnityEngine;
using UnityEngine.VR;
using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif  // UNITY_EDITOR

public static class GvrSettings {

  private const string PACKAGE_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";
  private const string METHOD_CURRENT_ACTIVITY = "currentActivity";
  private const string METHOD_GET_WINDOW = "getWindow";
  private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
  private const string METHOD_SET_SUSTAINED_PERFORMANCE_MODE = "setSustainedPerformanceMode";

  // Viewer type.
  public enum ViewerPlatformType {
    Error = -1,  // Plugin-only value; does not exist in the NDK.
    Cardboard,
    Daydream
  }
  public static ViewerPlatformType ViewerPlatform {
    // Expose a setter only for the editor emulator, for development testing purposes.
#if UNITY_EDITOR
    get {
      return editorEmulatorOnlyViewerPlatformType;
    }
    set {
      editorEmulatorOnlyViewerPlatformType = value;
    }
#else
    get {
      IntPtr gvrContextPtr = VRDevice.GetNativePtr();
      if (gvrContextPtr == IntPtr.Zero) {
        Debug.Log("Null GVR context pointer, could not get viewer platform type");
        return ViewerPlatformType.Error;
      }
      return (ViewerPlatformType) gvr_get_viewer_type(gvrContextPtr);
    }
#endif  // UNITY_EDITOR
  }
#if UNITY_EDITOR
  private static ViewerPlatformType editorEmulatorOnlyViewerPlatformType =
    ViewerPlatformType.Daydream;
#endif  // UNITY_EDITOR

  // The developer is expected to remember whether sustained performance mode is set
  // at runtime, via the checkbox in Player Settings.
  // This state may be recorded here in a future release.
  public static bool SustainedPerformanceMode {
    set {
      SetSustainedPerformanceMode(value);
    }
  }

  // Handedness preference.
  public enum UserPrefsHandedness {
    Error = -1,  // Plugin-only value, does not exist in the NDK.
    Right,
    Left
  }
  public static UserPrefsHandedness Handedness {
    // Expose a setter only for the editor emulator, for development testing purposes.
#if UNITY_EDITOR
    get {
      return (UserPrefsHandedness)EditorPrefs.GetInt(EMULATOR_HANDEDNESS_PREF_NAME, (int)UserPrefsHandedness.Right);
    }
    set {
      EditorPrefs.SetInt(EMULATOR_HANDEDNESS_PREF_NAME, (int)value);
    }
#else
    // Running on Android.
    get {
      IntPtr gvrContextPtr = VRDevice.GetNativePtr();
      if (gvrContextPtr == IntPtr.Zero) {
        Debug.Log("Null GVR context pointer, could not get GVR user prefs' handedness");
        return UserPrefsHandedness.Error;
      }

      IntPtr gvrUserPrefsPtr = gvr_get_user_prefs(gvrContextPtr);
      if (gvrUserPrefsPtr == IntPtr.Zero) {
        Debug.Log("Null GVR user prefs pointer, could not get handedness");
        return UserPrefsHandedness.Error;
      }

      return (UserPrefsHandedness) gvr_user_prefs_get_controller_handedness(gvrUserPrefsPtr);
    }
#endif  // UNITY_EDITOR
  }
#if UNITY_EDITOR
  // This allows developers to test handedness in the editor emulator.
  private const string EMULATOR_HANDEDNESS_PREF_NAME = "GoogleVREditorEmulatorHandedness";
#endif  // UNITY_EDITOR

  private static void SetSustainedPerformanceMode(bool enabled) {
#if !UNITY_EDITOR
    AndroidJavaObject androidActivity = null;
    try {
      using (AndroidJavaObject unityPlayer = new AndroidJavaClass(PACKAGE_UNITY_PLAYER)) {
        androidActivity = unityPlayer.GetStatic<AndroidJavaObject>(METHOD_CURRENT_ACTIVITY);
      }
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception while connecting to the Activity: " + e);
      return;
    }

    AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>(METHOD_GET_WINDOW);
    if (androidWindow == null) {
      Debug.LogError("No window found on the current android activity");
      return;
    }

    // The sim thread in Unity is single-threaded, so we don't need to lock when accessing
    // or assigning androidWindow.
    androidActivity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() => {
          androidWindow.Call(METHOD_SET_SUSTAINED_PERFORMANCE_MODE, enabled);
          Debug.Log("Set sustained performance mode: " + (enabled ? "ON" : "OFF"));
      })
    );
#endif  // !UNITY_EDITOR
  }


  private const string dllName = "gvr";

  [DllImport(dllName)]
  private static extern IntPtr gvr_get_user_prefs(IntPtr gvrContextPtr);

  [DllImport(dllName)]
  private static extern int gvr_get_viewer_type(IntPtr gvrContextPtr);

  [DllImport(dllName)]
  private static extern int gvr_user_prefs_get_controller_handedness(IntPtr gvrUserPrefsPtr);

}
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

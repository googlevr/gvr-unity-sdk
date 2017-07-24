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
using UnityEngine.VR;
using System;
using System.Runtime.InteropServices;

/// Main entry point Daydream specific APIs.
///
/// This class automatically instantiates an instance when this API is used for the first time.
/// For explicit control over when the instance is created and the Java references are setup
/// call the provided CreateAsync method, for example when no UI is being displayed to the user.
public class GvrDaydreamApi : IDisposable {
  private const string METHOD_CREATE = "create";
  private const string METHOD_LAUNCH_VR_HOMESCREEN = "launchVrHomescreen";
  private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
  private const string PACKAGE_DAYDREAM_API = "com.google.vr.ndk.base.DaydreamApi";

  private static GvrDaydreamApi m_instance;

#if UNITY_ANDROID && !UNITY_EDITOR
  private AndroidJavaObject m_daydreamApiObject;
  private AndroidJavaClass m_daydreamApiClass = new AndroidJavaClass(PACKAGE_DAYDREAM_API);

  public static AndroidJavaObject JavaInstance {
    get {
      EnsureCreated(null);
      return m_instance.m_daydreamApiObject;
    }
  }
#endif  // UNITY_ANDROID && !UNITY_EDITOR

  public static bool IsCreated {
    get {
#if !UNITY_ANDROID || UNITY_EDITOR
      return (m_instance != null);
#else
      return (m_instance != null) && (m_instance.m_daydreamApiObject != null);
#endif  // !UNITY_ANDROID || UNITY_EDITOR
    }
  }

  private static void EnsureCreated(Action<bool> callback) {
      if (!IsCreated) {
        CreateAsync(callback);
      } else {
          callback(true);
      }
  }

  /// @cond
  /// Call Dispose to free up memory used by this API.
  public void Dispose() {
    m_instance = null;
  }
  /// @endcond

  [System.Obsolete("Create() without arguments is deprecated. Use CreateAsync(callback) instead.")]
  public static void Create() {
    CreateAsync(null);
  }

  /// Asynchronously instantiates a GvrDayreamApi.
  ///
  /// The provided callback will be called with a bool argument indicating
  /// whether instance creation was successful.
  public static void CreateAsync(Action<bool> callback) {
    if (m_instance == null) {
      m_instance = new GvrDaydreamApi();
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    if (m_instance.m_daydreamApiObject != null) {
      return;
    }

    if (m_instance.m_daydreamApiClass == null) {
      Debug.LogErrorFormat("Failed to get DaydreamApi class, {0}", PACKAGE_DAYDREAM_API);
      return;
    }

    AndroidJavaObject activity = GvrActivityHelper.GetActivity();
    if (activity == null) {
      Debug.LogError("DaydreamApi.Create failed to get acitivty");
      return;
    }

    AndroidJavaObject context = GvrActivityHelper.GetApplicationContext(activity);
    if (context == null) {
      Debug.LogError("DaydreamApi.Create failed to get application context from activity");
      return;
    }

    activity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() => {
          m_instance.m_daydreamApiObject =
          m_instance.m_daydreamApiClass.CallStatic<AndroidJavaObject>(METHOD_CREATE, context);
          bool success = m_instance.m_daydreamApiObject != null;
          if (!success) {
            Debug.LogErrorFormat("DaydreamApi.Create call to {0} failed to instantiate object",
                METHOD_CREATE);
          }
          if (callback != null) {
            callback(success);
          }
      })
    );
#endif  // UNITY_ANDROID && !UNITY_EDITOR
  }

  [System.Obsolete("LaunchVrHome() deprecated. Use LaunchVrHomeAsync(callback) instead.")]
  public static void LaunchVrHome() {
    LaunchVrHomeAsync(null);
  }

  /// Asynchronously launches VR Home.
  /// Instantiates an instance of GvrDaydreamApi if necessary. If successful,
  /// launches VR Home.
  /// The provided callback will be called with a bool argument indicating
  /// whether instance creation and launch of VR Home was successful.
  public static void LaunchVrHomeAsync(Action<bool> callback) {
    EnsureCreated((success) => {
      if (success) {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_instance.m_daydreamApiObject.Call(METHOD_LAUNCH_VR_HOMESCREEN);
#else
        Debug.LogWarning("Launching VR Home is only possible on Android devices.");
#endif  // UNITY_ANDROID && !UNITY_EDITOR
      }
      if (callback != null) {
        callback(success);
      }
    });
  }
}

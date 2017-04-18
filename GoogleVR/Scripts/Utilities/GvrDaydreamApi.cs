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

// Accessor to DaydreamApi.java.
public class GvrDaydreamApi : IDisposable {
  private const string METHOD_CREATE = "create";
  private const string METHOD_LAUNCH_VR_HOMESCREEN = "launchVrHomescreen";
  private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
  private const string PACKAGE_DAYDREAM_API = "com.google.vr.ndk.base.DaydreamApi";

  private static GvrDaydreamApi m_instance;

#if !UNITY_EDITOR && UNITY_ANDROID
  private AndroidJavaObject m_daydreamApiObject;
  private AndroidJavaClass m_daydreamApiClass = new AndroidJavaClass(PACKAGE_DAYDREAM_API);
#endif  // !UNITY_EDITOR && UNITY_ANDROID

#if UNITY_ANDROID
  public static AndroidJavaObject JavaInstance {
    get {
#if UNITY_EDITOR || !UNITY_HAS_GOOGLEVR
      return null;
#else
      if (m_instance == null || m_instance.m_daydreamApiObject == null) {
        Debug.Log("GvrDaydreamApi not instantiated, please call CreateDaydreamApi() first");
        return null;
      }
      return m_instance.m_daydreamApiObject;
#endif  // UNITY_EDITOR || !UNITY_HAS_GOOGLEVR
    }
  }
#endif  // UNITY_ANDROID

  public static bool IsCreated {
    get {
#if UNITY_EDITOR || !UNITY_HAS_GOOGLEVR || !UNITY_ANDROID
      return (m_instance != null);
#else
      return (m_instance != null) && (m_instance.m_daydreamApiObject != null);
#endif  // UNITY_EDITOR || !UNITY_HAS_GOOGLEVR || !UNITY_ANDROID
    }
  }

  /// @cond
  /// The caller is responsible for ensuring that Dispose is called when
  /// the Activity is suspended.
  public void Dispose() {
    m_instance = null;
  }
  /// #endcond

  /// Instantiates a GvrDayreamApi.
  public static void Create() {
    if (m_instance == null) {
      m_instance = new GvrDaydreamApi();
    }
#if !UNITY_EDITOR && UNITY_HAS_GOOGLEVR && UNITY_ANDROID
    if (m_instance.m_daydreamApiObject != null) {
      return;
    }

    if (m_instance.m_daydreamApiClass == null) {
      Debug.LogErrorFormat("Failed to get DaydreamApi class, {0}", PACKAGE_DAYDREAM_API);
      return;
    }

    AndroidJavaObject activity = GvrActivityHelper.GetActivity();
    if (activity == null) {
      return;
    }

    AndroidJavaObject context = GvrActivityHelper.GetApplicationContext(activity);
    if (context == null) {
      return;
    }

    activity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() => {
          m_instance.m_daydreamApiObject =
          m_instance.m_daydreamApiClass.CallStatic<AndroidJavaObject>(METHOD_CREATE, context);
          if (m_instance.m_daydreamApiObject == null) {
            Debug.LogError("DaydreamApi.Create failed to instantiate object");
          }
      })
    );
#endif  // !UNITY_EDITOR && UNITY_HAS_GOOGLEVR && UNITY_ANDROID
  }

  /// Launches VrHome from a VR scene.
  public static void LaunchVrHome() {
#if !UNITY_EDITOR && UNITY_HAS_GOOGLEVR && UNITY_ANDROID
    if (m_instance == null || m_instance.m_daydreamApiObject == null) {
      return;
    }
    m_instance.m_daydreamApiObject.Call(METHOD_LAUNCH_VR_HOMESCREEN);
#endif  // !UNITY_EDITOR && UNITY_HAS_GOOGLEVR && UNITY_ANDROID
  }
}

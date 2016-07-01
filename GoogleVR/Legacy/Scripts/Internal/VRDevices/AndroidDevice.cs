// Copyright 2015 Google Inc. All rights reserved.
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
#if UNITY_ANDROID

using UnityEngine;

/// @cond
namespace Gvr.Internal {
  public class AndroidDevice : GvrDevice {
    private const string ActivityListenerClass =
        "com.google.vr.platform.unity.UnityVrActivityListener";

    private static AndroidJavaObject activityListener;

    public override void Init() {
      SetApplicationState();
      base.Init();
      ConnectToActivity();
    }

    protected override void ConnectToActivity() {
      base.ConnectToActivity();
      if (androidActivity != null && activityListener == null) {
        activityListener = Create(ActivityListenerClass);
      }
    }

    public override void SetVRModeEnabled(bool enabled) {
      CallObjectMethod(activityListener, "setVRModeEnabled", enabled);
    }

    public override void ShowSettingsDialog() {
      CallObjectMethod(activityListener, "launchConfigureActivity");
    }

    public override void OnPause(bool pause) {
      base.OnPause(pause);
      CallObjectMethod(activityListener, "onPause", pause);
    }

    private void SetApplicationState() {
      if (activityListener == null) {
        using (var listenerClass = GetClass(ActivityListenerClass)) {
          CallStaticMethod(listenerClass, "setUnityApplicationState");
        }
      }
    }
  }
}
/// @endcond

#endif

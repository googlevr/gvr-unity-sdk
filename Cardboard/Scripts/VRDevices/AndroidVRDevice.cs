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

public class AndroidVRDevice : VRDevice {
  private AndroidJavaObject activityListener;

  public override void Init() {
#if UNITY_5
    debugDisableNativeDistortion = true;
#endif
    base.Init();
    ConnectToActivity();
  }

  protected override void ConnectToActivity() {
    base.ConnectToActivity();
    if (cardboardActivity != null) {
      activityListener = Create("com.google.vr.platform.unity.UnityVrActivityListener");
    }
  }

  public override void SetVRModeEnabled(bool enabled) {
    CallObjectMethod(activityListener, "setVRModeEnabled", enabled);
  }

  public override void SetTouchCoordinates(int x, int y) {
    CallObjectMethod(activityListener, "setTouchCoordinates", x, y);
  }

  public override void LaunchSettingsDialog() {
    CallObjectMethod(activityListener, "launchSettingsDialog");
  }

  protected override void ProcessEvents() {
    base.ProcessEvents();
    if (!Cardboard.SDK.TapIsTrigger) {
      if (triggered) {
        CallObjectMethod(activityListener, "injectSingleTap");
      }
      if (tilted) {
        CallObjectMethod(activityListener, "injectKeypress", 111);  // Escape key.
      }
    }
  }

  public override void Destroy() {
    if (activityListener != null) {
      activityListener.Dispose();
    }
    base.Destroy();
  }
}

#endif

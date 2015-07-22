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

public class CardboardAndroidDevice : BaseCardboardDevice {
  private static AndroidJavaObject activityListener;

  public override void Init() {
    base.Init();
    ConnectToActivity();
  }

  protected override void ConnectToActivity() {
    base.ConnectToActivity();
    if (androidActivity != null && activityListener == null) {
      TextAsset button = Resources.Load<TextAsset>("CardboardSettingsButton.png");
      activityListener = Create("com.google.vr.platform.unity.UnityVrActivityListener",
                                button.bytes);
    }
  }

  public override void SetVRModeEnabled(bool enabled) {
    CallObjectMethod(activityListener, "setVRModeEnabled", enabled);
  }

  public override void SetSettingsButtonEnabled(bool enabled) {
    CallObjectMethod(activityListener, "setSettingsButtonEnabled", enabled);
  }

  public override void SetTapIsTrigger(bool enabled) {
    CallObjectMethod(activityListener, "setTapIsTrigger", enabled);
  }

  public override void SetTouchCoordinates(int x, int y) {
    CallObjectMethod(activityListener, "setTouchCoordinates", x, y);
  }

  public override bool SetDefaultDeviceProfile(System.Uri uri) {
    bool result = false;
    CallObjectMethod(ref result, activityListener, "setDefaultDeviceProfile", uri.ToString());
    return result;
  }

  public override void ShowSettingsDialog() {
    CallObjectMethod(activityListener, "launchConfigureActivity");
  }

  protected override void ProcessEvents() {
    base.ProcessEvents();
    if (!Cardboard.SDK.TapIsTrigger) {
      if (triggered) {
        CallObjectMethod(activityListener, "injectSingleTap");
      }
      if (tilted) {
        CallObjectMethod(activityListener, "injectKeyPress", 111);  // Escape key.
      }
    }
  }
}

#endif

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
#if UNITY_IOS

using System.Runtime.InteropServices;
using System.Collections.Generic;

public class CardboardiOSDevice : BaseCardboardDevice {
  // Native code libraries use OpenGL, but Unity picks Metal for iOS by default.
  bool isOpenGL = false;

  public override bool SupportsNativeDistortionCorrection(List<string> diagnostics) {
    bool support = base.SupportsNativeDistortionCorrection(diagnostics);
    if (!isOpenGL) {
      diagnostics.Add("Requires OpenGL");
      support = false;
    }
    return support;
  }

  public override bool SupportsNativeUILayer(List<string> diagnostics) {
    bool support = base.SupportsNativeUILayer(diagnostics);
    if (!isOpenGL) {
      diagnostics.Add("Requires OpenGL");
      support = false;
    }
    return support;
  }

  public override void SetVRModeEnabled(bool enabled) {
    setVRModeEnabled(enabled);
  }

  public override void SetSettingsButtonEnabled(bool enabled) {
    setSettingsButtonEnabled(enabled);
  }

  public override void SetAutoDriftCorrectionEnabled(bool enabled){
    // For iOS don't use Drift Correction.
    base.SetAutoDriftCorrectionEnabled(false);
  }

  public override void SetTapIsTrigger(bool enabled) {
    // Not supported on iOS.
  }

  public override bool SetDefaultDeviceProfile(System.Uri uri) {
    byte[] profile = System.Text.Encoding.UTF8.GetBytes(uri.ToString());
    return setDefaultDeviceProfile(profile, profile.Length);
  }

  public override void Init() {
    isOpenGL = isOpenGLAPI();
    setSyncWithCardboardEnabled(Cardboard.SDK.SyncWithCardboardApp);
    base.Init();
    // For iOS don't use Drift Correction.
    SetAutoDriftCorrectionEnabled(false);
  }

  public override void PostRender(bool vrMode) {
    // Do not call GL.IssuePluginEvent() unless OpenGL is the graphics API.
    base.PostRender(vrMode && isOpenGL);
  }

  private bool debugOnboarding = false;

  public override void OnFocus(bool focus) {
    if (focus && (debugOnboarding || !isOnboardingDone())) {
      debugOnboarding = false;
      launchOnboardingDialog();
    }
  }

  public override void OnPause(bool pause) {
    if (!pause) {
      readProfile();
    }
  }

  public override void ShowSettingsDialog() {
    launchSettingsDialog();
  }

  [DllImport("__Internal")]
  private static extern bool isOpenGLAPI();

  [DllImport("__Internal")]
  private static extern void setVRModeEnabled(bool enabled);

  [DllImport("__Internal")]
  private static extern void setSettingsButtonEnabled(bool enabled);

  [DllImport("__Internal")]
  private static extern void setSyncWithCardboardEnabled(bool enabled);

  [DllImport("__Internal")]
  private static extern void readProfile();

  [DllImport("__Internal")]
  private static extern bool setDefaultDeviceProfile(byte[] profileData, int size);

  [DllImport("__Internal")]
  private static extern bool isOnboardingDone();

  [DllImport("__Internal")]
  private static extern void launchSettingsDialog();

  [DllImport("__Internal")]
  private static extern void launchOnboardingDialog();
}

#endif

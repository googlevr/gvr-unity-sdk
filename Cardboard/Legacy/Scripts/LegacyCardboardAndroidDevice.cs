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

using System.Runtime.InteropServices;
using UnityEngine;

// Android device using the Google Cardboard SDK for Android.
public class LegacyCardboardAndroidDevice : BaseAndroidDevice {
  [DllImport("RenderingPlugin")]
  private static extern void InitFromUnity(int textureID);

  // Event IDs supported by our native render plugin.
  private const int kPerformDistortionCorrection = 1;
  private const int kDrawCardboardUILayer = 2;

  private const string cardboardClass =
      "com.google.vrtoolkit.cardboard.plugins.unity.UnityCardboardActivity";

  private Matrix4x4 headView = new Matrix4x4();
  private Matrix4x4 leftEyeView = new Matrix4x4();
  private Matrix4x4 rightEyeView = new Matrix4x4();

  private float[] frameInfo = null;

  public override void Init() {
    ConnectToActivity();
  }

  protected override void ConnectToActivity() {
    try {
      using (AndroidJavaClass player = new AndroidJavaClass(cardboardClass)) {
        androidActivity = player.CallStatic<AndroidJavaObject>("getActivity");
      }
    } catch (AndroidJavaException e) {
      androidActivity = null;
      Debug.LogError("Cannot access UnityCardboardActivity. "
        + "Verify that the jar is in Assets/Plugins/Android. " + e);
    }
  }

  public override void SetDistortionCorrectionEnabled(bool enabled) {
    CallObjectMethod(androidActivity, "setDistortionCorrectionEnabled", enabled);
  }

  public override void SetVRModeEnabled(bool enabled) {
    CallObjectMethod(androidActivity, "setVRModeEnabled", enabled);
  }

  public override void SetAlignmentMarkerEnabled(bool enabled) {
    CallObjectMethod(androidActivity, "setAlignmentMarkerEnabled", enabled);
  }

  public override void SetSettingsButtonEnabled(bool enabled) {
    CallObjectMethod(androidActivity, "setSettingsButtonEnabled", enabled);
  }

  public override void SetTapIsTrigger(bool enabled) {
    CallObjectMethod(androidActivity, "setTapIsTrigger", enabled);
  }

  public override void SetNeckModelScale(float scale) {
    CallObjectMethod(androidActivity, "setNeckModelFactor", scale);
  }

  public override void SetAutoDriftCorrectionEnabled(bool enabled) {
    CallObjectMethod(androidActivity, "setGyroBiasEstimationEnabled", enabled);
  }

  public override void SetStereoScreen(RenderTexture stereoScreen) {
    if (androidActivity != null) {
      InitFromUnity(stereoScreen != null ? stereoScreen.GetNativeTextureID() : 0);
    }
  }

  public override void UpdateState() {
    // Pass nominal clip distances - will correct later for each camera.
	if (!CallObjectMethod(ref frameInfo, androidActivity, "getFrameParams", 1.0f /* near */, 1000.0f /* far */)) {
      return;
    }

    // Extract the matrices (currently that's all we get back).
    int j = 0;
    for (int i = 0; i < 16; ++i, ++j) {
      headView[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      leftEyeView[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      leftEyeDistortedProjection[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      rightEyeView[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      rightEyeDistortedProjection[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      leftEyeUndistortedProjection[i] = frameInfo[j];
    }
    for (int i = 0; i < 16; ++i, ++j) {
      rightEyeUndistortedProjection[i] = frameInfo[j];
    }

    leftEyeUndistortedViewport = new Rect(frameInfo[j], frameInfo[j+1],
                                          frameInfo[j+2], frameInfo[j+3]);
    leftEyeDistortedViewport = leftEyeUndistortedViewport;
    j += 4;
    rightEyeUndistortedViewport = new Rect(frameInfo[j], frameInfo[j+1],
                                           frameInfo[j+2], frameInfo[j+3]);
    rightEyeDistortedViewport = rightEyeUndistortedViewport;

    // Convert views to left-handed coordinates because Unity uses them
    // for Transforms, which is what we will update from the views.
    // Also invert because the incoming matrices go from camera space to
    // cardboard space, and we want the opposite.
    // Lastly, cancel out the head rotation from the eye views,
    // because we are applying that on a parent object.
    leftEyePose.SetRightHanded(headView * leftEyeView.inverse);
    rightEyePose.SetRightHanded(headView * rightEyeView.inverse);
    headPose.SetRightHanded(headView.inverse);
  }

  override public void UpdateScreenData() {
    CardboardProfile.Device device = new CardboardProfile.Device();
    CardboardProfile.Screen screen = new CardboardProfile.Screen();

    float[] lensData = null;
	if (CallObjectMethod(ref lensData, androidActivity, "getLensParameters")) {
      device.lenses.separation = lensData[0];
      device.lenses.offset = lensData[1];
      device.lenses.screenDistance = lensData[2];
      device.lenses.alignment = (int)lensData[3];
    }

    float[] screenSize = null;
	if (CallObjectMethod(ref screenSize, androidActivity, "getScreenSizeMeters")) {
      screen.width = screenSize[0];
      screen.height = screenSize[1];
      screen.border = screenSize[2];
    }

    float[] distCoeff = null;
	if (CallObjectMethod(ref distCoeff, androidActivity, "getDistortionCoefficients")) {
      device.distortion.k1 = distCoeff[0];
      device.distortion.k2 = distCoeff[1];
    }

    float[] invDistCoeff = null;
	if (CallObjectMethod(ref invDistCoeff, androidActivity, "getInverseDistortionCoefficients")) {
      device.inverse.k1 = invDistCoeff[0];
      device.inverse.k2 = invDistCoeff[1];
    }

    float[] maxFov = null;
	if (CallObjectMethod(ref maxFov, androidActivity, "getLeftEyeMaximumFOV")) {
      device.maxFOV.outer = maxFov[0];
      device.maxFOV.upper = maxFov[1];
      device.maxFOV.inner = maxFov[2];
      device.maxFOV.lower = maxFov[3];
    }

    Profile.screen = screen;
    Profile.device = device;
  }

  public override void Recenter() {
    CallObjectMethod(androidActivity, "resetHeadTracker");
  }

  public override void SetTouchCoordinates(int x, int y) {
    CallObjectMethod(androidActivity, "setTouchCoordinates", x, y);
  }

  public override void PostRender(bool vrMode) {
    if (vrMode) {
      if (Cardboard.SDK.DistortionCorrection && Cardboard.SDK.NativeDistortionCorrectionSupported) {
        GL.IssuePluginEvent(kPerformDistortionCorrection);
      }
      if (Cardboard.SDK.EnableAlignmentMarker || Cardboard.SDK.EnableSettingsButton) {
        GL.IssuePluginEvent(kDrawCardboardUILayer);
      }
      GL.InvalidateState();
    }
  }
}

#endif

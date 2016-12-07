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

using UnityEngine;
using System.Runtime.InteropServices;

/// @cond
namespace Gvr.Internal {
  public abstract class GvrDevice :
#if UNITY_ANDROID
  BaseAndroidDevice
#else
  BaseVRDevice
#endif
  {
    // A relatively unique id to use when calling our C++ native render plugin.
    private const int kRenderEvent = 0x47554342;

    // Event IDs sent up from native layer.  Bit flags.
    // Keep in sync with the corresponding declaration in unity.h.
    private const int kTilted = 1 << 1;
    private const int kProfileChanged = 1 << 2;
    private const int kVRBackButtonPressed = 1 << 3;

    private float[] headData = new float[16];
    private float[] viewData = new float[16 * 6 + 12];
    private float[] profileData = new float[13];

    private Matrix4x4 headView = new Matrix4x4();
    private Matrix4x4 leftEyeView = new Matrix4x4();
    private Matrix4x4 rightEyeView = new Matrix4x4();

    protected bool debugDisableNativeProjections = false;
    protected bool debugDisableNativeUILayer = false;

    public override void SetDistortionCorrectionEnabled(bool enabled) {
      EnableDistortionCorrection(enabled);
    }

    public override void SetNeckModelScale(float scale) {
      SetNeckModelFactor(scale);
    }

    public override bool SetDefaultDeviceProfile(System.Uri uri) {
      byte[] profile = System.Text.Encoding.UTF8.GetBytes(uri.ToString());
      return SetDefaultProfile(profile, profile.Length);
    }

    public override void Init() {
      // Start will send a log event, so SetUnityVersion first.
      byte[] version = System.Text.Encoding.UTF8.GetBytes(Application.unityVersion);
      SetUnityVersion(version, version.Length);
      Start();
    }

    public override void UpdateState() {
      ProcessEvents();
      GetHeadPose(headData);
      ExtractMatrix(ref headView, headData);
      headPose.SetRightHanded(headView.inverse);
    }

    public override void UpdateScreenData() {
      UpdateProfile();
      if (debugDisableNativeProjections) {
        ComputeEyesFromProfile();
      } else {
        UpdateView();
      }
      profileChanged = true;
    }

    public override void Recenter() {
      ResetHeadTracker();
    }

    public override void PostRender(RenderTexture stereoScreen) {
      SetTextureId((int)stereoScreen.GetNativeTexturePtr());
      GL.IssuePluginEvent(kRenderEvent);
    }

    public override void OnPause(bool pause) {
      if (pause) {
        Pause();
      } else {
        Resume();
      }
    }

    public override void OnApplicationQuit() {
      Stop();
      base.OnApplicationQuit();
    }

    private void UpdateView() {
      GetViewParameters(viewData);
      int j = 0;

      j = ExtractMatrix(ref leftEyeView, viewData, j);
      j = ExtractMatrix(ref rightEyeView, viewData, j);
      leftEyePose.SetRightHanded(leftEyeView.inverse);
      rightEyePose.SetRightHanded(rightEyeView.inverse);

      j = ExtractMatrix(ref leftEyeDistortedProjection, viewData, j);
      j = ExtractMatrix(ref rightEyeDistortedProjection, viewData, j);
      j = ExtractMatrix(ref leftEyeUndistortedProjection, viewData, j);
      j = ExtractMatrix(ref rightEyeUndistortedProjection, viewData, j);

      leftEyeUndistortedViewport.Set(viewData[j], viewData[j+1], viewData[j+2], viewData[j+3]);
      leftEyeDistortedViewport = leftEyeUndistortedViewport;
      j += 4;

      rightEyeUndistortedViewport.Set(viewData[j], viewData[j+1], viewData[j+2], viewData[j+3]);
      rightEyeDistortedViewport = rightEyeUndistortedViewport;
      j += 4;

      leftEyeOrientation = (int)viewData[j];
      rightEyeOrientation = (int)viewData[j+1];
      j += 2;

      recommendedTextureSize = new Vector2(viewData[j], viewData[j+1]);
      j += 2;
    }

    private void UpdateProfile() {
      GetProfile(profileData);
      GvrProfile.Viewer device = new GvrProfile.Viewer();
      GvrProfile.Screen screen = new GvrProfile.Screen();
      device.maxFOV.outer = profileData[0];
      device.maxFOV.upper = profileData[1];
      device.maxFOV.inner = profileData[2];
      device.maxFOV.lower = profileData[3];
      screen.width = profileData[4];
      screen.height = profileData[5];
      screen.border = profileData[6];
      device.lenses.separation = profileData[7];
      device.lenses.offset = profileData[8];
      device.lenses.screenDistance = profileData[9];
      device.lenses.alignment = (int)profileData[10];
      device.distortion.Coef = new [] { profileData[11], profileData[12] };
      Profile.screen = screen;
      Profile.viewer = device;

      float[] rect = new float[4];
      Profile.GetLeftEyeNoLensTanAngles(rect);
      float maxRadius = GvrProfile.GetMaxRadius(rect);
      Profile.viewer.inverse = GvrProfile.ApproximateInverse(
          Profile.viewer.distortion, maxRadius);
    }

    private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0) {
      // Matrices returned from our native layer are in row-major order.
      for (int r = 0; r < 4; r++) {
        for (int c = 0; c < 4; c++, i++) {
          mat[r, c] = data[i];
        }
      }
      return i;
    }

    protected virtual void ProcessEvents() {
      int flags = GetEventFlags();
      tilted = ((flags & kTilted) != 0);
      backButtonPressed = ((flags & kVRBackButtonPressed) != 0);
      if ((flags & kProfileChanged) != 0) {
        UpdateScreenData();
      }
    }

#if UNITY_IOS
    private const string dllName = "__Internal";
#elif UNITY_HAS_GOOGLEVR
    private const string dllName = "gvr";
#else
    private const string dllName = "gvrunity";
#endif  // UNITY_IOS

    [DllImport(dllName)]
    private static extern void Start();

    [DllImport(dllName)]
    private static extern void SetTextureId(int id);

    [DllImport(dllName)]
    private static extern bool SetDefaultProfile(byte[] uri, int size);

    [DllImport(dllName)]
    private static extern void SetUnityVersion(byte[] version_str, int version_length);

    [DllImport(dllName)]
    private static extern void EnableDistortionCorrection(bool enable);

    [DllImport(dllName)]
    private static extern void SetNeckModelFactor(float factor);

    [DllImport(dllName)]
    private static extern void ResetHeadTracker();

    [DllImport(dllName)]
    private static extern int  GetEventFlags();

    [DllImport(dllName)]
    private static extern void GetProfile(float[] profile);

    [DllImport(dllName)]
    private static extern void GetHeadPose(float[] pose);

    [DllImport(dllName)]
    private static extern void GetViewParameters(float[] viewParams);

    [DllImport(dllName)]
    private static extern void Pause();

    [DllImport(dllName)]
    private static extern void Resume();

    [DllImport(dllName)]
    private static extern void Stop();
  }
}
/// @endcond

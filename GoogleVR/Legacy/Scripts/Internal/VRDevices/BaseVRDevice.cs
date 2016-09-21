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

#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IPHONE_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections.Generic;
using System;

/// @cond
namespace Gvr.Internal {
  // Represents a vr device that this plugin interacts with.
  public abstract class BaseVRDevice {
    private static BaseVRDevice device = null;

    protected BaseVRDevice() {
      Profile = GvrProfile.Default.Clone();
    }

    public GvrProfile Profile { get; protected set; }

    public abstract void Init();

    public abstract void SetVRModeEnabled(bool enabled);

    public abstract void SetNeckModelScale(float scale);

    public virtual bool SupportsNativeUILayer(List<string> diagnostics) {
      return true;
    }

    public virtual RenderTexture CreateStereoScreen() {
      float scale = GvrViewer.Instance.StereoScreenScale;
      int width = Mathf.RoundToInt(Screen.width * scale);
      int height = Mathf.RoundToInt(Screen.height * scale);

      //Debug.Log("Creating new default stereo screen texture "
      //    + width+ "x" + height + ".");
      var rt = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
      rt.anisoLevel = 0;
      rt.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1);
      return rt;
    }

    // Returns true if the URI was set as the device profile, else false.  A default URI
    // is only accepted if the user has not scanned a QR code already.
    public virtual bool SetDefaultDeviceProfile(Uri uri) {
      return false;
    }

    public virtual void ShowSettingsDialog() {
      // Do nothing.
    }

    public Pose3D GetHeadPose() {
      return this.headPose;
    }
    protected MutablePose3D headPose = new MutablePose3D();

    public Pose3D GetEyePose(GvrViewer.Eye eye) {
      switch(eye) {
        case GvrViewer.Eye.Left:
          return leftEyePose;
        case GvrViewer.Eye.Right:
          return rightEyePose;
        default:
          return null;
      }
    }
    protected MutablePose3D leftEyePose = new MutablePose3D();
    protected MutablePose3D rightEyePose = new MutablePose3D();

    public Matrix4x4 GetProjection(GvrViewer.Eye eye,
                                   GvrViewer.Distortion distortion = GvrViewer.Distortion.Distorted) {
      switch(eye) {
        case GvrViewer.Eye.Left:
          return distortion == GvrViewer.Distortion.Distorted ?
              leftEyeDistortedProjection : leftEyeUndistortedProjection;
        case GvrViewer.Eye.Right:
          return distortion == GvrViewer.Distortion.Distorted ?
              rightEyeDistortedProjection : rightEyeUndistortedProjection;
        default:
          return Matrix4x4.identity;
      }
    }
    protected Matrix4x4 leftEyeDistortedProjection;
    protected Matrix4x4 rightEyeDistortedProjection;
    protected Matrix4x4 leftEyeUndistortedProjection;
    protected Matrix4x4 rightEyeUndistortedProjection;

    public Rect GetViewport(GvrViewer.Eye eye,
                            GvrViewer.Distortion distortion = GvrViewer.Distortion.Distorted) {
      switch(eye) {
        case GvrViewer.Eye.Left:
          return distortion == GvrViewer.Distortion.Distorted ?
              leftEyeDistortedViewport : leftEyeUndistortedViewport;
        case GvrViewer.Eye.Right:
          return distortion == GvrViewer.Distortion.Distorted ?
              rightEyeDistortedViewport : rightEyeUndistortedViewport;
        default:
          return new Rect();
      }
    }
    protected Rect leftEyeDistortedViewport;
    protected Rect rightEyeDistortedViewport;
    protected Rect leftEyeUndistortedViewport;
    protected Rect rightEyeUndistortedViewport;

    protected Vector2 recommendedTextureSize;
    protected int leftEyeOrientation;
    protected int rightEyeOrientation;

    public bool tilted;
    public bool profileChanged;
    public bool backButtonPressed;

    public abstract void UpdateState();

    public abstract void UpdateScreenData();

    public abstract void Recenter();

    public virtual void OnPause(bool pause) {
      if (!pause) {
        UpdateScreenData();
      }
    }

    public virtual void OnFocus(bool focus) {
      // Do nothing.
    }

    public virtual void OnApplicationQuit() {
      // Do nothing.
    }

    public virtual void Destroy() {
      if (device == this) {
        device = null;
      }
    }

    // Helper functions.
    protected void ComputeEyesFromProfile() {
      // Compute left eye matrices from screen and device params
      Matrix4x4 leftEyeView = Matrix4x4.identity;
      leftEyeView[0, 3] = -Profile.viewer.lenses.separation / 2;
      leftEyePose.Set(leftEyeView);

      float[] rect = new float[4];
      Profile.GetLeftEyeVisibleTanAngles(rect);
      leftEyeDistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);
      Profile.GetLeftEyeNoLensTanAngles(rect);
      leftEyeUndistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);

      leftEyeUndistortedViewport = Profile.GetLeftEyeVisibleScreenRect(rect);
      leftEyeDistortedViewport = leftEyeUndistortedViewport;

      // Right eye matrices same as left ones but for some sign flippage.
      Matrix4x4 rightEyeView = leftEyeView;
      rightEyeView[0, 3] *= -1;
      rightEyePose.Set(rightEyeView);

      rightEyeDistortedProjection = leftEyeDistortedProjection;
      rightEyeDistortedProjection[0, 2] *= -1;
      rightEyeUndistortedProjection = leftEyeUndistortedProjection;
      rightEyeUndistortedProjection[0, 2] *= -1;

      rightEyeUndistortedViewport = leftEyeUndistortedViewport;
      rightEyeUndistortedViewport.x = 1 - rightEyeUndistortedViewport.xMax;
      rightEyeDistortedViewport = rightEyeUndistortedViewport;

      float width = Screen.width * (leftEyeUndistortedViewport.width+rightEyeDistortedViewport.width);
      float height = Screen.height * Mathf.Max(leftEyeUndistortedViewport.height,
                                               rightEyeUndistortedViewport.height);
      recommendedTextureSize = new Vector2(width, height);
    }

    private static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f) {
      Matrix4x4 m = Matrix4x4.zero;
      m[0, 0] = 2 * n / (r - l);
      m[1, 1] = 2 * n / (t - b);
      m[0, 2] = (r + l) / (r - l);
      m[1, 2] = (t + b) / (t - b);
      m[2, 2] = (n + f) / (n - f);
      m[2, 3] = 2 * n * f / (n - f);
      m[3, 2] = -1;
      return m;
    }

    public static BaseVRDevice GetDevice() {
      if (device == null) {
#if UNITY_EDITOR
        device = new EditorDevice();
#elif ANDROID_DEVICE
    #if UNITY_HAS_GOOGLEVR
        device = new UnityVRDevice();
    #else
        device = new AndroidDevice();
    #endif  // UNITY_HAS_GOOGLEVR
#elif IPHONE_DEVICE
        device = new iOSDevice();
#else
        throw new InvalidOperationException("Unsupported device.");
#endif  // UNITY_EDITOR
      }
      return device;
    }
  }
}
/// @endcond


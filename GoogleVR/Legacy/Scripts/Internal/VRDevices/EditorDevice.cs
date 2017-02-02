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
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// @cond
namespace Gvr.Internal {
  // Sends simulated values for use when testing within the Unity Editor.
  public class EditorDevice : BaseVRDevice {
    // Simulated neck model.  Vector from the neck pivot point to the point between the eyes.
    private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, 0.08f);

    // Use mouse to emulate head in the editor.
    // These variables must be static so that head pose is maintained between scene changes,
    // as it is on device.
    private static float mouseX = 0;
    private static float mouseY = 0;
    private static float mouseZ = 0;

    public override void Init() {
      Input.gyro.enabled = true;
    }

    public override bool SupportsNativeDistortionCorrection(List<string> diagnostics) {
      return false;  // No need for diagnostic message.
    }

    public override bool SupportsNativeUILayer(List<string> diagnostics) {
      return false;  // No need for diagnostic message.
    }

    // Since we can check all these settings by asking Gvr.Instance, no need
    // to keep a separate copy here.
    public override void SetVRModeEnabled(bool enabled) {}
    public override void SetDistortionCorrectionEnabled(bool enabled) {}
    public override void SetNeckModelScale(float scale) {}

    private Quaternion initialRotation = Quaternion.identity;

    private bool remoteCommunicating = false;
    private bool RemoteCommunicating {
      get {
        if (!remoteCommunicating) {
          remoteCommunicating = EditorApplication.isRemoteConnected;
        }
        return remoteCommunicating;
      }
    }

    public override void UpdateState() {
      Quaternion rot;
      if (GvrViewer.Instance.UseUnityRemoteInput && RemoteCommunicating) {
        var att = Input.gyro.attitude * initialRotation;
        att = new Quaternion(att.x, att.y, -att.z, -att.w);
        rot = Quaternion.Euler(90, 0, 0) * att;
      } else {
        bool rolled = false;
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
          mouseX += Input.GetAxis("Mouse X") * 5;
          if (mouseX <= -180) {
            mouseX += 360;
          } else if (mouseX > 180) {
            mouseX -= 360;
          }
          mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
          mouseY = Mathf.Clamp(mouseY, -85, 85);
        } else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
          rolled = true;
          mouseZ += Input.GetAxis("Mouse X") * 5;
          mouseZ = Mathf.Clamp(mouseZ, -85, 85);
        }
        if (!rolled && GvrViewer.Instance.autoUntiltHead) {
          // People don't usually leave their heads tilted to one side for long.
          mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
        }
        rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
      }
      var neck = (rot * neckOffset - neckOffset.y * Vector3.up) * GvrViewer.Instance.NeckModelScale;
      headPose.Set(neck, rot);

      tilted = Input.GetKeyUp(KeyCode.Escape);
    }

    public override void PostRender(RenderTexture stereoScreen) {
      // Do nothing.
    }

    public override void UpdateScreenData() {
      Profile = GvrProfile.GetKnownProfile(GvrViewer.Instance.ScreenSize, GvrViewer.Instance.ViewerType);
      ComputeEyesFromProfile();
      profileChanged = true;
    }

    public override void Recenter() {
      mouseX = mouseZ = 0;  // Do not reset pitch, which is how it works on the phone.
      if (RemoteCommunicating) {
        //initialRotation = Quaternion.Inverse(Input.gyro.attitude);
      }
    }
  }
}
/// @endcond

#endif

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
using UnityEngine.EventSystems;
using System.Collections;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
using XRNode = UnityEngine.VR.VRNode;
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// Helper functions common to GVR VR applications.
public static class GvrVRHelpers {
  public static Vector2 GetViewportCenter() {
    int viewportWidth = Screen.width;
    int viewportHeight = Screen.height;
    if (XRSettings.enabled) {
      viewportWidth = XRSettings.eyeTextureWidth;
      viewportHeight = XRSettings.eyeTextureHeight;
    }

    return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
  }

  public static Vector3 GetHeadForward() {
    return GetHeadRotation() * Vector3.forward;
  }

  public static Quaternion GetHeadRotation() {
#if UNITY_EDITOR
    if (GvrEditorEmulator.Instance == null) {
      Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please ensure that" +
        "GvrEditorEmulator exists in your scene.");
      return Quaternion.identity;
    }

    return GvrEditorEmulator.Instance.HeadRotation;
#else
    return InputTracking.GetLocalRotation(XRNode.Head);
#endif // UNITY_EDITOR
  }

  public static Vector3 GetHeadPosition() {
#if UNITY_EDITOR
    if (GvrEditorEmulator.Instance == null) {
      Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please ensure that" +
        "GvrEditorEmulator exists in your scene.");
      return Vector3.zero;
    }

    return GvrEditorEmulator.Instance.HeadPosition;
#else
    return InputTracking.GetLocalPosition(XRNode.Head);
#endif // UNITY_EDITOR
  }
}

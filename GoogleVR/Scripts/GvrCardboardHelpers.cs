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

// General GVR helpers.
public class GvrCardboardHelpers {
  /// Manual recenter for Cardboard.
  /// Do not use for controller-based Daydream recenter - Google VR Services will take care
  /// of that, no C# implementation behaviour is needed.
  /// Apply the recenteringOffset to the Camera or its parent at runtime.
  public static void Recenter() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    gvr_reset_tracking(VRDevice.GetNativePtr());
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    Debug.Log("Use GvrEditorEmulator for in-editor recentering");
  }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
  [DllImport("gvr")]
  private static extern void gvr_reset_tracking(IntPtr gvr_context);
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR

}

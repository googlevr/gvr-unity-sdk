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
using UnityEngine;

/// @cond
namespace Gvr.Internal {
  public class iOSDevice : GvrDevice {
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

    public override void SetVRModeEnabled(bool enabled) {
      setVRModeEnabled(enabled);
    }

    public override void Init() {
      isOpenGL = isOpenGLAPI();
      base.Init();
    }

    public override void ShowSettingsDialog() {
      launchSettingsDialog();
    }

    [DllImport("__Internal")]
    private static extern bool isOpenGLAPI();

    [DllImport("__Internal")]
    private static extern void setVRModeEnabled(bool enabled);

    [DllImport("__Internal")]
    private static extern void launchSettingsDialog();
  }
}
/// @endcond

#endif

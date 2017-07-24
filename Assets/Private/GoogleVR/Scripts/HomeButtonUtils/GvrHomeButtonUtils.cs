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

#if UNITY_ANDROID
using UnityEngine;

using System;
using System.Runtime.InteropServices;

/// @cond

/// GVR Home button handling utilities.
///
/// DO NOT RELEASE. THIS IS A TEMPORARY IMPLEMENTATION FOR INTERNAL USE.
/// If we are seriously considering adding home button support to the SDK, we should make it
/// part of GvrController itself, hooking it up properly in AndroidNativeControllerProvider, etc.
/// For reference, see this CL: cl/138930987
///
/// NOTE: the home button is not meant for general use, so this script is separate from
/// GvrController and should only be used internally for prototyping and experimentation.
///
/// Remember that, to prevent the home button from triggering VR Home, you have to add your
/// app's package name to the "Controller Home Button Bypass" list in the Google VR Services
/// Settings screen.
public static class GvrHomeButtonUtils {
  // As defined in //vr/gvr/capi/include/gvr_types.h
  private const int GVR_CONTROLLER_BUTTON_HOME = 2;

  /// Returns whether the home button is currently pressed. This is true while the button is
  /// being held down.
  public static bool GetHomeButton() {
    return (GvrControllerInput.StatePtr != IntPtr.Zero) && (0 != gvr_controller_state_get_button_state(
        GvrControllerInput.StatePtr, GVR_CONTROLLER_BUTTON_HOME));
  }

  /// Returns whether the home button was just PRESSED this frame. This is true for one frame
  /// after the button was pressed.
  public static bool GetHomeButtonDown() {
    return (GvrControllerInput.StatePtr != IntPtr.Zero) && (0 != gvr_controller_state_get_button_down(
        GvrControllerInput.StatePtr, GVR_CONTROLLER_BUTTON_HOME));
  }

  /// Returns whether the home button was just RELEASED this frame. This is true for one frame
  /// after the button was released.
  public static bool GetHomeButtonUp() {
    return (GvrControllerInput.StatePtr != IntPtr.Zero) && (0 != gvr_controller_state_get_button_up(
        GvrControllerInput.StatePtr, GVR_CONTROLLER_BUTTON_HOME));
  }

#if !UNITY_EDITOR
  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern byte gvr_controller_state_get_button_state(IntPtr state, int button);
  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern byte gvr_controller_state_get_button_down(IntPtr state, int button);
  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern byte gvr_controller_state_get_button_up(IntPtr state, int button);
#else
  private static byte gvr_controller_state_get_button_state(IntPtr state, int button) {
    return 0;
  }
  private static byte gvr_controller_state_get_button_down(IntPtr state, int button)  {
    return 0;
  }
  private static byte gvr_controller_state_get_button_up(IntPtr state, int button)  {
    return 0;
  }
#endif  // !UNITY_EDITOR
}
// @endcond
#endif  // UNITY_ANDROID

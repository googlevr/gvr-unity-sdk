// Copyright 2016 Google Inc. All rights reserved.
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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;

using System;
using System.Runtime.InteropServices;

/// @cond
namespace Gvr.Internal {

  /// Controller Provider that uses the native GVR C API to communicate with
  /// controllers via Google VR Services on Android.
  class GestureProvider {
    private static readonly GvrGesture.Button[] buttons =
      {
        GvrGesture.Button.Click,
        GvrGesture.Button.Home,
        GvrGesture.Button.App,
        GvrGesture.Button.VolumeUp,
        GvrGesture.Button.VolumeDown
      };
    private const string dllName = "gvr_gesture";
    private IntPtr context;

    // Please keep these numbers in sync with numbers from gvr_gesture.h
    private enum GestureType {
      Swipe = 0,
      ScrollStart = 1,
      ScrollUpdate = 2,
      ScrollEnd = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gvr_vec2 {
      internal float x;
      internal float y;
    }

    public void Initialize() {
      context = gvr_gesture_context_init();
    }

    public void Destroy() {
      gvr_gesture_context_destroy(ref context);
    }

    // Update the gesture information. If the controller state pointer is
    // not initialized, return false.
    public bool UpdateGesture() {
      if (GvrControllerInput.StatePtr == IntPtr.Zero) {
        return false;
      }
      gvr_gesture_update(GvrControllerInput.StatePtr, context);
      return true;
    }

    public bool SwipeDetected() {
      int num_gestures = gvr_gesture_get_count(context);
      return num_gestures == 2;
    }

    public GvrGesture.State GetDetectorState() {
      int num_gestures = gvr_gesture_get_count(context);
      // No gesture is detected.
      if (num_gestures == 0)
        return GvrGesture.State.Idle;

      GestureType type = gvr_gesture_get_type(gvr_gesture_get(context, 0));
      switch (type) {
        case GestureType.ScrollStart:
          return GvrGesture.State.Start;
        case GestureType.ScrollUpdate:
          return GvrGesture.State.Update;
        case GestureType.ScrollEnd:
          return GvrGesture.State.End;
        default:
          // Shouldn't happen.
          Debug.LogError("Invalid gesture type: " + type);
          return GvrGesture.State.Idle;
      }
    }

    public Vector2 GetDisplacement() {
      if (gvr_gesture_get_count(context) == 0) {
        return Vector2.zero;
      }
      gvr_vec2 displacement = gvr_gesture_get_displacement(gvr_gesture_get(context, 0));
      return new Vector2(displacement.x, -displacement.y);
    }

    public Vector2 GetVelocity() {
      if (gvr_gesture_get_count(context) == 0) {
        return Vector2.zero;
      }
      gvr_vec2 velocity = gvr_gesture_get_velocity(gvr_gesture_get(context, 0));
      return new Vector2(velocity.x, -velocity.y);
    }

    public GvrGesture.Direction GetDirection() {
      if (gvr_gesture_get_count(context) == 0)
        return GvrGesture.Direction.None;
      return gvr_gesture_get_direction(gvr_gesture_get(context, 0));
    }

    public GvrGesture.Button GetLongPressButton() {
      if (GvrControllerInput.StatePtr == IntPtr.Zero) {
        return GvrGesture.Button.None;
      }
      for (var i = 0; i < buttons.Length; i++) {
        if (gvr_get_button_long_press(GvrControllerInput.StatePtr, context, buttons[i])) {
          return buttons[i];
        }
      }
      return GvrGesture.Button.None;
    }

    [DllImport(dllName)]
    private static extern IntPtr gvr_gesture_context_init();

    [DllImport(dllName)]
    private static extern void gvr_gesture_update(IntPtr controller_state, IntPtr context);

    [DllImport(dllName)]
    private static extern int gvr_gesture_get_count(IntPtr context);

    [DllImport(dllName)]
    private static extern void gvr_gesture_context_destroy(ref IntPtr context);

    [DllImport(dllName)]
    private static extern void gvr_gesture_restart(IntPtr context);

    [DllImport(dllName)]
    private static extern IntPtr gvr_gesture_get(IntPtr context, int index);

    [DllImport(dllName)]
    private static extern GestureType gvr_gesture_get_type(IntPtr gesture);

    [DllImport(dllName)]
    private static extern GvrGesture.Direction gvr_gesture_get_direction(IntPtr gesture);

    [DllImport(dllName)]
    private static extern gvr_vec2 gvr_gesture_get_velocity(IntPtr gvr_gesture);

    [DllImport(dllName)]
    private static extern gvr_vec2 gvr_gesture_get_displacement(IntPtr gesture);

    [DllImport(dllName)]
    private static extern bool gvr_get_button_long_press(
      IntPtr controller_state,
      IntPtr context,
      GvrGesture.Button button);
  }
}
// @endcond

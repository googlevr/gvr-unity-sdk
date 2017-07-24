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
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.VR;
using System;
using System.Collections;

using Gvr.Internal;

/// Main entry point for the Daydream gesture API.
///
/// To use this API, add this behavior to a GameObject in your scene. There can
/// only be one object with this behavior on your scene.
///
/// This is a singleton object.
///
/// To access the gesture, simply read the static properties of this class.

public class GvrGesture : MonoBehaviour {
  public enum State {
    Idle = 0,
    Start = 1,
    // Gesture detection starts in currenat frame (transient state
    // for a single frame).
    Update = 2,
    // Gesture detection is on-going.
    End = 3,
    // Gesture detection is ending in current frame (transient state
    // for a single frame).
  }

  // Please keep these numbers in sync with numbers in gvr_gesture.h
  public enum Direction {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    None = 4,
    // No gesture is detected.
  }

  // Please keep the numbers in sync with gvr_controller_button in gvr_types.h.
  public enum Button {
    None = 0,
    Click = 1,
    Home = 2,
    App = 3,
    VolumeUp = 4,
    VolumeDown = 5,
  }

  private static GestureProvider gestureProvider = new GestureProvider();

  // Returns whether the current gesture is a swipe.
  public static bool SwipeDetected {
    get { return gestureProvider.SwipeDetected(); }
  }

  // Returns the state of current gesture (wait / start / update / end).
  public static State DetectorState {
    get { return gestureProvider.GetDetectorState(); }
  }

  // Returns the cumulated velocity of current gesture.
  public static Vector2 Velocity {
    get { return gestureProvider.GetVelocity(); }
  }

  // Returns the displacement of current gesture.
  public static Vector2 Displacement {
    get { return gestureProvider.GetDisplacement(); }
  }

  // Returns the direction of current gesture.
  public static Direction GestureDirection {
    get { return gestureProvider.GetDirection(); }
  }

  // Returns which button is being long-pressed.
  public static Button LongPressButton {
    get { return gestureProvider.GetLongPressButton(); }
  }

  void Awake() {
    gestureProvider.Initialize();
  }

  // Update the gesture information for current frame.
  public static void UpdateDetector() {
    if (!gestureProvider.UpdateGesture()) {
      Debug.Log("Controller state is not initialized.");
    }
  }

  void OnDestroy() {
    gestureProvider.Destroy();
  }
}

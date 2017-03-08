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

// The controller is not available for versions of Unity without the
// // GVR native integration.
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;

/// @cond
namespace Gvr.Internal {
  /// Controller provider that connects to the controller emulator to obtain controller events.
  class EmulatorControllerProvider : IControllerProvider {
    private ControllerState state = new ControllerState();

    /// Yaw correction due to recentering.
    private Quaternion yawCorrection = Quaternion.identity;

    /// True if we performed the initial recenter.
    private bool initialRecenterDone = false;

    /// The last (uncorrected) orientation received from the emulator.
    private Quaternion lastRawOrientation = Quaternion.identity;

    /// Creates a new EmulatorControllerProvider with the specified settings.
    internal EmulatorControllerProvider(GvrController.EmulatorConnectionMode connectionMode) {
      if (connectionMode == GvrController.EmulatorConnectionMode.USB) {
        EmulatorConfig.Instance.PHONE_EVENT_MODE = EmulatorConfig.Mode.USB;
      } else if (connectionMode == GvrController.EmulatorConnectionMode.WIFI) {
        EmulatorConfig.Instance.PHONE_EVENT_MODE = EmulatorConfig.Mode.WIFI;
      } else {
        EmulatorConfig.Instance.PHONE_EVENT_MODE = EmulatorConfig.Mode.OFF;
      }

      EmulatorManager.Instance.touchEventListeners += HandleTouchEvent;
      EmulatorManager.Instance.orientationEventListeners += HandleOrientationEvent;
      EmulatorManager.Instance.buttonEventListeners += HandleButtonEvent;
      EmulatorManager.Instance.gyroEventListeners += HandleGyroEvent;
      EmulatorManager.Instance.accelEventListeners += HandleAccelEvent;
    }

    public void ReadState(ControllerState outState) {
      lock (state) {
        state.connectionState = EmulatorManager.Instance.Connected ? GvrConnectionState.Connected :
            GvrConnectionState.Connecting;
        state.apiStatus = EmulatorManager.Instance.Connected ? GvrControllerApiStatus.Ok :
            GvrControllerApiStatus.Unavailable;
        outState.CopyFrom(state);
      }
      state.ClearTransientState();
    }

    public void OnPause() {}
    public void OnResume() {}

    private void HandleTouchEvent(EmulatorTouchEvent touchEvent) {
      if (touchEvent.pointers.Count < 1) return;
      EmulatorTouchEvent.Pointer pointer = touchEvent.pointers[0];

      lock (state) {
        state.touchPos = new Vector2(pointer.normalizedX, pointer.normalizedY);
        switch (touchEvent.getActionMasked()) {
          case EmulatorTouchEvent.Action.kActionDown:
            state.touchDown = true;
            state.isTouching = true;
            break;
          case EmulatorTouchEvent.Action.kActionMove:
            state.isTouching = true;
            break;
          case EmulatorTouchEvent.Action.kActionUp:
            state.isTouching = false;
            state.touchUp = true;
            break;
        }
      }
    }

    private void HandleOrientationEvent(EmulatorOrientationEvent orientationEvent) {
      lastRawOrientation = ConvertEmulatorQuaternion(orientationEvent.orientation);
      if (!initialRecenterDone) {
        Recenter();
        initialRecenterDone = true;
      }
      lock (state) {
        state.orientation = yawCorrection * lastRawOrientation;
      }
    }

    private void HandleButtonEvent(EmulatorButtonEvent buttonEvent) {
      if (buttonEvent.code == EmulatorButtonEvent.ButtonCode.kHome) {
        if (buttonEvent.down) {
          lock (state) {
            // Started the recentering gesture.
            state.recentering = true;
          }
        } else {
          // Finished the recentering gesture. Recenter controller.
          Recenter();
        }
        return;
      }

      if (buttonEvent.code != EmulatorButtonEvent.ButtonCode.kApp &&
          buttonEvent.code != EmulatorButtonEvent.ButtonCode.kClick) return;

      lock (state) {
        if (buttonEvent.code == EmulatorButtonEvent.ButtonCode.kApp) {
          state.appButtonState = buttonEvent.down;
          state.appButtonDown = buttonEvent.down;
          state.appButtonUp = !buttonEvent.down;
        } else {
          state.clickButtonState = buttonEvent.down;
          state.clickButtonDown = buttonEvent.down;
          state.clickButtonUp = !buttonEvent.down;
        }
      }
    }

    private void HandleGyroEvent(EmulatorGyroEvent gyroEvent) {
      lock (state) {
        state.gyro = ConvertEmulatorGyro(gyroEvent.value);
      }
    }

    private void HandleAccelEvent(EmulatorAccelEvent accelEvent) {
      lock (state) {
        state.accel = ConvertEmulatorAccel(accelEvent.value);
      }
    }

    private static Quaternion ConvertEmulatorQuaternion(Quaternion emulatorQuat) {
      // Convert from the emulator's coordinate space to Unity's standard coordinate space.
      return new Quaternion(emulatorQuat.x, -emulatorQuat.z, emulatorQuat.y, emulatorQuat.w);
    }

    private static Vector3 ConvertEmulatorGyro(Vector3 emulatorGyro) {
      // Convert from the emulator's coordinate space to Unity's standard coordinate space.
      return new Vector3(-emulatorGyro.x, -emulatorGyro.z, -emulatorGyro.y);
    }

    private static Vector3 ConvertEmulatorAccel(Vector3 emulatorAccel) {
      // Convert from the emulator's coordinate space to Unity's standard coordinate space.
      return new Vector3(emulatorAccel.x, emulatorAccel.z, emulatorAccel.y);
    }

    private void Recenter() {
      lock (state) {
        // We want the current orientation to be "forward" so, we set the yaw correction
        // to undo the current rotation's yaw.
        yawCorrection = Quaternion.AngleAxis(-lastRawOrientation.eulerAngles.y, Vector3.up);
        state.orientation = Quaternion.identity;
        state.recentering = false;
        state.recentered = true;
        state.headsetRecenterRequested = true;
      }
    }
  }
}
/// @endcond

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

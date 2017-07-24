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

// This class is only used in the Editor, so make sure to only compile it on that platform.
// Additionally, it depends on EmulatorManager which is only compiled in the editor.
#if UNITY_EDITOR

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

    public bool SupportsBatteryStatus {
      get { return true; }
    }

    /// Creates a new EmulatorControllerProvider with the specified settings.
    internal EmulatorControllerProvider(GvrControllerInput.EmulatorConnectionMode connectionMode) {
      if (connectionMode == GvrControllerInput.EmulatorConnectionMode.USB) {
        EmulatorConfig.Instance.PHONE_EVENT_MODE = EmulatorConfig.Mode.USB;
      } else if (connectionMode == GvrControllerInput.EmulatorConnectionMode.WIFI) {
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
        state.connectionState = GvrConnectionState.Connected;
        if (!EmulatorManager.Instance.Connected) {
          state.connectionState = EmulatorManager.Instance.Connecting ?
              GvrConnectionState.Connecting : GvrConnectionState.Disconnected;
        }
        state.apiStatus = EmulatorManager.Instance.Connected ? GvrControllerApiStatus.Ok :
            GvrControllerApiStatus.Unavailable;

        // During emulation, just assume the controller is fully charged
        state.isCharging = false;
        state.batteryLevel = GvrControllerBatteryLevel.Full;

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
      switch (buttonEvent.code) {
      case EmulatorButtonEvent.ButtonCode.kApp:
        lock (state) {
          state.appButtonState = buttonEvent.down;
          state.appButtonDown = buttonEvent.down;
          state.appButtonUp = !buttonEvent.down;
        }
        break;
      case EmulatorButtonEvent.ButtonCode.kHome:
        lock (state) {
          state.homeButtonState = buttonEvent.down;
          state.homeButtonDown = buttonEvent.down;
          if (buttonEvent.down) {
            // Started the recentering gesture.
            state.recentering = true;
          }
        }
        if (!buttonEvent.down) {
          // Finished the recentering gesture. Recenter controller.
          Recenter();
        }
        break;
      case EmulatorButtonEvent.ButtonCode.kClick:
        lock (state) {
          state.clickButtonState = buttonEvent.down;
          state.clickButtonDown = buttonEvent.down;
          state.clickButtonUp = !buttonEvent.down;
        }
        break;
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
      }
    }
  }
}
/// @endcond

#endif  // UNITY_EDITOR

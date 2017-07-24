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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using Gvr;
using UnityEngine;

namespace Gvr.Internal {
  /// Mocks controller input by using the mouse.
  /// The controller is connected when holding left shift.
  /// Move the mouse to control gyroscope and orientation.
  /// The left mouse button is used for the clickButton.
  /// The right mouse button is used for the appButton.
  /// The middle mouse button is used for the homeButton.
  class MouseControllerProvider : IControllerProvider {
    private ControllerState state = new ControllerState();

    private Vector3 lastMousePosition;

    /// Need to store the state of the buttons from the previous frame.
    /// This is because Input.GetMouseButtonDown and Input.GetMouseButtonUp
    /// Don't work when called after WaitForEndOfFrame, which is when ReadState is called.
    private bool wasClickButton;
    private bool wasAppButton;
    private bool wasHomeButton;
    private bool wasTouching;

    private const float GYRO_SENSITIVITY = 0.075f;
    private const float TOUCH_SENSITIVITY = 0.002f;

    public static bool IsMouseAvailable {
      get {
        return Input.mousePresent && IsActivateButtonPressed;
      }
    }

    public static bool IsActivateButtonPressed {
      get {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
      }
    }

    public static bool IsClickButtonPressed {
      get {
        return Input.GetMouseButton(0);
      }
    }

    public static bool IsAppButtonPressed {
      get {
        return Input.GetMouseButton(1);
      }
    }

    public static bool IsHomeButtonPressed {
      get {
        return Input.GetMouseButton(2);
      }
    }

    public static bool IsTouching {
      get {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
      }
    }

    public bool SupportsBatteryStatus {
      get { return false; }
    }

    internal MouseControllerProvider() {
    }

    public void ReadState(ControllerState outState) {
      lock (state) {
        UpdateState();

        outState.CopyFrom(state);
      }
      state.ClearTransientState();
    }

    public void OnPause() {}
    public void OnResume() {}

    private void UpdateState() {
      if (!IsMouseAvailable) {
        ClearState();
        lastMousePosition = Input.mousePosition;
        return;
      }

      state.connectionState = GvrConnectionState.Connected;
      state.apiStatus = GvrControllerApiStatus.Ok;
      state.isCharging = false;
      state.batteryLevel = GvrControllerBatteryLevel.Full;

      UpdateButtonStates();

      if (state.isTouching) {
        UpdateTouchPos();
      } else {
        UpdateOrientation();
      }

      lastMousePosition = Input.mousePosition;
    }

    private void UpdateTouchPos() {
      Vector3 currentMousePosition = Input.mousePosition;
      Vector3 mouseDelta = currentMousePosition - lastMousePosition;

      Vector2 touchDelta = mouseDelta * TOUCH_SENSITIVITY;
      touchDelta.y *= -1.0f;

      state.touchPos += touchDelta;
      state.touchPos.x = Mathf.Clamp01(state.touchPos.x);
      state.touchPos.y = Mathf.Clamp01(state.touchPos.y);
    }

    private void UpdateOrientation() {
      Vector3 currentMousePosition = Input.mousePosition;
      Vector3 mouseDelta = currentMousePosition - lastMousePosition;

      state.gyro = mouseDelta * GYRO_SENSITIVITY;
      state.gyro.y *= -1.0f;

      Quaternion yaw = Quaternion.AngleAxis(state.gyro.x, Vector3.up);
      Quaternion pitch = Quaternion.AngleAxis(state.gyro.y, Vector3.right);
      state.orientation = state.orientation * yaw * pitch;
    }

    private void UpdateButtonStates() {
      state.clickButtonState = IsClickButtonPressed;
      state.clickButtonDown = state.clickButtonState && !wasClickButton;
      state.clickButtonUp = !state.clickButtonState && wasClickButton;

      state.isTouching = IsTouching;
      state.touchDown = state.isTouching && !wasTouching;
      state.touchUp = !state.isTouching && wasTouching;
      if (state.touchUp) {
        ClearTouchPos();
      }

      state.appButtonState = IsAppButtonPressed;
      state.appButtonDown = state.appButtonState && !wasAppButton;
      state.appButtonUp = !state.appButtonState && wasAppButton;

      state.homeButtonState = IsHomeButtonPressed;
      state.homeButtonDown = state.homeButtonState && !wasHomeButton;

      state.recentering = state.homeButtonState;
      if (!state.homeButtonState && wasHomeButton) {
        Recenter();
      }

      wasClickButton = state.clickButtonState;
      wasTouching = state.isTouching;
      wasAppButton = state.appButtonState;
      wasHomeButton = state.homeButtonState;
    }

    private void Recenter() {
      Quaternion yawCorrection = Quaternion.AngleAxis(-state.orientation.eulerAngles.y, Vector3.up);
      state.orientation = state.orientation * yawCorrection;
      state.recentering = false;
      state.recentered = true;
    }

    private void ClearTouchPos() {
      state.touchPos = new Vector2(0.5f, 0.5f);
    }

    private void ClearState() {
      state.connectionState = GvrConnectionState.Disconnected;
      state.clickButtonState = false;
      state.clickButtonDown = false;
      state.clickButtonUp = false;
      state.appButtonState = false;
      state.appButtonDown = false;
      state.appButtonUp = false;
      ClearTouchPos();
    }
  }
}

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
using System;

using Gvr;

/// @cond
namespace Gvr.Internal {
  /// Internal representation of the controller's current state.
  /// This representation is used by controller providers to represent the controller's state.
  ///
  /// The fields in this class have identical meanings to their correspondents in the GVR C API,
  /// so they are not redundantly documented here.
  class ControllerState {
    internal GvrConnectionState connectionState = GvrConnectionState.Disconnected;
    internal GvrControllerApiStatus apiStatus = GvrControllerApiStatus.Unavailable;
    internal Quaternion orientation = Quaternion.identity;
    internal Vector3 position = Vector3.zero;
    internal Vector3 gyro = Vector3.zero;
    internal Vector3 accel = Vector3.zero;
    internal Vector2 touchPos = Vector2.zero;
    internal bool recentered = false;
    internal bool is6DoF = false;

    internal GvrControllerButton buttonsState;
    internal GvrControllerButton buttonsDown;
    internal GvrControllerButton buttonsUp;

    internal string errorDetails = "";
    internal IntPtr gvrPtr = IntPtr.Zero;

    internal bool isCharging = false;
    internal GvrControllerBatteryLevel batteryLevel = GvrControllerBatteryLevel.Unknown;

    public void CopyFrom(ControllerState other) {
      connectionState = other.connectionState;
      apiStatus = other.apiStatus;
      orientation = other.orientation;
      position = other.position;
      gyro = other.gyro;
      accel = other.accel;
      touchPos = other.touchPos;
      recentered = other.recentered;
      is6DoF = other.is6DoF;
      buttonsState = other.buttonsState;
      buttonsDown = other.buttonsDown;
      buttonsUp = other.buttonsUp;
      errorDetails = other.errorDetails;
      gvrPtr = other.gvrPtr;
      isCharging = other.isCharging;
      batteryLevel = other.batteryLevel;
    }

    /// Resets the transient state (the state variables that represent events, and which are true
    /// for only one frame).
    public void ClearTransientState() {
      recentered = false;
      buttonsState = 0;
      buttonsDown = 0;
      buttonsUp = 0;
    }

    public void SetButtonsUpDownFromPrevious(GvrControllerButton prevButtonsState) {
      buttonsDown = ~prevButtonsState & buttonsState;
      buttonsUp = prevButtonsState & ~buttonsState;
    }
  }
}
/// @endcond


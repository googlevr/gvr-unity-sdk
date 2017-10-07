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

using System;
using UnityEngine;

[System.Obsolete("Replaced by GvrControllerInput.")]
[AddComponentMenu("")]
public class GvrController : GvrControllerInput {
  public new static GvrConnectionState State {
    get {
      return GvrControllerInput.State;
    }
  }

  public new static GvrControllerApiStatus ApiStatus {
    get {
      return GvrControllerInput.ApiStatus;
    }
  }

  public new static Quaternion Orientation {
    get {
      return GvrControllerInput.Orientation;
    }
  }

  public new static Vector3 Gyro {
    get {
      return GvrControllerInput.Gyro;
    }
  }

  public new static Vector3 Accel {
    get {
      return GvrControllerInput.Accel;
    }
  }

  public new static bool IsTouching {
    get {
      return GvrControllerInput.IsTouching;
    }
  }

  public new static bool TouchDown {
    get {
      return GvrControllerInput.TouchDown;
    }
  }

  public new static bool TouchUp {
    get {
      return GvrControllerInput.TouchUp;
    }
  }

  public new static Vector2 TouchPos {
    get {
      return GvrControllerInput.TouchPos;
    }
  }

  public new static bool Recentered {
    get {
      return GvrControllerInput.Recentered;
    }
  }

  public new static bool ClickButton {
    get {
      return GvrControllerInput.ClickButton;
    }
  }

  public new static bool ClickButtonDown {
    get {
      return GvrControllerInput.ClickButtonDown;
    }
  }

  public new static bool ClickButtonUp {
    get {
      return GvrControllerInput.ClickButtonUp;
    }
  }

  public new static bool AppButton {
    get {
      return GvrControllerInput.AppButton;
    }
  }

  public new static bool AppButtonDown {
    get {
      return GvrControllerInput.AppButtonDown;
    }
  }

  public new static bool AppButtonUp {
    get {
      return GvrControllerInput.AppButtonUp;
    }
  }

  public new static bool HomeButtonDown {
    get {
      return GvrControllerInput.HomeButtonDown;
    }
  }

  public new static bool HomeButtonState {
    get {
      return GvrControllerInput.HomeButtonState;
    }
  }

  public new static string ErrorDetails {
    get {
      return GvrControllerInput.ErrorDetails;
    }
  }

  // Returns the GVR C library controller state pointer (gvr_controller_state*).
  public new static IntPtr StatePtr {
    get {
      return GvrControllerInput.StatePtr;
    }
  }

  public new static bool IsCharging {
    get {
      return GvrControllerInput.IsCharging;
    }
  }

  public new static GvrControllerBatteryLevel BatteryLevel {
    get {
      return GvrControllerInput.BatteryLevel;
    }
  }
}


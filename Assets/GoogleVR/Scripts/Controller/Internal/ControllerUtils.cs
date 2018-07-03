// Copyright 2018 Google Inc. All rights reserved.
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
  class ControllerUtils {

    /// Convenience array of all hands.
    public static GvrControllerHand[] AllHands = {
      GvrControllerHand.Right,
      GvrControllerHand.Left,
    };

    /// Returns true while the user holds down any of buttons specified in `buttons` on
    /// any controller.
    public static bool AnyButton(GvrControllerButton buttons) {
      bool ret = false;
      foreach (var hand in AllHands) {
        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
        ret |= device.GetButton(buttons);
      }
      return ret;
    }

    /// Returns true in the frame the user starts pressing down any of buttons specified
    /// in `buttons` on any controller.
    public static bool AnyButtonDown(GvrControllerButton buttons) {
      bool ret = false;
      foreach (var hand in AllHands) {
        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
        ret |= device.GetButtonDown(buttons);
      }
      return ret;
    }

    /// Returns true the frame after the user stops pressing down any of buttons specified
    /// in `buttons` on any controller.
    public static bool AnyButtonUp(GvrControllerButton buttons) {
      bool ret = false;
      foreach (var hand in AllHands) {
        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
        ret |= device.GetButtonUp(buttons);
      }
      return ret;
    }

  }
}
/// @endcond


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
using UnityEngine.UI;

/// Shows controller debug info. Must be used in a GameObject with a Text component.
public class ControllerDebugInfo : MonoBehaviour {
  private Text text;

  void Awake() {
    text = GetComponent<Text>();
    if (!text) {
      Debug.LogErrorFormat("ControllerDebugInfo must be used on an object with Text component.");
    }
  }

  void Update() {
    if (!text) return;

    Vector3 euler = GvrController.Orientation.eulerAngles;

    text.text = string.Format(
      "STATE: {0}\n" +
      "ORI: ({1,5:F2}, {2,5:F2}, {3,5:F2}, {4,5:F2})\n" +
      "EULER: ({5,6:F1}, {6,6:F1}, {7,6:F1})\n" +
      "GYRO: ({8,5:F1}, {9,5:F1}, {10,5:F1})\n" +
      "ACC: ({11,5:F1}, {12,5:F1}, {13,5:F1})\n" +
      "TOUCH: {14} {15} ({16,4:F2}, {17,4:F2})\n" +
      "BUTTONS: {18} {19}\n" +
      "RECENTER: {20}",
      GvrController.State,
      GvrController.Orientation.x, GvrController.Orientation.y, GvrController.Orientation.z,
      GvrController.Orientation.w,
      euler.x, euler.y, euler.z,
      GvrController.Gyro.x, GvrController.Gyro.y, GvrController.Gyro.z,
      GvrController.Accel.x, GvrController.Accel.y, GvrController.Accel.z,
      GvrController.IsTouching ? "TOUCHING" : "", GvrController.TouchDown ? "***DOWN***" :
          GvrController.TouchUp ? "***UP***" : "",
      GvrController.TouchPos.x, GvrController.TouchPos.y,
      GvrController.ClickButton ? "CLICK" : "",
      GvrController.AppButton ? "APP" : "",
      GvrController.Recentering ? "**RECENTERING**" : "");

    if (GvrController.TouchDown) {
      Debug.Log("CONTROLLER EVENT: Touch down");
    }
    if (GvrController.TouchUp) {
      Debug.Log("CONTROLLER EVENT: Touch up");
    }
    if (GvrController.ClickButtonDown) {
      Debug.Log("CONTROLLER EVENT: CLICK button down");
    }
    if (GvrController.ClickButtonUp) {
      Debug.Log("CONTROLLER EVENT: CLICK button up");
    }
    if (GvrController.AppButtonDown) {
      Debug.Log("CONTROLLER EVENT: APP button down");
    }
    if (GvrController.AppButtonUp) {
      Debug.Log("CONTROLLER EVENT: APP button up");
    }
    if (GvrController.Recentered) {
      Debug.Log("CONTROLLER EVENT: Recentered.");
    }
  }
}

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

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
using System.Collections;

/// Manages when the visual elements of GvrControllerPointer should be active.
/// When the controller is disconnected, the visual elements will be turned off.
public class GvrControllerVisualManager : MonoBehaviour {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  private bool wasControllerConnected = false;

  void Start() {
    wasControllerConnected = IsControllerConnected();
    SetChildrenActive(wasControllerConnected);
  }

  void Update() {
    bool isControllerConnected = IsControllerConnected();
    if (isControllerConnected != wasControllerConnected) {
      SetChildrenActive(isControllerConnected);
    }
    wasControllerConnected = isControllerConnected;
  }

  private bool IsControllerConnected() {
    return GvrController.State == GvrConnectionState.Connected;
  }

  /// Activate/Deactivate the children of the transform.
  /// It is expected that the children will be the visual elements
  /// of GvrControllerPointer (I.e. the Laser and the 3D Controller Model).
  private void SetChildrenActive(bool active) {
    for (int i = 0; i < transform.childCount; i++) {
      transform.GetChild(i).gameObject.SetActive(active);
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

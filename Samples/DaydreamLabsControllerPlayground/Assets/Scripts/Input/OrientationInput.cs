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

namespace GVR.Input {
  /// <summary>
  /// Outputs the Euler angles of the controller orientation as a Vector3 Event.
  /// </summary>
  public class OrientationInput : MonoBehaviour {
    [Tooltip("The world space Euler angles of the controller's orientation as a Vector3 event.")]
    public Vector3Event OnOrient;

    Vector3 currentOrientation = Vector3.zero;

    void Update() {
      // Only report orientation events if the controller is connected.
      if (GvrController.State == GvrConnectionState.Connected) {
        currentOrientation = GvrController.Orientation.eulerAngles;
        OnOrient.Invoke(currentOrientation);
      }
    }
  }
}

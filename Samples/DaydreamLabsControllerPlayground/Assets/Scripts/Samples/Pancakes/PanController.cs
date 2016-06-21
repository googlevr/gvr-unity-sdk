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
using UnityEngine.Events;

namespace GVR.Samples.Pancakes {
  /// <summary>
  ///  This component maps controller input and orientation to the pan.
  /// </summary>
  public class PanController : MonoBehaviour {
    [Tooltip("Reference to the pan's rigidbody")]
    public Rigidbody ActiveRigidbody;

    [Tooltip("Callback for the Pancake Dispenser's dispense function")]
    public UnityEvent DispensePancakeEvent;

    void FixedUpdate() {
      ActiveRigidbody.MoveRotation(Normalize(GvrController.Orientation));
    }

    void Update() {
      if (GvrController.ClickButtonDown) {
        DispensePancakeEvent.Invoke();
      }
    }

    private Quaternion Normalize(Quaternion q) {
      if (q.w == 0) {
        q = Quaternion.identity;
      }
      return q;
    }
  }
}

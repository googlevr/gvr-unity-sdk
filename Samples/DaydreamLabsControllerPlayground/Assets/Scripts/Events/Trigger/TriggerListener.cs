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

namespace GVR.Events.Trigger {
  public class TriggerListener : MonoBehaviour {
    public TriggerContract TriggerContract;

    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    void OnTriggerEnter(Collider other) {
      TriggerObject[] tos = other.GetComponents<TriggerObject>();
      for (int i = 0; i < tos.Length; i++) {
        tos[i].TriggerEnter();
        if (TriggerContract == tos[i].TriggerContract) {
          OnEnter.Invoke();
          tos[i].TriggerEnterAccepted();
        } else {
          tos[i].TriggerEnterRejected();
        }
      }
    }

    void OnTriggerExit(Collider other) {
      TriggerObject[] tos = other.GetComponents<TriggerObject>();
      for (int i = 0; i < tos.Length; i++) {
        tos[i].TriggerExit();
        if (TriggerContract == tos[i].TriggerContract) {
          OnExit.Invoke();
        }
      }
    }
  }
}

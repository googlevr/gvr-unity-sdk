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

namespace GVR.Samples.DragonTeam {
  [System.Serializable]
  public class SubTrigger {
    public string Label = "New Trigger";
    public bool Triggered = false;
    [HideInInspector]
    public float TimeRemaining = 0f;
  }

  /// <summary>
  /// Simple class to aggregate Event Triggers to require multiple triggers within a specific
  /// interval of time. Used in the Dragon Team demo for the instances where multiple Song Stones
  /// need to be activated at the same time in order to trigger something."
  /// </summary>
  public class MultiTrigger : MonoBehaviour {
    [Tooltip("The array of Sub-triggers necessary to fire the final trigger.")]
    public SubTrigger[] Triggers;

    [Tooltip("The amount of time, in seconds, that a single sub-trigger will remain active for " +
             "before deactivating.")]
    public float TriggerActiveTime = 1f;

    [Tooltip("The event to fire if all Sub-triggers in the Triggers array are activated at the " +
             "same time. Activating this will also automatically disable all sub triggers.")]
    public UnityEvent OnTriggered;

    void Update() {
      if (AllTriggered()) {
        OnTriggered.Invoke();
        ResetTriggers();
      }
      UpdateTriggerTimers();
    }

    public void SetTrigger(string name) {
      for (int i = 0; i < Triggers.Length; i++) {
        if (Triggers[i].Label == name) {
          Triggers[i].Triggered = true;
          Triggers[i].TimeRemaining = TriggerActiveTime;
        }
      }
    }

    void ResetTriggers() {
      for (int i = 0; i < Triggers.Length; i++) {
        Triggers[i].Triggered = false;
      }
    }

    bool AllTriggered() {
      for (int i = 0; i < Triggers.Length; i++) {
        if (Triggers[i].Triggered == false) {
          return false;
        }
      }
      return true;
    }

    void UpdateTriggerTimers() {
      for (int i = 0; i < Triggers.Length; i++) {
        if (Triggers[i].TimeRemaining >= 0) {
          Triggers[i].TimeRemaining -= Time.deltaTime;
        } else if (Triggers[i].Triggered) {
          Triggers[i].Triggered = false;
        }
      }
    }
  }
}

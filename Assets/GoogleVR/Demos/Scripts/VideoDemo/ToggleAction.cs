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

namespace GoogleVR.VideoDemo {
  using UnityEngine;
  using UnityEngine.Events;

  /// <summary>
  /// Throws a Unity event when the internal state is changed. This
  /// component can be used by other components the fire Unity Events in
  /// order to do some lightweight state tracking.
  /// </summary>
  public class ToggleAction : MonoBehaviour {
    private float lastUsage;
    private bool on;

    [Tooltip("Event to raise when this is toggled on.")]
    public UnityEvent OnToggleOn;

    [Tooltip("Event to raise when this is toggled off.")]
    public UnityEvent OnToggleOff;

    [Tooltip("Should this initial state be on or off?")]
    public bool InitialState;

    [Tooltip("Should an event be raised for the initial state on Start?")]
    public bool RaiseEventForInitialState;

    [Tooltip("Time required between toggle operations. Operations Toggles within this window " +
             "will be ignored.")]
    public float Cooldown;

    void Start() {
      on = InitialState;
      if (RaiseEventForInitialState) {
        RaiseToggleEvent(on);
      }
    }

    public void Toggle() {
      if (Time.time - lastUsage < Cooldown) {
        return;
      }
      lastUsage = Time.time;
      on = !on;
      RaiseToggleEvent(on);
    }

    public void Set(bool on) {
      if (this.on == on) {
        return;
      }
      this.on = on;
      RaiseToggleEvent(on);
    }

    private void RaiseToggleEvent(bool on) {
      if (on) {
        OnToggleOn.Invoke();
      } else {
        OnToggleOff.Invoke();
      }
    }
  }
}

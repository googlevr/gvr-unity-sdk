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

namespace GVR.Samples.Magic {
  /// <summary>
  /// Simple timer for delaying effects.
  /// </summary>
  public class DelayTimer {
    /// <summary>
    /// Gets true if timer is finished.
    /// </summary>
    public bool IsReady { get; private set; }

    public DelayTimer() {
      IsReady = true;
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    /// <param name="duration">Duration of the delay</param>
    /// <param name="callback">Action to perform when the delay finishes</param>
    public void Start(float duration, Action callback) {
      _callback = callback;
      _startTime = Time.realtimeSinceStartup;
      _duration = duration;
      IsReady = false;
    }

    public void Update() {
      if (!IsReady) {
        float elapsed = Time.realtimeSinceStartup - _startTime;
        if (elapsed > _duration) {
          _callback();
          IsReady = true;
        }
      }
    }

    private Action _callback;
    private float _duration;
    private float _startTime;
  }
}

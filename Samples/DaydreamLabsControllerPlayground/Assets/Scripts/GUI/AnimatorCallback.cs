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

namespace GVR.GUI {
  /// <summary>
  /// Receives a message from an AnimationStateChangeTrigger component that
  /// triggers one of several potential events. AnimationStateChangeTrigger
  /// lives on a mechanim graph and triggers a callback when the stat is on
  /// is entered. This should be adjacent to an Animator component.
  /// </summary>
  public class AnimatorCallback : MonoBehaviour {
    /// <summary>
    /// Potential events to call. The index in this array is the trigger ID
    /// that may be specified from a AnimationStateChangeTrigger.
    /// </summary>
    public UnityEvent[] EventsByTriggerIndex;

    public void Invoke(int index) {
      if (index < EventsByTriggerIndex.Length) {
        EventsByTriggerIndex[index].Invoke();
      }
    }
  }
}

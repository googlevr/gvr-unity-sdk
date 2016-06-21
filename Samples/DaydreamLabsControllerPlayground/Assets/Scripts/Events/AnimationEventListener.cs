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

namespace GVR.Events {
  [System.Serializable]
  public class AnimEvent {
    [Tooltip("Design-facing label for this event group. Used by TriggerEvent to identify which " +
             "event to call.")]
    public string Label = "New Animation Event.";

    [Tooltip("The event group to be called when TriggerEvent is called with the correct label.")]
    public UnityEvent Event;
  }

  /// <summary>
  /// Generic Component for handling Event calls from Animations, as well as aggregate events for
  /// component-based prototyping. The listener consists of an Array of Event calls, each paired
  /// with a Label. Each of these Event calls can be called via the "TriggerEvent" method, taking
  /// the Label of the Event group to call as a string.
  /// </summary>
  public class AnimationEventListener : MonoBehaviour {
    [Tooltip("The events to scrape for a matching Label when TriggerEvent is called.")]
    public AnimEvent[] AnimationEvents;

    public void TriggerEvent(string EventName) {
      for (int i = 0; i < AnimationEvents.Length; i++) {
        if (AnimationEvents[i].Label == EventName) {
          AnimationEvents[i].Event.Invoke();
        }
      }
    }
  }
}

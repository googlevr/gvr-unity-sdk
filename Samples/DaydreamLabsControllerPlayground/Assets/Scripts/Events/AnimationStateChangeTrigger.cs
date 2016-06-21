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

using GVR.GUI;
using UnityEngine;

namespace GVR.Events {
  /// <summary>
  /// Triggers a UnityEvent in all AnimatorCallback components on the same
  /// GameObject as the animator when this state is entered.
  /// </summary>
  public class AnimationStateChangeTrigger : StateMachineBehaviour {
    public enum StateEvent {
      StateEnter,
      StateExit
    }

    /// <summary>
    /// Index of the UnityEvent in the target AnimatorCallback that will be invoked.
    /// </summary>
    public int TriggerIndex;

    public StateEvent TriggerStateEvent = StateEvent.StateEnter;

    private void TriggerCallback(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      AnimatorCallback[] callbacks = animator.GetComponents<AnimatorCallback>();
      for (int i = 0; i < callbacks.Length; i++) {
        callbacks[i].Invoke(TriggerIndex);
      }
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      if (TriggerStateEvent == StateEvent.StateEnter) {
        TriggerCallback(animator, stateInfo, layerIndex);
      }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      if (TriggerStateEvent == StateEvent.StateExit) {
        TriggerCallback(animator, stateInfo, layerIndex);
      }
    }
  }
}

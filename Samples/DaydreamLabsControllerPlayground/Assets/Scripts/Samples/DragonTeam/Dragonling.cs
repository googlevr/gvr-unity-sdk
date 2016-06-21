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
  /// <summary>
  /// Object Based class to represent a Dragonling character in the Dragon Team demo.
  /// The purpose of this class is to be an aggregate reference for relevant components to the
  /// characters, as well as an interface for common actions and commands.
  /// </summary>
  public class Dragonling : MonoBehaviour {
    [Tooltip("A reference to the controller for the NavmeshAgent for this character.")]
    public NavmeshAgentController Agent;

    [Tooltip("A reference to the controller for the Animator for this character.")]
    public DragonlingAnimController AnimController;

    [Tooltip("A reference to the Selectable component for this character.")]
    public Selectable SelectionHandler;

    [Tooltip("If true, the Dragonling will begin the game asleep, which renders them " +
             "unselectable and inactive until woken up by another dragon's song.")]
    public bool StartGameSleeping = false;

    [Tooltip("Event called when the Dragonling is set to a sleeping state.")]
    public UnityEvent OnSleep;

    [Tooltip("Event called when the Dragonling is woken up.")]
    public UnityEvent OnWake;

    private bool sleeping = false;
    public bool IsSleeping { get { return sleeping; } }

    void Start() {
      if (StartGameSleeping) {
        SetSleeping(true);
      } else {
        SetSleeping(false);
      }
    }

    void Update() {
      AnimController.SetMoveSpeed(Agent.CurrentSpeed);
    }

    public void SetSleeping(bool shouldSleep) {
      if (sleeping != shouldSleep) {
        Agent.SetAgentActive(!shouldSleep);
        AnimController.SetSleepVisuals(shouldSleep);
        SelectionHandler.IsSelectable = !shouldSleep;
        sleeping = shouldSleep;
      }
      if (shouldSleep) {
        OnSleep.Invoke();
      } else {
        OnWake.Invoke();
      }
    }

    public void Sing() {
      if (!sleeping) {
        AnimController.Sing();
      }
    }

    public void MoveTo(Vector3 position) {
      if (!sleeping) {
        Agent.PathTo(position);
      }
    }
  }
}

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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// A simple, object based Moving Platform script designed to work with the built in Unity
  /// Navmesh System. A Moving Platform object is linearly interpolated between two Positions
  /// (A and B) in world space. Navmesh blockers are turned on cooresponding to each location,
  /// to block the navmesh for the area where the moving platform is no longer present.
  /// Navmesh agents on the platform when it moves are "packed" by the system, disabling their
  /// navmesh agent until the platform completes its sequence.
  /// </summary>
  public class MovingPlatformController : MonoBehaviour {
    [Tooltip("The first position for the platform.")]
    public GameObject PositionA;

    [Tooltip("The second position for the platform.")]
    public GameObject PositionB;

    [Tooltip("Navmesh blocker set to Active when the Platform is in or approaching position A.")]
    public GameObject PositionANavmeshBlocker;

    [Tooltip("Navmesh blocker set to active when the platform is in or approaching Position B.")]
    public GameObject PositionBNavmeshBlocker;

    [Tooltip("The object to be moved by this script.")]
    public GameObject MovingPlatformObject;

    [Tooltip("The speed, in world units per second, that the object is moved.")]
    public float Speed = 1f;

    private float currentPositionPercent = 0f;

    public enum PositionType {
      A,
      B
    }

    public PositionType TargetPosition = PositionType.A;

    [Tooltip("Event called when the platform begins moving.")]
    public UnityEvent OnMoveStart;

    [Tooltip("Event called when the platform finishes moving.")]
    public UnityEvent OnMoveComplete;

    private bool moving = false;

    private List<NavmeshAgentController> agents = new List<NavmeshAgentController>();

    void Start() {
      MovingPlatformObject.transform.position = PositionA.transform.position;
      PositionA.gameObject.SetActive(false);
      PositionB.gameObject.SetActive(false);
      PositionBNavmeshBlocker.SetActive(true);
      PositionANavmeshBlocker.SetActive(false);
    }

    void Update() {
      if (TargetPosition == PositionType.A && currentPositionPercent > 0) {
        if (moving == false) {
          StartNewMove();
        }
        currentPositionPercent -= Time.deltaTime * Speed;
      } else if (TargetPosition == PositionType.B && currentPositionPercent < 1) {
        if (moving == false) {
          StartNewMove();
        }
        currentPositionPercent += Time.deltaTime * Speed;
      } else if ((currentPositionPercent >= 1 || currentPositionPercent <= 0) && moving == true) {
        moving = false;
        OnMoveComplete.Invoke();
        if (TargetPosition == PositionType.A) {
          PositionBNavmeshBlocker.SetActive(true);
          PositionANavmeshBlocker.SetActive(false);
        } else {
          PositionBNavmeshBlocker.SetActive(false);
          PositionANavmeshBlocker.SetActive(true);
        }
        TogglePackedNavAgents(true);
      }
      if (currentPositionPercent >= 0 && currentPositionPercent <= 1)
        MovingPlatformObject.transform.position =
            Vector3.Lerp(PositionA.transform.position, PositionB.transform.position, currentPositionPercent);
    }

    void StartNewMove() {
      moving = true;
      OnMoveStart.Invoke();
      TogglePackedNavAgents(false);
    }

    public void Toggle() {
      TargetPosition = (TargetPosition == PositionType.A) ? PositionType.B : PositionType.A;
    }

    public void AddAgent(Transform agentTrans) {
      NavmeshAgentController agent = agentTrans.GetComponent<NavmeshAgentController>();
      if (agent) {
        agents.Add(agent);
      }
    }

    public void RemoveAgent(Transform agentTrans) {
      NavmeshAgentController agent = agentTrans.GetComponent<NavmeshAgentController>();
      if (agent) {
        agent.SetAgentActive(true);
        agent.transform.SetParent(null);
        agents.Remove(agent);
      }
    }

    void TogglePackedNavAgents(bool active) {
      for (int i = 0; i < agents.Count; i++) {
        agents[i].SetAgentActive(active);
        if (active) {
          agents[i].transform.parent = null;
        } else
          agents[i].transform.SetParent(MovingPlatformObject.transform, true);
      }
    }
  }
}

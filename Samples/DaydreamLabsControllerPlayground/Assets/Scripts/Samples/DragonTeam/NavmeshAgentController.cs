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

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// The controller for the NavmeshAgent component for a Dragonling in the Dragon Team demo.
  /// Includes a handler for a persistent "Current Target" used to direct the Dragonling around
  /// the navmesh. Assumes an RTS style control interface will be implemented overtop of this.
  /// </summary>
  public class NavmeshAgentController : MonoBehaviour {
    [Tooltip("The NavmeshAgent component for the Dragonling.")]
    public NavMeshAgent Agent;

    [Tooltip("Prefab to instantiate as a visible Target marker for the Dragonling.")]
    public GameObject TargetPrefab;

    [Tooltip("The minimum distance of a Target Point from the Dragonling to be worth accepting " +
    "as a new movement command.")]
    public float MinPathDistance = 0.5f;

    public float CurrentSpeed { get { return Agent.velocity.magnitude; } }

    private Transform currentTarget;

    void Awake() {
      GameObject go = GameObject.Instantiate(TargetPrefab);
      currentTarget = go.transform;
      currentTarget.position = transform.position;
      currentTarget.localEulerAngles = Vector3.zero;
      go.SetActive(false);
    }

    void Update() {
      if (Agent && Agent.isActiveAndEnabled) {
        if (MinPathDistance >= Agent.remainingDistance) {
          currentTarget.gameObject.SetActive(false);
        } else {
          currentTarget.gameObject.SetActive(true);
        }
      }
    }

    public void PathTo(Vector3 position) {
      if (Agent && Agent.isActiveAndEnabled) {
        currentTarget.position = position;
        Agent.SetDestination(currentTarget.position);
      }
    }

    public void SetAgentActive(bool active) {
      if (Agent) {
        Agent.enabled = active;
      }
    }
  }
}

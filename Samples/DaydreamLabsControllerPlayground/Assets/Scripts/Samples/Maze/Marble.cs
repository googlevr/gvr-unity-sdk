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

using System.Collections;
using UnityEngine;

namespace GVR.Samples.Maze {
  /// <summary>
  /// The marble in the maze that responds to hitting a win volume.
  /// </summary>
  public class Marble : MonoBehaviour {
    public Rigidbody RigidBody;
    public Animator Animator;
    public Collider Trigger;
    public GameObject WinEffect;
    public GameObject SpawnEffect;
    public float RespawnDelay = 2.0f;
    public string WinAnimParam = "Win";
    private int winAnimParam;
    public string SpawnAnimParam = "Spawn";
    private int spawnAnimParam;

    private Vector3 startPosition;

    void Start() {
      winAnimParam = Animator.StringToHash(WinAnimParam);
      spawnAnimParam = Animator.StringToHash(SpawnAnimParam);
      startPosition = transform.position;
    }

    void Reset() {
      transform.position = startPosition;
      transform.rotation = Quaternion.identity;
      RigidBody.velocity = Vector3.zero;
      RigidBody.angularVelocity = Vector3.zero;
    }

    private IEnumerator WinAndRespawn() {
      WinEffect.SetActive(true);
      Trigger.enabled = false;
      RigidBody.isKinematic = true;
      Animator.SetTrigger(winAnimParam);

      yield return new WaitForSeconds(RespawnDelay);
      Reset();

      Animator.SetTrigger(spawnAnimParam);
      RigidBody.isKinematic = false;
      Trigger.enabled = true;
      SpawnEffect.SetActive(true);
    }

    public void TriggerWin() {
      StartCoroutine(WinAndRespawn());
    }
  }
}

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

namespace GVR.Samples.NinjaTraining {
  /// <summary>
  /// Object-based Dispenser for Bamboo in the Ninja Training demo. It is solely responsible for
  /// spawning Bamboo obstacles in groups from a pre-defined pool of Bamboo.
  /// </summary>
  public class BambooHopper : MonoBehaviour {
    [Tooltip("Event thrown when the Hopper is activated.")]
    public UnityEvent OnActivate;

    [Tooltip("Event thrown when the Hopper is deactivated.")]
    public UnityEvent OnDeactivate;

    [Tooltip("The pool of Bamboo objects that exists, regardless of their state. This system " +
             "does NOT instantiate the Bamboo, and instead relies on an array of Bamboo already " +
             "existing in the scene and explicitly hooked up.")]
    public Bamboo[] BambooPool;

    [Tooltip("The Transform where inactive Bamboo is moved to.")]
    public Transform PoolLocation;

    [Tooltip("The transform where Bamboo is placed upon activation.")]
    public Transform SpawnLocation;

    [Tooltip("The number of Bamboo to be released in rapid succession.")]
    public int SequenceLength = 5;

    [Tooltip("The initial delay after activation before Bamboo is spawned.")]
    public float InitialDelay = 3f;

    [Tooltip("The amount of time, in seconds, between Sequences of Bamboo.")]
    public float PauseBetweenSequences = 3f;

    [Tooltip("The amount of time, in seconds, between individual Bamboo objects in the same sequence.")]
    public float PauseBetweenBamboo = 1f;

    [Tooltip("If true, the Hopper will begin as active when the scene is played. Typically left " +
             "as false, and set to true for debug purposes.")]
    public bool StartActive = false;

    [Tooltip("The amount of time after being spawned when the Bamboo is cleaned up and returned " +
             "to the pool if not Slashed.")]
    public float CleanupLifetime = 5f;

    [Tooltip("The amount of time that the Bamboo is left to remain dead/slashed before being cleaned up.")]
    public float CleanupDeadtime = 3f;

    [Tooltip("The amount of time, in seconds, between scrapes to clean up any expired Bamboo.")]
    public float CleanupInterval = 1f;

    bool isActive = false;

    List<Bamboo> freeBamboo = new List<Bamboo>();
    List<Bamboo> activeBamboo = new List<Bamboo>();

    float currentCooldown = 0f;
    int sequenceCounter = 0;
    float cleanupTimer = 0f;

    void Start() {
      if (StartActive)
        SetActive(true);
      for (int i = 0; i < BambooPool.Length; i++) {
        freeBamboo.Add(BambooPool[i]);
      }
    }

    public void ToggleActive() {
      SetActive(!isActive);
    }

    public void SetActive(bool active) {
      Debug.Log("Setting Hopper to: " + active);
      isActive = active;
      currentCooldown = InitialDelay;
      if (active)
        OnActivate.Invoke();
      else
        OnDeactivate.Invoke();
    }

    public void DisarmActiveBamboo() {
      for (int i = 0; i < activeBamboo.Count; i++) {
        activeBamboo[i].Disarm();
      }
    }

    public void ResetAllBamboo() {
      for (int i = 0; i < activeBamboo.Count; i++) {
        activeBamboo[i].Reset();
        activeBamboo[i].transform.position = PoolLocation.position;
      }

      activeBamboo.Clear();
      freeBamboo.Clear();

      for (int i = 0; i < BambooPool.Length; i++) {
        freeBamboo.Add(BambooPool[i]);
      }
    }

    void Update() {
      if (isActive) {
        if (currentCooldown > 0)
          currentCooldown -= Time.deltaTime;
        else {
          if (SequenceLength > 0) {
            SpawnBamboo();

            sequenceCounter++;
            if (sequenceCounter < SequenceLength)
              currentCooldown = PauseBetweenBamboo;
            else {
              currentCooldown = PauseBetweenSequences;
              sequenceCounter = 0;
            }
          }
        }
      }
      if (cleanupTimer > 0)
        cleanupTimer -= Time.deltaTime;
      else {
        cleanupTimer = CleanupInterval;
        CleanupBamboo();
      }
    }


    void CleanupBamboo() {
      List<Bamboo> bambooToRemove = new List<Bamboo>();
      for (int i = 0; i < activeBamboo.Count; i++) {
        if (activeBamboo[i].GetLifetime() >= CleanupLifetime
            || activeBamboo[i].GetDeadtime() >= CleanupDeadtime) {
          bambooToRemove.Add(activeBamboo[i]);
          freeBamboo.Add(activeBamboo[i]);
          activeBamboo[i].Reset();
          activeBamboo[i].transform.position = PoolLocation.position;
        }
      }
      for (int j = 0; j < bambooToRemove.Count; j++) {
        activeBamboo.Remove(bambooToRemove[j]);
      }
    }

    public void SetBambooSpeed(float speed) {
      for (int i = 0; i < BambooPool.Length; i++) {
        BambooPool[i].SetSpeed(speed);
      }
    }

    public void SpawnBamboo() {
      if (freeBamboo.Count == 0)
        return;

      int id = Random.Range(0, freeBamboo.Count);
      Bamboo selectedBamboo = freeBamboo[id];

      selectedBamboo.Activate();
      selectedBamboo.transform.position = SpawnLocation.position;
      selectedBamboo.transform.rotation = SpawnLocation.rotation;

      activeBamboo.Add(selectedBamboo);
      freeBamboo.Remove(selectedBamboo);
    }
  }
}

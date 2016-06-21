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

using GVR.Input;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Entity {
  /// <summary>
  /// The Hitbox class simulates basic Hitbox functionality, with an
  /// assortment of events fired off when legal objects trigger a
  /// collider attached to the same GameObject. It's worth mentioning
  /// that this Hitbox solution is better for Prototyping than for final
  /// work, as it uses Tags instead of Physics layers to filter collisions.
  /// </summary>
  [RequireComponent(typeof(Collider))]
  public class Hitbox : MonoBehaviour {
    [Tooltip("An array of tags that this Hitbox will look for when judging whether or not to " +
             "throw events on collision with other objects.")]
    [SerializeField]
    public string[] ReactsToTags;

    [Tooltip("Fired on Trigger Enter when colliding with another object with a legal tag.")]
    public UnityEvent OnTriggered;

    [Tooltip("Fired on Trigger Exit when a legal object exits the collision volume.")]
    public UnityEvent OnTriggeredExit;

    [Tooltip("A Transform Event that fires on Trigger Enter with a legal object, and passes a " +
             "reference to that object's Transform as part of the Event.")]
    public TransformEvent OnTransformTriggered;

    [Tooltip("A transform event that fires on Trigger Exit with a legal object, and passes a " +
             "reference to that object's Transform as part of the Event.")]
    public TransformEvent OnTransformTriggeredExit;

    [Tooltip("How long, in seconds, before the Hitbox can be triggered again by ANY legal source. " +
             "This has no impact on OnTriggerExit calls. Set this to zero for reliable tracking " +
             "of objects in proximity.")]
    public float TriggerCooldown = 0.5f;

    private float currentCooldown = 0;
    private Collider col;

    void Start() {
      col = GetComponent<Collider>();
      if (col.isTrigger == false) {
        col.isTrigger = true;
      }
    }

    void Update() {
      if (currentCooldown > 0) {
        currentCooldown -= Time.deltaTime;
      }
    }

    void OnTriggerEnter(Collider target) {
      if (currentCooldown <= 0f) {
        for (int i = 0; i < ReactsToTags.Length; i++) {
          if (target.CompareTag(ReactsToTags[i])) {
            OnTriggered.Invoke();
            OnTransformTriggered.Invoke(target.transform);
            currentCooldown = TriggerCooldown;
            break;
          }
        }
      }
    }

    void OnTriggerExit(Collider target) {
      for (int i = 0; i < ReactsToTags.Length; i++) {
        if (target.CompareTag(ReactsToTags[i])) {
          OnTriggeredExit.Invoke();
          OnTransformTriggeredExit.Invoke(target.transform);
          break;
        }
      }
    }
  }
}

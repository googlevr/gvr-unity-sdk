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
using System.Collections;
using System.Collections.Generic;

public class AndroidBehaviour : MonoBehaviour {
  public float speed_ = 2.0f;
  public float death_timer_ = -1.0f;
  public float node_threshold = 0.5f;
  public Waypoint NextNode;

  public List<AudioClip> near_impact_sounds_ = new List<AudioClip>();
  public List<AudioClip> impact_sounds_ = new List<AudioClip>();
  public List<AudioClip> attack_sounds_ = new List<AudioClip>();
  public List<AudioClip> charge_sounds_ = new List<AudioClip>();
  public List<AudioClip> walk_sounds_ = new List<AudioClip>();

  private GvrAudioSource audio_source_;

  void OnEnable() {
    audio_source_ = GetComponent<GvrAudioSource>();
    NextNode = FindNearestWaypoint();
    StartCoroutine(PlayAwakeSound());
  }

  IEnumerator PlayAwakeSound() {
    yield return new WaitForEndOfFrame();

    PlaySound(charge_sounds_);
  }

  void Update() {
    if (death_timer_ > 0.0f) {
      death_timer_ -= Time.deltaTime;
      if (death_timer_ <= 0.0f) {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
          rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        AndroidPool.Destroy(gameObject);
      }
    } else if (NextNode != null) {
      Vector3 pos = transform.position;
      Vector3 target_pos = NextNode.transform.position;

      target_pos.y = pos.y;

      Vector3 direction = (target_pos - pos).normalized;
      transform.Translate(direction * speed_ * Time.deltaTime, Space.World);
      transform.LookAt(target_pos);
      FixHeight();

      if (Vector3.Distance(transform.position, target_pos) <= node_threshold) {
        if (NextNode.Next.Count > 0) {
          NextNode = NextNode.Next[0];
        } else {
          NextNode = null;
        }
      }
    }
  }

  public void OnExplosion(Vector3 explosion_pos) {
    Vector3 dir = (transform.position - explosion_pos).normalized;
    Rigidbody rigidbody = GetComponent<Rigidbody>();
    if (rigidbody != null) {
      rigidbody.constraints = RigidbodyConstraints.None;
      rigidbody.AddForce(dir * 20.0f, ForceMode.Impulse);
    }

    death_timer_ = 2.5f;

    PlaySound(impact_sounds_);
  }

  public Waypoint FindNearestWaypoint() {
    Waypoint closest = null;
    float closestDistance = float.MaxValue;
    foreach (Waypoint node in Waypoint.Waypoints) {
      float distance = Vector3.Distance(node.transform.position, transform.position);
      if (distance < closestDistance) {
        closestDistance = distance;
        closest = node;
      }
    }
    return closest;
  }

  void FixHeight() {
    RaycastHit hitInfo;
    if (Physics.Raycast(transform.position + transform.forward + Vector3.up * 20.0f, Vector3.down, out hitInfo, 40.0f, 1 << 8)) {
      Vector3 pos = transform.position;
      pos.y = hitInfo.point.y;
      transform.position = pos;
    }
  }

  void OnNearImpact() {
    PlaySound(near_impact_sounds_);
  }

  void PlaySound(List<AudioClip> sounds) {
    if (sounds.Count == 0 || audio_source_ == null)
      return;

    AudioClip clip = sounds[Random.Range(0, sounds.Count)];
    audio_source_.clip = clip;
    audio_source_.loop = false;
    audio_source_.Play();
  }
}
 
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

public class CannonballBehaviour : MonoBehaviour {
  public Trajectory trajectory_;
  public float speed_;
  public float speed_multiplier_ = 1.0f;
  public float start_distance_;
  public Vector3 target_;
  public Vector3 start_pos_;
  public AudioClip impact_audio_clip_;
  public GameObject explosion_;

  private bool called_near_impact_ = false;
  private float timer_ = -1.0f;

  public float Fire(Vector3 at) {
    start_pos_ = transform.position;
    target_ = at;

    // Set the flat look at direction.
    Vector3 dir = GetFlatDirection(transform.position, at);
    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

    // Get a flat distance.
    start_distance_ = GetFlatDistance(transform.position, at);
    trajectory_ = new Trajectory(transform.position.y, at.y, start_distance_, speed_);

    speed_multiplier_ = Mathf.Min(start_distance_, 15.0f) / 15.0f;

    return trajectory_.fire_angle_;
  }

  void OnEnable() {
    GetComponent<Renderer>().enabled = true;
  }

  void Update() {
    if (trajectory_ != null) {
      // Move the cannon ball.
      Vector3 new_pos = transform.position;
      new_pos += transform.forward * speed_ * Time.deltaTime * speed_multiplier_;

      float distance_to_start = GetFlatDistance(new_pos, start_pos_);
      if (distance_to_start > start_distance_) {
        // if we passed our target, make sure we land at the end.
        transform.position = target_;
        trajectory_ = null;
        OnImpact(target_);
      } else if (called_near_impact_ == false &&
                 ((distance_to_start / start_distance_) > 0.7f)) {
        // if we passed our target, make sure we land at the end.
        OnNearImpact(target_);
      } else {
        // Otherwise, proceed by getting new height.
        new_pos.y = trajectory_.GetHeightAtDistance(distance_to_start);
        transform.position = new_pos;
      }
    }

    if (timer_ > 0.0f) {
      timer_ -= Time.deltaTime;

      if (timer_ <= 0.0f) {
        CannonballPool.Destroy(gameObject);

        if (explosion_ != null) {
          explosion_.SetActive(false);
        }
      }
    }
  }

  float GetFlatDistance(Vector3 vec0, Vector3 vec1) {
    vec0.y = 0.0f;
    vec1.y = 0.0f;

    return Vector3.Distance(vec0, vec1);
  }

  public Vector3 GetFlatDirection(Vector3 from, Vector3 to) {
    from.y = 0.0f;
    to.y = 0.0f;

    return (to - from).normalized;
  }

  void OnNearImpact(Vector3 at) {
    called_near_impact_ = true;
    Collider[] hitColliders = Physics.OverlapSphere(at, 3.0f);
    int i = 0;
    while (i < hitColliders.Length) {
      GameObject obj = hitColliders[i].gameObject;
      obj.SendMessage("OnNearImpact", at + Vector3.down, SendMessageOptions.DontRequireReceiver);
      i++;
    }
  }

  void OnImpact(Vector3 at) {
    GvrAudioSource audio = GetComponent<GvrAudioSource>();
    if (audio != null && impact_audio_clip_ != null) {
      audio.clip = impact_audio_clip_;
      audio.loop = false;
      audio.Play();
    }

    if (explosion_ != null) {
      explosion_.SetActive(true);
    }

    Collider[] hitColliders = Physics.OverlapSphere(at, 1.5f);
    int i = 0;
    while (i < hitColliders.Length) {
      GameObject obj = hitColliders[i].gameObject;
      obj.SendMessage("OnExplosion", at + Vector3.down, SendMessageOptions.DontRequireReceiver);
      i++;
    }

    timer_ = 3.0f;
    GetComponent<Renderer>().enabled = false;
  }
}

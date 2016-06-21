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

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component defines the raquet's ability to 'hit' incoming balls. Balls are actually
  ///  not colliding with the raquet, rather, they enter the raquet's trigger volume and are
  ///  reflected. This is done so that you can grow the size of your trigger box as swing speed
  ///  increases so avoid clipping through the ball at high speeds, and also so that you can
  ///  cheat the ball's resulting trajetory to make the game easier to play.
  /// </summary>
  public class RaquetBouncer : MonoBehaviour {
    [Tooltip("How much of the swing power is applied to the ball. (1 = 100%)")]
    public float Bounciness;

    [Tooltip("Transform whose position is tracked to determine swing speed")]
    public Transform RaquetCenter;

    [Tooltip("The transform scale modifier to apply to the raquet's trigger volume when swinging fast.")]
    public Vector3 SwingSpeedScale;

    [Tooltip("The minimum swing speed at which the trigger volume will upscale.")]
    public float MinSwingSpeed;

    [Tooltip("The swing speed at which the trigger volume will reach its maximum scale")]
    public float MaxSwingSpeed;

    [Tooltip("The priority of the raquet angle in proportion to velocity vector when determining ball reflection.")]
    public float RaquetAnglePriority;

    [Tooltip("The maximum speed of the reflected vector of the ball.")]
    public float MaxHitSpeed;

    [Tooltip("Reference to audio source that plays when a ball is hit.")]
    public GvrAudioSource RaquetAudioSource;

    private Vector3 velocityVector = Vector3.zero;
    private KeyValuePair<float, Vector3>[] trackedPositions = new KeyValuePair<float, Vector3>[5];
    private int trackedPositionIndex = 0;
    private Vector3 colliderThickness = Vector3.zero;
    private BoxCollider boxCollider;
    private Collider justHit = null;
    private float justHitCooldown = 0f;

    void Start() {
      boxCollider = GetComponent<BoxCollider>();
      colliderThickness = boxCollider.size;
    }

    void Update() {
      trackedPositionIndex = NextIndex();
      int root = NextIndex();
      trackedPositions[trackedPositionIndex] =
        new KeyValuePair<float, Vector3>(Time.time, RaquetCenter.position);

      velocityVector = (RaquetCenter.position - trackedPositions[root].Value) /
                       (Time.time - trackedPositions[root].Key);

      float colliderMod = (Mathf.Max(0, velocityVector.magnitude - MinSwingSpeed) /
                          (MaxSwingSpeed - MinSwingSpeed));
      boxCollider.size = new Vector3(
          colliderThickness.x * colliderMod * SwingSpeedScale.x + colliderThickness.x,
          colliderThickness.y * colliderMod * SwingSpeedScale.y + colliderThickness.y,
          colliderThickness.z * colliderMod * SwingSpeedScale.z + colliderThickness.z);

      justHitCooldown -= Time.deltaTime;
      if (justHitCooldown < 0) {
        justHit = null;
      }
    }

    void OnTriggerEnter(Collider collider) {
      Rigidbody otherRigidbody = collider.attachedRigidbody;
      if (otherRigidbody == null || collider.Equals(justHit) || !collider.CompareTag("TennisBall")) {
        return;
      }

      justHit = collider;
      justHitCooldown = .5f;
      bool forehandBackhand = (new Plane(transform.up, transform.position)).GetSide(transform.position + velocityVector);
      Vector3 raquetNormal = forehandBackhand ? transform.up : transform.up * -1;

      Vector3 reflectedVec = Vector3.zero;
      if (velocityVector.magnitude < 1f) {
        reflectedVec = Vector3.Reflect(otherRigidbody.velocity, raquetNormal);
      } else {
        Vector3 vel = otherRigidbody.velocity.normalized;
        Vector3 reflectedBallVel = new Vector3(vel.x, -vel.y, -vel.z);
        reflectedVec = (reflectedBallVel + raquetNormal * RaquetAnglePriority) / (RaquetAnglePriority + 1);
      }

      otherRigidbody.velocity = Vector3.ClampMagnitude((reflectedVec * (1 + velocityVector.magnitude)) * Bounciness, MaxHitSpeed);
      RaquetAudioSource.volume = Mathf.Min(Mathf.Pow(otherRigidbody.velocity.magnitude / 5f, 5f), 1f);
      RaquetAudioSource.Play();

      TrailRenderer trail = collider.GetComponent<TrailRenderer>();
      trail.enabled = true;
    }

    private int NextIndex() {
      int index = trackedPositionIndex + 1;
      if (index >= trackedPositions.Length) {
        index = 0;
      }
      return index;
    }
  }
}

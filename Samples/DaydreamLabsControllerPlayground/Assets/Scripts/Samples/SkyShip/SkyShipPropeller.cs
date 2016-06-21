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
using GVR.Utils;

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  Using the plane's heading and various imposed constraints, this component
  ///  propells the plane along the XY axis. The plane's speed is affected by
  ///  simulated physics.
  /// </summary>
  public class SkyShipPropeller : MonoBehaviour {
    [Tooltip("Reference to this object's rigidbody")]
    public Rigidbody ShipRigidbody;

    [Tooltip("Radius of this ship's allowed movement in worldspace X direction")]
    public float BoundsX;

    [Tooltip("Radius of this ship's allowed movement in worldspace Y direction")]
    public float BoundsY;

    [Tooltip("Radius of this ship's allowed movement in local Z direction")]
    public float BoundsZ;

    [Tooltip("Max speed of ship along the XY plane")]
    public float MaxSpeedXY;

    [Tooltip("Max speed of ship when moving forward (happens when you straighten ship / recover from hitting obstacle)")]
    public float MaxInSpeedZ;

    [Tooltip("Max speed of ship when moving backward (happens when you turn ship / hit an obstacle)")]
    public float MaxOutSpeedZ;

    [Tooltip("Speed bonus when 'pulling up' (imagine the wings having lift), bonus is multiplictive")]
    [Range(0, 1)]
    public float UpTiltInfluence;

    [Tooltip("Speed bonus when 'pushing down' (imagine the wings having drag), bonus is multiplictive")]
    [Range(0, 1)]
    public float DownTiltInfluence;

    [Tooltip("Duration that ship is disabled after hitting an obstacle")]
    public float DisableTimer;

    [Tooltip("Effect that plays when you hit an obstacle")]
    public EffectPlayer HitObstacleEffect;

    private float startZPos = 0f;
    private bool shipDisabled = false;
    private float disableTimer = 0f;
    private List<Collider> colliders = new List<Collider>();

    void Start() {
      startZPos = transform.position.z;
      disableTimer = DisableTimer;
      colliders.AddRange(GetComponents<Collider>());
      colliders.AddRange(GetComponentsInChildren<Collider>());
    }

    void Update() {
      if (shipDisabled) {
        disableTimer -= Time.deltaTime;
        if (disableTimer <= 0) {
          disableTimer = DisableTimer;
          shipDisabled = false;
          ShipRigidbody.velocity = Vector3.zero;
          ShipRigidbody.angularVelocity = Vector3.zero;
          for (int i = 0; i < colliders.Count; i++) {
            colliders[i].enabled = true;
          }
        }
      }
    }

    void FixedUpdate() {
      if (shipDisabled) {
        ShipRigidbody.MovePosition(new Vector3(
            Mathf.Clamp(transform.position.x, -BoundsX, BoundsX),
            Mathf.Clamp(transform.position.y, -BoundsY, BoundsY),
            Mathf.Clamp(transform.position.z, startZPos, startZPos + BoundsZ)));
      } else {
        Navigate();
      }
    }

    void OnCollisionEnter(Collision collision) {
      shipDisabled = true;
      ShipRigidbody.AddTorque(collision.contacts[0].normal * 10000, ForceMode.VelocityChange);
      for (int i = 0; i < colliders.Count; i++) {
        colliders[i].enabled = false;
      }
      HitObstacleEffect.transform.position = collision.contacts[0].point;
      HitObstacleEffect.transform.forward = collision.contacts[0].normal;
      HitObstacleEffect.Play();
    }

    public void SteerShip(Vector3 euler) {
      if (!shipDisabled) {
        transform.eulerAngles = euler;
      }
    }

    private void Navigate() {
      float tiltInfluence = transform.up.z < 0 ?
          Mathf.Abs(transform.up.z) * UpTiltInfluence : Mathf.Abs(transform.up.z) * DownTiltInfluence;
      Vector3 deltaXY = (transform.forward + transform.forward * tiltInfluence) * MaxSpeedXY * Time.fixedDeltaTime;
      deltaXY = new Vector3(deltaXY.x, deltaXY.y, 0f);

      float curZModifier = transform.position.z - startZPos;
      float targetZModifier = (1 - (Vector3.Angle(Vector3.forward, transform.forward) / 90f)) * BoundsZ;
      int dirZ = targetZModifier > curZModifier ? 1 : -1;
      float deltaZ = Mathf.Min(Mathf.Abs(targetZModifier - curZModifier),
                       (dirZ > 0 ? MaxInSpeedZ : MaxOutSpeedZ) * Time.fixedDeltaTime) * dirZ;
      float newZ = Mathf.Clamp(startZPos + curZModifier + deltaZ, startZPos, startZPos + BoundsZ);

      ShipRigidbody.MovePosition(new Vector3(
          Mathf.Clamp(transform.position.x + deltaXY.x, -BoundsX, BoundsX),
          Mathf.Clamp(transform.position.y + deltaXY.y, -BoundsY, BoundsY),
          newZ));
    }
  }
}

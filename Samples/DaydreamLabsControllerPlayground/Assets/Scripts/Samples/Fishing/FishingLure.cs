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

namespace GVR.Samples.Fishing {
  /// <summary>
  /// Catches a random fish when it lands on the water. Attach to a
  /// FishingController object. This component should be attached to
  /// a GameObject that IS NOT parented to the FishingController object.
  /// </summary>
  public class FishingLure : MonoBehaviour {
    [Header("Lure Events")] public UnityEvent OnFishCaught;
    public UnityEvent OnSetHook;
    public UnityEvent OnFishReeledIn;
    public UnityEvent OnHitWater;
    public UnityEvent OnHitGround;
    public FishingLureStateEvent OnFishingLureState;

    [Header("Lure Behavior")]
    [Tooltip("Name of the Unity tag assigned to the water collider/plane.")]
    public string WaterTag = "Water";

    [Tooltip("Speed the lure will reel in when reeling normally")]
    public float ReelSpeed = 0.06f;

    [Tooltip("Speed the lure will reel in when the hook is \"set\" after a fish is caught.")]
    public float CaughtReelSpeed = 0.125f;

    [Tooltip("Attach a fish collector instance. If null, fish will be destroyed when caught.")]
    public FishCollector FishCollector;

    [Tooltip("Visual model/component to safely move the bobber without changing the root transform.")]
    public Transform LureVisual;

    [Header("Fish")] [Tooltip("Maximum time (seconds) a user will wait for a fish to spawn.")]
    public float FishWaitMax = 1.5f;

    [Tooltip("Available fish to cast")]
    public GameObject[] FishPrefabs;

    void Awake() {
      _currentReelSpeed = ReelSpeed;
      _acceleration = Vector3.zero;
      _body = GetComponent<Rigidbody>();
      if (FishCollector == null) {
        Debug.LogError("No fish collector attached. Catching fish will break.");
      }
      SetState(FishingLureState.Reeling);
    }

    void FixedUpdate() {
      UpdatePosition();
      CheckIfFishOnLine();
    }

    void OnTriggerEnter(Collider other) {
      _landingPosition = transform.position;
      var triggerState = FishingLureState.OnGround;
      if (other.CompareTag(WaterTag)) {
        triggerState = FishingLureState.InWater;
        StartFishTimer();
        OnHitWater.Invoke();
      } else {
        OnHitGround.Invoke();
      }
      SetState(triggerState);
    }

    /// <summary>
    /// Sets the transform that will act as the pre-cast
    /// lure attach point
    /// </summary>
    /// <param name="transf">Transform to attach to</param>
    public void SetLureAttachPoint(Transform transf) {
      _attachPoint = transf;
    }

    /// <summary>
    /// Gets a value indicating whether this instance can cast.
    /// </summary>
    public bool CanCast {
      get { return _state == FishingLureState.ReeledIn; }
    }

    /// <summary>
    /// Gets the current velocity of the lure.
    /// </summary>
    public Vector3 Velocity {
      get { return _body.velocity; }
    }

    /// <summary>
    /// Sets the gravity force. A toward-earth gravity should
    /// be given as negative.
    /// </summary>
    /// <param name="gravity">Gravity force applied per frame</param>
    public void SetGravity(Vector3 gravity) {
      _acceleration = gravity;
    }

    /// <summary>
    /// Casts the lure at the specified velocity.
    /// </summary>
    /// <param name="velocity">Initial velocity to launch the lure.</param>
    public void Cast(Vector3 velocity) {
      _body.velocity = velocity;
      SetState(FishingLureState.Casting);
    }

    /// <summary>
    /// Reels in the lure if not attached to the fishing pole.
    /// </summary>
    public void ReelIn() {
      if (_state == FishingLureState.InWater
          || _state == FishingLureState.OnGround
          || _state == FishingLureState.Casting) {
        _currentReelSpeed = ReelSpeed;
        SetState(FishingLureState.Reeling);
      }
    }

    /// <summary>
    /// Sets the hook into the fish.
    /// </summary>
    /// <returns>True if a fish is caught, false otherwise</returns>
    public bool SetHook() {
      if (CaughtFish) {
        _currentReelSpeed = CaughtReelSpeed;
        SetState(FishingLureState.Reeling);
        OnSetHook.Invoke();
      }
      return CaughtFish;
    }

    private void CheckIfFishOnLine() {
      if (_state != FishingLureState.InWater || CaughtFish) {
        return;
      }
      float elapsed = Time.realtimeSinceStartup - _fishTimeStart;
      if (elapsed >= _fishCountdown) {
        _currentFish = RandomFish();
        if (_currentFish != null) {
          _currentFish.Catch(transform);
          OnFishCaught.Invoke();
        }
      }
    }

    private Fish RandomFish() {
      var prefab = FishPrefabs[Random.Range(0, 1000) % FishPrefabs.Length];
      var obj = (GameObject)Instantiate(prefab, transform.position, Quaternion.identity);
      return obj.GetComponent<Fish>();
    }

    private void UpdatePosition() {
      Vector3 nextPos = transform.position;
      if (_state == FishingLureState.ReeledIn) {
        // Keep the lure fixed in place until we cast it
        nextPos = AttachPoint;
        if (CaughtFish) {
          OnFishReeledIn.Invoke();
          DropFish();
        }
      } else if (_state == FishingLureState.Casting) {
        // Apply per-frame acceleration (gravity) while the lure is in the air
        _body.velocity += _acceleration * Time.fixedDeltaTime;
        nextPos += _body.velocity * Time.fixedDeltaTime;
      } else if (_state == FishingLureState.Reeling) {
        nextPos = Vector3.Lerp(nextPos, AttachPoint, _currentReelSpeed);
        float dist = Vector3.Distance(nextPos, AttachPoint);
        if (dist < .1f) {
          // Smoothly reel in into we're close enough, then just attach
          nextPos = AttachPoint;
          SetState(FishingLureState.ReeledIn);
        }
      } else if (_state == FishingLureState.InWater) {
        float yVal;
        if (CaughtFish) {
          // Bob quickly
          yVal = _landingPosition.y + Mathf.Sin(Time.time * 40f) * .03f;
        } else {
          // Bob slowly
          yVal = _landingPosition.y + Mathf.Sin(Time.time * .4f) * .05f;
        }
        // Only bob the our renderer so additional effects don't move
        LureVisual.position = new Vector3(_landingPosition.x, yVal, _landingPosition.z);
      }
      _body.MovePosition(nextPos);
    }

    private void DropFish() {
      FishCollector.DropFish(_currentFish);
      _currentFish = null;
    }

    private void StartFishTimer() {
      float waitMin = 0.75f;
      _fishCountdown = Random.Range(waitMin, Mathf.Max(waitMin, FishWaitMax));
      _fishTimeStart = Time.realtimeSinceStartup;
    }

    private void SetState(FishingLureState state) {
      _state = state;
      OnFishingLureState.Invoke(_state);
    }

    private Vector3 AttachPoint {
      get { return _attachPoint.position; }
    }

    private bool CaughtFish {
      get { return _currentFish != null; }
    }

    private Vector3 _acceleration;
    private Transform _attachPoint;
    private Rigidbody _body;
    private Fish _currentFish;
    private FishingLureState _state;
    private float _currentReelSpeed;
    private float _fishCountdown;
    private float _fishTimeStart;
    private Vector3 _landingPosition;
  }
}

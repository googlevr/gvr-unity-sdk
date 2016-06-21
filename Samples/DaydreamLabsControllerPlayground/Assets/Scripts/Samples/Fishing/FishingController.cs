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
  /// Central controller for a fishing rod.
  /// </summary>
  public class FishingController : MonoBehaviour {
    [Header("Controller Events")]
    public UnityEvent OnCast;

    public UnityEvent OnReeling;

    [Header("Constraints")]
    [Tooltip("Gravitational force to apply per frame to the lure/line")]
    public float Gravity = -4.5f;

    [Tooltip("Maximum force that can be applied to a lure (limits distance")]
    public float CastingForceCap = 12f;

    [Tooltip("If true, touch events will be used instead of click events for casting")]
    public bool UseTouch = true;

    [Header("GameObject Dependencies")]
    public FishingLure FishingLure;

    [Tooltip("Anchor for the fishing lure when it is reeled all the way in.")]
    public Transform LureAttachPoint;

    [Header("Camera Integration")]
    [Tooltip("Player's head for tracking.")]
    public Transform Head;

    void Start() {
      if (FishingLure != null) {
        FishingLure.SetLureAttachPoint(LureAttachPoint);
        FishingLure.SetGravity(new Vector3 { y = Gravity });
      } else {
        Debug.LogError("Attach a fishing lure object.");
      }
    }

    void Update() {
      if (IsInputHeld()) {
        FishingLure.ReelIn();
      }
      if (IsInputDown()) {
        if (!FishingLure.CanCast) {
          // Avoid sending the reeling event when the
          // lure is reeled all the way in.
          OnReeling.Invoke();
        }
      }

      Quaternion currentAngle = transform.rotation;
      if (IsInputUp()) {
        AttemptCast(currentAngle);
        OnCast.Invoke();
      }
      RecordRotation(currentAngle);
    }

    /// <summary>
    /// Set the hook into a caught fish, reeling it in quickly.
    /// </summary>
    public void SetHook() {
      if (FishingLure.SetHook())
        OnReeling.Invoke();
    }

    private bool IsInputUp() {
      return UseTouch ? GvrController.TouchUp : GvrController.ClickButtonUp;
    }

    private bool IsInputDown() {
      return UseTouch ? GvrController.TouchDown : GvrController.ClickButtonDown;
    }

    private bool IsInputHeld() {
      return UseTouch ? GvrController.IsTouching : GvrController.ClickButton;
    }

    private void AttemptCast(Quaternion currentAngle) {
      Vector3 castForce = Vector3.zero;
      if (FishingLure.CanCast) {
        Vector3 lureVel = FishingLure.Velocity;
        Vector3 headForward = Head.TransformDirection(Vector3.forward);

        // Assist toward look direction
        Vector3 forwardF = Vector3.Lerp(lureVel.normalized, headForward.normalized, 0.5f);
        forwardF.Normalize();

        //get speed of swing when you're releasing
        float angleVel = Quaternion.Angle(currentAngle, GetSampledRotation());
        angleVel = Mathf.Clamp(angleVel, 1f, CastingForceCap);

        //apply speed to velocity
        castForce = forwardF * angleVel;

        // Uncomment this line to debug your throwing vectors if changes are made
        // to easing, velocity sampling, etc.
        // DrawDebugVectors(lureVel, headForward, forwardF);
      }
      FishingLure.Cast(castForce);
    }

    private void RecordRotation(Quaternion rotation) {
      if (_angleQueue.Count >= ROTATION_SAMPLES) {
        _angleQueue.Dequeue();
      }
      _angleQueue.Enqueue(rotation);
    }

    private Quaternion GetSampledRotation() {
      Quaternion q = Quaternion.identity;
      Quaternion[] all = _angleQueue.ToArray();
      for (int i = 0; i < all.Length; i++) {
        q = Quaternion.Lerp(q, all[i], 0.5f);
      }
      return q;
    }

    private void DrawDebugVectors(Vector3 lureVel, Vector3 headForward, Vector3 forwardF) {
      Debug.DrawRay(transform.position, lureVel * 5f, Color.red, 5f);
      Debug.DrawRay(transform.position, headForward * 5f, Color.blue, 5f);
      Debug.DrawRay(transform.position, forwardF * 5f, Color.green, 5f);
    }

    private readonly Queue<Quaternion> _angleQueue = new Queue<Quaternion>();

    private const int ROTATION_SAMPLES = 10;
  }
}

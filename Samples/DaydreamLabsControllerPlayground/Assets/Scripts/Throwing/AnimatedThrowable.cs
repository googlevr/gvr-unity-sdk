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

namespace GVR.Throwing {
  /// <summary>
  /// A throwable object whose path is controlled by animation.
  /// </summary>
  public class AnimatedThrowable : Throwable {
    public Animator Animator;
    public TrailRenderer ThrowEffect;

    [Tooltip("Name of the animator boolean indicating if this object is currently held")]
    public string HeldBoolName = "Held";

    [Tooltip("Name of the animator boolean indicating handedness (true for right handed)")]
    public string RightHandedBoolName = "IsRightHand";

    [Tooltip("Used to flip the object visually so it looks right regardless of hand choice")]
    public VisualHandFlip Visual;

    void OnEnable() {
      Animator.enabled = false;
    }

    void Start() {
      if (Animator == null) {
        Debug.LogError("No animator attached");
      }
      if (ThrowEffect == null) {
        Debug.LogError("No trail renderer attached");
      }
    }

    /// <summary>
    /// Throws the object.
    /// </summary>
    /// <param name="thrower">
    /// Transform of the thrower, used to determine start of the throw.
    /// </param>
    /// <param name="isRightHanded">True: object thrown right handed</param>
    public override void Throw(Transform thrower, bool isRightHanded) {
      transform.rotation = Quaternion.AngleAxis(GvrViewer.Instance.HeadPose.Orientation.eulerAngles.y, Vector3.up);
      transform.position = thrower.position;
      //  GvrViewer.Instance.HeadPose.Orientation+;
      base.Throw(thrower, isRightHanded);
      ThrowEffect.enabled = true;
      Animator.enabled = true;
      Animator.applyRootMotion = true;
      Animator.SetBool(HeldBoolName, false);
      Animator.SetBool(RightHandedBoolName, isRightHanded);
    }

    /// <summary>
    /// Picks up up the throwable
    /// </summary>
    /// <param name="isRightHanded">TrueL object is held in the right hand</param>
    public override void PickUp(bool isRightHanded) {
      base.PickUp(isRightHanded);
      ThrowEffect.enabled = false;
      Animator.applyRootMotion = true;
      Animator.SetBool(HeldBoolName, true);
      Visual.Flip(isRightHanded);
    }
  }
}

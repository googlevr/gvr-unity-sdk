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

namespace GVR.Entity {
  /// <summary>
  ///  This property allows an object to be translated based
  ///  on remote orientation. The object will remain at PickupDistance from the
  ///  remote transform, and will move to where the remote is pointing.
  ///  If instant mode is used, the object will set the transform position.
  ///  If smooth mode is used, the object will rigidbody MovePosition.
  /// </summary>
  public class PickupProperty : InteractableProperty {
    public enum PickUpMode {
      instant,
      smooth
    };

    [Tooltip("Will the object teleport to the intended location, or lerp there")]
    public PickUpMode PickupMode;

    [Tooltip("The distance from the remote at which this object will held.")]
    public float PickupDistance;

    private KeyValuePair<float, Vector3>[] recentPositions = new KeyValuePair<float, Vector3>[10];
    private int index = 0;

    public override bool OnRemoteOrientation(Transform remoteTransform) {
      base.OnRemoteOrientation(remoteTransform);
      InteractableObj.RigidBody.velocity = Vector3.zero;
      InteractableObj.RigidBody.angularVelocity = Vector3.zero;
      switch (PickupMode) {
        case PickUpMode.instant:
          gameObject.transform.position =
              remoteTransform.position + remoteTransform.forward * PickupDistance;
          break;
        case PickUpMode.smooth:
          Vector3 smoothPosition =
              GetSmoothApproach(transform.position, 
                                remoteTransform.position + remoteTransform.forward * PickupDistance,
                                1f, Time.deltaTime);
          InteractableObj.RigidBody.MovePosition(smoothPosition);
          index = GetNextIndex();
          recentPositions[index] = new KeyValuePair<float, Vector3>(Time.time, smoothPosition);
          break;
      }
      return true;
    }

    public override bool OnRemotePressDown() {
      base.OnRemotePressUp();
      InteractableObj.RigidBody.useGravity = false;
      for (int i = 0; i < recentPositions.Length; i++) {
        recentPositions[i] = new KeyValuePair<float, Vector3>(Time.time, transform.position);
      }
      index = 0;
      return true;
    }

    public override bool OnRemotePressUp() {
      base.OnRemotePressUp();
      InteractableObj.RigidBody.useGravity = true;
      int nextIndex = GetNextIndex();
      Vector3 throwVelocity = Vector3.zero;
      if (recentPositions[index].Key - recentPositions[nextIndex].Key > 0) {
        throwVelocity = (recentPositions[index].Value - recentPositions[nextIndex].Value) /
                        (recentPositions[index].Key - recentPositions[nextIndex].Key);
      }
      InteractableObj.RigidBody.AddForce(throwVelocity, ForceMode.VelocityChange);
      return true;
    }

    public override void TransferOwnership(InteractableObject interactableObject) {
      base.TransferOwnership(interactableObject);
      OnRemotePressUp();
    }

    private int GetNextIndex() {
      int nextIndex = index + 1;
      if (nextIndex >= recentPositions.Length) {
        nextIndex = 0;
      }
      return nextIndex;
    }

    private Vector3 GetSmoothApproach(Vector3 current, Vector3 target, float duration, float dt) {
      float remainingPortion = Mathf.Pow(0.001f, dt / duration);
      return Vector3.Lerp(current, target, 1f - remainingPortion);
    }
  }
}

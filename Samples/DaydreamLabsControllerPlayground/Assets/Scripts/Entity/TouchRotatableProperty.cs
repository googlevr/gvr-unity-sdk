// Copyright 2016 Google Inc. All right s reserved.
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
  ///  This property rotates the held Interactable Object based on a
  ///  two-dimensional axis input (touch position). When a touch up
  ///  event is passed, the object will continue to spin as if it had
  ///  angular inertia.
  /// </summary>
  public class TouchRotatableProperty : InteractableProperty {
    [Tooltip("This is a multiplier for the effectiveness of a change in input position")]
    public float TouchSensitivity;

    [Tooltip("This is a multiplier for the effectiveness of angular inertia")]
    public float SwipeSensitivity;

    [Tooltip("The time it takes for angular inertia to degrade to zero")]
    public float SwipeDuration;

    private bool touchActive = false;
    private float swipeTimer = 0f;

    private Vector2 rotationVelocity = Vector2.zero;
    private KeyValuePair<float, Vector2>[] touchRecords = null;
    private int index = 0;

    private float rollAngle;
    
    void Start() {
      touchRecords = new KeyValuePair<float, Vector2>[5];
    }

    public override bool OnRemoteTouchDown(Vector2 touchPos, Transform remoteTransform) {
      base.OnRemoteTouchDown(touchPos, remoteTransform);
      rollAngle = remoteTransform.transform.eulerAngles.z;
      for (int i = 0; i < touchRecords.Length; i++) {
        touchRecords[i] = new KeyValuePair<float, Vector2>(Time.time, touchPos);
      }
      index = 0;
      rotationVelocity = Vector3.zero;
      touchActive = true;
      swipeTimer = 0;
      return true;
    }

    public override bool OnRemoteTouchDelta(Vector2 touchPos, Transform remoteTransform) {
      base.OnRemoteTouchDelta(touchPos, remoteTransform);
      if (!touchActive) {
        return true;
      }

      Vector2 prevPos = touchRecords[index].Value;
      Vector2 diff = touchPos - prevPos;
      transform.Rotate(remoteTransform.up, diff.x / Time.deltaTime * TouchSensitivity * -1, Space.World);
      transform.Rotate(remoteTransform.right, diff.y / Time.deltaTime * TouchSensitivity * -1, Space.World);
      index = GetNextIndex();
      touchRecords[index] = new KeyValuePair<float, Vector2>(Time.time, touchPos);
      return true;
    }

    public override bool OnRemoteTouchUp(Vector2 touchPos, Transform remoteTransform) {
      base.OnRemoteTouchUp(touchPos, remoteTransform);
      touchActive = false;
      int nextIndex = GetNextIndex();
      if (touchRecords[index].Key - touchRecords[nextIndex].Key > 0) {
        rotationVelocity = (touchRecords[index].Value - touchRecords[nextIndex].Value) /
                           (touchRecords[index].Key - touchRecords[nextIndex].Key) * SwipeSensitivity;
      } else {
        rotationVelocity = Vector3.zero;
      }
      swipeTimer = SwipeDuration;
      return true;
    }

    public override bool OnRemoteOrientation(Transform remoteTransform) {
      base.OnRemoteOrientation(remoteTransform);
      transform.Rotate(remoteTransform.forward, -1.0f*(rollAngle - remoteTransform.eulerAngles.z),Space.World);
      rollAngle = remoteTransform.eulerAngles.z;
      if (swipeTimer > 0 && !touchActive) {
        Vector2 velocity = rotationVelocity * (swipeTimer / SwipeDuration);
        transform.Rotate(remoteTransform.up, velocity.x * TouchSensitivity * -1, Space.World);
        transform.Rotate(remoteTransform.right, velocity.y * TouchSensitivity * -1, Space.World);
        swipeTimer -= Time.deltaTime;
      }
      return true;
    }

    public override void TransferOwnership(InteractableObject interactableObject) {
      base.TransferOwnership(interactableObject);
      swipeTimer = 0;
      touchActive = false;
      rotationVelocity = Vector3.zero;
    }

    private int GetNextIndex() {
      int nextIndex = index + 1;
      if (nextIndex >= touchRecords.Length) {
        nextIndex = 0;
      }
      return nextIndex;
    }
  }
}

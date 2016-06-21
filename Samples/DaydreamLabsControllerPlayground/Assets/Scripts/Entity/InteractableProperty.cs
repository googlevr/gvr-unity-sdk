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

namespace GVR.Entity {
  /// <summary>
  ///  This this is base component for all properties associated with Interactable Objects.
  ///  When an Interactable Object recieves a remote input event, it relays that event to
  ///  all of its Interactable Properties. Each property is responsible for performing its
  ///  behavior only on the event that that property recognizes. If the function is overriden
  ///  in its Property code, it should return true to show that that event has been handled.
  /// </summary>
  public class InteractableProperty : MonoBehaviour {
    public InteractableObject InteractableObj { get; set; }

    public virtual bool OnRemoteTouchDown(Vector2 touchPos, Transform remoteTransform) {
      return false;
    }

    public virtual bool OnRemoteTouchDelta(Vector2 touchPos, Transform remoteTransform) {
      return false;
    }

    public virtual bool OnRemoteTouchUp(Vector2 touchPos, Transform remoteTransform) {
      return false;
    }

    public virtual bool OnRemotePressDown() {
      return false;
    }

    public virtual bool OnRemotePressDelta() {
      return false;
    }

    public virtual bool OnRemotePressUp() {
      return false;
    }

    public virtual bool OnRemoteOrientation(Transform remoteTransform) {
      return false;
    }

    public virtual bool OnRemotePointEnter() {
      return false;
    }

    public virtual bool OnRemotePointExit() {
      return false;
    }

    public virtual void TransferOwnership(InteractableObject interactableObject) {
    }

    public virtual void Destruct() {
      this.InteractableObj.Properties.Remove(this);
      Destroy(this);
    }
  }
}

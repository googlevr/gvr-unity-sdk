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
  ///  This component marks this object as responding to IRemoteInputEvent events.
  ///  It stores a list of InteractableProperty, and relays all events it sees
  ///  to each of those properties.
  ///  Using this system, you can write in individual behaviors that are triggered by
  ///  remote input buttons/axis/ect and swap them in and out of Interactable Objects.
  /// </summary>
  public class InteractableObject : MonoBehaviour, IRemoteInputEventHandler {
    [Tooltip("Reference to this object's rigidbody. Many properties need it.")]
    public Rigidbody RigidBody;

    [Tooltip("List of properties attached to this gameobject.")]
    public List<InteractableProperty> Properties;

    void Start() {
      for (int i = 0; i < Properties.Count; i++) {
        Properties[i].InteractableObj = this;
      }
    }

    public void OnRemoteTouchDown(out bool handled, Vector2 touchPos, Transform remoteTransform) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemoteTouchDown(touchPos, remoteTransform);
      }
    }

    public void OnRemoteTouchDelta(out bool handled, Vector2 touchPos, Transform remoteTransform) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemoteTouchDelta(touchPos, remoteTransform);
      }
    }

    public void OnRemoteTouchUp(out bool handled, Vector2 touchPos, Transform remoteTransform) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemoteTouchUp(touchPos, remoteTransform);
      }
    }

    public void OnRemotePressDown(out bool handled) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemotePressDown();
      }
    }

    public void OnRemotePressDelta(out bool handled) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemotePressDelta();
      }
    }

    public void OnRemotePressUp(out bool handled) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemotePressUp();
      }
    }

    public void OnRemoteOrientation(out bool handled, Transform remoteTransform) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemoteOrientation(remoteTransform);
      }
    }

    public void OnRemotePointEnter(out bool handled) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemotePointEnter();
      }
    }

    public void OnRemotePointExit(out bool handled) {
      handled = false;
      for (int i = 0; i < Properties.Count; i++) {
        handled |= Properties[i].OnRemotePointExit();
      }
    }

    public void TransferOwnership(InteractableObject toInteractableObj) {
      for (int i = Properties.Count - 1; i >= 0; i--) {
        Properties[i].TransferOwnership(toInteractableObj);
      }
      Destroy(RigidBody);
      Destroy(this);
    }

    public void Nullify() {
      for (int i = Properties.Count - 1; i >= 0; i--) {
        Properties[i].Destruct();
      }
      Destroy(RigidBody);
      Destroy(this);
    }
  }
}

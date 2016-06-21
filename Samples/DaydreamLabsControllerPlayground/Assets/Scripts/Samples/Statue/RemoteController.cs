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

using GVR.Entity;
using GVR.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GVR.Samples.Statue {
  /// <summary>
  ///  This component describes the remote interface. It pushes button state events to
  ///  the currently selected object using an event bus handler, IRemoteInputEventHandler.
  /// </summary>
  public class RemoteController : MonoBehaviour {
    [Tooltip("The max distance to raycast for interactable objects.")]
    public float PointerDistance;

    [Tooltip("Reference to laser pointer component for the remote.")]
    public LaserPointer LaserPointer;

    private Ray pointerRay;
    private RaycastHit pointerHit;
    private bool clickActive = false;

    public GameObject TargetedObject { get; set; }

    public GameObject InteractedObject { get; set; }

    void Update() {
      UpdatePointer();
      ProcessTouchPad();
    }

    private void UpdatePointer() {
      bool handled = true;
      pointerRay = new Ray(transform.position, transform.forward);
      if (InteractedObject != null) {
        ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
            (x, y) => x.OnRemoteOrientation(out handled, transform));
        LaserPointer.PointerTarget = TargetedObject.transform.position;
        if (!handled) {
          InteractedObject = null;
        }
      } else if (Physics.Raycast(pointerRay, out pointerHit, PointerDistance)) {
        if (ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(pointerHit.collider.gameObject, null,
            (x, y) => x.OnRemotePointEnter(out handled))) {
          if (TargetedObject != null && !TargetedObject.Equals(pointerHit.collider.gameObject)) {
            ExecuteEvents.Execute<IRemoteInputEventHandler>(TargetedObject, null,
                (x, y) => x.OnRemotePointExit(out handled));
          }
          TargetedObject = pointerHit.collider.gameObject;
        } else if (TargetedObject != null) {
          ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(TargetedObject, null,
              (x, y) => x.OnRemotePointExit(out handled));
          TargetedObject = null;
        }
        LaserPointer.PointerTarget = pointerHit.point;
      } else {
        LaserPointer.PointerTarget = transform.position + transform.forward * PointerDistance;
        if (TargetedObject != null) {
          ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(TargetedObject, null,
              (x, y) => x.OnRemotePointExit(out handled));
          TargetedObject = null;
        }
      }
    }

    private void ProcessTouchPad() {
      bool handled = true;
      if (clickActive) {
        ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
          (x, y) => x.OnRemoteTouchDelta(out handled, GvrController.TouchPos, transform));
      }
      if (GvrController.ClickButtonUp) {
        if (InteractedObject == null && TargetedObject != null) {
          if (ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(pointerHit.collider.gameObject, null,
                (x, y) => x.OnRemotePressDown(out handled))) {
            if (handled) {
              InteractedObject = pointerHit.collider.gameObject;
              ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
                (x, y) => x.OnRemoteTouchDown(out handled, GvrController.TouchPos, transform));
            }
          } else {
            InteractedObject = null;
          }
          clickActive = true;
        } else if (InteractedObject != null) {
          ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
            (x, y) => x.OnRemotePressUp(out handled));
          InteractedObject = null;
          clickActive = false;
        }
      }
      if (GvrController.TouchDown && InteractedObject != null) {
        ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
          (x, y) => x.OnRemoteTouchDown(out handled, GvrController.TouchPos, transform));
      }
      if (GvrController.TouchUp && InteractedObject != null) {
        ExecuteEvents.ExecuteHierarchy<IRemoteInputEventHandler>(InteractedObject, null,
          (x, y) => x.OnRemoteTouchUp(out handled, GvrController.TouchPos, transform));
      }
    }
  }
}

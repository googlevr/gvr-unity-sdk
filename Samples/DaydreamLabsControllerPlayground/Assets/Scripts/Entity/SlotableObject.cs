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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Entity {
  /// <summary>
  ///  This component allows an object to slot into another one when they collide. When an object
  ///  successfully slots, its Interactable Properties self destruct and its slot children
  ///  are transfered to the SlotableObject of its new parent.
  /// </summary>
  public class SlotableObject : MonoBehaviour {
    [Tooltip("Reference to this object's interactable object. Not required.")]
    public InteractableObject InteractableObject;

    [Tooltip("Reference to the audiosource on this object that plays when succefully slotting.")]
    public GvrAudioSource SlotAudio;

    [Tooltip("The slot identifier for this object. Used for comparisons by SlotDescription.")]
    public string SlotKey;

    [Tooltip("Reference to transform that defines this object's location. Used for comparisons by SlotDescription.")]
    public Transform SlotTransform;

    [Tooltip("List of SlotDescription that defines slots for other objects to fit into.")]
    public List<SlotDescription> ChildrenSlots;

    void OnCollisionEnter(Collision collision) {
      CheckAgainstSlots(collision);
    }

    void OnCollisionStay(Collision collision) {
      CheckAgainstSlots(collision);
    }

    private void CheckAgainstSlots(Collision collision) {
      if (ChildrenSlots.Count == 0 || collision.collider.attachedRigidbody == null) {
        return;
      }
      SlotableObject slotableObj = collision.collider.GetComponent<SlotableObject>();
      if (slotableObj == null) {
        return;
      }
      Transform otherTransform = slotableObj.SlotTransform;
      for (int i = 0; i < ChildrenSlots.Count; i++) {
        SlotDescription sd = ChildrenSlots[i];
        if (!slotableObj.SlotKey.Equals(sd.IntendedSlotKey)) {
          continue;
        }
        Vector3 intendedPosition = sd.SlotTransform.position;
        Quaternion intendedRotation = sd.SlotTransform.rotation;
        if (Vector3.Distance(otherTransform.position, intendedPosition) < sd.PositionalForgiveness &&
            Quaternion.Angle(otherTransform.rotation, intendedRotation) < sd.RotationalForgiveness) {
          if (slotableObj.InteractableObject) {
            slotableObj.InteractableObject.Nullify();
          }
          ChildrenSlots.Remove(sd);
          ChildrenSlots.AddRange(slotableObj.ChildrenSlots);
          Destroy(slotableObj);

          collision.collider.transform.parent = sd.SlotTransform;
          collision.collider.transform.localPosition = Vector3.zero;
          collision.collider.transform.localRotation = Quaternion.identity;

          SlotAudio.Play();
          break;
        }
      }
    }
  }

  /// <summary>
  ///  This class is a serializable container for data on a child slot.
  /// </summary>
  [Serializable]
  public class SlotDescription {
    [Tooltip("Reference to Transform that compares to another slot's SlotTransform")]
    public Transform SlotTransform;

    [Tooltip("The key that compares to another slot's SlotKey")]
    public string IntendedSlotKey;

    [Tooltip("Within this distance, a slot comparison will be considered matching.")]
    public float PositionalForgiveness;

    [Tooltip("Within this angle (degrees), a slot comparison will be considereing matching.")]
    public float RotationalForgiveness;
  }
}

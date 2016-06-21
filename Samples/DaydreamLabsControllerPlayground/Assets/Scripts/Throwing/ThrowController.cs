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

using GVR.Events;
using GVR.Visual;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Throwing {
  /// <summary>
  /// Player controller that manages spawning and throwing objects.
  /// </summary>
  public class ThrowController : MonoBehaviour {
    [Header("Throw Events")]
    public UnityEvent OnObjectSpawned;
    public UnityEvent OnThrown;
    public UnityEvent OnCatch;

    [Header("Throwing")]
    [Tooltip("If true, the controller will spawn an object to throw if none is held")]
    public bool SpawnOnStart = true;

    [Tooltip("Throwable prefab")]
    public Throwable DefaultThrown;

    [Tooltip("Currently held object")]
    public Throwable InHand;

    [Tooltip("Game object parent for holding throwable objects")]
    public GameObject Hand;

    [Header("Held Object Positioning")]
    [Tooltip("Local position offset if object is held in the right hand")]
    public Vector3 RightPositionOffset;

    [Tooltip("Local position offset if object is held in the left hand")]
    public Vector3 LeftPositionOffset;

    [SerializeField]
    private bool _usingRightHand;

    public Transform ThrowOrigin;

    void Awake() {
      if (HandednessListener.IsLeftHanded) {
        UseLeftHand();
      } else
        UseRightHand();
    }

    void Start() {
      if (SpawnOnStart) {
        SpawnObject();
      }
    }

    void OnTriggerEnter(Collider coll) {
      if (Hand == null) {
        return;
      }
      var t = coll.gameObject.GetComponentInParent<Throwable>();
      if (t != null) {
        if (t.CanCatch) {
          TryToCatch(t);
        }
      }
    }

    /// <summary>
    /// Hold objects in the "right" hand.
    /// </summary>
    public void UseRightHand() {
      _usingRightHand = true;
      OrientHeldObject();
    }

    /// <summary>
    /// Hold objects in the "left" hand.
    /// </summary>
    public void UseLeftHand() {
      _usingRightHand = false;
      OrientHeldObject();
    }

    private void OrientHeldObject() {
      if (HoldingObject) {
        InHand.transform.localPosition = ActiveHeldPosition;
      }
    }

    /// <summary>
    /// Throws the object.
    /// </summary>
    /// <param name="isRightHanded">
    /// If set to <c>true</c> throw using the right hand.</param>
    public void ThrowObject(bool isRightHanded) {
      TryToThrow(isRightHanded);
    }

    /// <summary>
    /// Throws the object using the current handedness.
    /// </summary>
    public void ThrowObject() {
      TryToThrow(_usingRightHand);
    }

    /// <summary>
    /// Spawns an object to throw if one isn't held.
    /// </summary>
    public void SpawnObject() {
      if (HoldingObject) {
        return;
      }
      GameObject obj = Instantiate(DefaultThrown.gameObject);
      gameObject.GetComponent<GVR.Entity.SoundEventListener>().Source = obj.GetComponentInChildren<GvrAudioSource>();
      obj.SetActive(true);
      var cycle = obj.GetComponent<MaterialCycle>();
      if (cycle != null) {
        cycle.Set(_nextMaterial++);
      }
      Pickup(obj.GetComponent<Throwable>());
    }

    private void Pickup(Throwable t) {
      if (t == null) {
        return;
      }
      InHand = t;
      InHand.PickUp(_usingRightHand);
      GameObject obj = InHand.gameObject;
      obj.transform.localRotation = Quaternion.identity;
      obj.transform.SetParent(Hand.transform, false);
      OrientHeldObject();
      OnObjectSpawned.Invoke();
    }

    private void TryToThrow(bool isRightHanded) {
      if (HoldingObject) {
        // Prevent opposite handed motions from throwing
        if (isRightHanded == _usingRightHand) {
          InHand.Throw(ThrowOrigin, isRightHanded);
          InHand = null;
          OnThrown.Invoke();
        }
      }
    }

    private void TryToCatch(Throwable t) {
      if (HoldingObject) {
        t.CompleteThrow();
      } else {
        Pickup(t);
        OnCatch.Invoke();
      }
    }

    private bool HoldingObject {
      get { return InHand != null; }
    }

    private Vector3 ActiveHeldPosition {
      get { return _usingRightHand ? RightPositionOffset : LeftPositionOffset; }
    }

    private int _nextMaterial;
  }
}

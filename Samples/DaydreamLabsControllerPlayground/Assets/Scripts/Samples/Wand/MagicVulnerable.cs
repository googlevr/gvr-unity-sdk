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

namespace GVR.Samples.Magic {
  /// <summary>
  /// An object that can be hit by a magic projectile. When this object is
  /// hit it will randomly swap to one of the prefabs specified in the object
  /// arrays. All possible objects are pooled on startup for this object.
  /// </summary>
  public class MagicVulnerable : MonoBehaviour {
    [Header("Transformation")]
    public UnityEvent OnHitStrong;

    public UnityEvent OnHitWeak;

    [Tooltip("Time to wait before swapping to the new object")]
    public float Delay = 0.8f;

    [Header("Attached Entities")]
    [Tooltip("Reference to the player. If specified, objects will turn to look at the player when spawned")]
    public Transform Player;

    [Tooltip("Parent transform for spawned objects (use to align with effects).")]
    public Transform VisualParent;

    [Tooltip("Objects that will swap in when hit with weak magic")]
    public GameObject[] WeakObjects;

    [Tooltip("Objects that will swap in when hit with strong magic")]
    public GameObject[] StrongObjects;

    public MagicProjectile.MagicStrength StartStrength;

    void Start() {
      ValidateObjectLists();
      InitObjectPool();
      LookAtPlayer();
    }

    private void ValidateObjectLists() {
      if (WeakObjects == null)
        WeakObjects = new GameObject[0];

      if (WeakObjects.Length < 1)
        Debug.LogWarning("No objects attached for weak magic.");

      if (StrongObjects == null)
        StrongObjects = new GameObject[0];

      if (StrongObjects.Length < 1)
        Debug.LogWarning("No objects attached for strong magic.");
    }

    void OnDrawGizmos() {
      Gizmos.DrawCube(transform.position, new Vector3(2f, 2f, 2f));
    }

    void Update() {
      _delayTimer.Update();
    }

    /// <summary>
    /// Called when this object is hit.
    /// </summary>
    /// <param name="swapper">Object hit</param>
    public void OnObjectHit(MagicProjectile swapper) {
      if (_delayTimer.IsReady) {
        if (swapper.Strength == MagicProjectile.MagicStrength.Strong) {
          SwapObject(GetRandomUniqueStrong());
          OnHitStrong.Invoke();
        } else {
          SwapObject(GetRandomUniqueWeak());
          OnHitWeak.Invoke();
        }
      }
    }

    private void SwapObject(GameObject prefab) {
      _delayTimer.Start(Delay, () => {
        ActivatePooledObject(prefab);
      });
    }

    private GameObject GetRandomUniqueWeak() {
      _weakIndex = GetRandomIndex(WeakObjects.Length, _weakIndex);
      return _pooled[_weakIndex];
    }

    private GameObject GetRandomUniqueStrong() {
      _strongIndex = GetRandomIndex(StrongObjects.Length, _strongIndex);
      int poolOffset = _strongIndex + WeakObjects.Length;
      return _pooled[poolOffset];
    }

    private void ActivatePooledObject(GameObject prefab) {
      if (_active != null)
        _active.SetActive(false);

      _active = prefab;
      _active.SetActive(true);
    }

    private int GetRandomIndex(int length, int previous = -1) {
      int random = Random.Range(0, 1000) % length;
      if (random == previous)
        random = (random + 1) % length;

      return random;
    }

    private void InitObjectPool() {
      _pooled = new List<GameObject>();
      _pooled.AddRange(SpawnForArray(WeakObjects));
      _pooled.AddRange(SpawnForArray(StrongObjects));

      var start = StartStrength == MagicProjectile.MagicStrength.Weak
          ? GetRandomUniqueWeak()
          : GetRandomUniqueStrong();

      ActivatePooledObject(start);
    }

    private IEnumerable<GameObject> SpawnForArray(GameObject[] objs) {
      var result = new List<GameObject>();
      for (int i = 0; i < objs.Length; i++) {
        result.Add(SpawnObject(objs[i]));
      }
      return result;
    }

    private GameObject SpawnObject(GameObject prefab) {
      var obj = Instantiate(prefab);
      MagicHitBox hittable = obj.GetComponent<MagicHitBox>();
      if (hittable == null) {
        hittable = obj.AddComponent<MagicHitBox>();
        Debug.LogWarningFormat("{0} prefab does not have a MagicHittable attached (using default).", obj.name);
      }

      hittable.Subscribe(OnObjectHit);
      obj.transform.SetParent(VisualParent, false);
      obj.SetActive(false);
      return obj;
    }

    private void LookAtPlayer() {
      if (Player == null)
        return;

      VisualParent.LookAt(Player.transform);
      var localRot = VisualParent.transform.localRotation.eulerAngles;
      VisualParent.transform.localRotation = Quaternion.Euler(new Vector3 {
        x = 0f,
        y = localRot.y,
        z = localRot.z
      });
    }

    private int _weakIndex;
    private int _strongIndex;
    private GameObject _active;
    private List<GameObject> _pooled;

    private readonly DelayTimer _delayTimer = new DelayTimer();
  }
}

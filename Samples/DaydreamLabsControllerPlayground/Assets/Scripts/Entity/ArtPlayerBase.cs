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
using UnityEngine.Events;

namespace GVR.Entity {
  /// <summary>
  /// Base component for visual/sound effects playing components.
  /// Pre-assigned componets or attached prefabs can be used. Using
  /// the prefab option will allow iteration on art prefabs withouts
  /// breaking prefab connections.
  ///
  /// Instances of ArtPlayerBase also expose an event that allows
  /// chaining of additional players.
  /// </summary>
  /// <typeparam name="T">Type of Unity object to play</typeparam>
  public abstract class ArtPlayerBase<T> : MonoBehaviour where T : UnityEngine.Object {
    public T Player;

    [Header("Prefab Usage")]
    [Tooltip("If checked, the FX prefab to use will be instantiated and connected to this FX prefab.")]
    public bool UsePrefab = false;

    public GameObject FxPrefab;

    [Tooltip("Keep prefab rotation when parenting")]
    public TransformMode RotationMode;

    [Tooltip("Keep prefab position when parenting")]
    public TransformMode PositionMode;

    [Tooltip("If specified, partent to this transform instead")]
    public Transform OptionalParent;

    public UnityEvent OnPlayed;

    /// <summary>
    /// Unity Awake Message
    /// </summary>
    protected virtual void Awake() {
    }

    void Start() {
      if (UsePrefab && FxPrefab != null) {
        var obj = Instantiate(FxPrefab);
        Player = obj.GetComponent<T>();
        DoParenting(obj);
      }

      if (Player == null)
        Player = GetComponent<T>();

      OnAfterStart();
    }

    /// <summary>
    /// Called after the base class completes its startup operation.
    /// </summary>
    protected virtual void OnAfterStart() {
    }

    /// <summary>
    /// Fires the effect.
    /// </summary>
    public void Fire() {
      FireAction();
      OnPlayed.Invoke();
    }

    /// <summary>
    /// Implement this method to play/fire/active the player.
    /// </summary>
    protected abstract void FireAction();

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public virtual void Reset() {
    }

    private Transform RealParent {
      get {
        return OptionalParent ?? transform;
      }
    }

    private void DoParenting(GameObject obj) {
      Vector3 position = obj.transform.localPosition;
      Quaternion rotation = obj.transform.localRotation;
      obj.transform.SetParent(RealParent);
      AdjustPosition(obj.transform, PositionMode, position);
      AdjustRotation(obj.transform, RotationMode, rotation);
    }

    private void AdjustRotation(Transform xf, TransformMode mode, Quaternion original) {
      if (mode != TransformMode.DoNothing) {
        xf.localRotation = (mode == TransformMode.KeepOriginal) ? original : Quaternion.identity;
      }
    }

    private void AdjustPosition(Transform xf, TransformMode mode, Vector3 original) {
      if (mode != TransformMode.DoNothing) {
        xf.localPosition = (mode == TransformMode.KeepOriginal) ? original : Vector3.zero;
      }
    }
  }
}

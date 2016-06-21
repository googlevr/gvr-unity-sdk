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

namespace GVR.Throwing {
  /// <summary>
  /// A throwable object that uses look direction for aiming.
  /// A constant speed is specified, and aim-assist options are
  /// available.
  /// </summary>
  public class HeadLookThrowable : Throwable {
    [Tooltip("In seconds")]
    public float LifeSpan = 30f;

    [Tooltip("Maximum throwable range (unlimited if <= 0)")]
    public float MaxDistance;

    [Tooltip("Clear the object's local rotation before throwing")]
    public bool LevelWhenThrown = true;

    [Header("Aim Assist")]
    public bool AimAssistEnabled = true;

    [Tooltip("Radius from the gaze-center to look for auto-aim targets")]
    public float AssistRadius = 1f;

    [Tooltip("Thrown speed")]
    public float Speed = 5f;

    [Tooltip("Reference to the Head gameobject to get gaze direction.")]
    public Transform Head;

    void Update() {
      if (!_thrown) {
        return;
      }
      float elapsed = Time.realtimeSinceStartup - _lifetime;
      float distanceThrown = Vector3.Distance(_thrownPosition, transform.position);
      if (elapsed >= LifeSpan
          || (MaxDistance > 0f && distanceThrown >= MaxDistance)) {
        Destroy(gameObject);
      } else if (_transformTarget != null) {
        transform.position = Vector3.MoveTowards(transform.position, _transformTarget.position,
                                                 Speed * Time.deltaTime);
      } else {
        transform.position += _targetDirection * Speed * Time.deltaTime;
      }
    }

    /// <summary>
    /// Throws the object.
    /// </summary>
    /// <param name="thrower">Transform of the thrower, used to determine start of the throw.</param>
    /// <param name="isRightHanded">True: object thrown right handed</param>
    public override void Throw(Transform thrower, bool isRightHanded) {
      base.Throw(thrower, isRightHanded);
      if (LevelWhenThrown) {
        transform.eulerAngles = Vector3.zero;
      }
      _thrown = true;
      _transformTarget = null;
      _thrownPosition = transform.position;
      float distance = MaxDistance > 0f ? MaxDistance : 999f;
      _targetDirection = Head.TransformDirection(Vector3.forward * distance);
      if (AimAssistEnabled) {
        DoAimAssist();
      }
      _targetDirection.Normalize();
      _lifetime = Time.realtimeSinceStartup;
    }

    private void DoAimAssist() {
      RaycastHit info;
      var r = new Ray(_thrownPosition, _targetDirection);
      if (Physics.SphereCast(r, AssistRadius, out info)) {
        var t = info.transform;
        var target = t.gameObject.GetComponentInChildren<ThrowTarget>();
        if (target != null) {
          _transformTarget = target.transform;
          _targetDirection = _transformTarget.position - _thrownPosition;
        }
      }
    }

    private float _lifetime;
    private Transform _transformTarget;
    private Vector3 _thrownPosition;
    private Vector3 _targetDirection;
    private bool _thrown;
  }
}

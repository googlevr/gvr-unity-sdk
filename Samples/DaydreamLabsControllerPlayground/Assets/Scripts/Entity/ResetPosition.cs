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
  /// A resetable object that will return a gameobject transform
  /// to its position at Awake.
  /// </summary>
  /// <seealso cref="UnityEngine.MonoBehaviour" />
  /// <seealso cref="GVR.Entity.IResetable" />
  public class ResetPosition : MonoBehaviour, IResetable {
    void Awake() {
      _startPosition = transform.position;
      _startRotation = transform.rotation;
    }

    /// <summary>
    /// Resets the object.
    /// </summary>
    /// <param name="parent">Original parent transform</param>
    public virtual void ResetObject(Transform parent) {
      transform.SetParent(parent);
      transform.position = _startPosition;
      transform.rotation = _startRotation;
    }

    private Vector3 _startPosition;
    private Quaternion _startRotation;
  }
}

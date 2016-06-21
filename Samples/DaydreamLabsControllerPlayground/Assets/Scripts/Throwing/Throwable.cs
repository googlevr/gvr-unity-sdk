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
  /// Object that can be thrown by a throw controller.
  /// </summary>
  public class Throwable : MonoBehaviour {
    [Tooltip("Length from the player to the held object (approximate)")]
    public float ArmLength = 0.5f;

    [Tooltip("If true, object is thrown from where its located when released, otherwise throw " +
             "from player-center")]
    public bool ThrowFromReleasePoint;

    /// <summary>
    /// Gets true if the object can be caught
    /// </summary>
    public bool CanCatch {
      get;
      private set;
    }

    /// <summary>
    /// End the throw path, destroying the thrown object.
    /// </summary>
    public void CompleteThrow() {
      Destroy(gameObject);
    }

    /// <summary>
    /// Object is far enough away that it may be caught.
    /// </summary>
    public void OnCanCatch() {
      CanCatch = true;
    }

    /// <summary>
    /// Throws the object.
    /// </summary>
    /// <param name="thrower">
    /// Transform of the thrower, used to determine start of the throw.
    /// </param>
    /// <param name="isRightHanded">True: object thrown right handed</param>
    public virtual void Throw(Transform thrower, bool isRightHanded) {
      Drop(thrower);
    }

    /// <summary>
    /// Picks up up the throwable
    /// </summary>
    public virtual void PickUp(bool isRightHanded) {
      CanCatch = false;
    }

    private void Drop(Transform thrower) {
      transform.SetParent(null, true);
      if (!ThrowFromReleasePoint) {
        transform.position = thrower.position + new Vector3(0f, 0f, ArmLength);
        transform.rotation = thrower.rotation;
      }
    }
  }
}

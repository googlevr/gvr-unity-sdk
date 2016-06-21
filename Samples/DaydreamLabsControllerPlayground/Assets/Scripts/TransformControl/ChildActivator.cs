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

namespace GVR.TransformControl {
  /// <summary>
  /// Controls the gameobject active state of all child game objects.
  /// This script will not activate/deactivate the object it is attached to.
  /// </summary>
  /// <seealso cref="UnityEngine.MonoBehaviour" />
  public class ChildActivator : MonoBehaviour {
    /// <summary>
    /// Activates all child game objects.
    /// </summary>
    public void ActivateChildren() {
      for (int i = 0; i < transform.childCount; i++) {
        var child = transform.GetChild(i);
        child.gameObject.SetActive(true);
      }
    }

    /// <summary>
    /// Deactivate all child game objects.
    /// </summary>
    public void DeactivateChildren() {
      for (int i = 0; i < transform.childCount; i++) {
        var child = transform.GetChild(i);
        child.gameObject.SetActive(false);
      }
    }
  }
}

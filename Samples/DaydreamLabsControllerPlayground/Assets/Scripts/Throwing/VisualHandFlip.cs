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
using UnityEngine;

namespace GVR.Throwing {
  /// <summary>
  /// Locally rotate a transform based on which hand is in use.
  /// </summary>
  public class VisualHandFlip : MonoBehaviour {
    [Tooltip("Local rotation if object is held in the left hand")]
    public Vector3 LeftHand = new Vector3 { y = 60f, z = 180f };

    [Tooltip("Local rotation if object is held in the right hand")]
    public Vector3 RightHand = new Vector3 { y = -60f };

    void Start() {
      Flip(HandednessListener.IsRightHanded);
    }

    /// <summary>
    /// Flips the object.
    /// </summary>
    /// <param name="isRightHanded">
    /// If true, flip the object to the right hand
    /// </param>
    public void Flip(bool isRightHanded) {
      transform.localEulerAngles = isRightHanded ? RightHand : LeftHand;
    }
  }
}

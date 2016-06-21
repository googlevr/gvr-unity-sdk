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

using GVR.Utils;
using GVR.TransformControl;
using UnityEngine;

namespace GVR.Samples.Pancakes {
  /// <summary>
  ///  This component is in charge of dispensing pancakes.
  /// </summary>
  public class PancakeDispenser : MonoBehaviour {
    [Tooltip("Reference to the HMD prefab")]
    public Transform PlayerTransform;

    [Tooltip("Reference to the pancake object pool")]
    public ObjectPool PancakePool;

    [Tooltip("Reference to the pan's circle constraint")]
    public SignedCircleConstraint PanConstraints;

    public void Dispense() {
      GameObject pancake = PancakePool.GetFreeObject();
      if (pancake == null) {
        return;
      }
      pancake.transform.position = transform.position;
      pancake.transform.rotation = transform.rotation;
      pancake.GetComponent<PancakeLimiter>().PlayerRoot = PlayerTransform;
      PanConstraints.LockedObjects.Add(pancake.transform);
    }
  }
}

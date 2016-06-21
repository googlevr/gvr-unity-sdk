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

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Basic Orient constraint system for synchronizing one or more local Euler Axis between two
  /// Transforms on late update.
  /// </summary>
  public class OrientConstraint : MonoBehaviour {
    [Tooltip("The Driving transform whose local Euler Angles will be applied to the " +
             "Driven Transform in late update.")]
    public Transform Driver;

    [Tooltip("The Driven transform whose local Euler angles will be overwritten on late update " +
             "by the Driving Transform's.")]
    public Transform Driven;

    [Tooltip("Synchronize the local Euler X axis.")]
    public bool OrientX = false;

    [Tooltip("Synchronize the local Euler Y axis.")]
    public bool OrientY = true;

    [Tooltip("Synchronize the local Euler Z axis.")]
    public bool OrientZ = false;

    void LateUpdate() {
      Vector3 newEuler = Driven.transform.localEulerAngles;
      if (OrientX) {
        newEuler.x = Driver.transform.localEulerAngles.x;
      }
      if (OrientY) {
        newEuler.y = Driver.transform.localEulerAngles.y;
      }
      if (OrientZ) {
        newEuler.z = Driver.transform.localEulerAngles.z;
      }
      Driven.transform.localEulerAngles = newEuler;
    }
  }
}

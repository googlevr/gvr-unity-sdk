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
  /// Translates a Transform along a local-space Vector.
  /// </summary>
  public class LinearMotor : MonoBehaviour {
    [Tooltip("If true, the motor will be activated when the scene starts. " +
             "Otherwise the Activate method must be called.")]
    public bool ActiveByDefault = false;

    [Tooltip("The vector in LOCAL space that the motor will move along.")]
    public Vector3 Direction = Vector3.forward;

    [Tooltip("How many units per second will the Transform move?")]
    public float RatePerSecond = 2f;

    private bool isActive = false;

    void Start() {
      if (ActiveByDefault) {
        Activate();
      }
    }

    void Update() {
      if (isActive) {
        transform.localPosition += Direction.normalized * RatePerSecond * Time.deltaTime;
      }
    }

    public void Activate() {
      isActive = true;
    }

    public void Deactivate() {
      isActive = false;
    }

  }
}

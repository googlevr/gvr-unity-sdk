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

namespace GVR.Visual {
  /// <summary>
  /// Matches a gameobject's Y-axis rotation (world space) with
  /// the controller y axis orientation.
  /// </summary>
  public class RemoteOrbit : MonoBehaviour {
    void Update() {
      float newY = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                         GvrController.Orientation.eulerAngles.y,
                                         ref _velocity, 0.1f);
      transform.eulerAngles = newY * Vector3.up;
    }

    private float _velocity;
  }
}

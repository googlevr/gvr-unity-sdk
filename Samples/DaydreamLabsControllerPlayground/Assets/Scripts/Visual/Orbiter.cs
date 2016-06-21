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
  /// Matches the gameobject's rotation with the VR head object.
  /// An option is provided to also match head tilt
  /// </summary>
  public class Orbiter : MonoBehaviour {
    [Tooltip("Camera head object.")]
    public GameObject Head;

    [Tooltip("Time(secs) it takes the boom to catch up with the player's head.")]
    public float CatchupTime = 0.5f;

    [Tooltip("If true, the object will snap with the head without any smoothing")]
    public bool Snap;

    public bool MatchHeadTilt;

    public bool UseLocalRotation;

    void LateUpdate() {
      if (Head == null) {
        return;
      }
      float x = MatchHeadTilt ? Head.transform.eulerAngles.x : 0;
      float y = Snap ? Head.transform.eulerAngles.y
                     : Mathf.SmoothDampAngle(transform.eulerAngles.y, Head.transform.eulerAngles.y,
                                             ref _velocity, CatchupTime);
      if (UseLocalRotation) {
        Vector3 e = transform.eulerAngles;
        e.y = y;
        transform.localRotation = Quaternion.Euler(e);
      } else {
        transform.eulerAngles = new Vector3(x, y, 0f);
      }
    }

    private float _velocity;
  }
}

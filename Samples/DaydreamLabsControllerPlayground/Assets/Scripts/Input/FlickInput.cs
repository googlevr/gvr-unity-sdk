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

namespace GVR.Input {
  /// <summary>
  /// Detect controller flicks along the horizontal or vertical axes.
  /// </summary>
  public class FlickInput : MonoBehaviour {
    [Tooltip("A forward flick is back to front assuming a right-handed player")]
    public Vector3Event OnFlickForward;

    [Tooltip("A forward flick is front to back assuming a right-handed player")]
    public Vector3Event OnFlickBackward;

    [Tooltip("If true, flick events will fire with the app button")]
    public bool EzFlick = true;

    [Tooltip("How easy is it to trigger a flick (1 = hard)")]
    [Range(0, 1)]
    public float FlickSensitivity = 1;

    [Tooltip("If true, horizontal flicks are detected")]
    public bool Horizontal = true;

    [Tooltip("If true, vertical flicks are detected")]
    public bool Vertical;

    void Update() {
      if (EzFlick && GvrController.AppButton) {
        OnFlickForward.Invoke(Vector3.zero);
        OnFlickBackward.Invoke(Vector3.zero);
      }
      if (Horizontal) {
        if (GvrController.Gyro.y > FlickCheck) {
          OnFlickForward.Invoke(GvrController.Orientation.eulerAngles);
        }
        if (GvrController.Gyro.y < -FlickCheck) {
          OnFlickBackward.Invoke(GvrController.Orientation.eulerAngles);
        }
      }
      if (Vertical) {
        if (GvrController.Gyro.x > FlickCheck) {
          OnFlickForward.Invoke(GvrController.Orientation.eulerAngles);
        }
        if (GvrController.Gyro.x < -FlickCheck) {
          OnFlickBackward.Invoke(GvrController.Orientation.eulerAngles);
        }
      }
    }

    private float FlickCheck {
      get {
        return Mathf.Lerp(FlickWeak, FlickStrong, FlickSensitivity);
      }
    }

    private const float FlickStrong = 20f;
    private const float FlickWeak = 10f;
  }
}

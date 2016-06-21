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

namespace GVR.Samples.Pottery {
  public class Rotator : MonoBehaviour {
    public float Rate;
    public Transform Camera;
    public float PositionMultiplier;

    void Update() {
      transform.Rotate(new Vector3(0.0f, Rate * Time.deltaTime, 0.0f));
      if (Camera != null) {
        float degrees = transform.eulerAngles.y;
        if (degrees > 180) {
          degrees = degrees - 360;
        }
        Camera.position = new Vector3(Camera.position.x, Camera.position.y, degrees * PositionMultiplier);
      }
    }
  }
}

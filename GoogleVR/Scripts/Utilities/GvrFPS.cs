// Copyright 2015 Google Inc. All rights reserved.
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
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GvrFPS : MonoBehaviour {
  private const string DISPLAY_TEXT_FORMAT = "{0} msf\n({1} FPS)";
  private const string MSF_FORMAT = "#.#";
  private const float MS_PER_SEC = 1000f;

  private Text textField;
  private float fps = 60;

  public Camera cam;

  void Awake() {
    textField = GetComponent<Text>();
  }

  void Start() {
    if (cam == null) {
       cam = Camera.main;
    }

    if (cam != null) {
      // Tie this to the camera, and do not keep the local orientation.
      transform.SetParent(cam.GetComponent<Transform>(), true);
    }
  }

  void LateUpdate() {
    float interp = Time.deltaTime / (0.5f + Time.deltaTime);
    float currentFPS = 1.0f / Time.deltaTime;
    fps = Mathf.Lerp(fps, currentFPS, interp);
    float msf = MS_PER_SEC / fps;
    textField.text = string.Format(DISPLAY_TEXT_FORMAT,
        msf.ToString(MSF_FORMAT), Mathf.RoundToInt(fps));
  }
}

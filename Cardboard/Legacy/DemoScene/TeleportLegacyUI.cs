// Copyright 2014 Google Inc. All rights reserved.
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
using System.Collections;

public class TeleportLegacyUI : Teleport {
  private CardboardHead head;

  void Awake() {
    head = Camera.main.GetComponent<StereoController>().Head;
    CardboardOnGUI.IsGUIVisible = true;
    CardboardOnGUI.onGUICallback += this.OnGUI;
  }

  void Update() {
    RaycastHit hit;
    bool isLookedAt = GetComponent<Collider>().Raycast(head.Gaze, out hit, Mathf.Infinity);
    SetGazedAt(isLookedAt);
    if (Cardboard.SDK.Triggered && isLookedAt) {
      TeleportRandomly();
    }
  }

  void OnGUI() {
    if (!CardboardOnGUI.OKToDraw(this)) {
      return;
    }
    if (GUI.Button(new Rect(50, 50, 200, 50), "Reset")) {
      Reset();
    }
    if (GUI.Button(new Rect(50, 110, 200, 50), "Recenter")) {
      Cardboard.SDK.Recenter();
    }
    if (GUI.Button(new Rect(50, 170, 200, 50), "VR Mode")) {
      ToggleVRMode();
    }
  }

  void OnDestroy() {
    CardboardOnGUI.onGUICallback -= this.OnGUI;
  }
}

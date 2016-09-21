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

[RequireComponent(typeof(Collider))]
public class Teleport : MonoBehaviour, IGvrGazeResponder {
  private Vector3 startingPosition;

  void Start() {
    startingPosition = transform.localPosition;
    SetGazedAt(false);
  }

  void LateUpdate() {
    GvrViewer.Instance.UpdateState();
    if (GvrViewer.Instance.BackButtonPressed) {
      Application.Quit();
    }
  }

  public void SetGazedAt(bool gazedAt) {
    GetComponent<Renderer>().material.color = gazedAt ? Color.green : Color.red;
  }

  public void Reset() {
    transform.localPosition = startingPosition;
  }

  public void ToggleVRMode() {
    GvrViewer.Instance.VRModeEnabled = !GvrViewer.Instance.VRModeEnabled;
  }

  public void ToggleDistortionCorrection() {
    GvrViewer.Instance.DistortionCorrectionEnabled =
      !GvrViewer.Instance.DistortionCorrectionEnabled;
  }

#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
  public void ToggleDirectRender() {
    GvrViewer.Controller.directRender = !GvrViewer.Controller.directRender;
  }
#endif  //  !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

  public void TeleportRandomly() {
    Vector3 direction = Random.onUnitSphere;
    direction.y = Mathf.Clamp(direction.y, 0.5f, 1f);
    float distance = 2 * Random.value + 1.5f;
    transform.localPosition = direction * distance;
  }

  #region IGvrGazeResponder implementation

  /// Called when the user is looking on a GameObject with this script,
  /// as long as it is set to an appropriate layer (see GvrGaze).
  public void OnGazeEnter() {
    SetGazedAt(true);
  }

  /// Called when the user stops looking on the GameObject, after OnGazeEnter
  /// was already called.
  public void OnGazeExit() {
    SetGazedAt(false);
  }

  /// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
  public void OnGazeTrigger() {
    TeleportRandomly();
  }

  #endregion
}

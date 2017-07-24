// Copyright 2017 Google Inc. All rights reserved.
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

public abstract class GvrBaseControllerVisual : MonoBehaviour, IGvrArmModelReceiver {

  /// This is the preferred, maximum alpha value the object should have
  /// when it is a comfortable distance from the head.
  [Range(0.0f, 1.0f)]
  public float maximumAlpha = 1.0f;

  public GvrBaseArmModel ArmModel { get; set; }

  public float PreferredAlpha{
    get{
      return ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;
    }
  }

  /// Amount of normalized alpha transparency to change per second.
  private const float DELTA_ALPHA = 4.0f;

  protected virtual void Awake() {

  }

  void OnEnable() {
    GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
  }

  void OnDisable() {
    GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
  }

  /// Override this method to update materials and other visual changes
  /// that need to happen every frame.
  public abstract void OnVisualUpdate(bool updateImmediately = false);

  private void OnPostControllerInputUpdated() {
    OnVisualUpdate();
  }

}

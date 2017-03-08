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
using UnityEngine.EventSystems;
using System.Collections;

/// Base implementation of IGvrPointer
///
/// Automatically registers pointer with GvrPointerManager.
/// Uses transform that this script is attached to as the pointer transform.
///
public abstract class GvrBasePointer : MonoBehaviour, IGvrPointer {

  protected virtual void Start() {
    GvrPointerManager.OnPointerCreated(this);
  }

  public bool ShouldUseExitRadiusForRaycast {
    get;
    set;
  }

  /// Declare methods from IGvrPointer
  public abstract void OnInputModuleEnabled();

  public abstract void OnInputModuleDisabled();

  public abstract void OnPointerEnter(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive);

  public abstract void OnPointerHover(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive);

  public abstract void OnPointerExit(GameObject targetObject);

  public abstract void OnPointerClickDown();

  public abstract void OnPointerClickUp();

  public abstract float GetMaxPointerDistance();

  public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);

  public virtual Transform GetPointerTransform() {
    return transform;
  }
}

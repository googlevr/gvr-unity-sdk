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
using System.Collections.Generic;

/// This script provides shared functionality used by all Gvr raycasters.
public abstract class GvrBasePointerRaycaster : BaseRaycaster {
  private GvrBasePointer.PointerRay lastRay;

  protected GvrBasePointer.RaycastMode CurrentRaycastModeForHybrid { get; private set; }

  protected GvrBasePointerRaycaster() {
  }

  public GvrBasePointer.PointerRay GetLastRay() {
    return lastRay;
  }

  public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
    GvrBasePointer pointer = GvrPointerInputModule.Pointer;
    if (pointer == null || !pointer.IsAvailable) {
      return;
    }

    if (pointer.raycastMode == GvrBasePointer.RaycastMode.Hybrid) {
      RaycastHybrid(pointer, eventData, resultAppendList);
    } else {
      RaycastDefault(pointer, eventData, resultAppendList);
    }
  }

  protected abstract bool PerformRaycast(GvrBasePointer.PointerRay pointerRay, float radius,
    PointerEventData eventData, List<RaycastResult> resultAppendList);

  private void RaycastHybrid(GvrBasePointer pointer, PointerEventData eventData, List<RaycastResult> resultAppendList) {
    CurrentRaycastModeForHybrid = GvrBasePointer.RaycastMode.Direct;
    lastRay = GvrBasePointer.CalculateHybridRay(pointer, CurrentRaycastModeForHybrid);
    float radius = pointer.CurrentPointerRadius;
    bool foundHit = PerformRaycast(lastRay, radius, eventData, resultAppendList);

    if (!foundHit) {
      CurrentRaycastModeForHybrid = GvrBasePointer.RaycastMode.Camera;
      lastRay = GvrBasePointer.CalculateHybridRay(pointer, CurrentRaycastModeForHybrid);
      PerformRaycast(lastRay, radius, eventData, resultAppendList);
    }
  }

  private void RaycastDefault(GvrBasePointer pointer, PointerEventData eventData, List<RaycastResult> resultAppendList) {
    lastRay = GvrBasePointer.CalculateRay(pointer, pointer.raycastMode);
    float radius = pointer.CurrentPointerRadius;
    PerformRaycast(lastRay, radius, eventData, resultAppendList);
  }
}

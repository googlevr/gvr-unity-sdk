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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// Interface for manipulating an InputModule used by _GvrPointerInputModuleImpl_
public interface IGvrInputModuleController {
  EventSystem eventSystem { get; }
  List<RaycastResult> RaycastResultCache { get; }

  bool ShouldActivate();
  void Deactivate();
  GameObject FindCommonRoot(GameObject g1, GameObject g2);
  BaseEventData GetBaseEventData();
  RaycastResult FindFirstRaycast(List<RaycastResult> candidates);
}

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
using System.Collections;
using UnityEngine.EventSystems;

public class TerrainBehaviour : MonoBehaviour {
  public void OnClick(BaseEventData data){
    if (CannonBehaviour.player_ != null) {
      PointerEventData ped = (PointerEventData)data;
      Vector3 target_pos = ped.pointerCurrentRaycast.worldPosition;
      target_pos.y += 0.25f;
      CannonBehaviour.player_.FireAtTarget(target_pos);
    }
  }
}

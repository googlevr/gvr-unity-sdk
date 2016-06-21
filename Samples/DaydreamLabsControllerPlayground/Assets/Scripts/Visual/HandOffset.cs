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

using GVR.Events;
using UnityEngine;

namespace GVR.Visual {
  public class HandOffset : MonoBehaviour {
    public float XAxisOffset = 0.09f;

    void Start() {
      if (HandednessListener.IsLeftHanded) {
        LeftHanded();
      } else {
        RightHanded();
      }
    }

    // May be called by an event, such as from HandednessListener
    public void RightHanded() {
      MoveTransform(XAxisOffset);
    }

    // May be called by an event, such as from HandednessListener
    public void LeftHanded() {
      MoveTransform(-XAxisOffset);
    }

    private void MoveTransform(float x) {
      Vector3 pos = transform.localPosition;
      pos.x = x;
      transform.localPosition = pos;
    }
  }
}

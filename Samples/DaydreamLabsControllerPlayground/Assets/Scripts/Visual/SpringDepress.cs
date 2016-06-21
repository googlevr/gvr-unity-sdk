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

using System;
using UnityEngine;

namespace GVR.Visual {
  public class SpringDepress : MonoBehaviour {
    public float Distance = 0.5f;

    void Start() {
      _top = LocalY;
      _bottom = LocalY - Distance;
      _next = _bottom;
    }

    void Update() {
      LocalY = Mathf.SmoothDamp(LocalY, _next, ref _velocity, 0.05f);
      float diff = Math.Abs(LocalY - _bottom);
      if (diff <= 0.01) {
        _next = _top;
      }
    }

    public void Depress() {
      _next = _bottom;
    }

    private float LocalY {
      get { return transform.localPosition.y; }
      set {
        Vector3 pos = transform.localPosition;
        pos.y = value;
        transform.localPosition = pos;
      }
    }

    private float _velocity;
    private float _next;
    private float _top;
    private float _bottom;
  }
}

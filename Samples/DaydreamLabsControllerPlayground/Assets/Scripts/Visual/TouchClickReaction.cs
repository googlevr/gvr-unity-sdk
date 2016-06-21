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

namespace GVR.Visual {
  public class TouchClickReaction : MonoBehaviour {
    public Color Touching = Color.red;
    public MeshRenderer Renderer;
    public bool UseTouchInstead;

    void Start() {
      _mat = Renderer.materials[0];
      _default = _mat.color;
    }

    void Update() {
      bool react = UseTouchInstead ? GvrController.IsTouching : GvrController.ClickButton;
      _mat.color = react ? Touching : _default;
    }

    private Color _default;
    private Material _mat;
  }
}

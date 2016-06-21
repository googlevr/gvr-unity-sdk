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

namespace GVR.Gfx {
  public class RenderTextureCamera : MonoBehaviour {
    public Camera Camera;
    public Color ClearColor = Color.black;
    public float ClearDepth = 1.0f;

    void Start() {
      Reset();
    }

    public void Reset() {
      RenderTexture active = RenderTexture.active;
      RenderTexture.active = Camera.targetTexture;
      GL.Clear(true, true, ClearColor, ClearDepth);
      RenderTexture.active = active;
    }
  }
}

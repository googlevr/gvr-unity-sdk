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
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera), typeof(CustomProjector))]
  public class WorldSpaceProjectorBounds : MonoBehaviour {
    private Camera cam;
    private CustomProjector proj;

    [Tooltip("Size of the Projector's orthographic projection in world units.")]
    public Vector2 size;

    void Awake() {
      cam = GetComponent<Camera>();
      proj = GetComponent<CustomProjector>();
    }

    void OnEnable() {
      cam.orthographic = true;
    }

    void OnDisable() {
    }

    void Update() {
      cam.orthographicSize = size.y * .5f;
      proj.aspectRatio = size.x / size.y;
    }
  }
}

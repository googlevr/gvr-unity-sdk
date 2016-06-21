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

using System.Collections.Generic;
using UnityEngine;

namespace GVR.Visual {
  public class MaterialCycle : MonoBehaviour {
    public TrailRenderer Trail;
    public MeshRenderer Renderer;
    public Material[] Materials;
    public int MaterialIndex = 2;

    public void Set(int index) {
      if (Renderer == null || Materials.Length == 0) {
        return;
      }
      int next = index % Materials.Length;
      Material nextMat = Materials[next];
      var newMaterials = new List<Material>(Renderer.materials);
      newMaterials[MaterialIndex] = nextMat;
      Renderer.materials = newMaterials.ToArray();
      Trail.material = nextMat;
    }
  }
}

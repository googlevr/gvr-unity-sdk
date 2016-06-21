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
  [RequireComponent(typeof(Camera))]
  public class CustomProjector : MonoBehaviour {
    public float aspectRatio = 1f;

    private int id_matrix;
    private int id_direction;
    private Matrix4x4 mat;
    private Camera cam;

    void Awake() {
      DoValidation();
    }

    void Reset() {
      DoValidation();
    }

    void OnValidate() {
      if (cam == null) {
        cam = GetComponent<Camera>();
      }
    }

    void DoValidation() {
      if (cam == null) {
        cam = GetComponent<Camera>();
      }
      id_matrix = Shader.PropertyToID(ShaderLib.Variables.MATRIX_CUSTOM_PROJECTOR);
      id_direction = Shader.PropertyToID(ShaderLib.Variables.VECTOR_CUSTOM_PROJECTOR_WORLD_DIR);
    }

    void OnEnable() {
      DoValidation();
    }

    void LateUpdate() {
      cam.aspect = aspectRatio;
      mat = cam.projectionMatrix * cam.worldToCameraMatrix;
      //mat = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;
      Shader.SetGlobalMatrix(id_matrix, mat);
      Vector3 dir = transform.forward;
      Shader.SetGlobalVector(id_direction, new Vector4(dir.x, dir.y, dir.z, 0f));
    }
  }
}

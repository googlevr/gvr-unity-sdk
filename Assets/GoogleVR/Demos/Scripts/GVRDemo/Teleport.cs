// Copyright 2014 Google Inc. All rights reserved.
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

namespace GoogleVR.GVRDemo {
  using UnityEngine;

  [RequireComponent(typeof(Collider))]
  public class Teleport : MonoBehaviour {
    private Vector3 startingPosition;

    public Material inactiveMaterial;
    public Material gazedAtMaterial;

    void Start() {
      startingPosition = transform.localPosition;
      SetGazedAt(false);
    }

    public void SetGazedAt(bool gazedAt) {
      if (inactiveMaterial != null && gazedAtMaterial != null) {
        GetComponent<Renderer>().material = gazedAt ? gazedAtMaterial : inactiveMaterial;
        return;
      }
      GetComponent<Renderer>().material.color = gazedAt ? Color.green : Color.red;
    }

    public void Reset() {
      transform.localPosition = startingPosition;
    }

    public void Recenter() {
#if !UNITY_EDITOR
      GvrCardboardHelpers.Recenter();
#else
      if (GvrEditorEmulator.Instance != null) {
        GvrEditorEmulator.Instance.Recenter();
      }
#endif  // !UNITY_EDITOR
    }

    public void TeleportRandomly() {
      Vector3 direction = Random.onUnitSphere;
      direction.y = Mathf.Clamp(direction.y, 0.5f, 1f);
      float distance = 2 * Random.value + 1.5f;
      transform.localPosition = direction * distance;
    }
  }
}

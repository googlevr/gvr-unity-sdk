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

namespace GVR.GUI {
  public class SelectableScene : ScriptableObject {
#if UNITY_EDITOR
    public void ApplyID() {
      string path = UnityEditor.AssetDatabase.GetAssetPath(Scene);
      id = path;
    }

    private void OnValidate() {
      ApplyID();
    }
#endif

    [Tooltip("User-facing name for the scene.")]
    public string Name;

    [SerializeField]
    [Tooltip("String ID used to load the scene, the same as the filename.")]
    private string id;
    public string ID { get { return id; } }

    public Object Scene;
    public Sprite Icon;
    public Color NormalColor;
    public Color HighlighedColor;
    public Color PressedColor;
    public Color ActiveColor;
  }
}

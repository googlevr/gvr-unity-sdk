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
  /// <summary>
  /// When put on a camera game object, this can fade into and out of the
  /// supplied color. If the camera object has any sub-cameras (as is
  /// common in VR setups) this will copy itself on to the sub-objects and
  /// keep them in sync.
  /// </summary>
  [RequireComponent(typeof(Camera))]
  public class ScreenFade : MonoBehaviour {
    [Tooltip("The material to use in the fader.")]
    public Material Material;

    [Tooltip("Color to fade to. Alpha is ignored since it will be done internally.")]
    public Color Color = Color.black;

    [Tooltip("The time that this takes to fade to a color or to clear.")]
    public float FadeTime = 0.5f;

    [Tooltip("Amount of time to wait before fading in the first time to allow for smoother scene transitions.")]
    public float FadeToClearDelay = 0.5f;

    private bool isSubordinate;
    private ScreenFade[] subordinates = new ScreenFade[0];
    private float delayRemaining;
    private int direction = -1;

    void Start() {
      delayRemaining = FadeToClearDelay;
      Camera[] cameras = GetComponentsInChildren<Camera>();
      subordinates = new ScreenFade[cameras.Length - 1];
      int filled = 0;
      for (int i = 0; i < cameras.Length; i++) {
        if (cameras[i].gameObject != gameObject) {
          subordinates[filled] = cameras[i].gameObject.AddComponent<ScreenFade>();
          subordinates[filled].Material = Material;
          subordinates[filled].Color = Color;
          subordinates[filled].isSubordinate = true;
          filled++;
        }
      }
    }

    void Update() {
      if (isSubordinate) {
        return;
      }
      if (direction < 0) {
        delayRemaining -= Time.unscaledDeltaTime;
        if (delayRemaining <= 0.0f) {
          Color.a = Mathf.Max(0.0f, Color.a - (Time.unscaledDeltaTime / FadeTime));
        }
      } else if (direction > 0) {
        Color.a = Mathf.Min(1.0f, Color.a + (Time.unscaledDeltaTime / FadeTime));
      }
      for (int i = 0; i < subordinates.Length; i++) {
        subordinates[i].Color = Color;
        subordinates[i].direction = direction;
      }
    }

    void OnPostRender() {
      GL.PushMatrix();
      GL.LoadOrtho();
      Material.SetPass(0);
      GL.Begin(GL.QUADS);
      GL.Color(Color);
      GL.Vertex3(0, 0, 0);
      GL.Vertex3(1, 0, 0);
      GL.Vertex3(1, 1, 0);
      GL.Vertex3(0, 1, 0);
      GL.End();
      GL.PopMatrix();
    }

    public void FadeToClear() {
      direction = -1;
    }

    public void FadeToColor() {
      direction = 1;
    }
  }
}

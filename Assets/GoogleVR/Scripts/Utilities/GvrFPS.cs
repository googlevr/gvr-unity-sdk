// Copyright 2015 Google Inc. All rights reserved.
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
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GvrFPS : MonoBehaviour {
  private const string DISPLAY_TEXT_FORMAT = "{0:0.0} msf\n({1:0} FPS)";
  private const float MS_PER_SEC = 1000f;

  private const float UI_LABEL_START_X = 150.0f;
  private const float UI_LABEL_START_Y = 300.0f;
  private const float UI_LABEL_SIZE_X = 200.0f;
  private const float UI_LABEL_SIZE_Y = 150.0f;

  private GUIStyle guiLabelStyle;
  private Rect guiRectLeft =
    new Rect(UI_LABEL_START_X, Screen.height - UI_LABEL_START_Y, UI_LABEL_SIZE_X, UI_LABEL_SIZE_Y);
#if !UNITY_EDITOR
  private Rect guiRectRight = new Rect(Screen.width / 2 + UI_LABEL_START_X,
      Screen.height - UI_LABEL_START_Y, UI_LABEL_SIZE_X, UI_LABEL_SIZE_Y);
#endif  // !UNITY_EDITOR

  private string fpsText;
  private float fps = 60;

  public Color textColor = Color.white;

  void LateUpdate() {
    float deltaTime = Time.unscaledDeltaTime;
    float interp = deltaTime / (0.5f + deltaTime);
    float currentFPS = 1.0f / deltaTime;
    fps = Mathf.Lerp(fps, currentFPS, interp);
    float msf = MS_PER_SEC / fps;
    fpsText = string.Format(DISPLAY_TEXT_FORMAT, msf, fps);
  }

  void OnGUI() {
    if (guiLabelStyle == null) {
      guiLabelStyle = new GUIStyle(GUI.skin.label);
      guiLabelStyle.richText = false;
      guiLabelStyle.fontSize = 32;
    }

    // Draw FPS text.
    GUI.color = textColor;
    GUI.Label(guiRectLeft, fpsText, guiLabelStyle);
#if !UNITY_EDITOR
    GUI.Label(guiRectRight, fpsText, guiLabelStyle);
#endif  // !UNITY_EDITOR
  }
}

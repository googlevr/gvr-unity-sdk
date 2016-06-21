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
#if !UNITY_5_2
using UnityEngine.SceneManagement;
#endif
using UnityEngine.UI;

namespace GVR.GUI {
  /// <summary>
  /// A script with functionality for a level selection button. This supports
  /// different buttons for when the level is the current scene or not.
  /// </summary>
  public class LevelSelectButton : MonoBehaviour {
#if UNITY_EDITOR
    private void OnValidate() {
      if (sceneData != null) {
        SetSceneData(sceneData);
        gameObject.name = sceneData.name;
      }
    }
#endif
    [Tooltip("Button to display when clicking will load a different scene.")]
    public Button Button;

    [Tooltip("Icon that is swapped out for this button based on the scene.")]
    public Image Icon;

    [Tooltip("Function to call, with the active scene data, when the button is pressed.")]
    public LevelSelectMenu.SceneDataEvent OnClick;

    /// <summary> The scene data that a click will launch. </summary>
    public SelectableScene sceneData;

    void Start() {
      Button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked() {
      OnClick.Invoke(sceneData);
    }

    public void SetSceneData(SelectableScene sd) {
      sceneData = sd;
      Text t = Button.GetComponentInChildren<Text>();
      if (t != null) {
        t.text = sceneData.Name;
      }
      //Button.onClick.AddListener(OnButtonClicked);

#if UNITY_5_2
      string sceneName = Application.loadedLevelName;
#else
      string sceneName = SceneManager.GetActiveScene().name;
#endif

      bool current = System.IO.Path.GetFileNameWithoutExtension(sceneName) ==
                     System.IO.Path.GetFileNameWithoutExtension(sceneData.ID);
      Icon.sprite = sceneData.Icon;
      Button.colors = new ColorBlock() {
        normalColor = current? sd.ActiveColor : sd.NormalColor,
        highlightedColor = sd.HighlighedColor,
        pressedColor = sd.PressedColor,
        disabledColor = sd.ActiveColor,
        colorMultiplier = 1.0f,
        fadeDuration = 0.1f
      };
    }
  }
}

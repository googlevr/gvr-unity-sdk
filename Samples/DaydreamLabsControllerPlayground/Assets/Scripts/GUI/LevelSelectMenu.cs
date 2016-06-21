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

using System;
using System.Collections;
using GVR.Events;
using GVR.Visual;
using UnityEngine;
using UnityEngine.Events;
#if !UNITY_5_2
using UnityEngine.SceneManagement;
#endif

namespace GVR.GUI {
  /// <summary>
  /// Script for a GUI menu that provides a list of buttons for loading
  /// different scenes.
  /// Singleton pattern.
  /// </summary>
  public class LevelSelectMenu : MonoBehaviour {
    [Serializable]
    public class SceneDataEvent : UnityEvent<SelectableScene> { }

    [Tooltip("The button setup to use for all of the buttons in the list. " +
             "This will be cloned for each level button.")]
    public LevelSelectButton TemplateButton;

    [Tooltip("Transform to place all buttons under.")]
    public Transform ButtonRoot;

    [Tooltip("Should the destination scene be loaded asyncrounously, taking more overall time, " +
             "or syncronously, blocking but for shorter time.")]
    public bool UseAsyncLoading;

    [Tooltip("ScreenFade to be used for level transitions.")]
    public ScreenFade ScreenFade;

    [Tooltip("If true, the menu will tilt to look down or up at the player, if false, " +
             "it is always aligned on th Y axis.")]
    public bool AllowTilt;

    private bool loading;

    private Vector3 position;
    private Quaternion rotation;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private bool hashesCached;
    private int AnimatorOpenID;
    private Animator animator;

    private bool isMenuOpen;
    private static LevelSelectMenu instance;

    private void CacheHashes() {
      if (!hashesCached) {
        AnimatorOpenID = Animator.StringToHash("Open");
        animator = GetComponent<Animator>();
        hashesCached = true;
      }
    }

    #region -- Singleton instance property, constructor, and destructor. -------

    public static LevelSelectMenu Instance { 
      get { 
        return instance; 
      }
    }

    void Awake() {
      if (instance != null && instance != this) {
        Destroy(this.gameObject);
        return;
      } 
      instance = this;
      instance.isMenuOpen = true;
    }

    void OnDestroy() {
      if (this == instance) {
        instance = null;
      }
    }

    #endregion -- Singleton instance property, constructor, and destructor. ----

    void Start() {
      for (int i = 0; i < transform.childCount; i++) {
        transform.GetChild(i).gameObject.layer = 20;
        transform.GetChild(i).gameObject.SetActive(true);
      }
      originalLocalPosition = transform.localPosition;
      originalLocalRotation = transform.localRotation;
      CacheHashes();
    }

    #region -- Public properties. ----------------------------------

    public bool IsMenuOpen {
      get {
        return isMenuOpen;
      }
    }

    #endregion -- Public properties. -------------------------------

    public void TriggerWillClose() {
      LevelSelectMenuListener[] listeners = FindObjectsOfType<LevelSelectMenuListener>();
      for (int i = 0; i < listeners.Length; i++) {
        listeners[i].OnLevelSelectMenuWillClose.Invoke();
      }
    }

    public void TriggerOpened() {
      LevelSelectMenuListener[] listeners = FindObjectsOfType<LevelSelectMenuListener>();
      for (int i = 0; i < listeners.Length; i++) {
        listeners[i].OnLevelSelectMenuOpened.Invoke();
      }
      instance.isMenuOpen = true;
    }

    public void TriggerClosed() {
      LevelSelectMenuListener[] listeners = FindObjectsOfType<LevelSelectMenuListener>();
      for (int i = 0; i < listeners.Length; i++) {
        listeners[i].OnLevelSelectMenuClosed.Invoke();
      }
      instance.isMenuOpen = false;
    }

    public void Open() {
      CacheHashes();
      for (int i = 0; i < transform.childCount; i++) {
        transform.GetChild(i).gameObject.layer = 0;
      }
      enabled = true;
      TriggerOpened();
      animator.SetBool(AnimatorOpenID, true);
    }

    public void Close() {
      animator.SetBool(AnimatorOpenID, false);
      TriggerWillClose();
    }

    void OnEnable() {
      if (AllowTilt) {
        position = transform.position;
        transform.LookAt(transform.position + transform.forward, Vector3.up);
        rotation = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f));
      } else {
        Vector3 d = Vector3.ProjectOnPlane(transform.parent.parent.forward, Vector3.up).normalized;
        position = transform.parent.parent.position + (d * 3.0f);
        transform.forward = d.normalized;
        rotation = transform.rotation;
      }
    }

    void OnDisable() {
      transform.localPosition = originalLocalPosition;
      transform.localRotation = originalLocalRotation;
      TriggerClosed();
    }

    void Update() {
      transform.position = position;
      transform.rotation = rotation;
    }

    public void SetLefty(bool lefty) {
      HandednessListener.SetHandedness(lefty);
    }

    public void LoadLevel(SelectableScene scene) {
      if (UseAsyncLoading) {
        StartCoroutine(SceneLoadCoroutine(System.IO.Path.GetFileNameWithoutExtension(scene.ID)));
      } else {
#if UNITY_5_2
        Application.LoadLevel(System.IO.Path.GetFileNameWithoutExtension(scene.ID));
#else
        SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(scene.ID));
#endif
      }
    }

    private IEnumerator SceneLoadCoroutine(string id) {
      ScreenFade.FadeToColor();
      yield return new WaitForSeconds(ScreenFade.FadeTime);
      if (loading) {
        Debug.LogWarning("Unable to load scene while a scene is already loading");
        yield break;
      }
      loading = true;
#if UNITY_5_2
      AsyncOperation sceneLoad = Application.LoadLevelAsync(id);
#else
      AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(id);
#endif
      while (!sceneLoad.isDone) {
        yield return null;
      }
      loading = false;
    }

    public void CreateButtons(SelectableScene[] scenes) {
      TemplateButton.gameObject.SetActive(true);
      for (int i = ButtonRoot.childCount - 1; i >= 0; i--) {
        if (ButtonRoot.GetChild(i).gameObject != TemplateButton.gameObject) {
          Destroy(ButtonRoot.GetChild(i).gameObject);
        }
      }
      for (int i = 0; i < scenes.Length; i++) {
        GameObject go = Instantiate(TemplateButton.gameObject);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null) {
          rt.SetParent(ButtonRoot, false);
        }
        LevelSelectButton button = go.GetComponent<LevelSelectButton>();
        button.SetSceneData(scenes[i]);
      }
      TemplateButton.gameObject.SetActive(false);
    }
  }
}

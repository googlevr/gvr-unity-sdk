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
using System.Collections;

public class MenuHandler : MonoBehaviour {
  public GameObject[] menuObjects;

  public void HideMenu() {
    foreach (GameObject m in menuObjects) {
      Renderer r = m.GetComponent<Renderer>();
      if (r != null) {
        r.enabled = false;
      } else {
        m.SetActive(false);
      }
      StartCoroutine(DoFade());
    }
  }

  public void ShowMenu() {
    foreach (GameObject m in menuObjects) {
      Renderer r = m.GetComponent<Renderer>();
      if (r != null) {
        r.enabled = true;
      } else {
        m.SetActive(true);
      }
    }
    StartCoroutine(DoAppear());
  }

  IEnumerator DoAppear() {
    CanvasGroup cg = GetComponent<CanvasGroup>();
    while (cg.alpha < 1.0) {
      cg.alpha += Time.deltaTime * 2;
      yield return null;
    }
    cg.interactable = true;
    yield break;
  }

  IEnumerator DoFade() {
    CanvasGroup cg = GetComponent<CanvasGroup>();
    while (cg.alpha > 0) {
      cg.alpha -= Time.deltaTime;
      yield return null;
    }
    cg.interactable = false;
    yield break;
  }
}

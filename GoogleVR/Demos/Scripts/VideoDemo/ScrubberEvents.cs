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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrubberEvents : MonoBehaviour {
  private GameObject newPositionHandle;

  private Vector3[] corners;
  private Slider slider;

  private VideoControlsManager mgr;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  GvrPointerInputModule inp;
#endif

  public VideoControlsManager ControlManager
  {
    set
    {
      mgr = value;
    }
  }

  void Start() {
    foreach (Image im in GetComponentsInChildren<Image>(true)) {
      if (im.gameObject.name == "newPositionHandle") {
        newPositionHandle = im.gameObject;
        break;
      }
    }

    corners = new Vector3[4];
    GetComponent<Image>().rectTransform.GetWorldCorners(corners);
    slider = GetComponentInParent<Slider>();
  }

  void Update() {
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    if (inp != null && inp.transform.position != Vector3.zero) {
      newPositionHandle.transform.position = new Vector3(
          inp.transform.position.x,
          newPositionHandle.transform.position.y,
          newPositionHandle.transform.position.z);
    } else {
      newPositionHandle.transform.position = slider.handleRect.transform.position;
    }
#endif
  }

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  public void OnPointerEnter(BaseEventData data) {
    inp = data.currentInputModule as GvrPointerInputModule;
    if (inp != null && inp.transform.position != Vector3.zero) {
      newPositionHandle.transform.position = new Vector3(
          inp.transform.position.x,
          newPositionHandle.transform.position.y,
          newPositionHandle.transform.position.z);
    }
    newPositionHandle.SetActive(true);
  }

  public void OnPointerExit(BaseEventData data) {
    inp = null;
    newPositionHandle.SetActive(false);
  }

  public void OnPointerClick(BaseEventData data) {

    float minX = corners[0].x;
    float maxX = corners[3].x;

    float pct = (newPositionHandle.transform.position.x - minX) / (maxX - minX);

    if (mgr != null) {
      long p = (long)(slider.maxValue * pct);
      mgr.Player.CurrentPosition = p;
    }
  }

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

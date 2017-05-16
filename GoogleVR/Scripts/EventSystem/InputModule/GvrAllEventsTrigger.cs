// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the MIT License, you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//     http://www.opensource.org/licenses/mit-license.php
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// Exposes events from _GvrEventExecutor_ that are fired by _GvrPointerInputModule_ to the editor.
/// Makes it possible to handle EventSystem events globally.
public class GvrAllEventsTrigger : MonoBehaviour {

  [Serializable]
  public class TriggerEvent : UnityEvent<GameObject, PointerEventData>
  {}

  public TriggerEvent OnPointerClick;
  public TriggerEvent OnPointerDown;
  public TriggerEvent OnPointerUp;
  public TriggerEvent OnPointerEnter;
  public TriggerEvent OnPointerExit;

  void OnEnable() {
    // EventExecutor isn't available until after the first update.
    // So we must wait to add the listeners.
    StartCoroutine(AddListenersDelayed());
  }

  void OnDisable() {
    RemoveListeners();
  }

  private IEnumerator AddListenersDelayed() {
    yield return null;
    AddListeners();
  }

  private void AddListeners() {
    GvrEventExecutor eventExecutor = GvrPointerInputModule.FindEventExecutor();
    if (eventExecutor == null) {
      return;
    }

    eventExecutor.OnPointerClick += OnPointerClickHandler;
    eventExecutor.OnPointerDown += OnPointerDownHandler;
    eventExecutor.OnPointerUp += OnPointerUpHandler;
    eventExecutor.OnPointerEnter += OnPointerEnterHandler;
    eventExecutor.OnPointerExit += OnPointerExitHandler;
  }

  private void RemoveListeners() {
    GvrEventExecutor eventExecutor = GvrPointerInputModule.FindEventExecutor();
    if (eventExecutor == null) {
      return;
    }

    eventExecutor.OnPointerClick -= OnPointerClickHandler;
    eventExecutor.OnPointerDown -= OnPointerDownHandler;
    eventExecutor.OnPointerUp -= OnPointerUpHandler;
    eventExecutor.OnPointerEnter -= OnPointerEnterHandler;
    eventExecutor.OnPointerExit -= OnPointerExitHandler;
  }

  private void OnPointerClickHandler(GameObject target, PointerEventData eventData) {
    OnPointerClick.Invoke(target, eventData);
  }

  private void OnPointerDownHandler(GameObject target, PointerEventData eventData) {
    OnPointerDown.Invoke(target, eventData);
  }

  private void OnPointerUpHandler(GameObject target, PointerEventData eventData) {
    OnPointerUp.Invoke(target, eventData);
  }

  private void OnPointerEnterHandler(GameObject target, PointerEventData eventData) {
    OnPointerEnter.Invoke(target, eventData);
  }

  private void OnPointerExitHandler(GameObject target, PointerEventData eventData) {
    OnPointerExit.Invoke(target, eventData);
  }
}

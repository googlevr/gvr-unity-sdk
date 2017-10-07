// Copyright 2017 Google Inc. All rights reserved.
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
  public TriggerEvent OnScroll;

  private bool listenersAdded;

  void OnEnable() {
    AddListeners();
  }

  void OnDisable() {
    RemoveListeners();
  }

  void Start() {
    // The eventExecutor may not be available during OnEnable when the script is first created.
    AddListeners();
  }

  private void AddListeners() {
    GvrEventExecutor eventExecutor = GvrPointerInputModule.FindEventExecutor();
    if (eventExecutor == null) {
      return;
    }

    if (listenersAdded) {
      return;
    }

    eventExecutor.OnPointerClick += OnPointerClickHandler;
    eventExecutor.OnPointerDown += OnPointerDownHandler;
    eventExecutor.OnPointerUp += OnPointerUpHandler;
    eventExecutor.OnPointerEnter += OnPointerEnterHandler;
    eventExecutor.OnPointerExit += OnPointerExitHandler;
    eventExecutor.OnScroll += OnScrollHandler;

    listenersAdded = true;
  }

  private void RemoveListeners() {
    GvrEventExecutor eventExecutor = GvrPointerInputModule.FindEventExecutor();
    if (eventExecutor == null) {
      return;
    }

    if (!listenersAdded) {
      return;
    }

    eventExecutor.OnPointerClick -= OnPointerClickHandler;
    eventExecutor.OnPointerDown -= OnPointerDownHandler;
    eventExecutor.OnPointerUp -= OnPointerUpHandler;
    eventExecutor.OnPointerEnter -= OnPointerEnterHandler;
    eventExecutor.OnPointerExit -= OnPointerExitHandler;
    eventExecutor.OnScroll -= OnScrollHandler;

    listenersAdded = false;
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

  private void OnScrollHandler(GameObject target, PointerEventData eventData) {
    OnScroll.Invoke(target, eventData);
  }
}

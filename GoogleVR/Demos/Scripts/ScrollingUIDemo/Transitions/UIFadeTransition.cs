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
using UnityEngine.UI;
using System;
using System.Collections;

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
public class UIFadeTransition : MonoBehaviour, IUITransition {
  private bool transitioning;
  private Action runningInterruptCallback;

  /// The amount of time that the effect takes.
  [SerializeField]
  [Tooltip("The amount of time that the effect takes.")]
  private float durationSeconds = 0.25f;

  /// The amount of time to wait before transitioning in.
  [SerializeField]
  [Tooltip("The amount of time to wait before transitioning in.")]
  private float transitionInDelaySeconds = 0.1875f;

  /// The amount of time to wait before transitioning out.
  [SerializeField]
  [Tooltip("The amount of time to wait before transitioning out.")]
  private float transitionOutDelaySeconds = 0.0f;

  public void TransitionIn(Transform toTransition, Action completeCallback, Action interruptCallback) {
    Transition(true, transitionInDelaySeconds, toTransition, completeCallback, interruptCallback);
  }

  public void TransitionOut(Transform toTransition, Action completeCallback, Action interruptCallback) {
    Transition(false, transitionOutDelaySeconds, toTransition, completeCallback, interruptCallback);
  }

  private void Transition(bool transitionIn,
                          float delaySeconds, Transform toTransition,
                          Action completeCallback,
                          Action interruptCallback) {
    if (transitioning) {
      transitioning = false;
      StopAllCoroutines();

      if (runningInterruptCallback != null) {
        runningInterruptCallback();
      }
    }

    float targetAlpha = 0.0f;
    if (transitionIn) {
      targetAlpha = 1.0f;
    }

    StartCoroutine(RunTransition(targetAlpha, delaySeconds, toTransition, completeCallback));
    runningInterruptCallback = interruptCallback;
    transitioning = true;
  }

  private IEnumerator RunTransition(float targetAlpha, float delaySeconds, Transform toTransition, Action callback) {
    CanvasGroup canvasGroup = GetCanvasGroup(toTransition);
    canvasGroup.alpha = 1.0f - targetAlpha;

    yield return new WaitForSeconds(delaySeconds);

    yield return StartCoroutine(RunFade(canvasGroup, targetAlpha));

    runningInterruptCallback = null;
    transitioning = false;

    if (callback != null) {
      callback();
    }
  }

  private IEnumerator RunFade(CanvasGroup canvasGroup, float targetAlpha) {
    float minAlpha = 0.0f;
    float maxAlpha = 1.0f;
    targetAlpha = Mathf.Clamp(targetAlpha, minAlpha, maxAlpha);
    float speed = 1.0f / durationSeconds;

    if (targetAlpha > canvasGroup.alpha) {
      maxAlpha = targetAlpha;
    } else {
      minAlpha = targetAlpha;
      speed *= -1.0f;
    }

    while (canvasGroup.alpha != targetAlpha) {
      float newAlpha = canvasGroup.alpha;
      newAlpha += Time.deltaTime * speed;
      newAlpha = Mathf.Clamp(newAlpha, minAlpha, maxAlpha);
      canvasGroup.alpha = newAlpha;
      yield return null;
    }
  }

  private CanvasGroup GetCanvasGroup(Transform toTransition) {
    CanvasGroup canvasGroup = toTransition.GetComponent<CanvasGroup>();
    if (canvasGroup == null) {
      canvasGroup = toTransition.gameObject.AddComponent<CanvasGroup>();
    }
    return canvasGroup;
  }

}
#endif  // UNITY_HAS_GOOGLEVR &&(UNITY_ANDROID || UNITY_EDITOR

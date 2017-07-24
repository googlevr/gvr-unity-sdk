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

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// This class is used by _GvrPointerInputModule_ to route scroll events through Unity's Event System.
/// It maintains indepedent velocities for each instance of _IScrollHandler_ that is currently being scrolled.
/// Inertia can optionally be toggled off.
[System.Serializable]
public class GvrPointerScrollInput {
  public const string PROPERTY_NAME_INERTIA = "inertia";
  public const string PROPERTY_NAME_DECELERATION_RATE = "decelerationRate";

  private class ScrollInfo {
    public bool isScrolling = false;
    public Vector2 initScroll = Vector2.zero;
    public Vector2 lastScroll = Vector2.zero;
    public Vector2 scrollVelocity = Vector2.zero;
    public IGvrScrollSettings scrollSettings = null;
  }

  /// Inertia means that scroll events will continue for a while after the user stops
  /// touching the touchpad. It gradually slows down according to the decelerationRate.
  [Tooltip("Determines if movement inertia is enabled.")]
  public bool inertia = true;

  /// The deceleration rate is the speed reduction per second.
  /// A value of 0.5 halves the speed each second. The default is 0.05.
  /// The deceleration rate is only used when inertia is enabled.
  [Tooltip("The rate at which movement slows down.")]
  public float decelerationRate = 0.05f;

  /// Multiplier for calculating the scroll delta so that the scroll delta is
  /// within the order of magnitude that the UI system expects.
  public const float SCROLL_DELTA_MULTIPLIER = 1000.0f;

  private const float CUTOFF_HZ = 10.0f;
  private const float RC = (float) (1.0 / (2.0 * Mathf.PI * CUTOFF_HZ));
  private const float SPEED_CLAMP_RATIO = 0.05f;
  private const float SPEED_CLAMP = (SPEED_CLAMP_RATIO * SCROLL_DELTA_MULTIPLIER);
  private const float SPEED_CLAMP_SQUARED = SPEED_CLAMP * SPEED_CLAMP;
  private const float INERTIA_THRESHOLD_RATIO = 0.2f;
  private const float INERTIA_THRESHOLD = (INERTIA_THRESHOLD_RATIO * SCROLL_DELTA_MULTIPLIER);
  private const float INERTIA_THRESHOLD_SQUARED = INERTIA_THRESHOLD * INERTIA_THRESHOLD;
  private const float SLOP_VERTICAL = 0.165f * SCROLL_DELTA_MULTIPLIER;
  private const float SLOP_HORIZONTAL = 0.15f * SCROLL_DELTA_MULTIPLIER;

  private Dictionary<GameObject, ScrollInfo> scrollHandlers = new Dictionary<GameObject, ScrollInfo>();
  private List<GameObject> scrollingObjects = new List<GameObject>();

  public void HandleScroll(GameObject currentGameObject, PointerEventData pointerData,
    GvrBasePointer pointer, IGvrEventExecutor eventExecutor) {
    bool touchDown = false;
    bool touching = false;
    bool touchUp = false;
    Vector2 currentScroll = Vector2.zero;

    if (pointer != null && pointer.IsAvailable) {
      touchDown = pointer.TouchDown;
      touching = pointer.IsTouching;
      touchUp = pointer.TouchUp;
      currentScroll = pointer.TouchPos * SCROLL_DELTA_MULTIPLIER;
    }

    GameObject currentScrollHandler = eventExecutor.GetEventHandler<IScrollHandler>(currentGameObject);

    if (touchDown) {
      RemoveScrollHandler(currentScrollHandler);
    }

    if (currentScrollHandler != null && (touchDown || touching)) {
      OnTouchingScrollHandler(currentScrollHandler, pointerData, currentScroll, eventExecutor);
    } else if (touchUp && currentScrollHandler != null) {
      OnReleaseScrollHandler(currentScrollHandler);
    }

    StopScrollingIfNecessary(touching, currentScrollHandler);
    UpdateInertiaScrollHandlers(touching, currentScrollHandler, pointerData, eventExecutor);
  }

  private void OnTouchingScrollHandler(GameObject currentScrollHandler, PointerEventData pointerData,
    Vector2 currentScroll, IGvrEventExecutor eventExecutor) {
    ScrollInfo scrollInfo = null;
    if (!scrollHandlers.ContainsKey(currentScrollHandler)) {
      scrollInfo = AddScrollHandler(currentScrollHandler, currentScroll);
    } else {
      scrollInfo = scrollHandlers[currentScrollHandler];
    }

    // Determine if we should start scrolling this scrollHandler.
    // This is true if the current scroll is outside of the slop threshold.
    if (CanScrollStart(scrollInfo, currentScroll)) {
      scrollInfo.isScrolling = true;
    }

    if (scrollInfo.isScrolling) {
      if (ShouldUseInertia(scrollInfo)) {
        UpdateVelocity(scrollInfo, currentScroll);
      } else {
        // If inertia is disabled, then we send scroll events immediately.
        pointerData.scrollDelta = currentScroll - scrollInfo.lastScroll;
        eventExecutor.ExecuteHierarchy(currentScrollHandler, pointerData, ExecuteEvents.scrollHandler);
        pointerData.scrollDelta = Vector2.zero;
      }
    }

    scrollInfo.lastScroll = currentScroll;
  }

  private void OnReleaseScrollHandler(GameObject currentScrollHandler) {
    // When we touch up, immediately stop scrolling the currentScrollHandler if it's velocity is low.
    ScrollInfo scrollInfo;
    if (scrollHandlers.TryGetValue(currentScrollHandler, out scrollInfo)) {
      if (!scrollInfo.isScrolling || scrollInfo.scrollVelocity.sqrMagnitude <= INERTIA_THRESHOLD_SQUARED) {
        RemoveScrollHandler(currentScrollHandler);
      }
    }
  }

  private void UpdateVelocity(ScrollInfo scrollInfo, Vector2 currentScroll) {
    Vector2 scrollDisplacement = (currentScroll - scrollInfo.lastScroll);
    Vector2 newVelocity = scrollDisplacement / Time.deltaTime;
    float weight = Time.deltaTime / (RC + Time.deltaTime);
    scrollInfo.scrollVelocity = Vector2.Lerp(scrollInfo.scrollVelocity, newVelocity, weight);
  }

  private void StopScrollingIfNecessary(bool touching, GameObject currentScrollHandler) {
    if (scrollHandlers.Count == 0) {
      return;
    }

    // If inertia is disabled, stop scrolling any scrollHandler that isn't currently being touched.
    for (int i = scrollingObjects.Count - 1; i >= 0; i--) {
      GameObject scrollHandler = scrollingObjects[i];
      ScrollInfo scrollInfo = scrollHandlers[scrollHandler];

      bool isScrollling = scrollInfo.isScrolling;

      bool isVelocityBelowThreshold =
        isScrollling && scrollInfo.scrollVelocity.sqrMagnitude <= SPEED_CLAMP_SQUARED;

      bool isCurrentlyTouching = touching && scrollHandler == currentScrollHandler;

      bool shouldUseInertia = ShouldUseInertia(scrollInfo);

      bool shouldStopScrolling = (shouldUseInertia && isVelocityBelowThreshold)
        || ((!shouldUseInertia || !isScrollling) && !isCurrentlyTouching);

      if (shouldStopScrolling) {
        RemoveScrollHandler(scrollHandler);
      }
    }
  }

  private void UpdateInertiaScrollHandlers(bool touching, GameObject currentScrollHandler,
    PointerEventData pointerData, IGvrEventExecutor eventExecutor) {
    if (pointerData == null) {
      return;
    }

    // If the currentScrollHandler is null, then the currently scrolling scrollHandlers
    // must still be decelerated so the function does not return early.

    for (int i = 0; i < scrollingObjects.Count; i++) {
      GameObject scrollHandler = scrollingObjects[i];
      ScrollInfo scrollInfo = scrollHandlers[scrollHandler];

      if (!ShouldUseInertia(scrollInfo)) {
        continue;
      }

      if (scrollInfo.isScrolling) {
        // Decelerate the scrollHandler if necessary.
        if (!touching || scrollHandler != currentScrollHandler) {
          float finalDecelerationRate = GetDecelerationRate(scrollInfo);
          scrollInfo.scrollVelocity *= Mathf.Pow(finalDecelerationRate, Time.deltaTime);
        }

        // Send the scroll events.
        pointerData.scrollDelta = scrollInfo.scrollVelocity * Time.deltaTime;
        eventExecutor.ExecuteHierarchy(scrollHandler, pointerData, ExecuteEvents.scrollHandler);
      }
    }
    pointerData.scrollDelta = Vector2.zero;
  }

  private ScrollInfo AddScrollHandler(GameObject scrollHandler, Vector2 currentScroll) {
    ScrollInfo scrollInfo = new ScrollInfo();
    scrollInfo.initScroll = currentScroll;
    scrollInfo.lastScroll = currentScroll;
    scrollInfo.scrollSettings = scrollHandler.GetComponent<IGvrScrollSettings>();
    scrollHandlers[scrollHandler] = scrollInfo;
    scrollingObjects.Add(scrollHandler);
    return scrollInfo;
  }

  private void RemoveScrollHandler(GameObject scrollHandler) {
    // Check if it's null via object.Equals instead of doing a direct comparison
    // to avoid using Unity's equality check override for UnityEngine.Objects.
    // This is so that we can remove Unity objects that have been Destroyed from the dictionary,
    // but will still return early when an object is actually null.
    if (object.Equals(scrollHandler, null)) {
      return;
    }

    if (!scrollHandlers.ContainsKey(scrollHandler)) {
      return;
    }

    scrollHandlers.Remove(scrollHandler);
    scrollingObjects.Remove(scrollHandler);
  }

  private bool ShouldUseInertia(ScrollInfo scrollInfo) {
    if (scrollInfo != null && scrollInfo.scrollSettings != null) {
      return scrollInfo.scrollSettings.InertiaOverride;
    }

    return inertia;
  }

  private float GetDecelerationRate(ScrollInfo scrollInfo) {
    if (scrollInfo != null && scrollInfo.scrollSettings != null) {
      return scrollInfo.scrollSettings.DecelerationRateOverride;
    }

    return decelerationRate;
  }

  private static bool CanScrollStart(ScrollInfo scrollInfo, Vector2 currentScroll) {
    if (scrollInfo == null) {
      return false;
    }

    return (Mathf.Abs(currentScroll.x - scrollInfo.initScroll.x) >= SLOP_HORIZONTAL)
      || (Mathf.Abs(currentScroll.y - scrollInfo.initScroll.y) >= SLOP_VERTICAL);
  }
}

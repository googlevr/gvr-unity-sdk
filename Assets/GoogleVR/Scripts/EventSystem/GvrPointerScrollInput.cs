//-----------------------------------------------------------------------
// <copyright file="GvrPointerScrollInput.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class is used by `GvrPointerInputModule` to route scroll events through Unity's event
/// system.
/// </summary>
/// <remarks>
/// It maintains indepedent velocities for each instance of `IScrollHandler` that is currently being
/// scrolled.  Inertia can optionally be toggled off.
/// </remarks>
[System.Serializable]
public class GvrPointerScrollInput
{
    /// <summary>Property name for accessing inertia.</summary>
    public const string PROPERTY_NAME_INERTIA = "inertia";

    /// <summary>Property name for accessing deceleration rate.</summary>
    public const string PROPERTY_NAME_DECELERATION_RATE = "decelerationRate";

    /// <summary>Multiplier for calculating the scroll delta.</summary>
    /// <remarks>
    /// Used so that the scroll delta is within the order of magnitude that the UI system expects.
    /// </remarks>
    public const float SCROLL_DELTA_MULTIPLIER = 1000.0f;

    /// <summary>
    /// Inertia means that scroll events will continue for a while after the user stops touching the
    /// touchpad.
    /// </summary>
    /// <remarks>It gradually slows down according to the `decelerationRate`.</remarks>
    [Tooltip("Determines if movement inertia is enabled.")]
    public bool inertia = true;

    /// <summary>The deceleration rate is the speed reduction per second.</summary>
    /// <remarks>
    /// A value of 0.5 halves the speed each second. The default is 0.05.  The deceleration rate is
    /// only used when `inertia` is `true`.
    /// </remarks>
    [Tooltip("The rate at which movement slows down.")]
    public float decelerationRate = 0.05f;

    private const float CUTOFF_HZ = 10.0f;
    private const float RC = (float)(1.0 / (2.0 * Mathf.PI * CUTOFF_HZ));
    private const float SPEED_CLAMP_RATIO = 0.05f;
    private const float SPEED_CLAMP = SPEED_CLAMP_RATIO * SCROLL_DELTA_MULTIPLIER;
    private const float SPEED_CLAMP_SQUARED = SPEED_CLAMP * SPEED_CLAMP;
    private const float INERTIA_THRESHOLD_RATIO = 0.2f;
    private const float INERTIA_THRESHOLD = INERTIA_THRESHOLD_RATIO * SCROLL_DELTA_MULTIPLIER;
    private const float INERTIA_THRESHOLD_SQUARED = INERTIA_THRESHOLD * INERTIA_THRESHOLD;
    private const float SLOP_VERTICAL = 0.165f * SCROLL_DELTA_MULTIPLIER;
    private const float SLOP_HORIZONTAL = 0.15f * SCROLL_DELTA_MULTIPLIER;

    private Dictionary<GameObject, ScrollInfo> scrollHandlers =
        new Dictionary<GameObject, ScrollInfo>();

    private List<GameObject> scrollingObjects = new List<GameObject>();

    /// <summary>Performs scrolling if the user is touching the controller's touchpad.</summary>
    /// <remarks>Scroll speed is dependent upon touch position.</remarks>
    /// <param name="currentGameObject">
    /// The game object having the `IScrollHandler` component.
    /// </param>
    /// <param name="pointerData">The pointer event data.</param>
    /// <param name="pointer">The pointer object.</param>
    /// <param name="eventExecutor">The executor to use to process the event.</param>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public void HandleScroll(GameObject currentGameObject, PointerEventData pointerData,
                              GvrBasePointer pointer, IGvrEventExecutor eventExecutor)
    {
        bool touchDown = false;
        bool touching = false;
        bool touchUp = false;
        Vector2 currentScroll = Vector2.zero;

        if (pointer != null && pointer.IsAvailable)
        {
            touchDown = pointer.TouchDown;
            touching = pointer.IsTouching;
            touchUp = pointer.TouchUp;
            currentScroll = pointer.TouchPos * SCROLL_DELTA_MULTIPLIER;
        }

        GameObject currentScrollHandler =
            eventExecutor.GetEventHandler<IScrollHandler>(currentGameObject);

        if (touchDown)
        {
            RemoveScrollHandler(currentScrollHandler);
        }

        if (currentScrollHandler != null && (touchDown || touching))
        {
            OnTouchingScrollHandler(
                currentScrollHandler, pointerData, currentScroll, eventExecutor);
        }
        else if (touchUp && currentScrollHandler != null)
        {
            OnReleaseScrollHandler(currentScrollHandler);
        }

        StopScrollingIfNecessary(touching, currentScrollHandler);
        UpdateInertiaScrollHandlers(touching, currentScrollHandler, pointerData, eventExecutor);
    }

    private static bool CanScrollStartX(ScrollInfo scrollInfo, Vector2 currentScroll)
    {
        if (scrollInfo == null)
        {
            return false;
        }

        return Mathf.Abs(currentScroll.x - scrollInfo.initScroll.x) >= SLOP_HORIZONTAL;
    }

    private static bool CanScrollStartY(ScrollInfo scrollInfo, Vector2 currentScroll)
    {
        if (scrollInfo == null)
        {
            return false;
        }

        return Mathf.Abs(currentScroll.y - scrollInfo.initScroll.y) >= SLOP_VERTICAL;
    }

    private void OnTouchingScrollHandler(GameObject currentScrollHandler,
                                         PointerEventData pointerData,
                                         Vector2 currentScroll,
                                         IGvrEventExecutor eventExecutor)
    {
        ScrollInfo scrollInfo = null;
        if (!scrollHandlers.ContainsKey(currentScrollHandler))
        {
            scrollInfo = AddScrollHandler(currentScrollHandler, currentScroll);
        }
        else
        {
            scrollInfo = scrollHandlers[currentScrollHandler];
        }

        // Detect if we should start scrolling along the x-axis based on the horizontal slop
        // threshold.
        if (CanScrollStartX(scrollInfo, currentScroll))
        {
            scrollInfo.isScrollingX = true;
        }

        // Detect if we should start scrolling along the y-axis based on the vertical slop
        // threshold.
        if (CanScrollStartY(scrollInfo, currentScroll))
        {
            scrollInfo.isScrollingY = true;
        }

        if (scrollInfo.IsScrolling)
        {
            Vector2 clampedScroll = currentScroll;
            Vector2 clampedLastScroll = scrollInfo.lastScroll;
            if (!scrollInfo.isScrollingX)
            {
                clampedScroll.x = 0.0f;
                clampedLastScroll.x = 0.0f;
            }

            if (!scrollInfo.isScrollingY)
            {
                clampedScroll.y = 0.0f;
                clampedLastScroll.y = 0.0f;
            }

            Vector2 scrollDisplacement = clampedScroll - clampedLastScroll;
            UpdateVelocity(scrollInfo, scrollDisplacement);

            if (!ShouldUseInertia(scrollInfo))
            {
                // If inertia is disabled, then we send scroll events immediately.
                pointerData.scrollDelta = scrollDisplacement;

                eventExecutor.ExecuteHierarchy(
                    currentScrollHandler, pointerData, ExecuteEvents.scrollHandler);

                pointerData.scrollDelta = Vector2.zero;
            }
        }

        scrollInfo.lastScroll = currentScroll;
    }

    private void OnReleaseScrollHandler(GameObject currentScrollHandler)
    {
        // When we touch up, immediately stop scrolling the currentScrollHandler if it's velocity is
        // low.
        ScrollInfo scrollInfo;
        if (scrollHandlers.TryGetValue(currentScrollHandler, out scrollInfo))
        {
            if (!scrollInfo.IsScrolling ||
                scrollInfo.scrollVelocity.sqrMagnitude <= INERTIA_THRESHOLD_SQUARED)
            {
                RemoveScrollHandler(currentScrollHandler);
            }
        }
    }

    private void UpdateVelocity(ScrollInfo scrollInfo, Vector2 scrollDisplacement)
    {
        Vector2 newVelocity = scrollDisplacement / Time.deltaTime;
        float weight = Time.deltaTime / (RC + Time.deltaTime);
        scrollInfo.scrollVelocity = Vector2.Lerp(scrollInfo.scrollVelocity, newVelocity, weight);
    }

    private void StopScrollingIfNecessary(bool touching, GameObject currentScrollHandler)
    {
        if (scrollHandlers.Count == 0)
        {
            return;
        }

        // If inertia is disabled, stop scrolling any scrollHandler that isn't currently being
        // touched.
        for (int i = scrollingObjects.Count - 1; i >= 0; i--)
        {
            GameObject scrollHandler = scrollingObjects[i];
            ScrollInfo scrollInfo = scrollHandlers[scrollHandler];

            bool isScrollling = scrollInfo.IsScrolling;

            bool isVelocityBelowThreshold =
                isScrollling && scrollInfo.scrollVelocity.sqrMagnitude <= SPEED_CLAMP_SQUARED;

            bool isCurrentlyTouching = touching && scrollHandler == currentScrollHandler;

            bool shouldUseInertia = ShouldUseInertia(scrollInfo);

            bool shouldStopScrolling =
                isVelocityBelowThreshold ||
                ((!shouldUseInertia || !isScrollling) && !isCurrentlyTouching);

            if (shouldStopScrolling)
            {
                RemoveScrollHandler(scrollHandler);
            }
        }
    }

    private void UpdateInertiaScrollHandlers(bool touching,
                                             GameObject currentScrollHandler,
                                             PointerEventData pointerData,
                                             IGvrEventExecutor eventExecutor)
    {
        if (pointerData == null)
        {
            return;
        }

        // If the currentScrollHandler is null, then the currently scrolling scrollHandlers
        // must still be decelerated so the function does not return early.
        for (int i = 0; i < scrollingObjects.Count; i++)
        {
            GameObject scrollHandler = scrollingObjects[i];
            ScrollInfo scrollInfo = scrollHandlers[scrollHandler];

            if (!ShouldUseInertia(scrollInfo))
            {
                continue;
            }

            if (scrollInfo.IsScrolling)
            {
                // Decelerate the scrollHandler if necessary.
                if (!touching || scrollHandler != currentScrollHandler)
                {
                    float finalDecelerationRate = GetDecelerationRate(scrollInfo);
                    scrollInfo.scrollVelocity *= Mathf.Pow(finalDecelerationRate, Time.deltaTime);
                }

                // Send the scroll events.
                pointerData.scrollDelta = scrollInfo.scrollVelocity * Time.deltaTime;
                eventExecutor.ExecuteHierarchy(
                    scrollHandler, pointerData, ExecuteEvents.scrollHandler);
            }
        }

        pointerData.scrollDelta = Vector2.zero;
    }

    private ScrollInfo AddScrollHandler(GameObject scrollHandler, Vector2 currentScroll)
    {
        ScrollInfo scrollInfo = new ScrollInfo();
        scrollInfo.initScroll = currentScroll;
        scrollInfo.lastScroll = currentScroll;
        scrollInfo.scrollSettings = scrollHandler.GetComponent<IGvrScrollSettings>();
        scrollHandlers[scrollHandler] = scrollInfo;
        scrollingObjects.Add(scrollHandler);
        return scrollInfo;
    }

    private void RemoveScrollHandler(GameObject scrollHandler)
    {
        // Check if it's null via object.Equals instead of doing a direct comparison
        // to avoid using Unity's equality check override for UnityEngine.Objects.
        // This is so that we can remove Unity objects that have been Destroyed from the dictionary,
        // but will still return early when an object is actually null.
        if (object.Equals(scrollHandler, null))
        {
            return;
        }

        if (!scrollHandlers.ContainsKey(scrollHandler))
        {
            return;
        }

        scrollHandlers.Remove(scrollHandler);
        scrollingObjects.Remove(scrollHandler);
    }

    private bool ShouldUseInertia(ScrollInfo scrollInfo)
    {
        if (scrollInfo != null && scrollInfo.scrollSettings != null)
        {
            return scrollInfo.scrollSettings.InertiaOverride;
        }

        return inertia;
    }

    private float GetDecelerationRate(ScrollInfo scrollInfo)
    {
        if (scrollInfo != null && scrollInfo.scrollSettings != null)
        {
            return scrollInfo.scrollSettings.DecelerationRateOverride;
        }

        return decelerationRate;
    }

    private class ScrollInfo
    {
        public bool isScrollingX = false;
        public bool isScrollingY = false;
        public Vector2 initScroll = Vector2.zero;
        public Vector2 lastScroll = Vector2.zero;
        public Vector2 scrollVelocity = Vector2.zero;
        public IGvrScrollSettings scrollSettings = null;

        public bool IsScrolling
        {
            get
            {
                return isScrollingX || isScrollingY;
            }
        }
    }
}

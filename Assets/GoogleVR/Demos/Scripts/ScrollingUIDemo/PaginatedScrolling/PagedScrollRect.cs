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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class PagedScrollRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  /// Allows you to control how sensitive the paged
  /// Scroll rect is to events from the gvr controller.
  [Tooltip("The sensitivity to gvr touch events.")]
  public float ScrollSensitivity = 1.0f;

  /// The speed that the scroll rect snaps to a page
  /// When the gvr touchpad is released.
  [Tooltip("The speed that the rect snaps to a page.")]
  public float SnapSpeed = 6.0f;

  /// The index of the page to start the scroll rect on.
  /// Will changes the local position of the transform on Start.
  [Tooltip("The index of the page to start the scroll rect on.")]
  public int StartPage = 0;

  /// If true, the user can scroll continuously in any direction
  /// and the pages will loop.
  [Tooltip("Determines if the pages loop when scrolling.")]
  public bool loop = false;

  /// If true, the user must be pointing at the scroll rect with the controller
  /// to be able to scroll.
  [Tooltip("Determines whether the user must be pointing at the scroll rect with the controller to be able to scroll.")]
  public bool onlyScrollWhenPointing = true;

  /// Determines how many extra pages are shown on each side of
  /// the scroll rect is shown when the scroll view is not moving.
  /// If set to 0, only the activePage is shown.
  /// If set to 1, an extra page is shown on each side.
  [Tooltip("Determines how many extra pages are shown on each side of the scroll rect when the scroll view is not moving.")]
  public int numExtraPagesShown = 0;

  /// This is used to determine if the leftmost and rightmost page
  /// should be shown when the scroll rect is not moving.
  /// If numExtraPagesShown is zero, then this is the previous and next page.
  [Tooltip("Determines if the last extra page should be shown when the scroll rect is at rest.")]
  public bool showNextPagesAtRest = false;

  /// This is used to determine if the tiles will be interactable
  /// regardless of the state of the paged scroll rect. If false,
  /// then tiles will not be interactable if they aren't on the active page.
  [Tooltip("Determines if the tiles should always be interactable.")]
  public bool shouldTilesAlwaysBeInteractable = true;

  [Tooltip("Determines if scrolling is enabled.")]
  public bool scrollingEnabled = true;

  /// A callback to indicate that the active page has changed.
  public delegate void ActivePageChangedDelegate(RectTransform activePage,int activePageIndex,RectTransform previousPage,int previousPageIndex);

  /// Called whenever the active page changes.
  public event ActivePageChangedDelegate OnActivePageChanged;

  public UnityEvent OnSwipeLeft;
  public UnityEvent OnSwipeRight;
  public UnityEvent OnSnapClosest;

  /// Interface used as the data source for the content in this scroll rect.
  private IPageProvider pageProvider;

  /// Interface used to implement visual effect for scrolling this scroll rect.
  private BaseScrollEffect[] scrollEffects;

  /// Keep track of the last few frames of touch positions, and the initial position
  private bool isTrackingTouches = false;
  private Vector2 initialTouchPos;
  private Vector2 previousTouchPos;
  private float previousTouchTimestamp;
  private Vector2 overallVelocity;

  private bool isScrolling = false;
  private bool isPointerHovering = false;
  private float scrollOffset = float.MaxValue;

  /// Lerp towards the target scroll offset to smooth the motion.
  private float targetScrollOffset;

  // True is the scroll offset is overridden by an external source.
  private bool isScrollOffsetOverridden = false;

  private RectTransform activePage;
  private Coroutine activeSnapCoroutine;

  /// Keep track of the currently visible pages
  private Dictionary<int, RectTransform> indexToVisiblePage = new Dictionary<int, RectTransform>();
  private Dictionary<RectTransform, int> visiblePageToIndex = new Dictionary<RectTransform, int>();

  /// Store the visible pages in a separate list
  /// so that we have a collection that we can remove elements from while iterating through it.
  private List<RectTransform> visiblePages = new List<RectTransform>();

  /// Touch Delta is required to be higher than
  /// the click threshold to avoid detecting clicks as swipes.
  private const float kClickThreshold = 0.15f;

  /// overallVelocity must be greater than the swipe threshold
  /// to detect a swipe.
  private const float kSwipeThreshold = 0.75f;

  /// The difference between two timestamps must be greater than
  /// this value to be considered different. Helps reduce noise.
  private const float kTimestampDeltaThreshold = 1.0e-7f;

  /// If the difference between the target scroll offset
  /// and the current scroll offset is greater than the moving threshold,
  /// then we are considered to be moving. This coeff is multiplied by the spacing
  /// to get the moving threshold.
  private const float kIsMovingThresholdCoeff = 0.1f;

  // Snap the scroll offset to the target scroll offset when the delta between the two
  // becomes smaller than kSnapScrollOffsetThresholdCoeff * pageProvider.GetSpacing().
  private const float kSnapScrollOffsetThresholdCoeff = 0.002f;

  /// Values used for low-pass-filter to improve the accuracy of
  /// our tracked velocity.
  private const float kCuttoffHz = 10.0f;
  private const float kRc = (float) (1.0 / (2.0 * Mathf.PI * kCuttoffHz));

  private enum SnapDirection {
    Left,
    Right,
    Closest
  }

  /// The active page in the scroll rect.
  public RectTransform ActivePage {
    get {
      return activePage;
    }
    private set {
      if (value == ActivePage) {
        return;
      }

      RectTransform previousPage = ActivePage;
      int previousPageIndex = ActivePageIndex;

      activePage = value;
      activePage.SetAsLastSibling();

      if (OnActivePageChanged != null) {
        OnActivePageChanged(ActivePage, ActivePageIndex, previousPage, previousPageIndex);
      }
    }
  }

  /// The index of the active page.
  /// If there is no active page, returns -1.
  public int ActivePageIndex {
    get {
      if (ActivePage != null && visiblePageToIndex.ContainsKey(ActivePage)) {
        int index = PageIndexFromRealIndex(ActiveRealIndex);
        return index;
      }
      return -1;
    }
  }

  /// If loop is set to false, this will always be the same as the ActivePageIndex
  /// Otherwise, this will be the index the player is looking at, including all
  /// of the aditional loops that the player has swiped through.
  ///
  /// i.e.
  /// If the user has swiped to the right 8 times and there are 5 pages:
  /// ActivePageIndex will return 3.
  /// ActiveRealIndex will return 8.
  public int ActiveRealIndex {
    get {
      if (ActivePage != null && visiblePageToIndex.ContainsKey(ActivePage)) {
        int index = visiblePageToIndex[ActivePage];
        return index;
      }
      return -1;
    }
  }

  /// The number of pages in the scroll rect.
  /// If there is no pageProvider, returns -1.
  public int PageCount {
    get {
      if (pageProvider == null) {
        return -1;
      }
      return pageProvider.GetNumberOfPages();
    }
  }

  /// The spacing between pages in the local coordinate system of this PagedScrollRect.
  public float PageSpacing {
    get {
      if (pageProvider == null) {
        return 0.0f;
      }
      return pageProvider.GetSpacing();
    }
  }

  /// Returns the amount that the
  /// rect has been scrolled in local coordinates.
  public float ScrollOffset {
    get {
      return scrollOffset;
    }
    private set {
      if (value != ScrollOffset) {
        scrollOffset = value;
        OnScrolled();
      }
    }
  }

  /// Returns true if scrolling is currently allowed
  public bool CanScroll {
    get {
      return scrollingEnabled && (isPointerHovering || !onlyScrollWhenPointing);
    }
  }

  /// Returns true if the scroll region is currently moving.
  /// This is the case if the player is actively scrolling, and
  /// when the scroll region is snapping to a page.
  public bool IsMoving {
    get {
      if (isScrolling) {
        return true;
      }

      float moveDistance = CurrentMoveDistance;
      if (moveDistance > GetMovingThreshold()) {
        return true;
      }

      return false;
    }
  }

  /// Returns the distance between the targetScrollOffset and the ScrollOffset.
  /// This can be used to determine how quickly the PagedScrollRect is scrolling.
  public float CurrentMoveDistance {
    get {
      return Mathf.Abs(targetScrollOffset - ScrollOffset);
    }
  }

  /// <summary>
  /// Snaps the scroll rect to a particular page.
  /// </summary>
  /// <param name="index"> the index of the page to snap to.</param>
  /// <param name="immediate">If set to <c>true</c> then snapping happens instantly,
  /// otherwise it is animated.</param>
  public void SnapToPage(int index, bool immediate = false, bool supressEvents=false) {
    if (!loop && (index < 0 || index >= PageCount)) {
      Debug.LogWarning("Attempting to snap to non-existant page: " + index);
      return;
    }

    if (immediate) {
      float offset = OffsetFromIndex(index);
      targetScrollOffset = offset;
      ScrollOffset = offset;
    } else {
      activeSnapCoroutine = StartCoroutine(SnapToPageCoroutine(index));
    }

    if (!supressEvents) {
      int currentIndex = ActiveRealIndex;
      if (index < currentIndex) {
        OnSwipeLeft.Invoke();
      } else {
        OnSwipeRight.Invoke();
      }
    }
  }

  /// <summary>
  /// Snaps the scroll rect to a particular page. Only works for pages that are
  /// currently visible.
  /// </summary>
  /// <param name="visiblePage"> The page to snap to.</param>
  /// <param name="immediate">If set to <c>true</c> then snapping happens instantly,
  /// otherwise it is animated.</param>
  public void SnapToVisiblePage(RectTransform visiblePage, bool immediate = false) {
    if (visiblePage == null) {
      Debug.LogWarning("visiblePage is null, cannot snap to it.");
      return;
    }

    if (!visiblePageToIndex.ContainsKey(visiblePage)) {
      Debug.LogWarning(visiblePage.name + " is not a visible page, cannot snap to it.");
      return;
    }

    int index = visiblePageToIndex[visiblePage];
    SnapToPage(index, immediate);
  }

  /// <summary>
  /// Explicitly set the scroll offset of the PagedScrollRect. Useful when you need to control
  /// the scroll offset from an external source (I.E. a scroll bar). When unset, the PagedScrollRect
  /// will snap to the closest page.
  /// </summary>
  /// <param name="offsetOverride"> The scroll offset to set.</param>
  /// <param name="immediate">If set to <c>true</c> then the scroll offset is set instantly,
  /// otherwise it is animated.</param>
  public void SetScrollOffsetOverride(float? offsetOverride, bool immediate = false) {
    bool newIsScrollOffsetOverridden = offsetOverride != null;

    // If we didn't previously have an offset override, stop scrolling.
    if (!isScrollOffsetOverridden && newIsScrollOffsetOverridden) {
      StopScrolling(false);
      StopTouchTracking();
    }

    if (newIsScrollOffsetOverridden) {
      targetScrollOffset = offsetOverride.Value;
      if (immediate) {
        ScrollOffset = targetScrollOffset;
      }
    } else if (isScrollOffsetOverridden) {
      SnapToPageInDirection(SnapDirection.Closest);
    }

    isScrollOffsetOverridden = newIsScrollOffsetOverridden;
  }

  /// Removes all pages and goes back to the starting page.
  /// Call this function if the PageProvider changes.
  public void Reset() {
    foreach (KeyValuePair<RectTransform, int> pair in visiblePageToIndex) {
      pageProvider.RemovePage(pair.Value, pair.Key);
    }

    visiblePageToIndex.Clear();
    indexToVisiblePage.Clear();
    scrollOffset = float.MaxValue;
    targetScrollOffset = 0.0f;
    SetScrollOffsetOverride(null, true);
    ScrollOffset = targetScrollOffset;
  }

  void OnDisable() {
    if (pageProvider == null) {
      return;
    }

    SetScrollOffsetOverride(null, true);
    StopScrolling(true, true);
    StopTouchTracking();
  }

  void Start() {
    pageProvider = GetComponent<IPageProvider>();

    if (pageProvider == null) {
      throw new System.NullReferenceException(
        "PagedScrollRect is missing an IPageProvider. " +
        "Please look at IPageProvider.cs for details.");
    }

    scrollEffects = GetComponents<BaseScrollEffect>();
    if (scrollEffects.Length == 0) {
      Debug.LogWarning(
        "PagedScrollRect does not have any BaseScrollEffects. " +
        "Adding defaults.");
      gameObject.AddComponent<TranslateScrollEffect>();
      gameObject.AddComponent<FadeScrollEffect>();
      scrollEffects = GetComponents<BaseScrollEffect>();
    }

    // Immediately snap to the starting page.
    SnapToPage(StartPage, true, true);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (onlyScrollWhenPointing) {
      isPointerHovering = true;
    }
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (onlyScrollWhenPointing) {
      isPointerHovering = false;
    }
  }

  void Update() {
    if (isScrollOffsetOverridden) {
      LerpTowardsOffset(targetScrollOffset);
      return;
    }

    if (!CanScroll) {
      StopScrolling();
      StopTouchTracking();
      return;
    }

    /// Don't start scrolling until the touch pos has moved.
    /// This is to prevent scrolling when the user intended to click.
    if (!isScrolling && GvrControllerInput.IsTouching) {
      if (!isTrackingTouches) {
        StartTouchTracking();
      } else {
        Vector2 touchDelta = GvrControllerInput.TouchPos - initialTouchPos;
        float xDeltaMagnitude = Mathf.Abs(touchDelta.x);
        float yDeltaMagnitude = Mathf.Abs(touchDelta.y);

        if (xDeltaMagnitude > kClickThreshold && xDeltaMagnitude > yDeltaMagnitude) {
          StartScrolling();
        }
      }
    }

    if (isScrolling && GvrControllerInput.IsTouching) {
      Vector2 touchDelta = GvrControllerInput.TouchPos - previousTouchPos;

      if (Mathf.Abs(touchDelta.x) > 0) {
        // Translate directly based on the touch value.
        float spacingCoeff = -pageProvider.GetSpacing();
        targetScrollOffset += touchDelta.x * spacingCoeff * ScrollSensitivity;
      }

      LerpTowardsOffset(targetScrollOffset);
    }

    if (GvrControllerInput.TouchUp) {
      StopScrolling();
      StopTouchTracking();
    }

    if (isTrackingTouches && GvrControllerInput.IsTouching) {
      TrackTouch();
    }
  }

  private void StartScrolling() {
    if (isScrolling) {
      return;
    }

    targetScrollOffset = ScrollOffset;

    if (activeSnapCoroutine != null) {
      StopCoroutine(activeSnapCoroutine);
    }

    isScrolling = true;
  }

  private void StopScrolling(bool snapToPage = true, bool snapImmediate = false) {
    if (!isScrolling) {
      return;
    }

    if (snapToPage) {
      if (overallVelocity.x > kSwipeThreshold) {
        /// If I was swiping to the right.
        SnapToPageInDirection(SnapDirection.Left, snapImmediate);
      } else if (overallVelocity.x < -kSwipeThreshold) {
        /// If I was swiping to the left.
        SnapToPageInDirection(SnapDirection.Right, snapImmediate);
      } else {
        /// If the touch delta is not big enough, just snap to the closest page.
        SnapToPageInDirection(SnapDirection.Closest, snapImmediate);
      }
    }

    isScrolling = false;
  }

  private void StartTouchTracking() {
    isTrackingTouches = true;
    initialTouchPos = GvrControllerInput.TouchPos;
    previousTouchPos = initialTouchPos;
    previousTouchTimestamp = Time.time;
    overallVelocity = Vector2.zero;
  }

  private void StopTouchTracking() {
    if (!isTrackingTouches) {
      return;
    }

    isTrackingTouches = false;
    initialTouchPos = Vector2.zero;
    previousTouchPos = Vector2.zero;
    previousTouchTimestamp = 0.0f;
    overallVelocity = Vector2.zero;
  }

  private void TrackTouch() {
    if (!isTrackingTouches) {
      Debug.LogWarning("StartTouchTracking must be called before touches can be tracked.");
      return;
    }

    float timeElapsedSeconds = (Time.time - previousTouchTimestamp);

    // If the timestamp has not changed, do not update.
    if (timeElapsedSeconds < kTimestampDeltaThreshold) {
      return;
    }

    // Update velocity
    Vector2 touchDelta = GvrControllerInput.TouchPos - previousTouchPos;
    Vector2 velocity = touchDelta / timeElapsedSeconds;
    float weight = timeElapsedSeconds / (kRc + timeElapsedSeconds);
    overallVelocity = Vector2.Lerp(overallVelocity, velocity, weight);

    // Update the previous touch
    previousTouchPos = GvrControllerInput.TouchPos;
    previousTouchTimestamp = Time.time;
  }


  private void SnapToPageInDirection(SnapDirection snapDirection, bool immediate = false) {
    int closestPageIndex = 0;
    bool didClamp;
    float directionBias = pageProvider.GetSpacing() * 0.55f;

    switch (snapDirection) {
      case SnapDirection.Right:
        float rightOffset = targetScrollOffset + directionBias;
        closestPageIndex = IndexFromOffset(rightOffset, out didClamp);
        if (!didClamp) {
          OnSwipeRight.Invoke();
        }
        break;
      case SnapDirection.Left:
        float leftOffset = targetScrollOffset - directionBias;
        closestPageIndex = IndexFromOffset(leftOffset, out didClamp);
        if (!didClamp) {
          OnSwipeLeft.Invoke();
        }
        break;
      case SnapDirection.Closest:
        closestPageIndex = IndexFromOffset(targetScrollOffset, out didClamp);
        OnSnapClosest.Invoke();
        break;
      default:
        throw new System.Exception("Invalid SnapDirection: " + snapDirection);
    }

    /// If we found a page in that direction.
    SnapToPage(closestPageIndex, immediate, true);
  }

  private void OnScrolled() {
    bool didClamp;
    int newActiveIndex = IndexFromOffset(scrollOffset, out didClamp);

    /// Make sure to update the active page
    if (IsPageVisible(newActiveIndex)) {
      ActivePage = indexToVisiblePage[newActiveIndex];
    }

    /// Update existing pages
    for (int i = visiblePages.Count - 1; i >= 0; i--) {
      RectTransform page = visiblePages[i];

      /// If this object doesn't have a RectTransform it isn't a valid page.
      /// Not necessarily an issue, could be something else.
      if (page == null) {
        continue;
      }

      bool isVisiblePage = visiblePageToIndex.ContainsKey(page);

      /// This accounts for the case where not all of the children
      /// are visible pages. Helpful to keep the ScrollRect flexible
      /// and for potential pooling implementations.
      if (!isVisiblePage) {
        continue;
      }

      int pageIndex = visiblePageToIndex[page];

      if (ShouldShowIndexForOffset(ScrollOffset, pageIndex)) {
        ApplyScrollEffects(page);
      } else {
        RemovePage(page);
      }
    }

    /// Add active page if it doesn't already exist
    if (!indexToVisiblePage.ContainsKey(newActiveIndex)) {
      AddPage(newActiveIndex, true);
    }

    /// Add additional pages to the left of the active page.
    int nextIndex = newActiveIndex - 1;
    while (true) {
      if (!loop && nextIndex < 0) {
        break;
      }

      if (IsPageVisible(nextIndex)) {
        nextIndex--;
        continue;
      }

      if (!AddPageIfNecessary(nextIndex)) {
        break;
      }

      nextIndex--;
    }

    /// Add additional pages to the right of the active page.
    nextIndex = newActiveIndex + 1;
    while (true) {
      if (!loop && nextIndex >= pageProvider.GetNumberOfPages()) {
        break;
      }

      if (IsPageVisible(nextIndex)) {
        nextIndex++;
        continue;
      }

      if (!AddPageIfNecessary(nextIndex)) {
        break;
      }

      nextIndex++;
    }
  }

  private IEnumerator SnapToPageCoroutine(int index) {
    targetScrollOffset = OffsetFromIndex(index);

    while (true) {
      if (LerpTowardsOffset(targetScrollOffset)) {
        yield return null;
      } else {
        break;
      }
    }
  }

  /// Returns false if the ScrollOffset is already the same as the targetOffset.
  private bool LerpTowardsOffset(float targetOffset) {
    if (ScrollOffset == targetOffset) {
      return false;
    }

    float diff = Mathf.Abs(ScrollOffset - targetScrollOffset);
    float threshold = pageProvider.GetSpacing() * kSnapScrollOffsetThresholdCoeff;
    if (diff < threshold) {
      ScrollOffset = targetScrollOffset;
    } else {
      ScrollOffset = Mathf.Lerp(ScrollOffset, targetOffset, SnapSpeed * Time.deltaTime);
    }

    ScrollOffset = Mathf.Lerp(ScrollOffset, targetOffset, SnapSpeed * Time.deltaTime);
    return true;
  }

  private float OffsetFromIndex(int index) {
    return index * pageProvider.GetSpacing();
  }

  private int IndexFromOffset(float offset, out bool didClamp) {
    int index = Mathf.RoundToInt(offset / pageProvider.GetSpacing());
    didClamp = false;

    if (!loop) {
      int clampedIndex = Mathf.Clamp(index, 0, pageProvider.GetNumberOfPages() - 1);
      didClamp = clampedIndex != index;
      return clampedIndex;
    }

    return index;
  }

  private int PageIndexFromRealIndex(int index) {
    int loopAmount = Mathf.FloorToInt((float)index / (float)PageCount);
    index = index - (loopAmount * PageCount);

    return index;
  }

  private bool ShouldShowIndexForOffset(float offset, int index) {
    float indexOffset = OffsetFromIndex(index);
    float diff = Mathf.RoundToInt(indexOffset - offset);
    float absoluteDiff = Mathf.Abs(diff);

    int pagesShown = 1 + numExtraPagesShown;
    if (showNextPagesAtRest) {
      return absoluteDiff <= pageProvider.GetSpacing() * pagesShown;
    } else {
      return absoluteDiff < pageProvider.GetSpacing() * pagesShown;
    }
  }

  private bool IsPageVisible(int index) {
    return indexToVisiblePage.ContainsKey(index);
  }

  private bool AddPageIfNecessary(int index) {
    if (ShouldShowIndexForOffset(scrollOffset, index)) {
      AddPage(index);
      return true;
    }

    return false;
  }

  private void AddPage(int index, bool isActivePage=false) {
    int pageIndex = PageIndexFromRealIndex(index);
    RectTransform page = pageProvider.ProvidePage(pageIndex);
    page.SetParent(transform, false);
    indexToVisiblePage[index] = page;
    visiblePageToIndex[page] = index;
    visiblePages.Add(page);

    if (isActivePage) {
      ActivePage = page;
    }

    ApplyScrollEffects(page);

    if (activePage) {
      activePage.SetAsLastSibling();
    }
  }

  private void RemovePage(RectTransform page) {
    int index = visiblePageToIndex[page];
    int pageIndex = PageIndexFromRealIndex(index);

    visiblePageToIndex.Remove(page);
    indexToVisiblePage.Remove(index);

    // This could be slow if numExtraPagesShown is set to a large number.
    // Considering having numExtraPagesShown set above 1 is against UX recommendations,
    // this should be all right.
    visiblePages.Remove(page);

    pageProvider.RemovePage(pageIndex, page);
  }

  private void ApplyScrollEffects(RectTransform page) {
    int index = visiblePageToIndex[page];
    float offset = OffsetFromIndex(index);

    bool isActivePage = page == activePage;
    bool isInteractable = shouldTilesAlwaysBeInteractable || isActivePage;

    BaseScrollEffect.UpdateData updateData = new BaseScrollEffect.UpdateData();
    updateData.page = page;
    updateData.pageIndex = index;
    updateData.pageCount = PageCount;
    updateData.pageOffset = offset;
    updateData.scrollOffset = ScrollOffset;
    updateData.spacing = pageProvider.GetSpacing();
    updateData.looping = loop;
    updateData.isInteractable = isInteractable;
    updateData.moveDistance = CurrentMoveDistance;

    for (int i = 0; i < scrollEffects.Length; i++) {
      BaseScrollEffect scrollEffect = scrollEffects[i];
      if (scrollEffect.enabled) {
        scrollEffect.ApplyEffect(updateData);
      }
    }
  }

  private float GetMovingThreshold() {
    return pageProvider.GetSpacing() * kIsMovingThresholdCoeff;
  }

}

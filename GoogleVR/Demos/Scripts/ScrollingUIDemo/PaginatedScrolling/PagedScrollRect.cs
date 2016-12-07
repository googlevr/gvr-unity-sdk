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

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
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

  private bool canScroll = false;
  private bool isScrolling = false;
  private float scrollOffset = float.MaxValue;

  /// Lerp towards the target scroll offset to smooth the motion.
  private float targetScrollOffset;

  private RectTransform activePage;
  private Coroutine activeSnapCoroutine;

  /// Keep track of the currently visible pages
  private Dictionary<int, RectTransform> indexToVisiblePage = new Dictionary<int, RectTransform>();
  private Dictionary<RectTransform, int> visiblePageToIndex = new Dictionary<RectTransform, int>();

  /// Touch Delta is required to be higher than
  /// the click threshold to avoid detecting clicks as swipes.
  private const float kClickThreshold = 0.125f;

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
  private const float kSnapScrollOffsetThresholdCoeff = 0.02f;

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
      return canScroll;
    }
    set {
      if (canScroll == value) {
        return;
      }

      canScroll = value;

      if (!canScroll) {
        StopScrolling();
        StopTouchTracking();
      }
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

      float moveDistance = Mathf.Abs(targetScrollOffset - ScrollOffset);
      if (moveDistance > GetMovingThreshold()) {
        return true;
      }

      return false;
    }
  }

  /// <summary>
  /// Snaps the scroll rect to a particular page.
  /// </summary>
  /// <param name="index"> the index of the page to snap to.</param>
  /// <param name="immediate">If set to <c>true</c> then snapping happens instantly,
  /// otherwise it is animated.</param>
  public void SnapToPage(int index, bool immediate = false) {
    if (!loop && (index < 0 || index >= PageCount)) {
      Debug.LogWarning("Attempting to snap to non-existant page: " + index);
      return;
    }

    if (immediate) {
      ScrollOffset = OffsetFromIndex(index);
    } else {
      activeSnapCoroutine = StartCoroutine(SnapToPageCoroutine(index));
    }
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

    if (!onlyScrollWhenPointing) {
      CanScroll = true;
    }

    // Immediately snap to the starting page.
    SnapToPage(StartPage, true);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (onlyScrollWhenPointing) {
      CanScroll = true;
    }
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (onlyScrollWhenPointing) {
      CanScroll = false;
    }
  }

  void Update() {
    if (!CanScroll) {
      return;
    }

    /// Don't start scrolling until the touch pos has moved.
    /// This is to prevent scrolling when the user intended to click.
    if (!isScrolling && GvrController.IsTouching) {
      if (!isTrackingTouches) {
        StartTouchTracking();
      } else {
        Vector2 touchDelta = GvrController.TouchPos - initialTouchPos;
        float xDeltaMagnitude = Mathf.Abs(touchDelta.x);
        float yDeltaMagnitude = Mathf.Abs(touchDelta.y);

        if (xDeltaMagnitude > kClickThreshold && xDeltaMagnitude > yDeltaMagnitude) {
          StartScrolling();
        }
      }
    }

    if (isScrolling && GvrController.IsTouching) {
      Vector2 touchDelta = GvrController.TouchPos - previousTouchPos;

      if (Mathf.Abs(touchDelta.x) > 0) {
        // Translate directly based on the touch value.
        float spacingCoeff = -pageProvider.GetSpacing();
        targetScrollOffset += touchDelta.x * spacingCoeff * ScrollSensitivity;
      }

      LerpTowardsOffset(targetScrollOffset);
    }

    if (GvrController.TouchUp) {
      StopScrolling();
      StopTouchTracking();
    }

    if (isTrackingTouches && GvrController.IsTouching) {
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

  private void StopScrolling() {
    if (!isScrolling) {
      return;
    }

    if (overallVelocity.x > kSwipeThreshold) {
      /// If I was swiping to the right.
      SnapToPageInDirection(SnapDirection.Left);
    } else if (overallVelocity.x < -kSwipeThreshold) {
      /// If I was swiping to the left.
      SnapToPageInDirection(SnapDirection.Right);
    } else {
      /// If the touch delta is not big enough, just snap to the closest page.
      SnapToPageInDirection(SnapDirection.Closest);
    }

    isScrolling = false;
  }

  private void StartTouchTracking() {
    isTrackingTouches = true;
    initialTouchPos = GvrController.TouchPos;
    previousTouchPos = initialTouchPos;
    previousTouchTimestamp = Time.time;
    overallVelocity = Vector2.zero;
  }

  private void StopTouchTracking() {
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
    Vector2 touchDelta = GvrController.TouchPos - previousTouchPos;
    Vector2 velocity = touchDelta / timeElapsedSeconds;
    float weight = timeElapsedSeconds / (kRc + timeElapsedSeconds);
    overallVelocity = Vector2.Lerp(overallVelocity, velocity, weight);

    // Update the previous touch
    previousTouchPos = GvrController.TouchPos;
    previousTouchTimestamp = Time.time;
  }


  private void SnapToPageInDirection(SnapDirection snapDirection) {
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
    SnapToPage(closestPageIndex);
  }

  private void OnScrolled() {
    bool didClamp;
    int newActiveIndex = IndexFromOffset(scrollOffset, out didClamp);

    /// Make sure to update the active page
    if (IsPageVisible(newActiveIndex)) {
      ActivePage = indexToVisiblePage[newActiveIndex];
    }

    /// Update existing pages
    foreach (Transform pageTransform in transform) {
      RectTransform page = pageTransform.GetComponent<RectTransform>();

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

    if (numExtraPagesShown > 0) {
      return absoluteDiff <= pageProvider.GetSpacing() * numExtraPagesShown;
    }

    return absoluteDiff < pageProvider.GetSpacing();
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

    pageProvider.RemovePage(pageIndex, page);
  }

  private void ApplyScrollEffects(RectTransform page) {
    int index = visiblePageToIndex[page];
    float offset = OffsetFromIndex(index);

    bool isActivePage = page == activePage;
    bool isInteractable = !IsMoving && isActivePage;

    BaseScrollEffect.UpdateData updateData = new BaseScrollEffect.UpdateData();
    updateData.page = page;
    updateData.pageIndex = index;
    updateData.pageCount = PageCount;
    updateData.pageOffset = offset;
    updateData.scrollOffset = ScrollOffset;
    updateData.spacing = pageProvider.GetSpacing();
    updateData.looping = loop;
    updateData.isInteractable = isInteractable;

    foreach (BaseScrollEffect scrollEffect in scrollEffects) {
      if (scrollEffect.enabled) {
        scrollEffect.ApplyEffect(updateData);
      }
    }
  }

  private float GetMovingThreshold() {
    return pageProvider.GetSpacing() * kIsMovingThresholdCoeff;
  }
}
#endif  // UNITY_HAS_GOOGLEVR &&(UNITY_ANDROID || UNITY_EDITOR

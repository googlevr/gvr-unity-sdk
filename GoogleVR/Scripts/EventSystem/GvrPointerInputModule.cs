// Copyright 2016 Google Inc. All rights reserved.
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

using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
using UnityEngine.VR;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

/// This script provides an implemention of Unity's `BaseInputModule` class, so
/// that Canvas-based (_uGUI_) UI elements and 3D scene objects can be
/// interacted with in a Gvr Application.
///
/// This script is intended for use with either a
/// 3D pointer with the Daydream Controller (Recommended for Daydream),
/// or a Gaze-based-pointer (Recommended for Cardboard).
///
/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
/// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
/// for the pointer to take priority in the event system.
///
/// If you are using a **Canvas**, set the _Render Mode_ to **World Space**,
/// and add the _GvrPointerGraphicRaycaster_ script to the object.
///
/// If you'd like pointers to work with 3D scene objects, add a _GvrPointerPhysicsRaycaster_ to the main camera,
/// and add a component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely) to
/// an object with a collider.
///
/// GvrPointerInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
/// _Deselect_, _UpdateSelected_, and _GvrPointerHover_.  Scroll, move, and submit/cancel events are not emitted.
///
/// To use a 3D Pointer with the Daydream Controller:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrControllerPointer to your scene.
///   - Set the parent of GvrControllerPointer to the same parent as the main camera
///     (With a local position of 0,0,0).
///
/// To use a Gaze-based-pointer:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrReticlePointer to your scene.
///   - Set the parent of GvrReticlePointer to the main camera.
///
[AddComponentMenu("GoogleVR/GvrPointerInputModule")]
public class GvrPointerInputModule : BaseInputModule {
  /// Determines whether pointer input is active in VR Mode only (`true`), or all of the
  /// time (`false`).  Set to false if you plan to use direct screen taps or other
  /// input when not in VR Mode.
  [Tooltip("Whether pointer input is active in VR Mode only (true), or all the time (false).")]
  public bool vrModeOnly = false;

  private PointerEventData pointerData;
  private Vector2 lastPose;
  private Vector2 lastScroll;
  private bool eligibleForScroll = false;
  private bool isPointerHovering = false;

  // Active state
  private bool isActive = false;

  /// Time in seconds between the pointer down and up events sent by a trigger.
  /// Allows time for the UI elements to make their state transitions.
  private const float CLICK_TIME = 0.1f;
  // Based on default time for a button to animate to Pressed.

  /// Multiplier for calculating the scroll delta to that the scroll delta is
  /// within the order of magnitude that the UI system expects.
  private const float SCROLL_DELTA_MULTIPLIER = 100.0f;

  /// The GvrBasePointer which will be responding to pointer events.
  private GvrBasePointer pointer {
    get {
      return GvrPointerManager.Pointer;
    }
  }

  /// @cond
  public override bool ShouldActivateModule() {
    bool isVrModeEnabled = !vrModeOnly;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    isVrModeEnabled |= VRSettings.enabled;
#else
    isVrModeEnabled |= GvrViewer.Instance.VRModeEnabled;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    bool activeState = base.ShouldActivateModule() && isVrModeEnabled;

    if (activeState != isActive) {
      isActive = activeState;

      // Activate pointer
      if (pointer != null) {
        if (isActive) {
          pointer.OnInputModuleEnabled();
        }
      }
    }

    return activeState;
  }
  /// @endcond

  /// @cond
  public override void DeactivateModule() {
    DisablePointer();
    base.DeactivateModule();
    if (pointerData != null) {
      HandlePendingClick();
      HandlePointerExitAndEnter(pointerData, null);
      pointerData = null;
    }
    eventSystem.SetSelectedGameObject(null, GetBaseEventData());
  }
  /// @endcond

  /// @cond
  public override bool IsPointerOverGameObject(int pointerId) {
    return pointerData != null && pointerData.pointerEnter != null;
  }
  /// @endcond

  /// @cond
  public override void Process() {
    if (pointer == null) {
      return;
    }

    // Save the previous Game Object
    GameObject previousObject = GetCurrentGameObject();

    CastRay();
    UpdateCurrentObject(previousObject);
    UpdateReticle(previousObject);

    // True during the frame that the trigger has been pressed.
    bool triggerDown = Input.GetMouseButtonDown(0);
    // True if the trigger is held down.
    bool triggering = Input.GetMouseButton(0);

    #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    triggerDown |= GvrController.ClickButtonDown;
    triggering |= GvrController.ClickButton;
    #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    if (!IsPointerActiveAndAvailable()) {
      triggerDown = false;
      triggering = false;
    }

    bool handlePendingClickRequired = !triggering;

    // Handle input
    if (!triggerDown && triggering) {
      HandleDrag();
    } else if (pointerData != null && Time.unscaledTime - pointerData.clickTime < CLICK_TIME) {
      // Delay new events until clickTime has passed.
    } else if (triggerDown && !pointerData.eligibleForClick) {
      // New trigger action.
      HandleTriggerDown();
    } else if (handlePendingClickRequired) {
      // Check if there is a pending click to handle.
      HandlePendingClick();
    }

    HandleScroll();
  }
  /// @endcond

  private void CastRay() {
    if (pointer == null || pointer.PointerTransform == null) {
      return;
    }
    Vector2 currentPose = NormalizedCartesianToSpherical(pointer.PointerTransform.forward);

    if (pointerData == null) {
      pointerData = new PointerEventData(eventSystem);
      lastPose = currentPose;
    }

    // Store the previous raycast result.
    RaycastResult previousRaycastResult = pointerData.pointerCurrentRaycast;

    // The initial cast must use the enter radius.
    if (pointer != null) {
      pointer.ShouldUseExitRadiusForRaycast = false;
    }

    // Cast a ray into the scene
    pointerData.Reset();
    // Set the position to the center of the camera.
    // This is only necessary if using the built-in Unity raycasters.
    RaycastResult raycastResult;
    pointerData.position = GetViewportCenter();
    bool isPointerActiveAndAvailable = IsPointerActiveAndAvailable();
    if (isPointerActiveAndAvailable) {
      eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
      raycastResult = FindFirstRaycast(m_RaycastResultCache);
    } else {
      raycastResult = new RaycastResult();
      raycastResult.Clear();
    }

    // If we were already pointing at an object we must check that object against the exit radius
    // to make sure we are no longer pointing at it to prevent flicker.
    if (previousRaycastResult.gameObject != null
        && raycastResult.gameObject != previousRaycastResult.gameObject
        && isPointerActiveAndAvailable) {
      if (pointer != null) {
        pointer.ShouldUseExitRadiusForRaycast = true;
      }
      m_RaycastResultCache.Clear();
      eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
      RaycastResult firstResult = FindFirstRaycast(m_RaycastResultCache);
      if (firstResult.gameObject == previousRaycastResult.gameObject) {
        raycastResult = firstResult;
      }
    }

    if (raycastResult.gameObject != null && raycastResult.worldPosition == Vector3.zero) {
      raycastResult.worldPosition = GetIntersectionPosition(pointerData.enterEventCamera, raycastResult);
    }

    pointerData.pointerCurrentRaycast = raycastResult;

    // Find the real screen position associated with the raycast
    // Based on the results of the hit and the state of the pointerData.
    if (raycastResult.gameObject != null) {
      pointerData.position = raycastResult.screenPosition;
    } else {
      Transform pointerTransform = pointer.PointerTransform;
      float maxPointerDistance = pointer.MaxPointerDistance;
      Vector3 pointerPos = pointerTransform.position + (pointerTransform.forward * maxPointerDistance);
      if (pointerData.pressEventCamera != null) {
        pointerData.position = pointerData.pressEventCamera.WorldToScreenPoint(pointerPos);
      } else if (Camera.main != null) {
        pointerData.position = Camera.main.WorldToScreenPoint(pointerPos);
      }
    }

    m_RaycastResultCache.Clear();
    pointerData.delta = currentPose - lastPose;
    lastPose = currentPose;

    // Check to make sure the Raycaster being used is a GvrRaycaster.
    if (raycastResult.module != null
      && !(raycastResult.module is GvrPointerGraphicRaycaster)
      && !(raycastResult.module is GvrPointerPhysicsRaycaster)) {
      Debug.LogWarning("Using Raycaster (Raycaster: " + raycastResult.module.GetType() +
        ", Object: " + raycastResult.module.name + "). It is recommended to use " +
        "GvrPointerPhysicsRaycaster or GvrPointerGrahpicRaycaster with GvrPointerInputModule.");
    }
  }

  private void UpdateCurrentObject(GameObject previousObject) {
    if (pointer == null || pointerData == null) {
      return;
    }
    // Send enter events and update the highlight.
    GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
    HandlePointerExitAndEnter(pointerData, previousObject);

    // Update the current selection, or clear if it is no longer the current object.
    var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(currentObject);
    if (selected == eventSystem.currentSelectedGameObject) {
      ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
        ExecuteEvents.updateSelectedHandler);
    } else {
      eventSystem.SetSelectedGameObject(null, pointerData);
    }

    // Execute hover event.
    if (currentObject == previousObject) {
      ExecuteEvents.ExecuteHierarchy(currentObject, pointerData, GvrExecuteEventsExtension.pointerHoverHandler);
    }
  }

  private void UpdateReticle(GameObject previousObject) {
    if (pointer == null || pointerData == null) {
      return;
    }

    GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
    Vector3 intersectionPosition = pointerData.pointerCurrentRaycast.worldPosition;
    bool isInteractive = pointerData.pointerPress != null ||
                         ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject) != null ||
                         ExecuteEvents.GetEventHandler<IDragHandler>(currentObject) != null;

    if (isPointerHovering && currentObject != null && currentObject == previousObject) {
        pointer.OnPointerHover(currentObject, intersectionPosition, GetLastRay(), isInteractive);
    } else {
      // If the object's don't match or the hovering object has been destroyed
      // then the pointer has exited.
      if (previousObject != null || (currentObject == null && isPointerHovering)) {
        pointer.OnPointerExit(previousObject);
        isPointerHovering = false;
      }

      if (currentObject != null) {
        pointer.OnPointerEnter(currentObject, intersectionPosition, GetLastRay(), isInteractive);
        isPointerHovering = true;
      }
    }
  }

  private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
  {
    if (!useDragThreshold)
      return true;

    return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
  }

  private void HandleDrag() {
    bool moving = pointerData.IsPointerMoving();
    bool shouldStartDrag = ShouldStartDrag(pointerData.pressPosition,
      pointerData.position,
      eventSystem.pixelDragThreshold,
      pointerData.useDragThreshold);

    if (moving && shouldStartDrag && pointerData.pointerDrag != null && !pointerData.dragging) {
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
        ExecuteEvents.beginDragHandler);
      pointerData.dragging = true;
    }

    // Drag notification
    if (pointerData.dragging && moving && pointerData.pointerDrag != null) {
      // Before doing drag we should cancel any pointer down state
      // And clear selection!
      if (pointerData.pointerPress != pointerData.pointerDrag) {
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

        pointerData.eligibleForClick = false;
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
      }

      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
    }
  }

  private void HandlePendingClick() {
    if (pointerData == null || (!pointerData.eligibleForClick && !pointerData.dragging)) {
      return;
    }

    if (pointer != null) {
      pointer.OnPointerClickUp();
    }

    var go = pointerData.pointerCurrentRaycast.gameObject;

    // Send pointer up and click events.
    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
    GameObject pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
    if (pointerData.pointerPress == pointerClickHandler && pointerData.eligibleForClick) {
      ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
    } else if (pointerData.dragging) {
      ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
    }

    // Clear the click state.
    pointerData.pointerPress = null;
    pointerData.rawPointerPress = null;
    pointerData.eligibleForClick = false;
    pointerData.clickCount = 0;
    pointerData.clickTime = 0;
    pointerData.pointerDrag = null;
    pointerData.dragging = false;
  }

  private void HandleTriggerDown() {
    var go = pointerData.pointerCurrentRaycast.gameObject;

    // Send pointer down event.
    pointerData.pressPosition = pointerData.position;
    pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
    pointerData.pointerPress =
      ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
    ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

    // Save the pending click state.
    pointerData.rawPointerPress = go;
    pointerData.eligibleForClick = true;
    pointerData.delta = Vector2.zero;
    pointerData.dragging = false;
    pointerData.useDragThreshold = true;
    pointerData.clickCount = 1;
    pointerData.clickTime = Time.unscaledTime;

    // Save the drag handler as well
    pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
    if (pointerData.pointerDrag != null) {
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
    }

    if (pointer != null) {
      pointer.OnPointerClickDown();
    }
  }

  private void HandleScroll() {
    bool touchDown = false;
    bool touching = false;
    Vector2 currentScroll = Vector2.zero;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    touchDown |= GvrController.TouchDown;
    touching |= GvrController.IsTouching;
    currentScroll = GvrController.TouchPos;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    if (!IsPointerActiveAndAvailable()) {
      touchDown = false;
      touching = false;
    }

    if (touchDown && !eligibleForScroll) {
      lastScroll = currentScroll;
      eligibleForScroll = true;
    } else if (touching && eligibleForScroll) {
      pointerData.scrollDelta = (currentScroll - lastScroll) * SCROLL_DELTA_MULTIPLIER;
      lastScroll = currentScroll;

      GameObject currentGameObject = GetCurrentGameObject();
      if (currentGameObject != null && !Mathf.Approximately(pointerData.scrollDelta.sqrMagnitude, 0.0f)) {
        GameObject scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(currentGameObject);
        ExecuteEvents.ExecuteHierarchy(scrollHandler, pointerData, ExecuteEvents.scrollHandler);
      }
    } else if (eligibleForScroll) {
      eligibleForScroll = false;
      pointerData.scrollDelta = Vector2.zero;
    }
  }

  private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
    cartCoords.Normalize();
    if (cartCoords.x == 0)
      cartCoords.x = Mathf.Epsilon;
    float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
    if (cartCoords.x < 0)
      outPolar += Mathf.PI;
    float outElevation = Mathf.Asin(cartCoords.y);
    return new Vector2(outPolar, outElevation);
  }

  private GameObject GetCurrentGameObject() {
    if (pointerData != null) {
      return pointerData.pointerCurrentRaycast.gameObject;
    }

    return null;
  }

  private Ray GetLastRay() {
    if (pointerData != null) {
      GvrBasePointerRaycaster raycaster = pointerData.pointerCurrentRaycast.module as GvrBasePointerRaycaster;
      if (raycaster != null) {
        return raycaster.GetLastRay();
      } else if (pointerData.enterEventCamera != null) {
        Camera cam = pointerData.enterEventCamera;
        return new Ray(cam.transform.position, cam.transform.forward);
      }
    }

    return new Ray();
  }

  private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult) {
    // Check for camera
    if (cam == null) {
      return Vector3.zero;
    }

    float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
    Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
    return intersectionPosition;
  }

  private void DisablePointer() {
    if (pointer == null) {
      return;
    }

    GameObject currentGameObject = GetCurrentGameObject();
    if (currentGameObject) {
      pointer.OnPointerExit(currentGameObject);
    }

    pointer.OnInputModuleDisabled();
  }

  private Vector2 GetViewportCenter() {
    int viewportWidth = Screen.width;
    int viewportHeight = Screen.height;
    #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID
    // GVR native integration is supported.
    if (VRSettings.enabled) {
      viewportWidth = VRSettings.eyeTextureWidth;
      viewportHeight = VRSettings.eyeTextureHeight;
    }
    #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID

    return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
  }

  private bool IsPointerActiveAndAvailable() {
    if (pointer == null) {
      return false;
    }

    Transform pointerTransform = pointer.PointerTransform;
    if (pointerTransform == null) {
      return false;
    }

    return pointerTransform.gameObject.activeInHierarchy;
  }
}

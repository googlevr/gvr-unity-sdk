// The MIT License (MIT)
//
// Copyright (c) 2015, Unity Technologies & Google, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.

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
  private Vector2 lastHeadPose;

  // Active state
  private bool isActive = false;

  /// Time in seconds between the pointer down and up events sent by a trigger.
  /// Allows time for the UI elements to make their state transitions.
  private const float clickTime = 0.1f;
  // Based on default time for a button to animate to Pressed.

  /// The IGvrPointer which will be responding to pointer events.
  private IGvrPointer pointer {
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

  public override bool IsPointerOverGameObject(int pointerId) {
    return pointerData != null && pointerData.pointerEnter != null;
  }

  public override void Process() {
    // Save the previous Game Object
    GameObject previousObject = GetCurrentGameObject();

    CastRay();
    UpdateCurrentObject(previousObject);
    UpdateReticle(previousObject);

    bool isGvrTriggered = Input.GetMouseButtonDown(0);
    bool handlePendingClickRequired = !Input.GetMouseButton(0);

    #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    handlePendingClickRequired &= !GvrController.ClickButton;
    isGvrTriggered |= GvrController.ClickButtonDown;
    #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    // Handle input
    if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0)) {
      HandleDrag();
    } else if (Time.unscaledTime - pointerData.clickTime < clickTime) {
      // Delay new events until clickTime has passed.
    } else if (!pointerData.eligibleForClick &&
               (isGvrTriggered || Input.GetMouseButtonDown(0))) {
      // New trigger action.
      HandleTrigger();
    } else if (handlePendingClickRequired) {
      // Check if there is a pending click to handle.
      HandlePendingClick();
    }
  }

  /// @endcond

  private void CastRay() {
    Quaternion headOrientation;
    #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    headOrientation = InputTracking.GetLocalRotation(VRNode.Head);
    #else
    headOrientation = GvrViewer.Instance.HeadPose.Orientation;
    #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

    Vector2 headPose = NormalizedCartesianToSpherical(headOrientation * Vector3.forward);

    if (pointerData == null) {
      pointerData = new PointerEventData(eventSystem);
      lastHeadPose = headPose;
    }

    // Cast a ray into the scene
    pointerData.Reset();
    pointerData.position = GetPointerPosition();
    eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
    RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);
    if (raycastResult.worldPosition == Vector3.zero) {
      raycastResult.worldPosition = GetIntersectionPosition(pointerData.enterEventCamera, raycastResult);
    }

    pointerData.pointerCurrentRaycast = raycastResult;
    m_RaycastResultCache.Clear();
    pointerData.delta = headPose - lastHeadPose;
    lastHeadPose = headPose;
  }

  private void UpdateCurrentObject(GameObject previousObject) {
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
      ExecuteEvents.Execute(currentObject, pointerData, GvrExecuteEventsExtension.pointerHoverHandler);
    }
  }

  private void UpdateReticle(GameObject previousObject) {
    if (pointer == null) {
      return;
    }

    Camera camera = pointerData.enterEventCamera; // Get the camera
    GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
    Vector3 intersectionPosition = pointerData.pointerCurrentRaycast.worldPosition;
    bool isInteractive = pointerData.pointerPress != null ||
                         ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject) != null;

    if (currentObject == previousObject) {
      if (currentObject != null) {
        pointer.OnPointerHover(currentObject, intersectionPosition, GetLastRay(), isInteractive);
      }
    } else {
      if (previousObject != null) {
        pointer.OnPointerExit(previousObject);
      }

      if (currentObject != null) {
        pointer.OnPointerEnter(currentObject, intersectionPosition, GetLastRay(), isInteractive);
      }
    }
  }

  private void HandleDrag() {
    bool moving = pointerData.IsPointerMoving();

    if (moving && pointerData.pointerDrag != null && !pointerData.dragging) {
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
    if (!pointerData.eligibleForClick && !pointerData.dragging) {
      return;
    }

    if (pointer != null) {
      Camera camera = pointerData.enterEventCamera;
      pointer.OnPointerClickUp();
    }

    var go = pointerData.pointerCurrentRaycast.gameObject;

    // Send pointer up and click events.
    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
    if (pointerData.eligibleForClick) {
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

  private void HandleTrigger() {
    var go = pointerData.pointerCurrentRaycast.gameObject;

    // Send pointer down event.
    pointerData.pressPosition = pointerData.position;
    pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
    pointerData.pointerPress =
      ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
    ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

    // Save the drag handler as well
    pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
    if (pointerData.pointerDrag != null) {
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
    }

    // Save the pending click state.
    pointerData.rawPointerPress = go;
    pointerData.eligibleForClick = true;
    pointerData.delta = Vector2.zero;
    pointerData.dragging = false;
    pointerData.useDragThreshold = true;
    pointerData.clickCount = 1;
    pointerData.clickTime = Time.unscaledTime;

    if (pointer != null) {
      pointer.OnPointerClickDown();
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
      Camera camera = pointerData.enterEventCamera;
      pointer.OnPointerExit(currentGameObject);
    }

    pointer.OnInputModuleDisabled();
  }

  private Vector2 GetPointerPosition() {
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
}

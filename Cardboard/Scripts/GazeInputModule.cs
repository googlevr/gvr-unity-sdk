// The MIT License (MIT)
//
// Copyright (c) 2014, Unity Technologies & Google, Inc.
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

/// @ingroup Scripts
/// This script provides an implemention of Unity's `BaseInputModule` class, so
/// that Canvas-based UI elements (_uGUI_) can be selected by looking at them and
/// pulling the trigger or touching the screen.
/// This uses the player's gaze and the magnet trigger as a raycast generator.
///
/// To use, attach to the scene's EventSystem object.  Set the Canvas
/// object's Render Mode to World Space, and set its Event Camera to a (mono) camera that is
/// controlled by a CardboardHead.  If you'd like gaze to work with 3D scene objects, add a
/// PhysicsRaycaster to the gazing camera, and add a component that implements one of the Event
/// interfaces (EventTrigger will work nicely).  The objects must have colliders too.
///
/// GazeInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
/// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
public class GazeInputModule : BaseInputModule {
  /// Determines whether gaze input is active in VR Mode only (`true`), or all of the
  /// time (`false`).  Set to false if you plan to use direct screen taps or other
  /// input when not in VR Mode.
  [Tooltip("Whether gaze input is active in VR Mode only (true), or all the time (false).")]
  public bool vrModeOnly = false;

  /// An optional object to be placed at a raycast intersection, acting as a 3D
  /// cursor.  **Important:** Be sure to set any raycasters to ignore the layer that
  /// this object is in.
  [Tooltip("Optional object to place at raycast intersections as a 3D cursor. " +
           "Be sure it is on a layer that raycasts will ignore.")]
  public GameObject cursor;

  /// If cursor is not null, whether to show the cursor when a raycast occurs.
  public bool showCursor = true;

  /// If cursor is to be shown, whether to scale its size in order to appear the same size visually
  /// regardless of its distance.
  public bool scaleCursorSize = true;

  /// Time in seconds between the pointer down and up events sent by a magnet click.
  /// Allows time for the UI elements to make their state transitions.
  [HideInInspector]
  public float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.

  /// The pixel through which to cast rays, in viewport coordinates.  Generally, the center
  /// pixel is best, assuming a monoscopic camera is selected as the `Canvas`' event camera.
  [HideInInspector]
  public Vector2 hotspot = new Vector2(0.5f, 0.5f);

  private PointerEventData pointerData;
  private Vector2 lastHeadPose;

  /// @cond HIDDEN
  public override bool ShouldActivateModule() {
    if (!base.ShouldActivateModule()) {
      return false;
    }
    return Cardboard.SDK.VRModeEnabled || !vrModeOnly;
  }

  public override void DeactivateModule() {
    base.DeactivateModule();
    if (pointerData != null) {
      HandlePendingClick();
      HandlePointerExitAndEnter(pointerData, null);
      pointerData = null;
    }
    eventSystem.SetSelectedGameObject(null, GetBaseEventData());
    if (cursor != null) {
      cursor.SetActive(false);
    }
  }

  public override bool IsPointerOverGameObject(int pointerId) {
    return pointerData != null && pointerData.pointerEnter != null;
  }

  public override void Process() {
    CastRayFromGaze();
    UpdateCurrentObject();
    PlaceCursor();

    if (!Cardboard.SDK.TapIsTrigger && !Input.GetMouseButtonDown(0) && Input.GetMouseButton(0)) {
      // Drag is only supported if TapIsTrigger is false.
      HandleDrag();
    } else if (Time.unscaledTime - pointerData.clickTime < clickTime) {
      // Delay new events until clickTime has passed.
    } else if (!pointerData.eligibleForClick &&
               (Cardboard.SDK.Triggered || !Cardboard.SDK.TapIsTrigger && Input.GetMouseButtonDown(0))) {
      // New trigger action.
      HandleTrigger();
    } else if (!Cardboard.SDK.Triggered && !Input.GetMouseButton(0)) {
      // Check if there is a pending click to handle.
      HandlePendingClick();
    }
  }
  /// @endcond

  private void CastRayFromGaze() {
    Vector2 headPose = NormalizedCartesianToSpherical(Cardboard.SDK.HeadPose.Orientation * Vector3.forward);

    if (pointerData == null) {
      pointerData = new PointerEventData(eventSystem);
      lastHeadPose = headPose;
    }

    pointerData.Reset();
    pointerData.position = new Vector2(hotspot.x * Screen.width, hotspot.y * Screen.height);
    eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
    pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
    m_RaycastResultCache.Clear();
    pointerData.delta = headPose - lastHeadPose;
    lastHeadPose = headPose;
  }

  private void UpdateCurrentObject() {
    // Send enter events and update the highlight.
    var go = pointerData.pointerCurrentRaycast.gameObject;
    HandlePointerExitAndEnter(pointerData, go);
    // Update the current selection, or clear if it is no longer the current object.
    var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
    if (selected == eventSystem.currentSelectedGameObject) {
      ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
                            ExecuteEvents.updateSelectedHandler);
    }
    else {
      eventSystem.SetSelectedGameObject(null, pointerData);
    }
  }

  private void PlaceCursor() {
    if (cursor == null) {
      return;
    }
    var go = pointerData.pointerCurrentRaycast.gameObject;
    Camera cam = pointerData.enterEventCamera;  // Will be null for overlay hits.
    cursor.SetActive(go != null && cam != null && showCursor);
    if (cursor.activeInHierarchy) {
      // Note: rays through screen start at near clipping plane.
      float dist = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
      cursor.transform.position = cam.transform.position + cam.transform.forward * dist;
      if (scaleCursorSize) {
        cursor.transform.localScale = Vector3.one * dist;
      }
    }
  }

  private void HandleDrag() {
    bool moving = pointerData.IsPointerMoving();

    if (moving && pointerData.pointerDrag != null && !pointerData.dragging) {
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.beginDragHandler);
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
    if (!pointerData.eligibleForClick) {
      return;
    }
    var go = pointerData.pointerCurrentRaycast.gameObject;

    // Send pointer up and click events.
    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

    if (pointerData.pointerDrag != null) {
      ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
    }

    if (pointerData.pointerDrag != null && pointerData.dragging) {
      ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
    }

    // Clear the click state.
    pointerData.pointerPress = null;
    pointerData.rawPointerPress = null;
    pointerData.eligibleForClick = false;
    pointerData.clickCount = 0;
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
    if (pointerData.pointerDrag != null && !Cardboard.SDK.TapIsTrigger) {
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
}

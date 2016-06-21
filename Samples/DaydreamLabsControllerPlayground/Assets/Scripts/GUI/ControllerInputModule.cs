// The MIT License (MIT)
//
// Copyright (c) 2016, Unity Technologies & Google, Inc.
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

namespace GVR.GUI {
  /// <summary>
  /// An input module that takes position and rotation of a specified object
  /// and uses the controller to check for click down and up. This input module is
  /// lightweight and does not support drag events.
  /// </summary>
  public class ControllerInputModule : PointerInputModule {
    #region -- Constants --------------------------------------------------

    public const int POINTER_ID = 0;
    public const string GUI_CAMERA_NAME = "GUICamera";

    #endregion -- Constants -----------------------------------------------

    #region -- Inspector Variables ----------------------------------------

    [Tooltip("The GameObject to place at the location where a pointer hits UI.")]
    public GameObject Cursor;

    [Tooltip("The GameObject to place at an estimated UI depth when nothing is hit.")]
    public GameObject MissCursor;

    [Tooltip("Transform for the input controller. The position and rotation of this will be " +
             "used to raycast into the UI.")]
    public Transform Controller;

    [Tooltip("How far the line will render if there is no UI hit")]
    public float MaxLineLength = 10.0f;

    [Tooltip("Line Renderer to use for the cursor.")]
    public LineRenderer Line;

    [Tooltip("Camera used to project input to the UI.")]
    public Camera GuiCamera;

    #endregion -- Inspector Variables -------------------------------------

    #region -- Private Variables ------------------------------------------

    /// <summary>
    /// Transform for the UI camera, positions and rotates with the input
    /// device.
    /// </summary>
    private Transform guiCameraTransform;

    /// <summary> GameObject currently under the UI cursor. </summary>
    private GameObject selected;

    #endregion -- Private Variables ---------------------------------------

    protected override void Start() {
      base.Start();
      guiCameraTransform = GuiCamera.transform;

      Canvas[] canvases = FindObjectsOfType<Canvas>();
      for (int i = 0; i < canvases.Length; i++) {
        canvases[i].worldCamera = GuiCamera;
      }
    }

    public override void Process() {
      Controller.localRotation = GvrController.Orientation;

      guiCameraTransform.position = Controller.position;
      guiCameraTransform.rotation = Controller.rotation;

      PointerEventData pointer = ProcessPointer();
      selected = pointer.pointerCurrentRaycast.gameObject;

      // handle enter and exit events (highlight)
      HandlePointerExitAndEnter(pointer, selected);
      ProcessTrigger(pointer);

      if (pointer.pointerEnter != null) {
        RectTransform draggingPlane = pointer.pointerEnter.GetComponent<RectTransform>();
        Vector3 globalLookPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane,
              pointer.position, pointer.enterEventCamera, out globalLookPos)) {
          if (!Cursor.activeSelf) {
            Cursor.SetActive(true);
            MissCursor.SetActive(false);
          }
          Cursor.transform.position = globalLookPos;
          Cursor.transform.rotation = draggingPlane.rotation;
          MissCursor.transform.position = Cursor.transform.position;
          MissCursor.transform.rotation = Cursor.transform.rotation;
          if (Line != null) {
            Line.SetPosition(0, Controller.position);
            Line.SetPosition(1, Cursor.transform.position);
          }
        }
      } else {
        if (Cursor.activeSelf) {
          Cursor.SetActive(false);
          MissCursor.SetActive(true);
          MissCursor.transform.localPosition =
              new Vector3(0.0f, 0.0f, MissCursor.transform.localPosition.z);
        }
        if (Line != null) {
          Line.SetPosition(0, Controller.position);
          Line.SetPosition(1, Controller.position + Controller.forward * MaxLineLength);
        }
      }
    }

    private PointerEventData ProcessPointer() {
      PointerEventData pointerEventData;

      GetPointerData(POINTER_ID, out pointerEventData, true);
      pointerEventData.Reset();

      // Center if the camera on the controller
      Vector2 screenPosition = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);

      pointerEventData.position = screenPosition;
      //pointerEventData.button = PointerEventData.InputButton.Left;

      // Save the raycast results so we can query them later for bubbling
      m_RaycastResultCache.Clear();
      eventSystem.RaycastAll(pointerEventData, m_RaycastResultCache);
      pointerEventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

      return pointerEventData;
    }

    private void ProcessTrigger(PointerEventData pointer) {
      if (pointer.eligibleForClick) {
        ExecuteEvents.Execute(pointer.pointerPress, pointer, ExecuteEvents.pointerUpHandler);
        GameObject currentOverObject = pointer.pointerCurrentRaycast.gameObject;
        GameObject clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverObject);
        if (pointer.pointerPress == clickHandler) {
          ExecuteEvents.Execute(pointer.pointerPress, pointer, ExecuteEvents.pointerClickHandler);
        }
        pointer.eligibleForClick = false;
        pointer.pointerPress = null;
        pointer.rawPointerPress = null;
        pointer.dragging = false;
        pointer.pointerDrag = null;
        if (currentOverObject != pointer.pointerEnter) {
          HandlePointerExitAndEnter(pointer, null);
          HandlePointerExitAndEnter(pointer, currentOverObject);
        }
      } else if (GvrController.ClickButtonDown) {
        pointer.eligibleForClick = true;
        pointer.delta = Vector2.zero;
        pointer.dragging = false;
        pointer.useDragThreshold = true;
        pointer.pressPosition = pointer.position;
        pointer.pointerPressRaycast = pointer.pointerCurrentRaycast;
        GameObject currentOverObject = pointer.pointerCurrentRaycast.gameObject;
        DeselectIfSelectionChanged(currentOverObject, pointer);
        GameObject downHandler = ExecuteEvents.ExecuteHierarchy(currentOverObject, pointer,
                                                                ExecuteEvents.pointerDownHandler);
        if (!downHandler) {
          downHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverObject);
        }
        pointer.pointerPress = downHandler;
        pointer.rawPointerPress = currentOverObject;
        pointer.clickTime = Time.unscaledTime;
        pointer.pointerDrag = null;
      }
    }
  }
}

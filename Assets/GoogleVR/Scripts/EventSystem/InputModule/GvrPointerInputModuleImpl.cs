//-----------------------------------------------------------------------
// <copyright file="GvrPointerInputModuleImpl.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
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
// </copyright>
//-----------------------------------------------------------------------

using System;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// <summary>Implementation of `GvrPointerInputModule`.</summary>
public class GvrPointerInputModuleImpl
{
    private GvrBasePointer pointer;
    private Vector2 lastPose;
    private bool isPointerHovering = false;

    // Active state
    private bool isActive = false;

    /// <summary>Gets or sets an interface for controlling the actual input module.</summary>
    /// <value>The module controller interface for controlling the input module.</value>
    public IGvrInputModuleController ModuleController { get; set; }

    /// <summary>Gets or sets an interface for executing events.</summary>
    /// <value>An event executor interface for executing events.</value>
    public IGvrEventExecutor EventExecutor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer input is active in VR Mode only or
    /// whether it is always active.
    /// </summary>
    /// <remarks>
    /// Set to false if you plan to use direct screen taps or other input when not in VR Mode.
    /// </remarks>
    /// <value>
    /// Set this to `true` to make input active only in VR Mode.  Set to `false` to make input
    /// active all of the time.
    /// </value>
    public bool VrModeOnly { get; set; }

    /// <summary>
    /// Gets or sets the `GvrPointerScrollInput` used to route Scroll events through `EventSystem`.
    /// </summary>
    /// <value>The GvrPointerScrollInput used to route Scroll events through `EventSystem`.</value>
    public GvrPointerScrollInput ScrollInput { get; set; }

    /// <summary>
    /// Gets `PointerEventData` from the most recent frame.
    /// </summary>
    /// <value>The `PointerEventData` data from the most recent frame.</value>
    public GvrPointerEventData CurrentEventData { get; private set; }

    /// <summary>
    /// Gets or sets the `GvrBasePointer` which will be responding to pointer events.
    /// </summary>
    /// <value>The `GvrBasePointer` which will be responding to pointer events.</value>
    public GvrBasePointer Pointer
    {
        get
        {
            return pointer;
        }

        set
        {
            if (pointer == value)
            {
                return;
            }

            TryExitPointer();

            pointer = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this module should be activated.
    /// </summary>
    /// <returns>Returns `true` if this module should be activated, `false` otherwise.</returns>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public bool ShouldActivateModule()
    {
        bool isVrModeEnabled = !VrModeOnly;
        isVrModeEnabled |= XRSettings.enabled;

        bool activeState = ModuleController.ShouldActivate() && isVrModeEnabled;

        if (activeState != isActive)
        {
            isActive = activeState;
        }

        return activeState;
    }

    /// <summary>Deactivates this input module.</summary>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public void DeactivateModule()
    {
        TryExitPointer();
        ModuleController.Deactivate();
        if (CurrentEventData != null)
        {
            HandlePendingClick();
            HandlePointerExitAndEnter(CurrentEventData, null);
            CurrentEventData = null;
        }

        ModuleController.eventSystem.SetSelectedGameObject(null,
                                                           ModuleController.GetBaseEventData());
    }

    /// <summary>Determines whether the pointer is over a game object.</summary>
    /// <returns>
    /// Returns `true` if this instance is pointer over game object the specified `pointerId`.
    /// Returns `false` otherwise.
    /// </returns>
    /// <param name="pointerId">The pointer id to check.</param>
    public bool IsPointerOverGameObject(int pointerId)
    {
        return CurrentEventData != null &&
               CurrentEventData.pointerEnter != null &&
               CurrentEventData.pointerId == pointerId;
    }

    /// <summary>Process the input for the current frame.</summary>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public void Process()
    {
        // If the pointer is inactive, make sure it is exited if necessary.
        if (!IsPointerActiveAndAvailable())
        {
            TryExitPointer();
        }

        // Save the previous Game Object
        GameObject previousObject = GetCurrentGameObject();

        CastRay();
        UpdateCurrentObject(previousObject);
        UpdatePointer(previousObject);

        // True during the frame that the trigger has been pressed.
        bool triggerDown = false;

        // True if the trigger is held down.
        bool triggering = false;

        if (IsPointerActiveAndAvailable())
        {
            triggerDown = Pointer.TriggerDown;
            triggering = Pointer.Triggering;
        }

        bool handlePendingClickRequired = !triggering;

        // Handle input
        if (!triggerDown && triggering)
        {
            HandleDrag();
        }
        else if (triggerDown && !CurrentEventData.eligibleForClick)
        {
            // New trigger action.
            HandleTriggerDown();
        }
        else if (handlePendingClickRequired)
        {
            // Check if there is a pending click to handle.
            HandlePendingClick();
        }

        ScrollInput.HandleScroll(GetCurrentGameObject(), CurrentEventData, Pointer, EventExecutor);
    }

    private static bool ShouldStartDrag(Vector2 pressPos,
                                        Vector2 currentPos,
                                        float threshold,
                                        bool useDragThreshold)
    {
        if (!useDragThreshold)
        {
            return true;
        }

        return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
    }

    private void CastRay()
    {
        Vector2 currentPose = lastPose;
        if (IsPointerActiveAndAvailable())
        {
            currentPose = GvrMathHelpers.NormalizedCartesianToSpherical(
                    Pointer.PointerTransform.forward);
        }

        if (CurrentEventData == null)
        {
            CurrentEventData = new GvrPointerEventData(ModuleController.eventSystem);
            lastPose = currentPose;
        }

        // Store the previous raycast result.
        RaycastResult previousRaycastResult = CurrentEventData.pointerCurrentRaycast;

        // The initial cast must use the enter radius.
        if (IsPointerActiveAndAvailable())
        {
            Pointer.ShouldUseExitRadiusForRaycast = false;
        }

        // Cast a ray into the scene
        CurrentEventData.Reset();

        // Set the position to the center of the camera.
        // This is only necessary if using the built-in Unity raycasters.
        RaycastResult raycastResult;
        CurrentEventData.position = GvrVRHelpers.GetViewportCenter();
        bool isPointerActiveAndAvailable = IsPointerActiveAndAvailable();
        if (isPointerActiveAndAvailable)
        {
            RaycastAll();
            raycastResult = ModuleController.FindFirstRaycast(ModuleController.RaycastResultCache);
            if (Pointer.ControllerInputDevice == null
                || Pointer.ControllerInputDevice.IsDominantHand)
            {
                CurrentEventData.pointerId = (int)GvrControllerHand.Dominant;
            }
            else
            {
                CurrentEventData.pointerId = (int)GvrControllerHand.NonDominant;
            }
        }
        else
        {
            raycastResult = new RaycastResult();
            raycastResult.Clear();
        }

        // If we were already pointing at an object we must check that object against the exit
        // radius to make sure we are no longer pointing at it to prevent flicker.
        if (previousRaycastResult.gameObject != null
              && raycastResult.gameObject != previousRaycastResult.gameObject
              && isPointerActiveAndAvailable)
        {
            Pointer.ShouldUseExitRadiusForRaycast = true;
            RaycastAll();
            RaycastResult firstResult = ModuleController.FindFirstRaycast(
                ModuleController.RaycastResultCache);
            if (firstResult.gameObject == previousRaycastResult.gameObject)
            {
                raycastResult = firstResult;
            }
        }

        if (raycastResult.gameObject != null && raycastResult.worldPosition == Vector3.zero)
        {
            raycastResult.worldPosition = GvrMathHelpers.GetIntersectionPosition(
                CurrentEventData.enterEventCamera, raycastResult);
        }

        CurrentEventData.pointerCurrentRaycast = raycastResult;

        // Find the real screen position associated with the raycast
        // Based on the results of the hit and the state of the pointerData.
        if (raycastResult.gameObject != null)
        {
            CurrentEventData.position = raycastResult.screenPosition;
        }
        else if (IsPointerActiveAndAvailable() && CurrentEventData.enterEventCamera != null)
        {
            Vector3 pointerPos = Pointer.MaxPointerEndPoint;
            CurrentEventData.position =
                CurrentEventData.enterEventCamera.WorldToScreenPoint(pointerPos);
        }

        ModuleController.RaycastResultCache.Clear();
        CurrentEventData.delta = currentPose - lastPose;
        lastPose = currentPose;

        // Check to make sure the Raycaster being used is a GvrRaycaster.
        if (raycastResult.module != null
              && !(raycastResult.module is GvrPointerGraphicRaycaster)
              && !(raycastResult.module is GvrPointerPhysicsRaycaster))
        {
            Debug.LogWarning("Using Raycaster (Raycaster: " + raycastResult.module.GetType() +
            ", Object: " + raycastResult.module.name + "). It is recommended to use " +
            "GvrPointerPhysicsRaycaster or GvrPointerGrahpicRaycaster with GvrPointerInputModule.");
        }
    }

    private void UpdateCurrentObject(GameObject previousObject)
    {
        if (CurrentEventData == null)
        {
            return;
        }

        // Send enter events and update the highlight.
        GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
        HandlePointerExitAndEnter(CurrentEventData, currentObject);

        // Update the current selection, or clear if it is no longer the current object.
        var selected = EventExecutor.GetEventHandler<ISelectHandler>(currentObject);
        if (selected == ModuleController.eventSystem.currentSelectedGameObject)
        {
            EventExecutor.Execute(ModuleController.eventSystem.currentSelectedGameObject,
                                  ModuleController.GetBaseEventData(),
                                  ExecuteEvents.updateSelectedHandler);
        }
        else
        {
            ModuleController.eventSystem.SetSelectedGameObject(null, CurrentEventData);
        }

        // Execute hover event.
        if (currentObject != null && currentObject == previousObject)
        {
            EventExecutor.ExecuteHierarchy(currentObject,
                                           CurrentEventData,
                                           GvrExecuteEventsExtension.pointerHoverHandler);
        }
    }

    private void UpdatePointer(GameObject previousObject)
    {
        if (CurrentEventData == null)
        {
            return;
        }

        GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
        bool isPointerActiveAndAvailable = IsPointerActiveAndAvailable();

        bool isInteractive = CurrentEventData.pointerPress != null ||
                             EventExecutor.GetEventHandler<IPointerClickHandler>(
                                 currentObject) != null ||
                             EventExecutor.GetEventHandler<IDragHandler>(currentObject) != null;

        if (isPointerHovering && currentObject != null && currentObject == previousObject)
        {
            if (isPointerActiveAndAvailable)
            {
                Pointer.OnPointerHover(CurrentEventData.pointerCurrentRaycast, isInteractive);
            }
        }
        else
        {
            // If the object's don't match or the hovering object has been destroyed
            // then the pointer has exited.
            if (previousObject != null || (currentObject == null && isPointerHovering))
            {
                if (isPointerActiveAndAvailable)
                {
                    Pointer.OnPointerExit(previousObject);
                }

                isPointerHovering = false;
            }

            if (currentObject != null)
            {
                if (isPointerActiveAndAvailable)
                {
                    Pointer.OnPointerEnter(CurrentEventData.pointerCurrentRaycast, isInteractive);
                }

                isPointerHovering = true;
            }
        }
    }

    private void HandleDrag()
    {
        bool moving = CurrentEventData.IsPointerMoving();
        bool shouldStartDrag = ShouldStartDrag(CurrentEventData.pressPosition,
                                     CurrentEventData.position,
                                     ModuleController.eventSystem.pixelDragThreshold,
                                     CurrentEventData.useDragThreshold);

        if (moving
            && shouldStartDrag
            && CurrentEventData.pointerDrag != null
            && !CurrentEventData.dragging)
        {
            EventExecutor.Execute(CurrentEventData.pointerDrag, CurrentEventData,
                ExecuteEvents.beginDragHandler);
            CurrentEventData.dragging = true;
        }

        // Drag notification
        if (CurrentEventData.dragging && moving && CurrentEventData.pointerDrag != null)
        {
            // Before doing drag we should cancel any pointer down state
            // And clear selection!
            if (CurrentEventData.pointerPress != CurrentEventData.pointerDrag)
            {
                EventExecutor.Execute(CurrentEventData.pointerPress,
                                      CurrentEventData,
                                      ExecuteEvents.pointerUpHandler);

                CurrentEventData.eligibleForClick = false;
                CurrentEventData.gvrButtonsDown = 0;
                CurrentEventData.button = 0;
                CurrentEventData.pointerPress = null;
                CurrentEventData.rawPointerPress = null;
            }

            EventExecutor.Execute(CurrentEventData.pointerDrag,
                                  CurrentEventData,
                                  ExecuteEvents.dragHandler);
        }
    }

    private void HandlePendingClick()
    {
        if (CurrentEventData == null
            || (!CurrentEventData.eligibleForClick && !CurrentEventData.dragging))
        {
            return;
        }

        if (IsPointerActiveAndAvailable())
        {
            Pointer.OnPointerClickUp();
        }

        var go = CurrentEventData.pointerCurrentRaycast.gameObject;

        // Send pointer up and click events.
        EventExecutor.Execute(CurrentEventData.pointerPress,
                              CurrentEventData,
                              ExecuteEvents.pointerUpHandler);

        GameObject pointerClickHandler = EventExecutor.GetEventHandler<IPointerClickHandler>(go);
        if (CurrentEventData.pointerPress == pointerClickHandler
            && CurrentEventData.eligibleForClick)
        {
            EventExecutor.Execute(CurrentEventData.pointerPress,
                                  CurrentEventData,
                                  ExecuteEvents.pointerClickHandler);
        }

        if (CurrentEventData != null
            && CurrentEventData.pointerDrag != null
            && CurrentEventData.dragging)
        {
            EventExecutor.ExecuteHierarchy(go, CurrentEventData, ExecuteEvents.dropHandler);
            EventExecutor.Execute(CurrentEventData.pointerDrag,
                                  CurrentEventData,
                                  ExecuteEvents.endDragHandler);
        }

        if (CurrentEventData != null)
        {
            // Clear the click state.
            CurrentEventData.gvrButtonsDown = 0;
            CurrentEventData.button = 0;
            CurrentEventData.pointerPress = null;
            CurrentEventData.rawPointerPress = null;
            CurrentEventData.eligibleForClick = false;
            CurrentEventData.clickCount = 0;
            CurrentEventData.clickTime = 0;
            CurrentEventData.pointerDrag = null;
            CurrentEventData.dragging = false;
        }
    }

    private void HandleTriggerDown()
    {
        var go = CurrentEventData.pointerCurrentRaycast.gameObject;

        // Send pointer down event.
        CurrentEventData.gvrButtonsDown = Pointer.ControllerButtonDown;
        CurrentEventData.button = Pointer.InputButtonDown;
        CurrentEventData.pressPosition = CurrentEventData.position;
        CurrentEventData.pointerPressRaycast = CurrentEventData.pointerCurrentRaycast;
        CurrentEventData.pointerPress =
      EventExecutor.ExecuteHierarchy(go, CurrentEventData, ExecuteEvents.pointerDownHandler) ??
        EventExecutor.GetEventHandler<IPointerClickHandler>(go);

        // Save the pending click state.
        CurrentEventData.rawPointerPress = go;
        CurrentEventData.eligibleForClick = true;
        CurrentEventData.delta = Vector2.zero;
        CurrentEventData.dragging = false;
        CurrentEventData.useDragThreshold = true;
        CurrentEventData.clickCount = 1;
        CurrentEventData.clickTime = Time.unscaledTime;

        // Save the drag handler as well
        CurrentEventData.pointerDrag = EventExecutor.GetEventHandler<IDragHandler>(go);
        if (CurrentEventData.pointerDrag != null)
        {
            EventExecutor.Execute(CurrentEventData.pointerDrag,
                                  CurrentEventData,
                                  ExecuteEvents.initializePotentialDrag);
        }

        if (IsPointerActiveAndAvailable())
        {
            Pointer.OnPointerClickDown();
        }
    }

    private GameObject GetCurrentGameObject()
    {
        if (CurrentEventData != null)
        {
            return CurrentEventData.pointerCurrentRaycast.gameObject;
        }

        return null;
    }

    // Modified version of BaseInputModule.HandlePointerExitAndEnter that calls EventExecutor
    // instead of UnityEngine.EventSystems.ExecuteEvents.
    private void HandlePointerExitAndEnter(PointerEventData currentPointerData,
                                           GameObject newEnterTarget)
    {
        // If we have no target or pointerEnter has been deleted then
        // just send exit events to anything we are tracking.
        // Afterwards, exit.
        if (newEnterTarget == null || currentPointerData.pointerEnter == null)
        {
            for (var i = 0; i < currentPointerData.hovered.Count; ++i)
            {
                EventExecutor.Execute(currentPointerData.hovered[i],
                                      currentPointerData,
                                      ExecuteEvents.pointerExitHandler);
            }

            currentPointerData.hovered.Clear();

            if (newEnterTarget == null)
            {
                currentPointerData.pointerEnter = newEnterTarget;
                return;
            }
        }

        // If we have not changed hover target.
        if (newEnterTarget && currentPointerData.pointerEnter == newEnterTarget)
        {
            return;
        }

        GameObject commonRoot = ModuleController.FindCommonRoot(currentPointerData.pointerEnter,
                                                                newEnterTarget);

        // We already an entered object from last time.
        if (currentPointerData.pointerEnter != null)
        {
            // Send exit handler call to all elements in the chain
            // until we reach the new target, or null!
            Transform t = currentPointerData.pointerEnter.transform;

            while (t != null)
            {
                // If we reach the common root break out!
                if (commonRoot != null && commonRoot.transform == t)
                {
                    break;
                }

                EventExecutor.Execute(t.gameObject,
                                      currentPointerData,
                                      ExecuteEvents.pointerExitHandler);
                currentPointerData.hovered.Remove(t.gameObject);
                t = t.parent;
            }
        }

        // Now issue the enter call up to but not including the common root.
        currentPointerData.pointerEnter = newEnterTarget;
        if (newEnterTarget != null)
        {
            Transform t = newEnterTarget.transform;

            while (t != null && t.gameObject != commonRoot)
            {
                EventExecutor.Execute(t.gameObject,
                                      currentPointerData,
                                      ExecuteEvents.pointerEnterHandler);
                currentPointerData.hovered.Add(t.gameObject);
                t = t.parent;
            }
        }
    }

    private void TryExitPointer()
    {
        if (Pointer == null)
        {
            return;
        }

        GameObject currentGameObject = GetCurrentGameObject();
        if (currentGameObject)
        {
            Pointer.OnPointerExit(currentGameObject);
        }
    }

    private bool IsPointerActiveAndAvailable()
    {
        return pointer != null && pointer.IsAvailable;
    }

    private void RaycastAll()
    {
        ModuleController.RaycastResultCache.Clear();
        ModuleController.eventSystem.RaycastAll(CurrentEventData,
                                                ModuleController.RaycastResultCache);
    }
}

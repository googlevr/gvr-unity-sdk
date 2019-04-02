//-----------------------------------------------------------------------
// <copyright file="GvrBasePointer.cs" company="Google Inc.">
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

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>An abstract class for handling pointer based input.</summary>
/// <remarks><para>
/// This abstract class should be implemented for pointer based input, and used with
/// the GvrPointerInputModule script.
/// </para><para>
/// It provides methods called on pointer interaction with in-game objects and UI,
/// trigger events, and 'BaseInputModule' class state changes.
/// </para><para>
/// To have the methods called, an instance of this (implemented) class must be
/// registered with the **GvrPointerManager** script in 'Start' by calling
/// GvrPointerInputModule.OnPointerCreated.
/// </para><para>
/// This abstract class should be implemented by pointers doing 1 of 2 things:
/// 1. Responding to movement of the users head (Cardboard gaze-based-pointer).
/// 2. Responding to the movement of the daydream controller (Daydream 3D pointer).
/// </para></remarks>
public abstract class GvrBasePointer : MonoBehaviour, IGvrControllerInputDeviceReceiver
{
    /// <summary>Determines which raycast mode to use for this raycaster.</summary>
    /// <remarks>
    /// Supports the following modes: <ul>
    /// <li>Camera - Ray is cast from the camera through the pointer.</li>
    /// <li>Direct - Ray is cast forward from the pointer.</li>
    /// <li>Hybrid - Begins with a Direct ray and transitions to a Camera ray.</li>
    /// </ul>
    /// </remarks>
    [Tooltip("Determines which raycast mode to use for this raycaster.\n" +
    " • Camera - Ray is cast from camera.\n" +
    " • Direct - Ray is cast from pointer.\n" +
    " • Hybrid - Transitions from Direct ray to Camera ray.")]
    public RaycastMode raycastMode = RaycastMode.Hybrid;

    /// <summary>
    /// Determines the `eventCamera` for `GvrPointerPhysicsRaycaster` and
    /// `GvrPointerGraphicRaycaster`.
    /// </summary>
    /// <remarks>
    /// Additionaly, this is used to control what camera to use when calculating the Camera ray for
    /// the Hybrid and Camera raycast modes.
    /// </remarks>
    [Tooltip("Optional: Use a camera other than Camera.main.")]
    public Camera overridePointerCamera;

#if UNITY_EDITOR
    /// <summary>
    /// Determines if the rays used for raycasting will be drawn in the editor.
    /// </summary>
    [Tooltip("Determines if the rays used for raycasting will be drawn in the editor.")]
    public bool drawDebugRays = false;
#endif  // UNITY_EDITOR

    // When using a Daydream (3DoF) controller:
    // - Only TouchPadButton is mapped to mouse left click.
    private const GvrControllerButton LEFT_BUTTON_MASK_3DOF =
        GvrControllerButton.TouchPadButton;

    // When using a Daydream 6DoF controller:
    // - TouchPadButton and Trigger are mapped to mouse left click.
    // - App and Grip are mapped to mouse right click.
    private const GvrControllerButton LEFT_BUTTON_MASK_6DOF =
        GvrControllerButton.TouchPadButton |
        GvrControllerButton.Trigger;

    private const GvrControllerButton RIGHT_BUTTON_MASK_6DOF =
        GvrControllerButton.App |
        GvrControllerButton.Grip;

    private GvrControllerButton triggerButton;

    private GvrControllerButton triggerButtonDown;

    private GvrControllerButton triggerButtonUp;

    private int lastUpdateFrame;

    /// <summary>The method by which GvrPointer perorms Raycasts.</summary>
    /// <remarks>
    /// Camera is usually ideal at long range,
    /// Direct is usually ideal at close range, and Hybrid interpolates between the two depending
    /// on range.
    /// </remarks>
    public enum RaycastMode
    {
        /// <summary>
        /// Camera-based raycasting.  Detects collisions for the pointer from the Camera.
        /// </summary>
        /// <remarks><para>
        /// Casts a ray from the camera through the target of the pointer. This is ideal for
        /// reticles that are always rendered on top. The object that is selected will always be
        /// the object that appears underneath the reticle from the perspective of the camera.
        /// This also prevents the reticle from appearing to "jump" when it starts/stops hitting
        /// an object.
        /// </para><para>
        /// Recommended for reticles that are always rendered on top such as the GvrReticlePointer
        /// prefab which is used for cardboard apps.
        /// </para><para>
        /// Note: This will prevent the user from pointing around an object to hit something that
        /// is out of sight.  This isn't a problem in a typical use case.
        /// </para><para>
        /// When used with the standard daydream controller, the hit detection will not account for
        /// the laser correctly for objects that are closer to the camera than the end of the
        /// laser.
        /// In that case, it is recommended to do one of the following things:
        /// 1. Hide the laser.
        /// 2. Use a full-length laser pointer in Direct mode.
        /// 3. Use the Hybrid raycast mode.
        /// </para></remarks>
        Camera,

        /// <summary>
        /// Direct raycasting.  Detects collisions for the pointer from the Controller.
        /// </summary>
        /// <remarks><para>
        /// Cast a ray directly from the pointer origin.
        /// </para><para>
        /// Recommended for full-length laser pointers.
        /// </para></remarks>
        Direct,

        /// <summary>
        /// Hybrid raycasting.  Interpolates between Camera and Direct based on distance.
        /// </summary>
        /// <remarks><para>
        /// Default method for casting ray.
        /// </para><para>
        /// Combines the Camera and Direct raycast modes. Uses a Direct ray up until the
        /// CameraRayIntersectionDistance, and then switches to use a Camera ray starting from the
        /// point where the two rays intersect.
        /// </para><para>
        /// Recommended for use with the standard settings of the GvrControllerPointer prefab.
        /// This is the most versatile raycast mode. Like Camera mode, this prevents the reticle
        /// appearing jumpy. Additionally, it still allows the user to target objects that are
        /// close to them by using the laser as a visual reference.
        /// </para></remarks>
        Hybrid,
    }

    /// <summary>Gets the current RaycastResult.</summary>
    /// <remarks>
    /// A convenience function for fetching the object the pointer is currently hitting.
    /// </remarks>
    /// <value>The current raycast result.</value>
    public RaycastResult CurrentRaycastResult
    {
        get { return GvrPointerInputModule.CurrentRaycastResult; }
    }

    /// @deprecated Replaced by `CurrentRaycastResult.worldPosition`.
    /// <summary>Gets the pointer intersection.</summary>
    /// <value>The pointer intersection.</value>
    [System.Obsolete("Replaced by CurrentRaycastResult.worldPosition")]
    public Vector3 PointerIntersection
    {
        get
        {
            RaycastResult raycastResult = CurrentRaycastResult;
            return raycastResult.worldPosition;
        }
    }

    /// @deprecated Replaced by `CurrentRaycastResult.gameObject != null`.
    /// <summary>
    /// Gets a value indicating whether the pointer raycast intersects any object.
    /// </summary>
    /// <value>
    /// Value `true` if the pointer raycast intersects any object, otherwise `false`.
    /// </value>
    [System.Obsolete("Replaced by CurrentRaycastResult.gameObject != null")]
    public bool IsPointerIntersecting
    {
        get
        {
            RaycastResult raycastResult = CurrentRaycastResult;
            return raycastResult.gameObject != null;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the `enterRadius` should be used for the raycast
    /// or the `exitRadius` should be used.
    /// </summary>
    /// <remarks>
    /// It is set by `GvrPointerInputModule` and doesn't need to be controlled manually.
    /// </remarks>
    /// <value>
    /// Value `true` if enterRadius should be used for the racyast, `false` otherwise.
    /// </value>
    public bool ShouldUseExitRadiusForRaycast { get; set; }

    /// <summary>Gets the current radius of the pointer.</summary>
    /// <remarks>
    /// Gets the `exitRadius` if `ShouldUseExitRadiusForRaycast` is `true`, otherwise returns the
    /// `enterRadius`.
    /// </remarks>
    /// <value>The current pointer radius.</value>
    public float CurrentPointerRadius
    {
        get
        {
            float enterRadius, exitRadius;
            GetPointerRadius(out enterRadius, out exitRadius);
            if (ShouldUseExitRadiusForRaycast)
            {
                return exitRadius;
            }
            else
            {
                return enterRadius;
            }
        }
    }

    /// <summary>Gets the transform that represents this pointer.</summary>
    /// <remarks>It is used by `GvrBasePointerRaycaster` as the origin of the ray.</remarks>
    /// <value>The pointer transform.</value>
    public virtual Transform PointerTransform
    {
        get { return transform; }
    }

    /// <summary>Gets or sets the reference to the controller input device.</summary>
    /// <value>The reference to the controller input device.</value>
    public GvrControllerInputDevice ControllerInputDevice { get; set; }

    /// <summary>Gets a value indicating whether the trigger was just pressed.</summary>
    /// <remarks>
    /// This is an event flag.  It will be true for only one frame after the event happens.
    /// Defaults to mouse button 0 down on Cardboard or
    /// `ControllerInputDevice.GetButtonDown(TouchPadButton)` on Daydream.
    /// Can be overridden to change the trigger.
    /// </remarks>
    /// <value>Value `true` if the trigger was just pressed, `false` otherwise.</value>
    public virtual bool TriggerDown
    {
        get
        {
            UpdateTriggerState();
            return triggerButtonDown != 0;
        }
    }

    /// <summary>Gets a value indicating whether the trigger is currently being pressed.</summary>
    /// <remarks>
    /// This is not an event; it represents the trigger's state (it remains true while
    /// the trigger is being pressed).
    /// Defaults to mouse button 0 state on Cardboard or
    /// `ControllerInputDevice.GetButton(TouchPadButton)` on Daydream.
    /// Can be overridden to change the trigger.
    /// </remarks>
    /// <value>Value `true` if the trigger is currently being pressed, `false` otherwise.</value>
    public virtual bool Triggering
    {
        get
        {
            UpdateTriggerState();
            return triggerButton != 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the trigger was just released.
    /// </summary>
    /// <remarks>
    /// This is an event flag; it will be true for only one frame after the event happens.
    /// Defaults to mouse button 0 up on Cardboard or
    /// `ControllerInputDevice.GetButtonUp(TouchPadButton)` on Daydream.
    /// Can be overridden to change the trigger.
    /// </remarks>
    /// <value>Value `true` if the trigger was just released, `false` otherwise.</value>
    public virtual bool TriggerUp
    {
        get
        {
            UpdateTriggerState();
            return triggerButtonUp != 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user just started touching the touchpad.
    /// </summary>
    /// <remarks>
    /// This is an event flag; it is `true` for only one frame after the event happens,
    /// then reverts to false.
    /// Used by `GvrPointerScrollInput` to generate `OnScroll` events using Unity's event system.
    /// Defaults to `ControllerInputDevice.GetButtonDown(TouchPadTouch)`, can be overridden to
    /// change the input source.
    /// </remarks>
    /// <value>
    /// Value `true` if the user just started touching the touchpad, `false` otherwise.
    /// </value>
    public virtual bool TouchDown
    {
        get
        {
            if (ControllerInputDevice == null)
            {
                return false;
            }
            else
            {
                return ControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadTouch);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user is currently touching the touchpad.
    /// </summary>
    /// <remarks>
    /// Used by `GvrPointerScrollInput` to generate `OnScroll` events using Unity's event system.
    /// Defaults to `ControllerInputDevice.GetButton(TouchPadTouch)`, can be overridden to change
    /// the input source.
    /// </remarks>
    /// <value>
    /// Value `true` the user is currently touching the touchpad; otherwise, `false`.
    /// </value>
    public virtual bool IsTouching
    {
        get
        {
            if (ControllerInputDevice == null)
            {
                return false;
            }
            else
            {
                return ControllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user just stopped touching the touchpad.
    /// </summary>
    /// <remarks>
    /// This is an event flag; it is `true` for only one frame after the event happens,
    /// then reverts to false.
    /// Used by `GvrPointerScrollInput` to generate `OnScroll` events using Unity's event system.
    /// Defaults to `ControllerInputDevice.GetButtonUp(TouchPadTouch)`, can be overridden to change
    /// the input source.
    /// </remarks>
    /// <value>The touch up.</value>
    public virtual bool TouchUp
    {
        get
        {
            if (ControllerInputDevice == null)
            {
                return false;
            }
            else
            {
                return ControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadTouch);
            }
        }
    }

    /// <summary>
    /// Gets the position of the current touch, if touching the touchpad. If not touching, this
    /// is the position of the last touch (when the finger left the touchpad).
    /// </summary>
    /// <remarks>
    /// The X and Y range is from 0 to 1.
    /// (0, 0) is the top left of the touchpad and (1, 1) is the bottom right of the touchpad.
    /// Used by `GvrPointerScrollInput` to generate `OnScroll` events using Unity's event system.
    /// Defaults to `ControllerInputDevice.TouchPos` but translated to top-left-relative coordinates
    /// for backwards compatibility. Can be overridden to change the input source.
    /// </remarks>
    /// <value>The touch position.</value>
    public virtual Vector2 TouchPos
    {
        get
        {
            if (ControllerInputDevice == null)
            {
                return Vector2.zero;
            }
            else
            {
                Vector2 touchPos = ControllerInputDevice.TouchPos;
                touchPos.x = (touchPos.x / 2.0f) + 0.5f;
                touchPos.y = (-touchPos.y / 2.0f) + 0.5f;
                return touchPos;
            }
        }
    }

    /// <summary>
    /// Gets the end point of the pointer when it is MaxPointerDistance away from the origin.
    /// </summary>
    /// <value>
    /// The end point of the pointer when it is MaxPointerDistance away from the origin.
    /// </value>
    public virtual Vector3 MaxPointerEndPoint
    {
        get
        {
            Transform pointerTransform = PointerTransform;
            if (pointerTransform == null)
            {
                return Vector3.zero;
            }

            Vector3 maxEndPoint = GetPointAlongPointer(MaxPointerDistance);
            return maxEndPoint;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the pointer will be used for generating input events by
    /// `GvrPointerInputModule`.
    /// </summary>
    /// <value>
    /// Value `true` if the pointer will be used for generating input events by
    /// `GvrPointerInputModule`, `false`.
    /// </value>
    public virtual bool IsAvailable
    {
        get
        {
            Transform pointerTransform = PointerTransform;
            if (pointerTransform == null)
            {
                return false;
            }

            if (!enabled)
            {
                return false;
            }

            return pointerTransform.gameObject.activeInHierarchy;
        }
    }

    /// <summary>
    /// Gets the location where the ray from the pointer will intersect with the ray from the
    /// camera when using the Hybrid raycast mode.
    /// </summary>
    /// <value>
    /// The location where the ray from the pointer will intersect with the ray from the camera
    /// when using the Camera raycast mode.
    /// </value>
    public virtual float CameraRayIntersectionDistance
    {
        get { return MaxPointerDistance; }
    }

    /// <summary>Gets the camera used as the pointer.</summary>
    /// <value>The camera used as the pointer.</value>
    public Camera PointerCamera
    {
        get
        {
            if (overridePointerCamera != null)
            {
                return overridePointerCamera;
            }

            return Camera.main;
        }
    }

    /// <summary>
    /// Gets the max distance from the pointer that raycast hits will be detected.
    /// </summary>
    /// <value>The max pointer distance from the pointer that raycast hits will be detected.</value>
    public abstract float MaxPointerDistance { get; }

    internal PointerEventData.InputButton InputButtonDown
    {
        get
        {
            if (triggerButton == 0 ||
               (triggerButton & LEFT_BUTTON_MASK_6DOF) != 0)
            {
                return PointerEventData.InputButton.Left;
            }
            else
            {
                return PointerEventData.InputButton.Right;
            }
        }
    }

    internal GvrControllerButton ControllerButtonDown
    {
        get { return triggerButton; }
    }

    /// <summary>
    /// Calculates the ray for a given Raycast mode.
    /// </summary>
    /// <remarks>
    /// Will throw an exception if the raycast mode Hybrid is passed in.
    /// If you need to calculate the ray for the direct or camera segment of the Hybrid raycast,
    /// use CalculateHybridRay instead.
    /// </remarks>
    /// <param name="pointer">Which pointer to project the ray from.</param>
    /// <param name="mode">Which Raycast mode to use.  Must be Camera or Direct.</param>
    /// <returns>The PointerRay as projected from the GvrbasePointer in the given mode.</returns>
    public static PointerRay CalculateRay(GvrBasePointer pointer, RaycastMode mode)
    {
        PointerRay result = new PointerRay();

        if (pointer == null || !pointer.IsAvailable)
        {
            Debug.LogError("Cannot calculate ray when the pointer isn't available.");
            return result;
        }

        Transform pointerTransform = pointer.PointerTransform;

        if (pointerTransform == null)
        {
            Debug.LogError("Cannot calculate ray when pointerTransform is null.");
            return result;
        }

        result.distance = pointer.MaxPointerDistance;

        switch (mode)
        {
            case RaycastMode.Camera:
                Camera camera = pointer.PointerCamera;
                if (camera == null)
                {
                    Debug.LogError("Cannot calculate ray because pointer.PointerCamera is null." +
                        "To fix this, either tag a Camera as \"MainCamera\" or set " +
                        "overridePointerCamera.");
                    return result;
                }

                Vector3 rayPointerStart = pointerTransform.position;
                Vector3 rayPointerEnd = rayPointerStart +
                                        (pointerTransform.forward *
                                         pointer.CameraRayIntersectionDistance);

                Vector3 cameraLocation = camera.transform.position;
                Vector3 finalRayDirection = rayPointerEnd - cameraLocation;
                finalRayDirection.Normalize();

                Vector3 finalRayStart = cameraLocation + (finalRayDirection * camera.nearClipPlane);

                result.ray = new Ray(finalRayStart, finalRayDirection);
                break;
            case RaycastMode.Direct:
                result.ray = new Ray(pointerTransform.position, pointerTransform.forward);
                break;
            default:
                throw new UnityException(
                    "Invalid RaycastMode " + mode + " passed into CalculateRay.");
        }

        return result;
    }

    /// <summary>
    /// Calculates the ray for the segment of the Hybrid raycast determined by the raycast mode
    /// passed in.
    /// </summary>
    /// <remarks>
    /// Throws an exception if Hybrid is passed in.
    /// </remarks>
    /// <param name="pointer">Which pointer to project the ray from.</param>
    /// <param name="hybridMode">
    /// Which Raycast sub-mode to use within Hybrid mode.  Must be Camera or Direct.
    /// </param>
    /// <returns>The PointerRay as projected from the GvrbasePointer in the given mode.</returns>
    public static PointerRay CalculateHybridRay(GvrBasePointer pointer, RaycastMode hybridMode)
    {
        PointerRay result;

        switch (hybridMode)
        {
            case RaycastMode.Direct:
                result = CalculateRay(pointer, hybridMode);
                result.distance = pointer.CameraRayIntersectionDistance;
                break;
            case RaycastMode.Camera:
                result = CalculateRay(pointer, hybridMode);
                PointerRay directRay = CalculateHybridRay(pointer, RaycastMode.Direct);
                result.ray.origin = directRay.ray.GetPoint(directRay.distance);
                result.distanceFromStart = directRay.distance;
                result.distance = pointer.MaxPointerDistance - directRay.distance;
                break;
            default:
                throw new UnityException(
                    "Invalid RaycastMode " + hybridMode + " passed into CalculateHybridRay.");
        }

        return result;
    }

    /// <summary>Called when the pointer is facing a valid GameObject.</summary>
    /// <remarks>This can be a 3D or UI element.</remarks>
    /// <param name="raycastResult">
    /// The hit detection result for the object being pointed at.
    /// </param>
    /// <param name="isInteractive">
    /// Value `true` if the object being pointed at is interactive.
    /// </param>
    public abstract void OnPointerEnter(RaycastResult raycastResult, bool isInteractive);

    /// <summary>Called every frame the user is still pointing at a valid GameObject.</summary>
    /// <remarks>This can be a 3D or UI element.</remarks>
    /// <param name="raycastResultResult">
    /// The hit detection result for the object being pointed at.
    /// </param>
    /// <param name="isInteractive">
    /// Value `true` if the object being pointed at is interactive.
    /// </param>
    public abstract void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive);

    /// <summary>
    /// Called when the pointer no longer faces an object previously
    /// intersected with a ray projected from the camera.
    /// </summary>
    /// <remarks>
    /// This is also called just before **OnInputModuleDisabled**
    /// previousObject will be null in this case.
    /// </remarks>
    /// <param name="previousObject">
    /// The object that was being pointed at the previous frame.
    /// </param>
    public abstract void OnPointerExit(GameObject previousObject);

    /// <summary>
    /// Called when a click is initiated.
    /// </summary>
    public abstract void OnPointerClickDown();

    /// <summary>
    /// Called when click is finished.
    /// </summary>
    public abstract void OnPointerClickUp();

    /// <summary>Return the radius of the pointer.</summary>
    /// <remarks><para>
    /// It is used by GvrPointerPhysicsRaycaster when searching for valid pointer targets. If a
    /// radius is 0, then a ray is used to find a valid pointer target. Otherwise it will use a
    /// SphereCast.
    /// </para><para>
    /// The *enterRadius* is used for finding new targets while the *exitRadius*
    /// is used to see if you are still nearby the object currently pointed at
    /// to avoid a flickering effect when just at the border of the intersection.
    /// </para><para>
    /// NOTE: This is only works with GvrPointerPhysicsRaycaster. To use it with uGUI,
    /// add 3D colliders to your canvas elements.
    /// </para></remarks>
    /// <param name="enterRadius">Used for finding new targets.</param>
    /// <param name="exitRadius">
    /// Used to see if the pointer is still nearby the object currently pointed at.  This exists
    /// to avoid a flickering effect when just at the border of the intersection.
    /// </param>
    public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);

    /// <summary>
    /// Returns a point in worldspace a specified distance along the pointer.
    /// </summary>
    /// <remarks><para>
    /// What this point will be is different depending on the raycastMode.
    /// </para><para>
    /// Because raycast modes differ, use this function instead of manually calculating a point
    /// projected from the pointer.
    /// </para></remarks>
    /// <param name="distance">The distance along the pointer's laser.</param>
    /// <returns>A worlspace position along the pointer's laser.</returns>
    public Vector3 GetPointAlongPointer(float distance)
    {
        PointerRay pointerRay = GetRayForDistance(distance);
        return pointerRay.ray.GetPoint(distance - pointerRay.distanceFromStart);
    }

    /// <summary>
    /// Returns the ray used for projecting points out of the pointer for the given distance.
    /// </summary>
    /// <remarks>
    /// In Hybrid raycast mode, the ray will be different depending upon the distance.
    /// In Camera or Direct raycast mode, the ray will always be the same.
    /// </remarks>
    /// <param name="distance">The distance to check.</param>
    /// <returns>
    /// Either the Camera or Controller's PointerRay. For Hybrid mode, this will return Camera at
    /// large distances and Controller at close distances.  For other modes, the ray will always
    /// be the mode's associated ray (Camera=Camera, Direct=Controller).
    /// </returns>
    public PointerRay GetRayForDistance(float distance)
    {
        PointerRay result = new PointerRay();

        if (raycastMode == RaycastMode.Hybrid)
        {
            float directDistance = CameraRayIntersectionDistance;
            if (distance < directDistance)
            {
                result = CalculateHybridRay(this, RaycastMode.Direct);
            }
            else
            {
                result = CalculateHybridRay(this, RaycastMode.Camera);
            }
        }
        else
        {
            result = CalculateRay(this, raycastMode);
        }

        return result;
    }

    /// @cond
    /// <summary>
    /// This MonoBehavior's Start() implementation.
    /// </summary>
    protected virtual void Start()
    {
        GvrPointerInputModule.OnPointerCreated(this);
    }

    /// @endcond
#if UNITY_EDITOR
    /// <summary>Draws gizmos that visualize the rays used by the pointer for raycasting.</summary>
    /// <remarks><para>
    /// These rays will change based on the `raycastMode` selected.
    /// </para><para>
    /// This is a `MonoBehavior` builtin implementation: Implement `OnDrawGizmos` if you want to
    /// draw gizmos that are also pickable and always drawn.  This allows you to quickly pick
    /// important objects in your Scene.  Note that `OnDrawGizmos` will use a mouse position that is
    /// relative to the Scene View.  This function does not get called if the component is collapsed
    /// in the inspector. Use `OnDrawGizmosSelected` to draw gizmos when the game object is
    /// selected.
    /// </para></remarks>
    protected virtual void OnDrawGizmos()
    {
        if (drawDebugRays && Application.isPlaying && isActiveAndEnabled)
        {
            switch (raycastMode)
            {
                case RaycastMode.Camera:
                    // Camera line.
                    Gizmos.color = Color.green;
                    PointerRay pointerRay = CalculateRay(this, RaycastMode.Camera);
                    Gizmos.DrawLine(pointerRay.ray.origin,
                                    pointerRay.ray.GetPoint(pointerRay.distance));
                    Camera camera = PointerCamera;

                    // Pointer to intersection dotted line.
                    Vector3 intersection = PointerTransform.position +
                                           (PointerTransform.forward *
                                            CameraRayIntersectionDistance);
                    UnityEditor.Handles.DrawDottedLine(PointerTransform.position,
                                                       intersection,
                                                       1.0f);
                    break;
                case RaycastMode.Direct:
                    // Direct line.
                    Gizmos.color = Color.blue;
                    pointerRay = CalculateRay(this, RaycastMode.Direct);
                    Gizmos.DrawLine(pointerRay.ray.origin,
                                    pointerRay.ray.GetPoint(pointerRay.distance));
                    break;
                case RaycastMode.Hybrid:
                    // Direct line.
                    Gizmos.color = Color.blue;
                    pointerRay = CalculateHybridRay(this, RaycastMode.Direct);
                    Gizmos.DrawLine(pointerRay.ray.origin,
                                    pointerRay.ray.GetPoint(pointerRay.distance));

                    // Camera line.
                    Gizmos.color = Color.green;
                    pointerRay = CalculateHybridRay(this, RaycastMode.Camera);
                    Gizmos.DrawLine(pointerRay.ray.origin,
                                    pointerRay.ray.GetPoint(pointerRay.distance));

                    // Camera to intersection dotted line.
                    camera = PointerCamera;
                    if (camera != null)
                    {
                        UnityEditor.Handles.DrawDottedLine(camera.transform.position,
                                                           pointerRay.ray.origin,
                                                           1.0f);
                    }

                    break;
                default:
                    break;
            }
        }
    }
#endif // UNITY_EDITOR

    /// <summary>This MonoBehavior's OnEnable behavior.</summary>
    private void OnEnable()
    {
        triggerButton = 0;
        triggerButtonDown = 0;
        triggerButtonUp = 0;
    }

    private void UpdateTriggerState()
    {
        if (lastUpdateFrame != Time.frameCount)
        {
            lastUpdateFrame = Time.frameCount;

            GvrControllerButton allButtonsMask = 0;
            if (ControllerInputDevice != null
                && ControllerInputDevice.SupportsPositionalTracking)
            {
                allButtonsMask = LEFT_BUTTON_MASK_6DOF | RIGHT_BUTTON_MASK_6DOF;
            }
            else
            {
                allButtonsMask = LEFT_BUTTON_MASK_3DOF;
            }

            GvrControllerButton buttonDown = 0;
            GvrControllerButton buttonUp = 0;

            // Cardboard button events come through as mouse button 0 and are
            // mapped to TouchPadButton.
            if (Input.GetMouseButtonDown(0))
            {
                buttonDown |= GvrControllerButton.TouchPadButton;
            }

            if (Input.GetMouseButtonUp(0))
            {
                buttonUp |= GvrControllerButton.TouchPadButton;
            }

            if (ControllerInputDevice != null)
            {
                buttonDown |= ControllerInputDevice.ButtonsDown;
                buttonUp |= ControllerInputDevice.ButtonsUp;
            }

            buttonDown &= allButtonsMask;
            buttonUp &= allButtonsMask;

            // Only allow one button down at a time. If one is down, ignore the rest.
            if (triggerButton != 0)
            {
                buttonDown &= triggerButton;
            }
            else
            {
                // Mask off everything except the right-most bit that is set in case
                // more than one button went down in the same frame.
                buttonDown &= (GvrControllerButton)(-(int)buttonDown);
            }

            // Ignore ups from buttons whose down we ignored.
            buttonUp &= triggerButton;

            // Build trigger button state from filtered ups and downs to ensure
            // actual (A-down, B-down, A-up, B-up) results in
            // event (A-down, A-up) instead of
            // event (A-down, A-up, B-down, B-up).
            triggerButton |= buttonDown;
            triggerButton &= ~buttonUp;
            triggerButtonDown = buttonDown;
            triggerButtonUp = buttonUp;
        }
    }

    /// <summary>Represents a ray segment for a series of intersecting rays.</summary>
    /// <remarks>This is useful for Hybrid raycast mode, which uses two sequential rays.</remarks>
    public struct PointerRay
    {
        /// <summary>The ray for this segment of the pointer.</summary>
        public Ray ray;

        /// <summary>
        /// The distance along the pointer from the origin of the first ray to this ray.
        /// </summary>
        public float distanceFromStart;

        /// <summary>Distance that this ray extends to.</summary>
        public float distance;
    }
}

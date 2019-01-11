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

/// This abstract class should be implemented for pointer based input, and used with
/// the GvrPointerInputModule script.
///
/// It provides methods called on pointer interaction with in-game objects and UI,
/// trigger events, and 'BaseInputModule' class state changes.
///
/// To have the methods called, an instance of this (implemented) class must be
/// registered with the **GvrPointerManager** script in 'Start' by calling
/// GvrPointerInputModule.OnPointerCreated.
///
/// This abstract class should be implemented by pointers doing 1 of 2 things:
/// 1. Responding to movement of the users head (Cardboard gaze-based-pointer).
/// 2. Responding to the movement of the daydream controller (Daydream 3D pointer).
public abstract class GvrBasePointer : MonoBehaviour, IGvrControllerInputDeviceReceiver
{
    // When using a Daydream (3DoF) controller:
    // - Only TouchPadButton is mapped to mouse left click.
    private const GvrControllerButton leftButtonMask3Dof =
        GvrControllerButton.TouchPadButton;

    // When using a Daydream 6DoF controller:
    // - TouchPadButton and Trigger are mapped to mouse left click.
    // - App and Grip are mapped to mouse right click.
    private const GvrControllerButton leftButtonMask6Dof =
        GvrControllerButton.TouchPadButton |
        GvrControllerButton.Trigger;

    private const GvrControllerButton rightButtonMask6Dof =
        GvrControllerButton.App |
        GvrControllerButton.Grip;

    private GvrControllerButton triggerButton;

    private GvrControllerButton triggerButtonDown;

    private GvrControllerButton triggerButtonUp;

    private int lastUpdateFrame;

    /// <summary>Raycast mode values.</summary>
    public enum RaycastMode
    {
        /// Casts a ray from the camera through the target of the pointer.
        /// This is ideal for reticles that are always rendered on top.
        /// The object that is selected will always be the object that appears
        /// underneath the reticle from the perspective of the camera.
        /// This also prevents the reticle from appearing to "jump" when it starts/stops hitting an object.
        ///
        /// Recommended for reticles that are always rendered on top such as the GvrReticlePointer
        /// prefab which is used for cardboard apps.
        ///
        /// Note: This will prevent the user from pointing around an object to hit something that is out of sight.
        /// This isn't a problem in a typical use case.
        ///
        /// When used with the standard daydream controller,
        /// the hit detection will not account for the laser correctly for objects that are closer to the
        /// camera than the end of the laser.
        /// In that case, it is recommended to do one of the following things:
        ///
        /// 1. Hide the laser.
        /// 2. Use a full-length laser pointer in Direct mode.
        /// 3. Use the Hybrid raycast mode.
        Camera,

        /// Cast a ray directly from the pointer origin.
        ///
        /// Recommended for full-length laser pointers.
        Direct,

        /// Default method for casting ray.
        ///
        /// Combines the Camera and Direct raycast modes.
        /// Uses a Direct ray up until the CameraRayIntersectionDistance, and then switches to use
        /// a Camera ray starting from the point where the two rays intersect.
        ///
        /// Recommended for use with the standard settings of the GvrControllerPointer prefab.
        /// This is the most versatile raycast mode. Like Camera mode, this prevents the reticle
        /// appearing jumpy. Additionally, it still allows the user to target objects that are close
        /// to them by using the laser as a visual reference.
        Hybrid,
    }

    /// Represents a ray segment for a series of intersecting rays.
    /// This is useful for Hybrid raycast mode, which uses two sequential rays.
    public struct PointerRay
    {
        /// The ray for this segment of the pointer.
        public Ray ray;

        /// The distance along the pointer from the origin of the first ray to this ray.
        public float distanceFromStart;

        /// Distance that this ray extends to.
        public float distance;
    }

    /// Determines which raycast mode to use for this raycaster.
    /// • Camera - Ray is cast from the camera through the pointer.
    /// • Direct - Ray is cast forward from the pointer.
    /// • Hybrid - Begins with a Direct ray and transitions to a Camera ray.
    [Tooltip("Determines which raycast mode to use for this raycaster.\n" +
    " • Camera - Ray is cast from camera.\n" +
    " • Direct - Ray is cast from pointer.\n" +
    " • Hybrid - Transitions from Direct ray to Camera ray.")]
    public RaycastMode raycastMode = RaycastMode.Hybrid;

    /// Determines the eventCamera for _GvrPointerPhysicsRaycaster_ and _GvrPointerGraphicRaycaster_.
    /// Additionaly, this is used to control what camera to use when calculating the Camera ray for
    /// the Hybrid and Camera raycast modes.
    [Tooltip("Optional: Use a camera other than Camera.main.")]
    public Camera overridePointerCamera;

#if UNITY_EDITOR
    /// Determines if the rays used for raycasting will be drawn in the editor.
    [Tooltip("Determines if the rays used for raycasting will be drawn in the editor.")]
    public bool drawDebugRays = false;
#endif  // UNITY_EDITOR

    /// Convenience function to access what the pointer is currently hitting.
    public RaycastResult CurrentRaycastResult
    {
        get { return GvrPointerInputModule.CurrentRaycastResult; }
    }

    /// @deprecated Replaced by `CurrentRaycastResult.worldPosition`
    [System.Obsolete("Replaced by CurrentRaycastResult.worldPosition")]
    public Vector3 PointerIntersection
    {
        get
        {
            RaycastResult raycastResult = CurrentRaycastResult;
            return raycastResult.worldPosition;
        }
    }

    /// @deprecated Replaced by `CurrentRaycastResult.gameObject != null`
    [System.Obsolete("Replaced by CurrentRaycastResult.gameObject != null")]
    public bool IsPointerIntersecting
    {
        get
        {
            RaycastResult raycastResult = CurrentRaycastResult;
            return raycastResult.gameObject != null;
        }
    }

    /// This is used to determine if the enterRadius or the exitRadius should be used for the raycast.
    /// It is set by GvrPointerInputModule and doesn't need to be controlled manually.
    public bool ShouldUseExitRadiusForRaycast { get; set; }

    /// If ShouldUseExitRadiusForRaycast is true, returns the exit radius.
    /// Otherwise, returns the enter radius.
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

    /// Returns the transform that represents this pointer.
    /// It is used by GvrBasePointerRaycaster as the origin of the ray.
    public virtual Transform PointerTransform
    {
        get { return transform; }
    }

    /// <summary>The reference to the controller input device.</summary>
    public GvrControllerInputDevice ControllerInputDevice { get; set; }

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
                allButtonsMask = leftButtonMask6Dof | rightButtonMask6Dof;
            }
            else
            {
                allButtonsMask = leftButtonMask3Dof;
            }

            GvrControllerButton buttonDown = 0;
            GvrControllerButton buttonUp = 0;
#if !UNITY_EDITOR
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
#endif
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

    /// If true, the trigger was just pressed. This is an event flag:
    /// it will be true for only one frame after the event happens.
    /// Defaults to mouse button 0 down on Cardboard or
    /// ControllerInputDevice.GetButtonDown(TouchPadButton) on Daydream.
    /// Can be overridden to change the trigger.
    public virtual bool TriggerDown
    {
        get
        {
            UpdateTriggerState();
            return triggerButtonDown != 0;
        }
    }

    /// If true, the trigger is currently being pressed. This is not
    /// an event: it represents the trigger's state (it remains true while the trigger is being
    /// pressed).
    /// Defaults to mouse button 0 state on Cardboard or
    /// ControllerInputDevice.GetButton(TouchPadButton) on Daydream.
    /// Can be overridden to change the trigger.
    public virtual bool Triggering
    {
        get
        {
            UpdateTriggerState();
            return triggerButton != 0;
        }
    }

    /// If true, the trigger was just released. This is an event flag:
    /// it will be true for only one frame after the event happens.
    /// Defaults to mouse button 0 up on Cardboard or
    /// ControllerInputDevice.GetButtonUp(TouchPadButton) on Daydream.
    /// Can be overridden to change the trigger.
    public virtual bool TriggerUp
    {
        get
        {
            UpdateTriggerState();
            return triggerButtonUp != 0;
        }
    }

    internal PointerEventData.InputButton InputButtonDown
    {
        get
        {
            if (triggerButton == 0 ||
               (triggerButton & leftButtonMask6Dof) != 0)
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

    /// If true, the user just started touching the touchpad. This is an event flag (it is true
    /// for only one frame after the event happens, then reverts to false).
    /// Used by _GvrPointerScrollInput_ to generate OnScroll events using Unity's Event System.
    /// Defaults to ControllerInputDevice.GetButtonDown(TouchPadTouch), can be overridden to change
    /// the input source.
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

    /// If true, the user is currently touching the touchpad.
    /// Used by _GvrPointerScrollInput_ to generate OnScroll events using Unity's Event System.
    /// Defaults to ControllerInputDevice.GetButton(TouchPadTouch), can be overridden to change
    /// the input source.
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

    /// If true, the user just stopped touching the touchpad. This is an event flag (it is true
    /// for only one frame after the event happens, then reverts to false).
    /// Used by _GvrPointerScrollInput_ to generate OnScroll events using Unity's Event System.
    /// Defaults to ControllerInputDevice.GetButtonUp(TouchPadTouch), can be overridden to change
    /// the input source.
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

    /// Position of the current touch, if touching the touchpad.
    /// If not touching, this is the position of the last touch (when the finger left the touchpad).
    /// The X and Y range is from 0 to 1.
    /// (0, 0) is the top left of the touchpad and (1, 1) is the bottom right of the touchpad.
    /// Used by `GvrPointerScrollInput` to generate OnScroll events using Unity's Event System.
    /// Defaults to `ControllerInputDevice.TouchPos` but translated to top-left-relative coordinates
    /// for backwards compatibility. Can be overridden to change the input source.
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

    /// Returns the end point of the pointer when it is MaxPointerDistance away from the origin.
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

    /// If true, the pointer will be used for generating input events by _GvrPointerInputModule_.
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

    /// When using the Camera raycast mode, this is used to calculate
    /// where the ray from the pointer will intersect with the ray from the camera.
    public virtual float CameraRayIntersectionDistance
    {
        get { return MaxPointerDistance; }
    }

    /// <summary>The camera used as the pointer.</summary>
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

    /// Returns the max distance from the pointer that raycast hits will be detected.
    public abstract float MaxPointerDistance { get; }

    /// Called when the pointer is facing a valid GameObject. This can be a 3D
    /// or UI element.
    ///
    /// **raycastResult** is the hit detection result for the object being pointed at.
    /// **isInteractive** is true if the object being pointed at is interactive.
    public abstract void OnPointerEnter(RaycastResult raycastResult, bool isInteractive);

    /// Called every frame the user is still pointing at a valid GameObject. This
    /// can be a 3D or UI element.
    ///
    /// **raycastResult** is the hit detection result for the object being pointed at.
    /// **isInteractive** is true if the object being pointed at is interactive.
    public abstract void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive);

    /// Called when the pointer no longer faces an object previously
    /// intersected with a ray projected from the camera.
    /// This is also called just before **OnInputModuleDisabled**
    /// previousObject will be null in this case.
    ///
    /// **previousObject** is the object that was being pointed at the previous frame.
    public abstract void OnPointerExit(GameObject previousObject);

    /// Called when a click is initiated.
    public abstract void OnPointerClickDown();

    /// Called when click is finished.
    public abstract void OnPointerClickUp();

    /// Return the radius of the pointer. It is used by GvrPointerPhysicsRaycaster when
    /// searching for valid pointer targets. If a radius is 0, then a ray is used to find
    /// a valid pointer target. Otherwise it will use a SphereCast.
    /// The *enterRadius* is used for finding new targets while the *exitRadius*
    /// is used to see if you are still nearby the object currently pointed at
    /// to avoid a flickering effect when just at the border of the intersection.
    ///
    /// NOTE: This is only works with GvrPointerPhysicsRaycaster. To use it with uGUI,
    /// add 3D colliders to your canvas elements.
    public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);

    /// Returns a point in worldspace a specified distance along the pointer.
    /// What this point will be is different depending on the raycastMode.
    ///
    /// Because raycast modes differ, use this function instead of manually calculating a point
    /// projected from the pointer.
    public Vector3 GetPointAlongPointer(float distance)
    {
        PointerRay pointerRay = GetRayForDistance(distance);
        return pointerRay.ray.GetPoint(distance - pointerRay.distanceFromStart);
    }

    /// Returns the ray used for projecting points out of the pointer for the given distance.
    /// In Hybrid raycast mode, the ray will be different depending upon the distance.
    /// In Camera or Direct raycast mode, the ray will always be the same.
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

    /// Calculates the ray for a given Raycast mode.
    /// Will throw an exception if the raycast mode Hybrid is passed in.
    /// If you need to calculate the ray for the direct or camera segment of the Hybrid raycast,
    /// use CalculateHybridRay instead.
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
                    "To fix this, either tag a Camera as \"MainCamera\" or set overridePointerCamera.");
                    return result;
                }

                Vector3 rayPointerStart = pointerTransform.position;
                Vector3 rayPointerEnd = rayPointerStart +
                                       (pointerTransform.forward * pointer.CameraRayIntersectionDistance);

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
                throw new UnityException("Invalid RaycastMode " + mode + " passed into CalculateRay.");
        }

        return result;
    }

    /// Calculates the ray for the segment of the Hybrid raycast determined by the raycast mode
    /// passed in. Throws an exception if Hybrid is passed in.
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
                throw new UnityException("Invalid RaycastMode " + hybridMode + " passed into CalculateHybridRay.");
        }

        return result;
    }

    /// @cond
    protected virtual void Start()
    {
        GvrPointerInputModule.OnPointerCreated(this);
    }

    /// @endcond

    #if UNITY_EDITOR
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
                    Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));
                    Camera camera = PointerCamera;

                    // Pointer to intersection dotted line.
                    Vector3 intersection =
                        PointerTransform.position + (PointerTransform.forward * CameraRayIntersectionDistance);
                    UnityEditor.Handles.DrawDottedLine(PointerTransform.position, intersection, 1.0f);
                    break;
                case RaycastMode.Direct:
                    // Direct line.
                    Gizmos.color = Color.blue;
                    pointerRay = CalculateRay(this, RaycastMode.Direct);
                    Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));
                    break;
                case RaycastMode.Hybrid:
                    // Direct line.
                    Gizmos.color = Color.blue;
                    pointerRay = CalculateHybridRay(this, RaycastMode.Direct);
                    Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));

                    // Camera line.
                    Gizmos.color = Color.green;
                    pointerRay = CalculateHybridRay(this, RaycastMode.Camera);
                    Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));

                    // Camera to intersection dotted line.
                    camera = PointerCamera;
                    if (camera != null)
                    {
                        UnityEditor.Handles.DrawDottedLine(camera.transform.position, pointerRay.ray.origin, 1.0f);
                    }

                    break;
                default:
                    break;
            }
        }
    }
#endif // UNITY_EDITOR
}

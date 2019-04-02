//-----------------------------------------------------------------------
// <copyright file="GvrControllerInputDevice.cs" company="Google Inc.">
// Copyright 2018 Google Inc. All rights reserved.
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

using System;
using System.Collections;
using Gvr.Internal;
using UnityEngine;

/// <summary>Device instance of the Daydream controller API.</summary>
public class GvrControllerInputDevice
{
    private IControllerProvider controllerProvider;
    private int controllerId;

    private ControllerState controllerState = new ControllerState();
    private Vector2 touchPosCentered = Vector2.zero;

    private int lastUpdatedFrameCount = -1;
    private bool valid;

    internal GvrControllerInputDevice(IControllerProvider provider, int controller_id)
    {
        controllerProvider = provider;
        controllerId = controller_id;
        valid = true;
    }

    /// <summary>Event handler for when the connection state of the controller changes.</summary>
    public event GvrControllerInput.OnStateChangedEvent OnStateChanged;

    /// <summary>Gets a value indicating whether this instance is dominant hand.</summary>
    /// <value>Value `true` if this instance is dominant hand, `false` otherwise.</value>
    public bool IsDominantHand
    {
        get { return controllerId == 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is configured as being the right hand.
    /// </summary>
    /// <value>Value `true` if this instance is right hand, `false` otherwise.</value>
    public bool IsRightHand
    {
        [SuppressMemoryAllocationError(IsWarning = true)]
        get
        {
            if (controllerId == 0)
            {
                return GvrSettings.Handedness == GvrSettings.UserPrefsHandedness.Right;
            }
            else
            {
                return GvrSettings.Handedness == GvrSettings.UserPrefsHandedness.Left;
            }
        }
    }

    /// <summary>Gets the controller's current connection state.</summary>
    /// <value>The state.</value>
    public GvrConnectionState State
    {
        [SuppressMemoryAllocationError(IsWarning = true)]
        get
        {
            Update();
            return controllerState.connectionState;
        }
    }

    /// <summary>Gets the API status of the current controller state.</summary>
    /// <value>The API status.</value>
    public GvrControllerApiStatus ApiStatus
    {
        get
        {
            Update();
            return controllerState.apiStatus;
        }
    }

    /// <summary>Gets the controller's current orientation in space, as a quaternion.</summary>
    /// <remarks>
    /// The rotation is provided in 'orientation space' which means the rotation is given relative
    /// to the last time the user recentered their controller. To make a game object in your scene
    /// have the same orientation as the controller, simply assign this quaternion to the object's
    /// `transform.rotation`. To match the relative rotation, use `transform.localRotation` instead.
    /// </remarks>
    /// <value>The orientation.</value>
    public Quaternion Orientation
    {
        get
        {
            Update();
            return controllerState.orientation;
        }
    }

    /// <summary>Gets the controller's current position in world space.</summary>
    /// <value>The controller's current position in world space.</value>
    public Vector3 Position
    {
        get
        {
            Update();
            return controllerState.position;
        }
    }

    /// <summary>Gets the controller's current angular speed in radians per second.</summary>
    /// <remarks>
    /// Generated using the right-hand rule (positive means a right-hand rotation about the given
    /// axis), as measured by the controller's gyroscope.
    /// <para>
    /// The controller's axes are:
    /// - X points to the right.
    /// - Y points perpendicularly up from the controller's top surface.
    /// - Z lies along the controller's body, pointing towards the front.
    /// </para></remarks>
    /// <value>The gyro's angular speed.</value>
    public Vector3 Gyro
    {
        get
        {
            Update();
            return controllerState.gyro;
        }
    }

    /// <summary>Gets the controller's current acceleration in meters per second squared.</summary>
    /// <remarks>
    /// The controller's axes are:
    /// - X points to the right.
    /// - Y points perpendicularly up from the controller's top surface.
    /// - Z lies along the controller's body, pointing towards the front.
    /// <para>
    /// Note that gravity is indistinguishable from acceleration, so when the controller is resting
    /// on a surface, expect to measure an acceleration of 9.8 m/s^2 on the Y axis.  The
    /// accelerometer reading will be zero on all three axes only if the controller is in free fall,
    /// or if the user is in a zero gravity environment like a space station.
    /// </para></remarks>
    /// <value>The acceleration in m/s^2.</value>
    public Vector3 Accel
    {
        get
        {
            Update();
            return controllerState.accel;
        }
    }

    /// <summary>Gets the position of the current touch, if touching the touchpad.</summary>
    /// <remarks>
    /// The X and Y range is from -1.0 to 1.0. (0, 0) is the center of the touchpad.  (-.707, -.707)
    /// is bottom left, (.707, .707) is upper right.  The magnitude of the touch vector is
    /// guaranteed to be less than or equal to 1.0.
    /// </remarks>
    /// <value>
    /// The position of the current touch, if touching the touchpad.  If not touching, this is the
    /// position of the last touch (when the finger left the touchpad).
    /// </value>
    public Vector2 TouchPos
    {
        get
        {
            Update();
            return touchPosCentered;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user just completed the recenter gesture.
    /// </summary>
    /// <remarks>
    /// The headset and the controller's orientation are now being reported in the new recentered
    /// coordinate system.
    /// </remarks>
    /// <value>
    /// Value `true` if the user just completed the recenter gesture, `false` otherwise.  This is an
    /// event flag (it is `true` for only one frame after the event happens, then reverts to
    /// `false`).
    /// </value>
    public bool Recentered
    {
        get
        {
            Update();
            return controllerState.recentered;
        }
    }

    /// <summary>Gets the bitmask of the buttons that are down in the current frame.</summary>
    /// <value>The bitmask of currently-down buttons.</value>
    public GvrControllerButton Buttons
    {
        get { return controllerState.buttonsState; }
    }

    /// <summary>
    /// Gets the bitmask of the buttons that began being pressed in the current frame.
    /// </summary>
    /// <remarks>
    /// Each individual button enum is guaranteed to be followed by exactly one ButtonsUp event in a
    /// later frame.  `ButtonsDown` and `ButtonsUp` will never both be `true` in the same frame for
    /// an individual button.
    /// </remarks>
    /// <value>The bitmask of just-pressed-down buttons.</value>
    public GvrControllerButton ButtonsDown
    {
        get { return controllerState.buttonsDown; }
    }

    /// <summary>
    /// Gets the bitmask of the buttons that ended being pressed in the current frame.
    /// </summary>
    /// <remarks>
    /// Each individual button enum is guaranteed to be preceded by exactly one `ButtonsDown`
    /// event in an earlier frame.  `ButtonsDown` and `ButtonsUp` will never both be `true` in the
    /// same frame for an individual button.
    /// </remarks>
    /// <value>The bitmask of just-released buttons.</value>
    public GvrControllerButton ButtonsUp
    {
        get { return controllerState.buttonsUp; }
    }

    /// <summary>Gets `GvrConnectionState` error details.</summary>
    /// <remarks>
    /// If `State == GvrConnectionState.Error`, this contains details about the error.
    /// </remarks>
    /// <value>The error details.</value>
    public string ErrorDetails
    {
        get
        {
            Update();
            return controllerState.connectionState == GvrConnectionState.Error ?
                controllerState.errorDetails : "";
        }
    }

    /// <summary>
    /// Gets the GVR C library controller state pointer (`gvr_controller_state*`).
    /// </summary>
    /// <value>The state pointer.</value>
    public IntPtr StatePtr
    {
        get
        {
            Update();
            return controllerState.gvrPtr;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the controller is currently being charged.
    /// </summary>
    /// <value>Value `true` if this instance is charging. Otherwise, `false`.</value>
    public bool IsCharging
    {
        get
        {
            Update();
            return controllerState.isCharging;
        }
    }

    /// <summary>Gets the controller's current battery charge level.</summary>
    /// <value>The battery charge level.</value>
    public GvrControllerBatteryLevel BatteryLevel
    {
        get
        {
            Update();
            return controllerState.batteryLevel;
        }
    }

    // Returns `true` if the controller can be positionally tracked.
    internal bool SupportsPositionalTracking
    {
        get { return controllerState.is6DoF; }
    }

    /// <summary>
    /// Gets a value indicating whether the user is holding down any of the buttons specified in
    /// `buttons`.
    /// </summary>
    /// <remarks>
    /// Multiple `GvrControllerButton` types can be checked at once using a bitwise-or operation.
    /// </remarks>
    /// <returns>Returns `true` if the designated button is being held this frame.</returns>
    /// <param name="buttons">The button to get the held state for this frame.</param>
    public bool GetButton(GvrControllerButton buttons)
    {
        Update();
        return (controllerState.buttonsState & buttons) != 0;
    }

    /// <summary>
    /// Gets a value indicating whether the user starts pressing down any of the buttons specified
    /// in `buttons`.
    /// </summary>
    /// <remarks>
    /// For an individual button enum, every `ButtonDown` event is guaranteed to be followed by
    /// exactly one `ButtonUp` event in a later frame.  `ButtonDown` and `ButtonUp` will never both
    /// be `true` in the same frame for an individual button.
    /// <para>
    /// Using multiple button enums together with an `or` statement can result in multiple
    /// `ButtonDown`s before a `ButtonUp`.
    /// </para></remarks>
    /// <returns>Returns `true` if the designated button was pressed this frame.</returns>
    /// <param name="buttons">The button to get the press-state for this frame.</param>
    public bool GetButtonDown(GvrControllerButton buttons)
    {
        Update();
        return (controllerState.buttonsDown & buttons) != 0;
    }

    /// <summary>
    /// Gets a value indicating whether this is the frame after the user stops pressing down any of
    /// the buttons specified in `buttons`.
    /// </summary>
    /// <remarks>
    /// For an individual button enum, every `ButtonUp` event is guaranteed to be preceded by
    /// exactly one `ButtonDown` event in an earlier frame.  `ButtonDown` and `ButtonUp` will never
    /// both be `true` in the same frame for an individual button.
    /// <para>
    /// Using multiple button enums together with an `or` statement can result in multiple
    /// `ButtonUp`s after multiple `ButtonDown`s.
    /// </para></remarks>
    /// <returns>Returns `true` if the designated button was released this frame.</returns>
    /// <param name="buttons">The button to get the release-state for this frame.</param>
    public bool GetButtonUp(GvrControllerButton buttons)
    {
        Update();
        return (controllerState.buttonsUp & buttons) != 0;
    }

    internal void Invalidate()
    {
        valid = false;
    }

    internal void Update()
    {
        if (lastUpdatedFrameCount != Time.frameCount)
        {
            if (!valid)
            {
                Debug.LogError(
                    "Using an invalid GvrControllerInputDevice. "
                    + "Please acquire a new one from GvrControllerInput.GetDevice().");
                return;
            }

            // The controller state must be updated prior to any function using the
            // controller API to ensure the state is consistent throughout a frame.
            lastUpdatedFrameCount = Time.frameCount;

            GvrConnectionState oldState = State;

            controllerProvider.ReadState(controllerState, controllerId);
            UpdateTouchPosCentered();

#if UNITY_EDITOR
            if (IsDominantHand)
            {
                // Make sure the EditorEmulator is updated immediately.
                if (GvrEditorEmulator.Instance != null)
                {
                    GvrEditorEmulator.Instance.UpdateEditorEmulation();
                }
            }
#endif  // UNITY_EDITOR

            if (OnStateChanged != null && State != oldState)
            {
                OnStateChanged(State, oldState);
            }
        }
    }

    private void UpdateTouchPosCentered()
    {
        touchPosCentered.x = (controllerState.touchPos.x - 0.5f) * 2.0f;
        touchPosCentered.y = -(controllerState.touchPos.y - 0.5f) * 2.0f;

        float magnitude = touchPosCentered.magnitude;
        if (magnitude > 1)
        {
            touchPosCentered.x /= magnitude;
            touchPosCentered.y /= magnitude;
        }
    }
}

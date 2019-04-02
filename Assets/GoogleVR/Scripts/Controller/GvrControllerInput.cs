//-----------------------------------------------------------------------
// <copyright file="GvrControllerInput.cs" company="Google Inc.">
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

using System;
using System.Collections;
using Gvr.Internal;
using UnityEngine;

/// <summary>Represents a controller's current connection state.</summary>
/// <remarks>
/// All values and semantics below (except for Error) are from gvr_types.h in the GVR C API.
/// </remarks>
public enum GvrConnectionState
{
    /// <summary>Indicates that an error has occurred.</summary>
    Error = -1,

    /// <summary>Indicates a controller is disconnected.</summary>
    Disconnected = 0,

    /// <summary>Indicates that the device is scanning for controllers.</summary>
    Scanning = 1,

    /// <summary>Indicates that the device is connecting to a controller.</summary>
    Connecting = 2,

    /// <summary>Indicates that the device is connected to a controller.</summary>
    Connected = 3,
}

/// <summary>Represents the status of the controller API.</summary>
/// <remarks>Values and semantics are from `gvr_types.h` in the GVR C API.</remarks>
public enum GvrControllerApiStatus
{
    /// <summary>A Unity-localized error occurred.</summary>
    /// <remarks>This is the only value that isn't in `gvr_types.h`.</remarks>
    Error = -1,

    /// <summary>API is happy and healthy.</summary>
    /// <remarks>
    /// This doesn't mean any controllers are connected, it just means that the underlying service
    /// is working properly.
    /// </remarks>
    Ok = 0,

    // Any other status represents a permanent failure that requires
    // external action to fix:

    /// <summary>
    /// API failed because this device does not support controllers (API is too low, or other
    /// required feature not present).
    /// </summary>
    Unsupported = 1,

    /// <summary>
    /// This app was not authorized to use the service (e.g., missing permissions, the app is
    /// blacklisted by the underlying service, etc).
    /// </summary>
    NotAuthorized = 2,

    /// <summary>The underlying VR service is not present.</summary>
    Unavailable = 3,

    /// <summary>The underlying VR service is too old, needs upgrade.</summary>
    ApiServiceObsolete = 4,

    /// <summary>
    /// The underlying VR service is too new, is incompatible with current client.
    /// </summary>
    ApiClientObsolete = 5,

    /// <summary>The underlying VR service is malfunctioning. Try again later.</summary>
    ApiMalfunction = 6,
}

/// <summary>Represents a controller's current battery level.</summary>
/// <remarks>Values and semantics from gvr_types.h in the GVR C API.</remarks>
public enum GvrControllerBatteryLevel
{
    /// <summary>A Unity-localized error occurred.</summary>
    /// <remarks>This is the only value that isn't in `gvr_types.h`.</remarks>
    Error = -1,

    /// <summary>The battery state is currently unreported.</summary>
    Unknown = 0,

    /// <summary>Equivalent to 1 out of 5 bars on the battery indicator.</summary>
    CriticalLow = 1,

    /// <summary>Equivalent to 2 out of 5 bars on the battery indicator.</summary>
    Low = 2,

    /// <summary>Equivalent to 3 out of 5 bars on the battery indicator.</summary>
    Medium = 3,

    /// <summary>Equivalent to 4 out of 5 bars on the battery indicator.</summary>
    AlmostFull = 4,

    /// <summary>Equivalent to 5 out of 5 bars on the battery indicator.</summary>
    Full = 5,
}

/// <summary>Represents controller buttons.</summary>
/// <remarks>
/// Values 0-9 are from `gvr_types.h` in the GVR C API.  Value 31 is not represented in the C API.
/// </remarks>
public enum GvrControllerButton
{
    /// <summary>The Button under the touch pad.</summary>
    /// <remarks>Formerly known as Click.</remarks>
    TouchPadButton = 1 << 1,

    /// <summary>Touch pad touching indicator.</summary>
    TouchPadTouch = 1 << 31,

    /// <summary>General application button.</summary>
    App = 1 << 3,

    /// <summary>System button.</summary>
    /// <remarks>Formerly known as Home.</remarks>
    System = 1 << 2,

    /// <summary>Primary button on the underside of the controller.</summary>
    Trigger = 1 << 6,

    /// <summary>Secondary button on the underside of the controller.</summary>
    Grip = 1 << 7,

    /// <summary>Buttons reserved for future use. Subject to name change.</summary>
    Reserved2 = 1 << 8,
}

/// <summary>Represents controller handedness.</summary>
public enum GvrControllerHand
{
    /// <summary>Right hand.</summary>
    Right,

    /// <summary>Left hand.</summary>
    Left,

    /// <summary>Alias for dominant hand as specified by `GvrSettings.Handedness`.</summary>
    Dominant,

    /// <summary>Alias for non-dominant hand.</summary>
    NonDominant,
}

/// <summary>Main entry point for the Daydream controller API.</summary>
/// <remarks>
/// To use this API, add this script to a game object in your scene, or use the
/// **GvrControllerMain** prefab.  This is a singleton object. There can only be one object with
/// this script in your scene.
/// <para>
/// To access a controller's state, get a device from `GvrControllerInput.GetDevice` then query it
/// for state. For example, to the dominant controller's current orientation, use
/// `GvrControllerInput.GetDevice(GvrControllerHand.Dominant).Orientation`.
/// </para></remarks>
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrControllerInput")]
public class GvrControllerInput : MonoBehaviour
{
    /// <summary>Indicates how to connect to the controller emulator.</summary>
#if UNITY_EDITOR
    [GvrInfo("Hold Shift to use the Mouse as the dominant controller.\n\n" +
             "Controls:  Shift +\n" +
             "   • Move Mouse = Change Orientation\n" +
             "   • Left Mouse Button = ClickButton\n" +
             "   • Right Mouse Button = AppButton\n" +
             "   • Middle Mouse Button = HomeButton/Recenter\n" +
             "   • Ctrl = IsTouching\n" +
             "   • Ctrl + Move Mouse = Change TouchPos", 8, UnityEditor.MessageType.None)]
    [Tooltip("How to connect to the emulator: USB cable (recommended) or WIFI.")]
    [GvrInfo("Controller Emulator is now Deprecated", 2, UnityEditor.MessageType.Warning)]
#endif  // UNITY_EDITOR
    public EmulatorConnectionMode emulatorConnectionMode = EmulatorConnectionMode.USB;

    private static GvrControllerInputDevice[] instances = new GvrControllerInputDevice[0];
    private static IControllerProvider controllerProvider;
    private static GvrSettings.UserPrefsHandedness handedness;
    private static Action onDevicesChangedInternal;

    /// <summary>Event handler for when the connection state of a controller changes.</summary>
    /// <param name="state">The new state.</param>
    /// <param name="oldState">The previous state.</param>
    public delegate void OnStateChangedEvent(GvrConnectionState state, GvrConnectionState oldState);

    /// <summary>
    /// Event handler for receiving button, touchpad, and IMU updates from the controllers.
    /// </summary>
    /// <remarks>Use this handler to update app state based on controller input.</remarks>
    public static event Action OnControllerInputUpdated;

    /// <summary>
    /// Event handler for receiving a second notification callback, after all
    /// `OnControllerInputUpdated` events have fired.
    /// </summary>
    public static event Action OnPostControllerInputUpdated;

    /// <summary>Event handler for when controller devices have changed.</summary>
    /// <remarks>
    /// Any code that stores a `GvrControllerInputDevice` should get a new device instance from
    /// `GetDevice`. Existing `GvrControllerInputDevice`s will be marked invalid and will log errors
    /// when used. Event handlers are called immediately when added.
    /// </remarks>
    public static event Action OnDevicesChanged
    {
        [SuppressMemoryAllocationError(
            IsWarning = false, Reason = "Only called on input device change.")]
        add
        {
            onDevicesChangedInternal += value;
            value();
        }

        [SuppressMemoryAllocationError(
            IsWarning = false, Reason = "Only called on input device change.")]
        remove
        {
            onDevicesChangedInternal -= value;
        }
    }

    /// <summary>
    /// Event handler for when the connection state of the dominant controller changes.
    /// </summary>
    [System.Obsolete("Replaced by GvrControllerInputDevice.OnStateChangedEvent.")]
    public static event OnStateChangedEvent OnStateChanged
    {
        add
        {
            if (instances.Length > 0)
            {
                instances[0].OnStateChanged += value;
            }
            else
            {
                Debug.LogError(
                    "GvrControllerInput: Adding OnStateChanged event before instance created.");
            }
        }

        remove
        {
            if (instances.Length > 0)
            {
                instances[0].OnStateChanged -= value;
            }
            else
            {
                Debug.LogError(
                    "GvrControllerInput: Removing OnStateChanged event before instance created.");
            }
        }
    }

    /// <summary>Controller Emulatuor connection modes.</summary>
    public enum EmulatorConnectionMode
    {
        /// <summary>Emulator disconnected.</summary>
        OFF,

        /// <summary>Emulator connects over USB.</summary>
        USB,

        /// <summary>Emulator connects over WIFI.</summary>
        WIFI,
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.State`.
    /// <summary>Gets the dominant controller's current connection state.</summary>
    /// <remarks>
    /// Returns `GvrConnectionState.Error` if `GvrControllerInput` is uninitialized.
    /// </remarks>
    /// <value>The state.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.State.")]
    public static GvrConnectionState State
    {
        get
        {
            if (instances.Length == 0)
            {
                return GvrConnectionState.Error;
            }

            return instances[0].State;
        }
    }

    /// <summary>Gets the status of the controller API.</summary>
    /// <remarks>
    /// Returns `GvrControllerApiStatus.Error` if `GvrControllerInput` is uninitialized.
    /// </remarks>
    /// <value>The api status.</value>
    public static GvrControllerApiStatus ApiStatus
    {
        get
        {
            if (instances.Length == 0)
            {
                return GvrControllerApiStatus.Error;
            }

            return instances[0].ApiStatus;
        }
    }

    /// <summary>Gets a value indicating whether battery status is supported.</summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// Value `true` if the GVR Controller Input supports BatteryStatus calls, `false` otherwise.
    /// </value>
    public static bool SupportsBatteryStatus
    {
        get
        {
            if (controllerProvider == null)
            {
                return false;
            }

            return controllerProvider.SupportsBatteryStatus;
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.Orientation`.
    /// <summary>
    /// Gets the dominant controller's current orientation in space, as a quaternion.
    /// </summary>
    /// <remarks>
    /// The rotation is provided in 'orientation space' which means the rotation is given relative
    /// to the last time the user recentered their controllers.  To make a game object in your scene
    /// have the same orientation as the dominant controller, simply assign this quaternion to the
    /// object's `transform.rotation`.  To match the relative rotation, use
    /// `transform.localRotation` instead.
    /// </remarks>
    /// <value>
    /// The orientation.  This is `Quaternion.identity` if `GvrControllerInput` is uninitialized.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.Orientation.")]
    public static Quaternion Orientation
    {
        get
        {
            if (instances.Length == 0)
            {
                return Quaternion.identity;
            }

            return instances[0].Orientation;
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.Gyro`.
    /// <summary>
    /// Gets the dominant controller's current angular speed in radians per second.
    /// </summary>
    /// <remarks>
    /// Uses the right-hand rule (positive means a right-hand rotation about the given axis), as
    /// measured by the controller's gyroscope.  Returns `Vector3.zero` if `GvrControllerInput` is
    /// uninitialized.
    /// <para>
    /// The controller's axes are:
    /// - X points to the right.
    /// - Y points perpendicularly up from the controller's top surface.
    /// - Z lies along the controller's body, pointing towards the front.
    /// </para></remarks>
    /// <value>The gyro's angular speed.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.Gyro.")]
    public static Vector3 Gyro
    {
        get
        {
            if (instances.Length == 0)
            {
                return Vector3.zero;
            }

            return instances[0].Gyro;
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.Accel`.
    /// <summary>
    /// Gets the dominant controller's current acceleration in meters per second squared.
    /// </summary>
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
    /// <value>
    /// The acceleration.  Will be `Vector3.zero` if `GvrControllerInput` is uninitialized.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.Accel.")]
    public static Vector3 Accel
    {
        get
        {
            if (instances.Length == 0)
            {
                return Vector3.zero;
            }

            return instances[0].Accel;
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch)`.
    /// <summary>
    /// Gets a value indicating whether the user is touching the dominant controller's touchpad.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>Value `true` if is touching. Otherwise, `false`.</value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch).")]
    public static bool IsTouching
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButton(GvrControllerButton.TouchPadTouch);
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadTouch)`.
    /// <summary>
    /// Gets a value indicating whether this frame is the frame the user starts touching the
    /// dominant controller's touchpad.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized.  Every `TouchDown` event is
    /// guaranteed to be followed by exactly one `TouchUp` event in a later frame. Also, `TouchDown`
    /// and `TouchUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` if this is the frame after the user starts touching the dominant controller's
    /// touchpad, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadTouch).")]
    public static bool TouchDown
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonDown(GvrControllerButton.TouchPadTouch);
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadTouch)`.
    /// <summary>
    /// Gets a value indicating whether this frame is the frame after the user stops touching the
    /// dominant controller's touchpad.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized.  Every `TouchUp` event is
    /// guaranteed to be preceded by exactly one `TouchDown` event in an earlier frame. Also,
    /// `TouchDown` and `TouchUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` if this is the frame after the user stops touching the dominant controller's
    /// touchpad, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadTouch).")]
    public static bool TouchUp
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonUp(GvrControllerButton.TouchPadTouch);
        }
    }

    /// @deprecated Please migrate to the center-relative `GvrControllerInputDevice.TouchPos`.
    /// <summary>
    /// Gets the position of the dominant controller's current touch, if touching the touchpad.
    /// </summary>
    /// <remarks>
    /// Returns `Vector2(0.5f, 0.5f)` if `GvrControllerInput` is uninitialized.  If not touching,
    /// this is the position of the last touch (when the finger left the touchpad).  The X and Y
    /// range is from 0 to 1.  (0, 0) is the top left of the touchpad and (1, 1) is the bottom right
    /// of the touchpad.
    /// </remarks>
    /// <value>The touch position.</value>
    [System.Obsolete("Obsolete. Migrate to the center-relative GvrControllerInputDevice.TouchPos.")]
    public static Vector2 TouchPos
    {
        get
        {
            if (instances.Length == 0)
            {
                return new Vector2(0.5f, 0.5f);
            }

            Vector2 touchPos = instances[0].TouchPos;
            touchPos.x = (touchPos.x / 2.0f) + 0.5f;
            touchPos.y = (-touchPos.y / 2.0f) + 0.5f;
            return touchPos;
        }
    }

    /// @deprecated Please migrate to the center-relative `GvrControllerInputDevice.TouchPos`.
    /// <summary>
    /// Gets the position of the dominant controller's current touch, if touching the touchpad.
    /// </summary>
    /// <remarks>
    /// Returns `Vector2.zero` if `GvrControllerInput` is uninitialized.  If not touching, this is
    /// the position of the last touch (when the finger left the touchpad).  The X and Y range is
    /// from -1 to 1. (-.707,-.707) is bottom left, (.707,.707) is upper right.  (0, 0) is the
    /// center of the touchpad.  The magnitude of the touch vector is guaranteed to be less than or
    /// equal to 1.
    /// </remarks>
    /// <value>The touch position centered.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.TouchPos.")]
    public static Vector2 TouchPosCentered
    {
        get
        {
            if (instances.Length == 0)
            {
                return Vector2.zero;
            }

            return instances[0].TouchPos;
        }
    }

    /// @deprecated Use `Recentered` to detect when user has completed the recenter gesture.
    /// <summary>Gets a value indicating whether the user is currently recentering.</summary>
    /// <value>Value `true` if the user is currently recentering, `false` otherwise.</value>
    [System.Obsolete("Use Recentered to detect when user has completed the recenter gesture.")]
    public static bool Recentering
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user just completed the recenter gesture.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized.  The headset and the dominant
    /// controller's orientation are now being reported in the new recentered coordinate system.
    /// This is an event flag (it is true for only one frame after the event happens, then reverts
    /// to false).
    /// </remarks>
    /// <value>Value `true` if the user has just finished recentering, `false` otherwise.</value>
    public static bool Recentered
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].Recentered;
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadButton)`.
    /// <summary>
    /// Gets a value indicating whether the user currently holds down the dominant controller's
    /// touchpad button.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// Value `true` if the user currently holds down the dominant controller's touchpad button,
    /// `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.TouchPadButton).")]
    public static bool ClickButton
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButton(GvrControllerButton.TouchPadButton);
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadButton)`.
    /// <summary>
    /// Gets a value indicating whether this is the frame the user starts pressing down the dominant
    /// controller's touchpad button.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized.  Every `ClickButtonDown` event is
    /// guaranteed to be followed by exactly one `ClickButtonUp` event in a later frame. Also,
    /// `ClickButtonDown` and `ClickButtonUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` this is the frame the user started pressing down the dominant controller's
    /// touchpad button, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.TouchPadButton).")]
    public static bool ClickButtonDown
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonDown(GvrControllerButton.TouchPadButton);
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadButton)`.
    /// <summary>
    /// Gets a value indicating whether this is the frame after the user stops pressing down the
    /// dominant controller's touchpad button.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized.  Every `ClickButtonUp` event is
    /// guaranteed to be preceded by exactly one `ClickButtonDown` event in an earlier frame. Also,
    /// `ClickButtonDown` and `ClickButtonUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` if this is the frame after the user stops pressing down the dominant
    /// controller's touchpad button, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.TouchPadButton).")]
    public static bool ClickButtonUp
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonUp(GvrControllerButton.TouchPadButton);
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.GetButton(GvrControllerButton.App)`.
    /// <summary>
    /// Gets a value indicating whether the user is currently holding down the dominant controller's
    /// app button.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// Value `true` if the user is currently holding down the dominant controller's app button,
    /// `false` otherwise.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.App).")]
    public static bool AppButton
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButton(GvrControllerButton.App);
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.GetButtonDown(GvrControllerButton.App)`.
    /// <summary>
    /// Gets a value indicating whether this is the frame the user started pressing down the
    /// dominant controller's app button.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized. Every `AppButtonDown` event is
    /// guaranteed to be followed by exactly one `AppButtonUp` event in a later frame.
    /// `AppButtonDown` and `AppButtonUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` if this is the frame the user started pressing down the dominant controller's
    /// app button, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.App).")]
    public static bool AppButtonDown
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonDown(GvrControllerButton.App);
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.GetButtonUp(GvrControllerButton.App)`.
    /// <summary>
    /// Gets a value indicating whether this is the frame after the user stopped pressing down the
    /// dominant controller's app button.
    /// </summary>
    /// <remarks>
    /// Returns `false` if `GvrControllerInput` is uninitialized. Every `AppButtonUp` event is
    /// guaranteed to be preceded by exactly one `AppButtonDown` event in an earlier frame. Also,
    /// `AppButtonDown` and `AppButtonUp` will never both be `true` in the same frame.
    /// </remarks>
    /// <value>
    /// Value `true` if this is the frame after the user stopped pressing down the dominant
    /// controller's app button, `false` otherwise.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.GetButtonUp(GvrControllerButton.App).")]
    public static bool AppButtonUp
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonUp(GvrControllerButton.App);
        }
    }

    /// @deprecated Replaced by
    /// `GvrControllerInputDevice.GetButtonDown(GvrControllerButton.System)`.
    /// <summary>
    /// Gets a value indicating whether this is the frame the user started pressing down the
    /// dominant controller's system button.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// Value `true` if this is the frame the user started pressing down the dominant controller's
    /// system button, `false` otherwise.
    /// </value>
    [System.Obsolete(
        "Replaced by GvrControllerInputDevice.GetButtonDown(GvrControllerButton.System).")]
    public static bool HomeButtonDown
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButtonDown(GvrControllerButton.System);
        }
    }

    /// @deprecated Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.System).
    /// <summary>
    /// Gets a value indicating whether the user is holding down the dominant controller's system
    /// button.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// Value `true` if the user is holding down the dominant controller's system button, `false`
    /// otherwise.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.GetButton(GvrControllerButton.System).")]
    public static bool HomeButtonState
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].GetButton(GvrControllerButton.System);
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.ErrorDetails`.
    /// <summary>
    /// Gets details about the reasoning behind the Dominant Controller's error state.
    /// </summary>
    /// <remarks>
    /// If the dominant controller's state `== GvrConnectionState.Error`, this contains details
    /// about the error.  If `GvrControllerInput` is uninitialized this returns an error string
    /// describing the uninitialized state.
    /// </remarks>
    /// <value>Details about the reasoning behind the dominant controller's error state.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.ErrorDetails.")]
    public static string ErrorDetails
    {
        get
        {
            if (instances.Length > 0)
            {
                return instances[0].ErrorDetails;
            }
            else
            {
                return "No GvrControllerInput initialized instance found in scene. "
                       + "It may be missing, or it might not have initialized yet.";
            }
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.StatePtr`.
    /// <summary>
    /// Gets the GVR C library controller state pointer (`gvr_controller_state*`) for the dominant
    /// controller.
    /// </summary>
    /// <remarks>Returns `IntPtr.Zero` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>
    /// The GVR C library controller state pointer (`gvr_controller_state*`) for the dominant
    /// controller.
    /// </value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.StatePtr.")]
    public static IntPtr StatePtr
    {
        get
        {
            if (instances.Length == 0)
            {
                return IntPtr.Zero;
            }

            return instances[0].StatePtr;
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.IsCharging`.
    /// <summary>
    /// Gets a value indicating whether the dominant controller is currently being charged.
    /// </summary>
    /// <remarks>Returns `false` if `GvrControllerInput` is uninitialized.</remarks>
    /// <value>Value `true` if the dominant controller is charging.  Otherwise, `false`.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.IsCharging.")]
    public static bool IsCharging
    {
        get
        {
            if (instances.Length == 0)
            {
                return false;
            }

            return instances[0].IsCharging;
        }
    }

    /// @deprecated Replaced by `GvrControllerInputDevice.BatteryLevel`.
    /// <summary>Gets the dominant controller's current battery charge level.</summary>
    /// <remarks>
    /// Returns `GvrControllerBatteryLevel.Error` if `GvrControllerInput` is uninitialized.
    /// </remarks>
    /// <value>The dominant controller's current battery charge level.</value>
    [System.Obsolete("Replaced by GvrControllerInputDevice.BatteryLevel.")]
    public static GvrControllerBatteryLevel BatteryLevel
    {
        get
        {
            if (instances.Length == 0)
            {
                return GvrControllerBatteryLevel.Error;
            }

            return instances[0].BatteryLevel;
        }
    }

    /// <summary>Returns a controller device for the specified hand.</summary>
    /// <returns>The controller input device.</returns>
    /// <param name="hand">The hand whose input device should be fetched.</param>
    public static GvrControllerInputDevice GetDevice(GvrControllerHand hand)
    {
        if (instances.Length == 0)
        {
            return null;
        }

        // Remap Right and Left to Dominant or NonDominant according to settings handedness.
        if (hand == GvrControllerHand.Left || hand == GvrControllerHand.Right)
        {
            if ((int)hand != (int)handedness)
            {
                hand = GvrControllerHand.NonDominant;
            }
            else
            {
                hand = GvrControllerHand.Dominant;
            }
        }

        if (hand == GvrControllerHand.NonDominant)
        {
            return instances[1];
        }
        else
        {
            // Dominant is always controller 0.
            return instances[0];
        }
    }

    private void Awake()
    {
        if (instances.Length > 0)
        {
            Debug.LogError(
                "More than one active GvrControllerInput instance was found in your scene. "
                + "Ensure that there is only one GvrControllerInput.");
            this.enabled = false;
            return;
        }

        if (controllerProvider == null)
        {
            controllerProvider = ControllerProviderFactory.CreateControllerProvider(this);
        }

        handedness = GvrSettings.Handedness;
        int controllerCount = 2;
        instances = new GvrControllerInputDevice[controllerCount];
        for (int i = 0; i < controllerCount; i++)
        {
            instances[i] = new GvrControllerInputDevice(controllerProvider, i);
        }

        if (onDevicesChangedInternal != null)
        {
            onDevicesChangedInternal();
        }

        // Keep screen on here, since GvrControllerInput must be in any GVR scene in order to enable
        // controller capabilities.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Update()
    {
        foreach (var instance in instances)
        {
            if (instance != null)
            {
                instance.Update();
            }
        }

        if (OnControllerInputUpdated != null)
        {
            OnControllerInputUpdated();
        }

        if (OnPostControllerInputUpdated != null)
        {
            OnPostControllerInputUpdated();
        }
    }

    private void OnDestroy()
    {
        foreach (var instance in instances)
        {
            // Ensure this device will error if used again.
            instance.Invalidate();
        }

        instances = new GvrControllerInputDevice[0];
        if (onDevicesChangedInternal != null)
        {
            onDevicesChangedInternal();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (controllerProvider == null)
        {
            return;
        }

        if (paused)
        {
            controllerProvider.OnPause();
        }
        else
        {
            handedness = GvrSettings.Handedness;
            controllerProvider.OnResume();
        }
    }
}

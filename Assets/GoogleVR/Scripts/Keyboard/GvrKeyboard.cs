//-----------------------------------------------------------------------
// <copyright file="GvrKeyboard.cs" company="Google Inc.">
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
using System.Collections.Generic;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Events to update the keyboard.</summary>
/// <remarks>These values depend on C API keyboard values.</remarks>
public enum GvrKeyboardEvent
{
    /// <summary>Unknown error.</summary>
    GVR_KEYBOARD_ERROR_UNKNOWN = 0,

    /// <summary>The keyboard service could not be connected.</summary>
    /// <remarks>This is usually due to the keyboard service not being installed.</remarks>
    GVR_KEYBOARD_ERROR_SERVICE_NOT_CONNECTED = 1,

    /// <summary>No locale was found in the keyboard service.</summary>
    GVR_KEYBOARD_ERROR_NO_LOCALES_FOUND = 2,

    /// <summary>The keyboard service tried to load dynamically but failed.</summary>
    /// <remarks>
    /// This is usually due to the keyboard service not being installed or being out of date.
    /// </remarks>
    GVR_KEYBOARD_ERROR_SDK_LOAD_FAILED = 3,

    /// <summary>Keyboard becomes visible.</summary>
    GVR_KEYBOARD_SHOWN = 4,

    /// <summary>Keyboard becomes hidden.</summary>
    GVR_KEYBOARD_HIDDEN = 5,

    /// <summary>Text has been updated.</summary>
    GVR_KEYBOARD_TEXT_UPDATED = 6,

    /// <summary>Text has been committed.</summary>
    GVR_KEYBOARD_TEXT_COMMITTED = 7
}

/// <summary>Keyboard error codes.</summary>
/// <remarks>These values depend on C API keyboard values.</remarks>
public enum GvrKeyboardError
{
    /// <summary>Unknown error.</summary>
    UNKNOWN = 0,

    /// <summary>The keyboard service could not be connected.</summary>
    /// <remarks>This is usually due to the keyboard service not being installed.</remarks>
    SERVICE_NOT_CONNECTED = 1,

    /// <summary>No locale was found in the keyboard service.</summary>
    NO_LOCALES_FOUND = 2,

    /// <summary>The keyboard service tried to load dynamically but failed.</summary>
    /// <remarks>
    /// This is usually due to the keyboard service not being installed or being out of date.
    /// </remarks>
    SDK_LOAD_FAILED = 3
}

/// <summary>The keyboard input modes.</summary>
/// <remarks>These values depend on C API keyboard values.</remarks>
public enum GvrKeyboardInputMode
{
    /// <summary>A default input mode.</summary>
    /// <remarks>For typing letters.</remarks>
    DEFAULT = 0,

    /// <summary>Indicates a numeric input mode.</summary>
    /// <remarks>For typing numbers and symbols.</remarks>
    NUMERIC = 1
}

/// <summary>
/// Handles keyboard state management such as hiding and displaying the keyboard, directly modifying
/// text and stereoscopic rendering.
/// </summary>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrKeyboard")]
public class GvrKeyboard : MonoBehaviour
{
    /// <summary>Delegate to handle keyboard events and input.</summary>
    public GvrKeyboardDelegateBase keyboardDelegate = null;

    /// <summary>The input mode of the keyboard.</summary>
    public GvrKeyboardInputMode inputMode = GvrKeyboardInputMode.DEFAULT;

    /// <summary>Flag to use the recommended world matrix for the keyboard.</summary>
    public bool useRecommended = true;

    /// <summary>The distance to the keyboard.</summary>
    public float distance = 0;

    private const float EXECUTOR_WAIT = 0.01f;

    private static GvrKeyboard instance;

    private static IKeyboardProvider keyboardProvider;

    private static List<GvrKeyboardEvent> threadSafeCallbacks =
        new List<GvrKeyboardEvent>();

    private static System.Object callbacksLock = new System.Object();

    private KeyboardState keyboardState = new KeyboardState();
    private IEnumerator keyboardUpdate;

    // Private data and callbacks.
    private ErrorCallback errorCallback = null;

    private StandardCallback showCallback = null;
    private StandardCallback hideCallback = null;
    private EditTextCallback updateCallback = null;
    private EditTextCallback enterCallback = null;

#if UNITY_ANDROID
    // Which eye is currently being rendered.
    private bool isRight = false;
#endif // UNITY_ANDROID

    private bool isKeyboardHidden = false;

    /// <summary>Standard keyboard delegate type.</summary>
    public delegate void StandardCallback();

    /// <summary>Edit text keyboard delegate type.</summary>
    /// <param name="edit_text">The edited text which has been typed into the keyboard.</param>
    public delegate void EditTextCallback(string edit_text);

    /// <summary>Keyboard error delegate type.</summary>
    /// <param name="err">The error which raised this callback.</param>
    public delegate void ErrorCallback(GvrKeyboardError err);

    /// <summary>Keyboard delegate type.</summary>
    /// <param name="closure">A closure around the method to call.</param>
    /// <param name="evt">The event which prompted this callback.</param>
    public delegate void KeyboardCallback(IntPtr closure, GvrKeyboardEvent evt);

    /// <summary>Gets or sets the text being affected by this keyboard.</summary>
    /// <value>The text being affected by this keyboard.</value>
    public string EditorText
    {
        get
        {
            return instance != null ? instance.keyboardState.editorText : "";
        }

        set
        {
            keyboardProvider.EditorText = value;
        }
    }

    /// <summary>Gets the current input mode of the keyboard.</summary>
    /// <value>The current input mode of the keyboard.</value>
    public GvrKeyboardInputMode Mode
    {
        get
        {
            return instance != null ? instance.keyboardState.mode : GvrKeyboardInputMode.DEFAULT;
        }
    }

    /// <summary>Gets a value indicating whether this keyboard instance is valid.</summary>
    /// <value>Value `true` if this keyboard instance is valid, `false` otherwise.</value>
    public bool IsValid
    {
        get
        {
            return instance != null ? instance.keyboardState.isValid : false;
        }
    }

    /// <summary>Gets a value indicating whether this keyboard is ready.</summary>
    /// <value>Value `true` if this keyboard is ready, `false` otherwise.</value>
    public bool IsReady
    {
        get
        {
            return instance != null ? instance.keyboardState.isReady : false;
        }
    }

    /// <summary> Gets the world matrix of the keyboard.</summary>
    /// <value>The world matrix of the keyboard.</value>
    public Matrix4x4 WorldMatrix
    {
        get
        {
            return instance != null ? instance.keyboardState.worldMatrix : Matrix4x4.zero;
        }
    }

    /// <summary>Resets keyboard text.</summary>
    public void ClearText()
    {
        if (keyboardProvider != null)
        {
            keyboardProvider.EditorText = "";
        }
    }

    /// <summary>Shows the keyboard.</summary>
    public void Show()
    {
        if (keyboardProvider == null)
        {
            return;
        }

        // Get user matrix.
        Quaternion fixRot = new Quaternion(transform.rotation.x * -1, transform.rotation.y * -1,
                                           transform.rotation.z, transform.rotation.w);

        // Need to convert from left handed to right handed for the Keyboard coordinates.
        Vector3 fixPos =
            new Vector3(transform.position.x, transform.position.y, transform.position.z * -1);
        Matrix4x4 modelMatrix = Matrix4x4.TRS(fixPos, fixRot, Vector3.one);
        Matrix4x4 mat = Matrix4x4.identity;
        Vector3 position = gameObject.transform.position;
        if (position.x == 0 && position.y == 0 && position.z == 0 && !useRecommended)
        {
            // Force use recommended to be true, otherwise keyboard won't show up.
            keyboardProvider.Show(mat, true, distance, modelMatrix);
            return;
        }

        // Matrix needs to be set only if we're not using the recommended one.
        // Uses the values of the keyboard gameobject transform as reported by Unity. If this is
        // the zero vector, parent it under another gameobject instead.
        if (!useRecommended)
        {
            mat = GetKeyboardObjectMatrix(position);
        }

        keyboardProvider.Show(mat, useRecommended, distance, modelMatrix);
    }

    /// <summary>Hides the keyboard.</summary>
    public void Hide()
    {
        if (keyboardProvider != null)
        {
            keyboardProvider.Hide();
        }
    }

    /// <summary>Handle a pointer click on the keyboard.</summary>
    /// <param name="data">The event data associated with this callback.</param>
    public void OnPointerClick(BaseEventData data)
    {
        if (isKeyboardHidden)
        {
            Show();
        }
    }

    [AOT.MonoPInvokeCallback(typeof(GvrKeyboardEvent))]
    private static void OnKeyboardCallback(IntPtr closure, GvrKeyboardEvent keyboardEvent)
    {
        lock (callbacksLock)
        {
            threadSafeCallbacks.Add(keyboardEvent);
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GvrKeyboard instance was found in your scene. "
            + "Ensure that there is only one GvrKeyboard.");
            enabled = false;
            return;
        }

        instance = this;
        if (keyboardProvider == null)
        {
            keyboardProvider = KeyboardProviderFactory.CreateKeyboardProvider(this);
        }
    }

    private void OnDestroy()
    {
        instance = null;
        threadSafeCallbacks.Clear();
    }

    // Use this for initialization.
    private void Start()
    {
        if (keyboardDelegate != null)
        {
            errorCallback = keyboardDelegate.OnKeyboardError;
            showCallback = keyboardDelegate.OnKeyboardShow;
            hideCallback = keyboardDelegate.OnKeyboardHide;
            updateCallback = keyboardDelegate.OnKeyboardUpdate;
            enterCallback = keyboardDelegate.OnKeyboardEnterPressed;
            keyboardDelegate.KeyboardHidden += KeyboardDelegate_KeyboardHidden;
            keyboardDelegate.KeyboardShown += KeyboardDelegate_KeyboardShown;
        }

        keyboardProvider.ReadState(keyboardState);

        if (IsValid)
        {
            if (keyboardProvider.Create(OnKeyboardCallback))
            {
                keyboardProvider.SetInputMode(inputMode);
            }
        }
        else
        {
            Debug.LogError("Could not validate keyboard");
        }
    }

    // Update per-frame data.
    private void Update()
    {
        if (keyboardProvider == null)
        {
            return;
        }

        keyboardProvider.ReadState(keyboardState);
        if (IsReady)
        {
            // Reset position of keyboard.
            if (transform.hasChanged)
            {
                Show();
                transform.hasChanged = false;
            }

            keyboardProvider.UpdateData();
        }
    }

    // Use this function for procedural rendering
    // Gets called twice per frame, once for each eye.
    // On each frame, left eye renders before right eye so
    // we keep track of a boolean that toggles back and forth
    // between each eye.
    private void OnRenderObject()
    {
        if (keyboardProvider == null || !IsReady)
        {
            return;
        }

#if UNITY_ANDROID
        Camera camera = Camera.current;
        if (camera && camera == Camera.main)
        {
            // Get current eye.
            Camera.StereoscopicEye camEye = isRight ?
                                            Camera.StereoscopicEye.Right :
                                            Camera.StereoscopicEye.Left;

            // Camera matrices.
            Matrix4x4 proj = camera.GetStereoProjectionMatrix(camEye);
            Matrix4x4 modelView = camera.GetStereoViewMatrix(camEye);

            // Camera viewport.
            Rect viewport = camera.pixelRect;

            // Render keyboard.
            keyboardProvider.Render((int)camEye, modelView, proj, viewport);

            // Swap.
            isRight = !isRight;
        }
#endif  // !UNITY_ANDROID
    }

    private void OnEnable()
    {
        keyboardUpdate = Executer();
        StartCoroutine(keyboardUpdate);
    }

    private void OnDisable()
    {
        StopCoroutine(keyboardUpdate);
    }

    private void OnApplicationPause(bool paused)
    {
        if (keyboardProvider == null)
        {
            return;
        }

        if (paused)
        {
            keyboardProvider.OnPause();
        }
        else
        {
            keyboardProvider.OnResume();
        }
    }

    private IEnumerator Executer()
    {
        while (true)
        {
            yield return new WaitForSeconds(EXECUTOR_WAIT);

            while (threadSafeCallbacks.Count > 0)
            {
                GvrKeyboardEvent keyboardEvent = threadSafeCallbacks[0];
                PoolKeyboardCallbacks(keyboardEvent);
                lock (callbacksLock)
                {
                    threadSafeCallbacks.RemoveAt(0);
                }
            }
        }
    }

    private void PoolKeyboardCallbacks(GvrKeyboardEvent keyboardEvent)
    {
        switch (keyboardEvent)
        {
            case GvrKeyboardEvent.GVR_KEYBOARD_ERROR_UNKNOWN:
                errorCallback(GvrKeyboardError.UNKNOWN);
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_ERROR_SERVICE_NOT_CONNECTED:
                errorCallback(GvrKeyboardError.SERVICE_NOT_CONNECTED);
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_ERROR_NO_LOCALES_FOUND:
                errorCallback(GvrKeyboardError.NO_LOCALES_FOUND);
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_ERROR_SDK_LOAD_FAILED:
                errorCallback(GvrKeyboardError.SDK_LOAD_FAILED);
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_SHOWN:
                showCallback();
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_HIDDEN:
                hideCallback();
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_TEXT_UPDATED:
                updateCallback(keyboardProvider.EditorText);
                break;
            case GvrKeyboardEvent.GVR_KEYBOARD_TEXT_COMMITTED:
                enterCallback(keyboardProvider.EditorText);
                break;
        }
    }

    private void KeyboardDelegate_KeyboardShown(object sender, System.EventArgs e)
    {
        isKeyboardHidden = false;
    }

    private void KeyboardDelegate_KeyboardHidden(object sender, System.EventArgs e)
    {
        isKeyboardHidden = true;
    }

    // Returns a matrix populated by the keyboard's gameobject position. If the position is not
    // zero, but comes back as zero, parent this under another gameobject instead.
    private Matrix4x4 GetKeyboardObjectMatrix(Vector3 position)
    {
        // Set keyboard position based on this gameObject's position.
        float angleX = Mathf.Atan2(position.y, position.x);
        float kTanAngleX = Mathf.Tan(angleX);
        float newPosX = kTanAngleX * position.x;

        float angleY = Mathf.Atan2(position.x, position.y);
        float kTanAngleY = Mathf.Tan(angleY);
        float newPosY = kTanAngleY * position.y;

        float angleZ = Mathf.Atan2(position.y, position.z);
        float kTanAngleZ = Mathf.Tan(angleZ);
        float newPosZ = kTanAngleZ * position.z;

        Vector3 keyboardPosition = new Vector3(newPosX, newPosY, newPosZ);
        Vector3 lookPosition = Camera.main.transform.position;

        Quaternion rotation = Quaternion.LookRotation(lookPosition);
        Matrix4x4 mat = new Matrix4x4();
        mat.SetTRS(keyboardPosition, rotation, position);

        // Set diagonal to identity if any of them are zero.
        if (mat[0, 0] == 0)
        {
            Vector4 row0 = mat.GetRow(0);
            mat.SetRow(0, new Vector4(1, row0.y, row0.z, row0.w));
        }

        if (mat[1, 1] == 0)
        {
            Vector4 row1 = mat.GetRow(1);
            mat.SetRow(1, new Vector4(row1.x, 1, row1.z, row1.w));
        }

        if (mat[2, 2] == 0)
        {
            Vector4 row2 = mat.GetRow(2);
            mat.SetRow(2, new Vector4(row2.x, row2.y, 1, row2.w));
        }

        return mat;
    }
}

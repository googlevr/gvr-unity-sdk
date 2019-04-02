//-----------------------------------------------------------------------
// <copyright file="GvrSettings.cs" company="Google Inc.">
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
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif  // UNITY_EDITOR

using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRDevice = UnityEngine.VR.VRDevice;
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// <summary>Accesses and configures Daydream settings.</summary>
public static class GvrSettings
{
    /// <summary>Name of 'None' VR SDK, as returned by `VRSettings.loadedDeviceName`.</summary>
    public const string VR_SDK_NONE = "None";

    /// <summary>Name of Daydream GVR SDK, as returned by `VRSettings.loadedDeviceName`.</summary>
    public const string VR_SDK_DAYDREAM = "daydream";

    /// <summary>
    /// Name of Cardboard GVR SDK, as returned by <see cref="VRSettings.loadedDeviceName" />.
    /// </summary>
    public const string VR_SDK_CARDBOARD = "cardboard";

    private const string METHOD_GET_WINDOW = "getWindow";
    private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
    private const string METHOD_SET_SUSTAINED_PERFORMANCE_MODE = "setSustainedPerformanceMode";

#if UNITY_EDITOR
    // This allows developers to test handedness in the editor emulator.
    private const string EMULATOR_HANDEDNESS_PREF_NAME = "GoogleVREditorEmulatorHandedness";

    private static ViewerPlatformType editorEmulatorOnlyViewerPlatformType =
        ViewerPlatformType.Daydream;
#endif  // UNITY_EDITOR

    /// <summary>Viewer type.</summary>
    public enum ViewerPlatformType
    {
        /// <summary>An error value indicating that something has gone wrong.</summary>
        /// <remarks>Plugin-only value, does not exist in the NDK.</remarks>
        Error = -1,

        /// <summary>The Google Cardboard platform.</summary>
        Cardboard,

        /// <summary>The Google Daydream platform.</summary>
        Daydream
    }

    /// <summary>Handedness preference values.</summary>
    public enum UserPrefsHandedness
    {
        /// <summary>An error value indicating that something has gone wrong.</summary>
        /// <remarks>Plugin-only value, does not exist in the NDK.</remarks>
        Error = -1,

        /// <summary>A right-handed preference.</summary>
        Right,

        /// <summary>A left-handed preference.</summary>
        Left
    }

// Expose a setter only for the editor emulator, for development testing purposes.
#if UNITY_EDITOR
    /// <summary>Gets or sets the viewer platform type setting.</summary>
    /// <remarks> In the editor this can be set for devlopment testing.</remarks>
    /// <value>The viewer platform type setting.</value>
#else // UNITY_EDITOR
    /// <summary>Gets the viewer platform type setting.</summary>
    /// <remarks> In the editor this can be set for devlopment testing.</remarks>
    /// <value>The viewer platform type setting.</value>
#endif // UNITY_EDITOR
    public static ViewerPlatformType ViewerPlatform
    {
#if UNITY_EDITOR
// Running in-editor.
        get { return editorEmulatorOnlyViewerPlatformType; }
        set { editorEmulatorOnlyViewerPlatformType = value; }
#elif !UNITY_ANDROID
// Running in non-Android player.
        get { return ViewerPlatformType.Error; }
#else
// Running on Android.
        get
        {
            IntPtr gvrContextPtr = GetValidGvrNativePtrOrLogError();
            if (gvrContextPtr == IntPtr.Zero)
            {
                return ViewerPlatformType.Error;
            }

            return (ViewerPlatformType)gvr_get_viewer_type(gvrContextPtr);
        }
#endif  // UNITY_EDITOR
    }

    /// <summary>Sets a value indicating whether sustained performance mode is enabled.</summary>
    /// <remarks><para>
    /// The developer is expected to remember whether sustained performance mode is set at runtime,
    /// via the checkbox in Player Settings.
    /// </para><para>
    /// This state may be recorded here in a future release.
    /// </para></remarks>
    /// <value>The sustained performance mode setting.</value>
    public static bool SustainedPerformanceMode
    {
        set { SetSustainedPerformanceMode(value); }
    }

#if UNITY_EDITOR
    /// <summary>Gets or sets the user's handedness preference value.</summary>
    /// <value>The user's handedness preference value.</value>
#else   // UNITY_EDITOR
    /// <summary>Gets the user's handedness preference value.</summary>
    /// <value>The user's handedness preference value.</value>
#endif  // UNITY_EDITOR
    public static UserPrefsHandedness Handedness
    {
#if UNITY_EDITOR
// Expose a setter only for the editor emulator, for development testing purposes.
        get
        {
            if (Gvr.Internal.InstantPreview.IsActive)
            {
                return Gvr.Internal.InstantPreview.Instance.handedness;
            }
            else
            {
                return (UserPrefsHandedness)EditorPrefs.GetInt(
                    EMULATOR_HANDEDNESS_PREF_NAME, (int)UserPrefsHandedness.Right);
            }
        }

        set
        {
            EditorPrefs.SetInt(EMULATOR_HANDEDNESS_PREF_NAME, (int)value);
        }
#elif !UNITY_ANDROID
// Running in non-Android player.
        get { return UserPrefsHandedness.Error; }
#else
// Running on Android.
        get
        {
            IntPtr gvrContextPtr = GetValidGvrNativePtrOrLogError();
            if (gvrContextPtr == IntPtr.Zero)
            {
                Debug.LogError("Unable to determine GVR user prefs' handedness");
                return UserPrefsHandedness.Error;
            }

            IntPtr gvrUserPrefsPtr = gvr_get_user_prefs(gvrContextPtr);
            if (gvrUserPrefsPtr == IntPtr.Zero)
            {
                Debug.Log(
                    "Zero GVR user prefs pointer, unable to determine GVR user prefs' handedness");
                return UserPrefsHandedness.Error;
            }

            return (UserPrefsHandedness)gvr_user_prefs_get_controller_handedness(gvrUserPrefsPtr);
        }
#endif  // UNITY_EDITOR
    }

    /// <summary>Wraps call to `VRDevice.GetNativePtr()`.</summary>
    /// <remarks>
    /// Logs error if a supported GVR SDK is not active or if the returned native pointer is
    /// `IntPtr.Zero`.
    /// </remarks>
    /// <returns>An int pointer representing a GVR context.</returns>
    public static IntPtr GetValidGvrNativePtrOrLogError()
    {
        if (!XRSettings.enabled)
        {
            Debug.LogError("VR is disabled");
            return IntPtr.Zero;
        }

#if UNITY_2018_3_OR_NEWER
        string loadedDeviceName = GvrXREventsSubscriber.loadedDeviceName;
#else // !UNITY_2018_3_OR_NEWER; this leaks 30 bytes of memory per update.
        string loadedDeviceName = XRSettings.loadedDeviceName;
#endif // UNITY_2018_3_OR_NEWER
        if (loadedDeviceName != VR_SDK_DAYDREAM && loadedDeviceName != VR_SDK_CARDBOARD)
        {
            Debug.LogErrorFormat("Loaded VR SDK '{0}' must be '{1}' or '{2}'",
                loadedDeviceName, VR_SDK_DAYDREAM, VR_SDK_CARDBOARD);
            return IntPtr.Zero;
        }

        IntPtr gvrContextPtr = XRDevice.GetNativePtr();
        if (gvrContextPtr == IntPtr.Zero)
        {
            Debug.LogError("Unexpected zero GVR native context pointer");
            return gvrContextPtr;
        }

        return gvrContextPtr;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
    internal static extern IntPtr gvr_get_user_prefs(IntPtr gvrContextPtr);

    [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
    private static extern int gvr_get_viewer_type(IntPtr gvrContextPtr);

    [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
    private static extern int gvr_user_prefs_get_controller_handedness(IntPtr gvrUserPrefsPtr);
#endif  // UNITY_ANDROID && !UNITY_EDITOR

    private static void SetSustainedPerformanceMode(bool enabled)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject androidActivity = null;
        try
        {
            androidActivity = GvrActivityHelper.GetActivity();
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("Exception while connecting to the Activity: " + e);
            return;
        }

        AndroidJavaObject androidWindow
            = androidActivity.Call<AndroidJavaObject>(METHOD_GET_WINDOW);
        if (androidWindow == null)
        {
            Debug.LogError("No window found on the current android activity");
            return;
        }

        // The sim thread in Unity is single-threaded, so we don't need to lock when accessing
        // or assigning androidWindow.
        androidActivity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() =>
        {
            androidWindow.Call(METHOD_SET_SUSTAINED_PERFORMANCE_MODE, enabled);
            Debug.Log("Set sustained performance mode: " + (enabled ? "ON" : "OFF"));
        }));
#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }
}

//-----------------------------------------------------------------------
// <copyright file="GvrCardboardHelpers.cs" company="Google Inc.">
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

using System;
using System.Runtime.InteropServices;

/// <summary>General Cardboard helper methods.</summary>
public class GvrCardboardHelpers
{
    /// Manual recenter for Cardboard apps. After recentering the camera's orientation will be given
    /// in the new recentered coordinate system.
    /// Do not use for Daydream apps as controller based recentering is handled automatically by
    /// Google VR Services, see `GvrControllerInput.Rencentered` for details.
    public static void Recenter()
    {
#if UNITY_EDITOR
        if (GvrEditorEmulator.Instance != null)
        {
            GvrEditorEmulator.Instance.Recenter();
        }

#elif (UNITY_ANDROID || UNITY_IOS)
        IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
        if (gvrContextPtr == IntPtr.Zero)
        {
            return;
        }

        gvr_reset_tracking(gvrContextPtr);
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Debug.Log("Use GvrEditorEmulator for in-editor recentering");
    }

    /// Set the Cardboard viewer params.
    /// Example URI for 2015 Cardboard Viewer V2:
    /// http://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAASEIAAEhCAABIQgAASEJYADUpXA89OggeZnc-Ej6aPlAAYAM
    public static void SetViewerProfile(String viewerProfileUri)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
        if (gvrContextPtr == IntPtr.Zero)
        {
            return;
        }

        gvr_set_default_viewer_profile(gvrContextPtr, viewerProfileUri);
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Debug.Log("Unavailable for non-Android and non-iOS builds");
    }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
    private static extern void gvr_reset_tracking(IntPtr gvr_context);

    [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
    private static extern void gvr_set_default_viewer_profile(
        IntPtr gvr_context,
        [MarshalAs(UnmanagedType.LPStr)] string viewer_profile_uri);
#endif  // (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
}

//-----------------------------------------------------------------------
// <copyright file="GvrVRHelpers.cs" company="Google Inc.">
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
using System.Collections;
using Gvr.Internal;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
using XRNode = UnityEngine.VR.VRNode;
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// <summary>Helper functions common to GVR VR applications.</summary>
public static class GvrVRHelpers
{
    /// <summary>Returns the center of the viewport.</summary>
    public static Vector2 GetViewportCenter()
    {
        int viewportWidth = Screen.width;
        int viewportHeight = Screen.height;
        if (XRSettings.enabled)
        {
            viewportWidth = XRSettings.eyeTextureWidth;
            viewportHeight = XRSettings.eyeTextureHeight;
        }

        return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
    }

    /// <summary>Returns the forward vector relative to the head rotation.</summary>
    public static Vector3 GetHeadForward()
    {
        return GetHeadRotation() * Vector3.forward;
    }

    /// <summary>Returns the head rotation.</summary>
    public static Quaternion GetHeadRotation()
    {
#if UNITY_EDITOR
        if (InstantPreview.Instance != null && InstantPreview.Instance.IsCurrentlyConnected)
        {
            // In-editor; Instant Preview is active:
            return Camera.main.transform.localRotation;
        }
        else
        {
            // In-editor; Instant Preview is not active:
            if (GvrEditorEmulator.Instance == null)
            {
                Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please ensure that " +
                "GvrEditorEmulator exists in your scene.");
                return Quaternion.identity;
            }

            return GvrEditorEmulator.Instance.HeadRotation;
        }
#else
        // Not running in editor:
        return InputTracking.GetLocalRotation(XRNode.Head);
#endif // UNITY_EDITOR
    }

    /// <summary>Returns the head position.</summary>
    public static Vector3 GetHeadPosition()
    {
#if UNITY_EDITOR
        if (GvrEditorEmulator.Instance == null)
        {
            Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please ensure that " +
            "GvrEditorEmulator exists in your scene.");
            return Vector3.zero;
        }

        return GvrEditorEmulator.Instance.HeadPosition;
#else
        return InputTracking.GetLocalPosition(XRNode.Head);
#endif // UNITY_EDITOR
    }

    /// <summary>Returns the recommended maximum laser distance for the given mode.</summary>
    public static float GetRecommendedMaxLaserDistance(GvrBasePointer.RaycastMode mode)
    {
        switch (mode)
        {
            case GvrBasePointer.RaycastMode.Direct:
                return 20.0f;
            case GvrBasePointer.RaycastMode.Hybrid:
                return 1.0f;
            case GvrBasePointer.RaycastMode.Camera:
            default:
                return 0.75f;
        }
    }

    /// <summary>Returns the distance of the ray intersection for the given mode.</summary>
    public static float GetRayIntersection(GvrBasePointer.RaycastMode mode)
    {
        switch (mode)
        {
            case GvrBasePointer.RaycastMode.Direct:
                return 0.0f;
            case GvrBasePointer.RaycastMode.Hybrid:
                return 0.0f;
            case GvrBasePointer.RaycastMode.Camera:
            default:
                return 2.5f;
        }
    }

    /// <summary>Returns true if the laser should be shrunk based on the given mode.</summary>
    public static bool GetShrinkLaser(GvrBasePointer.RaycastMode mode)
    {
        switch (mode)
        {
            case GvrBasePointer.RaycastMode.Direct:
                return false;
            case GvrBasePointer.RaycastMode.Hybrid:
                return true;
            case GvrBasePointer.RaycastMode.Camera:
            default:
                return false;
        }
    }
}

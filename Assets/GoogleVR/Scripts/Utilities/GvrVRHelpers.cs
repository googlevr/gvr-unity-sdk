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

using System.Collections;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
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
    /// <summary>Gets the center of the screen or eye texture, in pixels.</summary>
    /// <returns>The center of the screen, in pixels.</returns>
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

    /// <summary>Gets the forward vector relative to the headset's rotation.</summary>
    /// <returns>The forward vector relative to the headset's rotation.</returns>
    public static Vector3 GetHeadForward()
    {
        return GetHeadRotation() * Vector3.forward;
    }

    /// <summary>Gets the headset's rotation.</summary>
    /// <returns>The headset's rotation.</returns>
    public static Quaternion GetHeadRotation()
    {
#if UNITY_EDITOR
        if (InstantPreview.IsActive)
        {
            // In-editor; Instant Preview is active:
            return Camera.main.transform.localRotation;
        }
        else
        {
            // In-editor; Instant Preview is not active:
            if (GvrEditorEmulator.Instance == null)
            {
                Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please " +
                                 "ensure that GvrEditorEmulator exists in your scene.");
                return Quaternion.identity;
            }

            return GvrEditorEmulator.Instance.HeadRotation;
        }
#else
        // Not running in editor:
        return InputTracking.GetLocalRotation(XRNode.Head);
#endif // UNITY_EDITOR
    }

    /// <summary>Gets the head's position.</summary>
    /// <returns>The head's position.</returns>
    public static Vector3 GetHeadPosition()
    {
#if UNITY_EDITOR
        if (GvrEditorEmulator.Instance == null)
        {
            Debug.LogWarning("No GvrEditorEmulator instance was found in your scene. Please " +
                             "ensure that GvrEditorEmulator exists in your scene.");
            return Vector3.zero;
        }

        return GvrEditorEmulator.Instance.HeadPosition;
#else
        return InputTracking.GetLocalPosition(XRNode.Head);
#endif // UNITY_EDITOR
    }

    /// <summary>Gets the recommended max laser distance, based on raycast mode.</summary>
    /// <param name="mode">The `RaycastMode` for which to get the recommended distance.</param>
    /// <returns>The recommended maximum laser distance for the given mode.</returns>
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

    /// <summary>Gets the distance at which the `Direct` and `Camera` raycasts intersect.</summary>
    /// <remarks>
    /// This is the the point at which `Hybrid` mode will transition from `Direct` (closer than the
    /// intersection) to `Camera` (further than the intersection) mode.
    /// </remarks>
    /// <param name="mode">
    /// The `RaycastMode` for which to get the intersection distance.  Only returns non-zero when
    /// this is `RaycastMode.Camera`.
    /// </param>
    /// <returns>The distance at which the `Direct` and `Camera` raycasts intersect.</returns>
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

    /// <summary>Returns a value indicating whether the laser is visually shrunken.</summary>
    /// <param name="mode">The `RaycastMode` for which to check behavior.</param>
    /// <returns>Returns `true` if the laser is shrunken, `false`a otherwise.</returns>
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

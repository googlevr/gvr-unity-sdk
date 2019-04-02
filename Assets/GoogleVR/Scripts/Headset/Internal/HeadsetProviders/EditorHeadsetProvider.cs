//-----------------------------------------------------------------------
// <copyright file="EditorHeadsetProvider.cs" company="Google Inc.">
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

/// @cond
namespace Gvr.Internal
{
    using Gvr;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif // UNITY_EDITOR
    using UnityEngine;

    public class EditorHeadsetProvider : IHeadsetProvider
    {
        public const float DEFAULT_FLOOR_HEIGHT_3DOF = -1.6f;
        public static readonly Vector3 DEFAULT_RECENTER_TRANSFORM_POSITION = Vector3.zero;
        public static readonly Quaternion DEFAULT_RECENTER_TRANSFORM_ROTATION
                = Quaternion.identity;
        public const GvrSafetyRegionType DEFAULT_SAFETY_REGION_TYPE_3DOF
                = GvrSafetyRegionType.Cylinder;
        public const float DEFAULT_SAFETY_CYLINDER_ENTER_RADIUS_3DOF = 0.6f;
        public const float DEFAULT_SAFETY_CYLINDER_EXIT_RADIUS_3DOF = 0.7f;

        private HeadsetState dummyState;
#if UNITY_EDITOR
        private HashSet<string> printedErrorMessages = new HashSet<string>();
#endif // UNITY_EDITOR

        public bool SupportsPositionalTracking
        {
            get
            {
#if UNITY_EDITOR
                UnityEditor.XR.Daydream.SupportedHeadTracking minTrackingState
                        = UnityEditor.PlayerSettings.VRDaydream.minimumSupportedHeadTracking;
                UnityEditor.XR.Daydream.SupportedHeadTracking maxTrackingState
                        = UnityEditor.PlayerSettings.VRDaydream.maximumSupportedHeadTracking;

                if (minTrackingState == UnityEditor.XR.Daydream.SupportedHeadTracking.ThreeDoF
                    && maxTrackingState == UnityEditor.XR.Daydream.SupportedHeadTracking.ThreeDoF)
                {
                    return false;
                }
                else if (
                    minTrackingState == UnityEditor.XR.Daydream.SupportedHeadTracking.ThreeDoF
                    && maxTrackingState == UnityEditor.XR.Daydream.SupportedHeadTracking.SixDoF)
                {
                    if (InstantPreview.IsActive
                        && InstantPreview.Instance.supportsPositionalHeadTracking.isValid)
                    {
                        return InstantPreview.Instance.supportsPositionalHeadTracking.value;
                    }
                    else
                    {
                        return GvrHeadset.editorSupportsPositionalHeadTracking;
                    }
                }
                else // Positional head tracking required.
                {
                    if (InstantPreview.IsActive
                        && InstantPreview.Instance.supportsPositionalHeadTracking.isValid
                        && !InstantPreview.Instance.supportsPositionalHeadTracking.value)
                    {
                        string err_msg = "XRSettings > Daydream > Positional Head Tracking is "
                                + "'required', but the connected device only supports "
                                + "orientation.";
                        if (!printedErrorMessages.Contains(err_msg))
                        {
                            Debug.LogError(err_msg);
                            printedErrorMessages.Add(err_msg);
                        }
                    }
                    return true;
                }
#else // UNITY_EDITOR
                return false;
#endif // UNITY_EDITOR
            }
        }

        public void PollEventState(ref HeadsetState state)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.events.Count > 0)
                {
                    InstantPreview.UnityGvrEvent eventState
                        = InstantPreview.Instance.events.Dequeue();
                    switch (eventState.type)
                    {
                        case InstantPreview.GvrEventType.GVR_EVENT_NONE:
                            state.eventType = GvrEventType.Invalid;
                            break;
                        case InstantPreview.GvrEventType.GVR_EVENT_RECENTER:
                            state.eventType = GvrEventType.Recenter;
                            break;
                        case InstantPreview.GvrEventType.GVR_EVENT_SAFETY_REGION_EXIT:
                            state.eventType = GvrEventType.SafetyRegionExit;
                            break;
                        case InstantPreview.GvrEventType.GVR_EVENT_SAFETY_REGION_ENTER:
                            state.eventType = GvrEventType.SafetyRegionEnter;
                            break;
                        case InstantPreview.GvrEventType.GVR_EVENT_HEAD_TRACKING_RESUMED:
                            // Currently not supported.
                            state.eventType = GvrEventType.Invalid;
                            break;
                        case InstantPreview.GvrEventType.GVR_EVENT_HEAD_TRACKING_PAUSED:
                            // Currently not supported.
                            state.eventType = GvrEventType.Invalid;
                            break;
                    }

                    state.eventFlags = (int)eventState.flags;
                    state.eventTimestampNs = eventState.timestamp;

                    // Only add recenter-specific fields if this is a recenter event.
                    if (eventState.type == InstantPreview.GvrEventType.GVR_EVENT_RECENTER)
                    {
                        switch (eventState.gvr_recenter_event_data.recenter_type)
                        {
                            case InstantPreview.GvrRecenterEventType.GVR_RECENTER_EVENT_NONE:
                                state.recenterEventType = GvrRecenterEventType.Invalid;
                                break;
                            case InstantPreview.GvrRecenterEventType.GVR_RECENTER_EVENT_RESTART:
                                state.recenterEventType = GvrRecenterEventType.RecenterEventRestart;
                                break;
                            case InstantPreview.GvrRecenterEventType.GVR_RECENTER_EVENT_ALIGNED:
                                state.recenterEventType = GvrRecenterEventType.RecenterEventAligned;
                                break;
                            case InstantPreview.GvrRecenterEventType.GVR_RECENTER_EVENT_DON:
                                // Currently not supported.
                                state.recenterEventType = GvrRecenterEventType.Invalid;
                                break;
                        }

                        state.recenterEventFlags
                            = eventState.gvr_recenter_event_data.recenter_event_flags;
                        GvrMathHelpers.GvrMatrixToUnitySpace(
                            eventState.gvr_recenter_event_data
                                .start_space_from_tracking_space_transform,
                            out state.recenteredPosition,
                            out state.recenteredRotation);
                    }
                }
                else
                {
                    state.eventType = GvrEventType.Invalid;
                }
            }

            return;
#endif // UNITY_ANDROID && UNITY_EDITOR
            // Events are unavailable through emulation.
        }

        public bool TryGetFloorHeight(ref float floorHeight)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.floorHeight.isValid)
                {
                    floorHeight = InstantPreview.Instance.floorHeight.value;
                }

                return InstantPreview.Instance.floorHeight.isValid;
            }
#endif // UNITY_ANDROID && UNITY_EDITOR
            floorHeight = DEFAULT_FLOOR_HEIGHT_3DOF;
            return true;
        }

        public bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.recenterTransform.isValid)
                {
                    GvrMathHelpers.GvrMatrixToUnitySpace(
                        InstantPreview.Instance.recenterTransform.value,
                        out position,
                        out rotation);
                }

                return InstantPreview.Instance.recenterTransform.isValid;
            }
#endif // UNITY_ANDROID && UNITY_EDITOR
            position = DEFAULT_RECENTER_TRANSFORM_POSITION;
            rotation = DEFAULT_RECENTER_TRANSFORM_ROTATION;
            return true;
        }

        public bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.safetyRegionType.isValid)
                {
                    safetyType
                        = (GvrSafetyRegionType)InstantPreview.Instance.safetyRegionType.value;
                }

                return InstantPreview.Instance.safetyRegionType.isValid;
            }
#endif // UNITY_ANDROID && UNITY_EDITOR
            safetyType = DEFAULT_SAFETY_REGION_TYPE_3DOF;
            return true;
        }

        public bool TryGetSafetyCylinderInnerRadius(ref float innerRadius)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.safetyCylinderEnterRadius.isValid)
                {
                    innerRadius = InstantPreview.Instance.safetyCylinderEnterRadius.value;
                }

                return InstantPreview.Instance.safetyCylinderEnterRadius.isValid;
            }
#endif // UNITY_ANDROID && UNITY_EDITOR
            innerRadius = DEFAULT_SAFETY_CYLINDER_ENTER_RADIUS_3DOF;
            return true;
        }

        public bool TryGetSafetyCylinderOuterRadius(ref float outerRadius)
        {
#if UNITY_ANDROID && UNITY_EDITOR
            if (InstantPreview.IsActive)
            {
                if (InstantPreview.Instance.safetyCylinderExitRadius.isValid)
                {
                    outerRadius = InstantPreview.Instance.safetyCylinderExitRadius.value;
                }

                return InstantPreview.Instance.safetyCylinderExitRadius.isValid;
            }
#endif // UNITY_ANDROID && UNITY_EDITOR
            outerRadius = DEFAULT_SAFETY_CYLINDER_EXIT_RADIUS_3DOF;
            return true;
        }
    }
}

/// @endcond

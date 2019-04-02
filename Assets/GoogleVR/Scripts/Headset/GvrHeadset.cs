//-----------------------------------------------------------------------
// <copyright file="GvrHeadset.cs" company="Google Inc.">
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
using System.ComponentModel;
using Gvr.Internal;
using UnityEngine;

/// <summary>Main entry point for Standalone headset APIs.</summary>
/// <remarks>
/// To use this API, use the GvrHeadset prefab. There can be only one such prefab in a scene, since
/// this is a singleton object.
/// </remarks>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrHeadset")]
public class GvrHeadset : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>Whether this app supports Positional Head Tracking in editor play mode.</summary>
    /// <remarks>
    /// This is a user-controlled field which can be toggled in the inspector for the GvrHeadset.
    /// Its value is ignored if there is a connected device running Instant Preview.
    /// </remarks>
    public static bool editorSupportsPositionalHeadTracking = false;
#endif // UNITY_EDITOR

    private static GvrHeadset instance;

    private IHeadsetProvider headsetProvider;
    private HeadsetState headsetState;
    private IEnumerator headsetUpdate;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    // Delegates for GVR events.
    private OnSafetyRegionEvent safetyRegionDelegate;
    private OnRecenterEvent recenterDelegate;

    /// <summary>Initializes a new instance of the <see cref="GvrHeadset" /> class.</summary>
    protected GvrHeadset()
    {
        headsetState.Initialize();
    }

    // Delegate definitions.

    /// <summary>
    /// This delegate is called when the headset crosses the safety region boundary.
    /// </summary>
    /// <param name="enter">
    /// Set to `true` if the safety region is being entered, or `false` if the safety region is
    /// being exited.
    /// </param>
    public delegate void OnSafetyRegionEvent(bool enter);

    /// <summary>This delegate is called after the headset is recentered.</summary>
    /// <param name="recenterType">Indicates the reason recentering occurred.</param>
    /// <param name="recenterFlags">Flags related to recentering.  See |GvrRecenterFlags|.</param>
    /// <param name="recenteredPosition">The positional offset from the session start pose.</param>
    /// <param name="recenteredOrientation">
    /// The rotational offset from the session start pose.
    /// </param>
    public delegate void OnRecenterEvent(GvrRecenterEventType recenterType,
                                         GvrRecenterFlags recenterFlags,
                                         Vector3 recenteredPosition,
                                         Quaternion recenteredOrientation);

#region DELEGATE_HANDLERS
    /// <summary>Event handlers for `OnSafetyRegionChange`.</summary>
    /// <remarks>Triggered when the safety region has been entered or exited.</remarks>
    public static event OnSafetyRegionEvent OnSafetyRegionChange
    {
        add
        {
            if (instance != null)
            {
                instance.safetyRegionDelegate += value;
            }
        }

        remove
        {
            if (instance != null)
            {
                instance.safetyRegionDelegate -= value;
            }
        }
    }

    /// <summary>Event handlers for `OnRecenter`.</summary>
    /// <remarks>Triggered when a recenter command has been issued by the user.</remarks>
    public static event OnRecenterEvent OnRecenter
    {
        add
        {
            if (instance != null)
            {
                instance.recenterDelegate += value;
            }
        }

        remove
        {
            if (instance != null)
            {
                instance.recenterDelegate -= value;
            }
        }
    }
#endregion  // DELEGATE_HANDLERS

#region GVR_HEADSET_PROPERTIES
    /// <summary>
    /// Gets a value indicating whether this headset supports 6DoF positional tracking.
    /// </summary>
    /// <value>
    /// Value `true` if this headset supports 6DoF positional tracking, or `false` if only 3DoF
    /// rotation-based head tracking is supported.
    /// </value>
    public static bool SupportsPositionalTracking
    {
        get
        {
            if (instance == null)
            {
                return false;
            }

            try
            {
                return instance.headsetProvider.SupportsPositionalTracking;
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading SupportsPositionalTracking: " + e.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this headset provides an Editor Emulator.
    /// </summary>
    /// <value>
    /// Value `true` if this headset provides an Editor Emulator, or `false` otherwise.
    /// </value>
    public bool ProvidesEditorEmulator
    {
        get
        {
            return headsetProvider as EditorHeadsetProvider != null;
        }
    }

    /// <summary>Populates `floorHeight` with the detected height, if one is available.</summary>
    /// <remarks>This may be unavailable if the underlying GVR API call fails.</remarks>
    /// <returns>
    /// Returns `true` if value retrieval was successful, `false` otherwise (depends on tracking
    /// state).
    /// </returns>
    /// <param name="floorHeight">
    /// If this call returns `true`, this value is set to the retrieved `floorHeight`.  Otherwise
    /// leaves the value unchanged.
    /// </param>
    [SuppressMemoryAllocationError(
        IsWarning = true, Reason = "A getter for a float should not allocate.")]
    public static bool TryGetFloorHeight(ref float floorHeight)
    {
        if (instance == null)
        {
            return false;
        }

        return instance.headsetProvider.TryGetFloorHeight(ref floorHeight);
    }

    /// <summary>
    /// Populates position and rotation with the last recenter transform, if one is available.
    /// </summary>
    /// <remarks>This may be unavailable if the underlying GVR API call fails.</remarks>
    /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
    /// <param name="position">
    /// If this call returns `true`, this value is set to the retrieved position.
    /// </param>
    /// <param name="rotation">
    /// If this call returns `true`, this value is set to the retrieved rotation.
    /// </param>
    public static bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation)
    {
        if (instance == null)
        {
            return false;
        }

        return instance.headsetProvider.TryGetRecenterTransform(ref position, ref rotation);
    }

    /// <summary>Populates `safetyType` with the safety region type, if one is available.</summary>
    /// <remarks>
    /// Populates `safetyType` with the available safety region feature on the currently-running
    /// device.  This may be unavailable if the underlying GVR API call fails.
    /// </remarks>
    /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
    /// <param name="safetyType">
    /// If this call returns `true`, this value is set to the retrieved `safetyType`.
    /// </param>
    public static bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType)
    {
        if (instance == null)
        {
            return false;
        }

        return instance.headsetProvider.TryGetSafetyRegionType(ref safetyType);
    }

    /// <summary>
    /// Populates `innerRadius` with the safety cylinder inner radius, if one is available.
    /// </summary>
    /// <remarks>
    /// This is the radius at which safety management (e.g. safety fog) may cease taking effect.
    /// <para>
    /// If the safety region is of type `GvrSafetyRegionType.Cylinder`, populates `innerRadius` with
    /// the inner radius size of the safety cylinder in meters.  Before using, confirm that the
    /// safety region type is `GvrSafetyRegionType.Cylinder`.  This may be unavailable if the
    /// underlying GVR API call fails.
    /// </para></remarks>
    /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
    /// <param name="innerRadius">
    /// If this call returns `true`, this value is set to the retrieved `innerRadius`.
    /// </param>
    public static bool TryGetSafetyCylinderInnerRadius(ref float innerRadius)
    {
        if (instance == null)
        {
            return false;
        }

        return instance.headsetProvider.TryGetSafetyCylinderInnerRadius(ref innerRadius);
    }

    /// <summary>
    /// Populates `outerRadius` with the safety cylinder outer radius, if one is available.
    /// </summary>
    /// <remarks>
    /// If the safety region is of type `GvrSafetyRegionType.Cylinder`, populates `outerRadius` with
    /// the outer radius size of the safety cylinder in meters.  Before using, confirm that the
    /// safety region type is `GvrSafetyRegionType.Cylinder`.  This may be unavailable if the
    /// underlying GVR API call fails.
    /// <para>
    /// This is the radius at which safety management (e.g. safety fog) may start to take effect.
    /// </para></remarks>
    /// <returns>Returns `true` if value retrieval was successful, `false` otherwise.</returns>
    /// <param name="outerRadius">
    /// If this call returns `true`, this value is set to the retrieved `outerRadius`.
    /// </param>
    public static bool TryGetSafetyCylinderOuterRadius(ref float outerRadius)
    {
        if (instance == null)
        {
            return false;
        }

        return instance.headsetProvider.TryGetSafetyCylinderOuterRadius(ref outerRadius);
    }

#endregion  // GVR_HEADSET_PROPERTIES
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GvrHeadset instance was found in your scene. "
            + "Ensure that there is only one GvrHeadset.");
            this.enabled = false;
            return;
        }

        instance = this;
        if (headsetProvider == null)
        {
            headsetProvider = HeadsetProviderFactory.CreateProvider();
        }
    }

    private void OnEnable()
    {
        if (!SupportsPositionalTracking)
        {
            return;
        }

        headsetUpdate = EndOfFrame();
        StartCoroutine(headsetUpdate);
    }

    private void OnDisable()
    {
        if (!SupportsPositionalTracking)
        {
            return;
        }

        if (headsetUpdate != null)
        {
            StopCoroutine(headsetUpdate);
        }
    }

    private void OnDestroy()
    {
        if (!SupportsPositionalTracking)
        {
            return;
        }

        instance = null;
    }

    private void UpdateStandalone()
    {
        // Events are stored in a queue, so poll until we get Invalid.
        headsetProvider.PollEventState(ref headsetState);
        while (headsetState.eventType != GvrEventType.Invalid)
        {
            switch (headsetState.eventType)
            {
                case GvrEventType.Recenter:
                    if (recenterDelegate != null)
                    {
                        recenterDelegate(headsetState.recenterEventType,
                            (GvrRecenterFlags)headsetState.recenterEventFlags,
                            headsetState.recenteredPosition,
                            headsetState.recenteredRotation);
                    }

                    break;
                case GvrEventType.SafetyRegionEnter:
                    if (safetyRegionDelegate != null)
                    {
                        safetyRegionDelegate(true);
                    }

                    break;
                case GvrEventType.SafetyRegionExit:
                    if (safetyRegionDelegate != null)
                    {
                        safetyRegionDelegate(false);
                    }

                    break;
                case GvrEventType.Invalid:
                    throw new InvalidEnumArgumentException(
                        "Invalid headset event: " + headsetState.eventType);
                default:  // Fallthrough, should never get here.
                    break;
            }

            headsetProvider.PollEventState(ref headsetState);
        }
    }

    private IEnumerator EndOfFrame()
    {
        while (true)
        {
            // This must be done at the end of the frame to ensure that all GameObjects had a chance
            // to read transient state (e.g. events, etc) for the current frame before it gets
            // reset.
            yield return waitForEndOfFrame;
            UpdateStandalone();
        }
    }
}

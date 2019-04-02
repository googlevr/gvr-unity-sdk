//-----------------------------------------------------------------------
// <copyright file="GvrPointerInputModule.cs" company="Google Inc.">
// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the MIT License, you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//     http://www.opensource.org/licenses/mit-license.php
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>This script provides an implemention of Unity's `BaseInputModule` class.</summary>
/// <remarks><para>
/// Exists so that Canvas-based (`uGUI`) UI elements and 3D scene objects can be interacted with in
/// a Gvr Application.
/// </para><para>
/// This script is intended for use with either a 3D Pointer with the Daydream Controller
/// (Recommended for Daydream), or a Gaze-based-Pointer (Recommended for Cardboard).
/// </para><para>
/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
/// other modules, such as `TouchInputModule` and `StandaloneInputModule`, in order
/// for the Pointer to take priority in the event system.
/// </para><para>
/// If you are using a **Canvas**, set the `Render Mode` to **World Space**, and add the
/// `GvrPointerGraphicRaycaster` script to the object.
/// </para><para>
/// If you'd like pointers to work with 3D scene objects, add a `GvrPointerPhysicsRaycaster` to the
/// main camera, and add a component that implements one of the `Event` interfaces (`EventTrigger`
/// will work nicely) to an object with a collider.
/// </para><para>
/// `GvrPointerInputModule` emits the following events: `Enter`, `Exit`, `Down`, `Up`, `Click`,
/// `Select`, `Deselect`, `UpdateSelected`, and `GvrPointerHover`.  Scroll, move, and submit/cancel
/// events are not emitted.
/// </para><para>
/// To use a 3D Pointer with the Daydream Controller:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrControllerPointer to your scene.
///   - Set the parent of `GvrControllerPointer` to the same parent as the main camera
///     (With a local position of 0,0,0).
/// </para><para>
/// To use a Gaze-based-pointer:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrReticlePointer to your scene.
///   - Set the parent of `GvrReticlePointer` to the main camera.
/// </para></remarks>
[AddComponentMenu("GoogleVR/GvrPointerInputModule")]
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrPointerInputModule")]
public class GvrPointerInputModule : BaseInputModule, IGvrInputModuleController
{
    /// <summary>
    /// If `true`, pointer input is active in VR Mode only.
    /// If `false`, pointer input is active all of the time.
    /// </summary>
    /// <remarks>
    /// Set to false if you plan to use direct screen taps or other input when not in VR Mode.
    /// </remarks>
    [Tooltip("Whether Pointer input is active in VR Mode only (true), or all the time (false).")]
    public bool vrModeOnly = false;

    /// <summary>Manages scroll events for the input module.</summary>
    [Tooltip("Manages scroll events for the input module.")]
    public GvrPointerScrollInput scrollInput = new GvrPointerScrollInput();

    /// <summary>Gets or sets the static reference to the `GvrBasePointer`.</summary>
    /// <value>The static reference to the `GvrBasePointer`.</value>
    public static GvrBasePointer Pointer
    {
        get
        {
            GvrPointerInputModule module = FindInputModule();
            if (module == null || module.Impl == null)
            {
                return null;
            }

            return module.Impl.Pointer;
        }

        set
        {
            GvrPointerInputModule module = FindInputModule();
            if (module == null || module.Impl == null)
            {
                return;
            }

            module.Impl.Pointer = value;
        }
    }

    /// <summary>Gets the current `RaycastResult`.</summary>
    /// <value>The current `RaycastResult`.</value>
    public static RaycastResult CurrentRaycastResult
    {
        get
        {
            GvrPointerInputModule inputModule = GvrPointerInputModule.FindInputModule();
            if (inputModule == null)
            {
                return new RaycastResult();
            }

            if (inputModule.Impl == null)
            {
                return new RaycastResult();
            }

            if (inputModule.Impl.CurrentEventData == null)
            {
                return new RaycastResult();
            }

            return inputModule.Impl.CurrentEventData.pointerCurrentRaycast;
        }
    }

    /// <summary>Gets the implementation object of this module.</summary>
    /// <value>The implementation object of this module.</value>
    public GvrPointerInputModuleImpl Impl { get; private set; }

    /// <summary>Gets the executor this module uses to process events.</summary>
    /// <value>The executor this module uses to process events.</value>
    public GvrEventExecutor EventExecutor { get; private set; }

    /// <summary>Gets the event system reference.</summary>
    /// <value>The event system reference.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.LegacyGvrStyleRules",
        "VR1001:AccessibleNonConstantPropertiesMustBeUpperCamelCase",
        Justification = "Legacy Public API.")]
    public new EventSystem eventSystem
    {
        get
        {
            return base.eventSystem;
        }
    }

    /// <summary>Gets the list of raycast results used as a cache.</summary>
    /// <value>The list of raycast results used as a cache.</value>
    public List<RaycastResult> RaycastResultCache
    {
        get
        {
            return m_RaycastResultCache;
        }
    }

    /// <summary>The `GvrBasePointer` calls this when it is created.</summary>
    /// <remarks>
    /// If a pointer hasn't already been assigned, it will assign the newly created one by default.
    /// This simplifies the common case of having only one `GvrBasePointer` so it can be
    /// automatically hooked up to the manager.  If multiple `GvrBasePointers` are in the scene,
    /// the app has to take responsibility for setting which one is active.
    /// </remarks>
    /// <param name="createdPointer">The pointer whose creation triggered this call.</param>
    public static void OnPointerCreated(GvrBasePointer createdPointer)
    {
        GvrPointerInputModule module = FindInputModule();
        if (module == null || module.Impl == null)
        {
            return;
        }

        if (module.Impl.Pointer == null)
        {
            module.Impl.Pointer = createdPointer;
        }
    }

    /// <summary>
    /// Helper function to find the Event executor that is part of the input module if one exists
    /// in the scene.
    /// </summary>
    /// <returns>A found GvrEventExecutor or null.</returns>
    public static GvrEventExecutor FindEventExecutor()
    {
        GvrPointerInputModule gvrInputModule = FindInputModule();
        if (gvrInputModule == null)
        {
            return null;
        }

        return gvrInputModule.EventExecutor;
    }

    /// <summary>
    /// Helper function to find the input module if one exists in the scene and it is the active
    /// module.
    /// </summary>
    /// <returns>A found `GvrPointerInputModule` or null.</returns>
    public static GvrPointerInputModule FindInputModule()
    {
        if (EventSystem.current == null)
        {
            return null;
        }

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return null;
        }

        GvrPointerInputModule gvrInputModule =
            eventSystem.GetComponent<GvrPointerInputModule>();

        return gvrInputModule;
    }

    /// <inheritdoc/>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public override bool ShouldActivateModule()
    {
        return Impl.ShouldActivateModule();
    }

    /// <inheritdoc/>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public override void DeactivateModule()
    {
        Impl.DeactivateModule();
    }

    /// <inheritdoc/>
    public override bool IsPointerOverGameObject(int pointerId)
    {
        return Impl.IsPointerOverGameObject(pointerId);
    }

    /// <inheritdoc/>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public override void Process()
    {
        UpdateImplProperties();
        Impl.Process();
    }

    /// <summary>Whether the module should be activated.</summary>
    /// <returns>Returns `true` if this module should be activated, `false` otherwise.</returns>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public bool ShouldActivate()
    {
        return base.ShouldActivateModule();
    }

    /// <summary>Deactivate this instance.</summary>
    public void Deactivate()
    {
        base.DeactivateModule();
    }

    /// <summary>Finds the common root between two `GameObject`s.</summary>
    /// <returns>The common root.</returns>
    /// <param name="g1">The first `GameObject`.</param>
    /// <param name="g2">The second `GameObject`.</param>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public new GameObject FindCommonRoot(GameObject g1, GameObject g2)
    {
        return BaseInputModule.FindCommonRoot(g1, g2);
    }

    /// <summary>Gets the base event data.</summary>
    /// <returns>The base event data.</returns>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public new BaseEventData GetBaseEventData()
    {
        return base.GetBaseEventData();
    }

    /// <summary>Finds the first raycast.</summary>
    /// <returns>The first raycast.</returns>
    /// <param name="candidates">
    /// The list of `RaycastResult`s to search for the first Raycast.
    /// </param>
    public new RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
    {
        return BaseInputModule.FindFirstRaycast(candidates);
    }

    /// @cond
    /// <inheritdoc/>
    protected override void Awake()
    {
        base.Awake();
        Impl = new GvrPointerInputModuleImpl();
        EventExecutor = new GvrEventExecutor();
        UpdateImplProperties();
    }

    /// @endcond
    /// <summary>Update implementation properties.</summary>
    private void UpdateImplProperties()
    {
        if (Impl == null)
        {
            return;
        }

        Impl.ScrollInput = scrollInput;
        Impl.VrModeOnly = vrModeOnly;
        Impl.ModuleController = this;
        Impl.EventExecutor = EventExecutor;
    }
}

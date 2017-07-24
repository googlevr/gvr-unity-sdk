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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// This script provides an implemention of Unity's `BaseInputModule` class, so
/// that Canvas-based (_uGUI_) UI elements and 3D scene objects can be
/// interacted with in a Gvr Application.
///
/// This script is intended for use with either a
/// 3D Pointer with the Daydream Controller (Recommended for Daydream),
/// or a Gaze-based-Pointer (Recommended for Cardboard).
///
/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
/// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
/// for the Pointer to take priority in the event system.
///
/// If you are using a **Canvas**, set the _Render Mode_ to **World Space**,
/// and add the _GvrPointerGraphicRaycaster_ script to the object.
///
/// If you'd like pointers to work with 3D scene objects, add a _GvrPointerPhysicsRaycaster_ to the main camera,
/// and add a component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely) to
/// an object with a collider.
///
/// GvrPointerInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
/// _Deselect_, _UpdateSelected_, and _GvrPointerHover_.  Scroll, move, and submit/cancel events are not emitted.
///
/// To use a 3D Pointer with the Daydream Controller:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrControllerPointer to your scene.
///   - Set the parent of GvrControllerPointer to the same parent as the main camera
///     (With a local position of 0,0,0).
///
/// To use a Gaze-based-pointer:
///   - Add the prefab GoogleVR/Prefabs/UI/GvrReticlePointer to your scene.
///   - Set the parent of GvrReticlePointer to the main camera.
///
[AddComponentMenu("GoogleVR/GvrPointerInputModule")]
public class GvrPointerInputModule : BaseInputModule, IGvrInputModuleController {
  /// Determines whether Pointer input is active in VR Mode only (`true`), or all of the
  /// time (`false`).  Set to false if you plan to use direct screen taps or other
  /// input when not in VR Mode.
  [Tooltip("Whether Pointer input is active in VR Mode only (true), or all the time (false).")]
  public bool vrModeOnly = false;

  [Tooltip("Manages scroll events for the input module.")]
  public GvrPointerScrollInput scrollInput = new GvrPointerScrollInput();

  public GvrPointerInputModuleImpl Impl { get; private set; }

  public GvrEventExecutor EventExecutor { get; private set; }

  public new EventSystem eventSystem {
    get {
      return base.eventSystem;
    }
  }

  public List<RaycastResult> RaycastResultCache {
    get {
      return m_RaycastResultCache;
    }
  }

  public static GvrBasePointer Pointer {
    get {
      GvrPointerInputModule module = FindInputModule();
      if (module == null || module.Impl == null) {
        return null;
      }

      return module.Impl.Pointer;
    }
    set {
      GvrPointerInputModule module = FindInputModule();
      if (module == null || module.Impl == null) {
        return;
      }

      module.Impl.Pointer = value;
    }
  }

  /// GvrBasePointer calls this when it is created.
  /// If a pointer hasn't already been assigned, it
  /// will assign the newly created one by default.
  ///
  /// This simplifies the common case of having only one
  /// GvrBasePointer so is can be automatically hooked up
  /// to the manager.  If multiple GvrBasePointers are in
  /// the scene, the app has to take responsibility for
  /// setting which one is active.
  public static void OnPointerCreated(GvrBasePointer createdPointer) {
    GvrPointerInputModule module = FindInputModule();
    if (module == null || module.Impl == null) {
      return;
    }

    if (module.Impl.Pointer == null) {
      module.Impl.Pointer = createdPointer;
    }
  }

  /// Helper function to find the Event Executor that is part of
  /// the input module if one exists in the scene.
  public static GvrEventExecutor FindEventExecutor() {
    GvrPointerInputModule gvrInputModule = FindInputModule();
    if (gvrInputModule == null) {
      return null;
    }

    return gvrInputModule.EventExecutor;
  }

  /// Helper function to find the input module if one exists in the
  /// scene and it is the active module.
  public static GvrPointerInputModule FindInputModule() {
    if (EventSystem.current == null) {
      return null;
    }

    EventSystem eventSystem = EventSystem.current;
    if (eventSystem == null) {
      return null;
    }

    GvrPointerInputModule gvrInputModule =
      eventSystem.GetComponent<GvrPointerInputModule>();

    return gvrInputModule;
  }

  /// Convenience function to access what the current RaycastResult.
  public static RaycastResult CurrentRaycastResult {
    get {
      GvrPointerInputModule inputModule = GvrPointerInputModule.FindInputModule();
      if (inputModule == null) {
        return new RaycastResult();
      }

      if (inputModule.Impl == null) {
        return new RaycastResult();
      }

      if (inputModule.Impl.CurrentEventData == null) {
        return new RaycastResult();
      }

      return inputModule.Impl.CurrentEventData.pointerCurrentRaycast;
    }
  }

  public override bool ShouldActivateModule() {
    return Impl.ShouldActivateModule();
  }

  public override void DeactivateModule() {
    Impl.DeactivateModule();
  }

  public override bool IsPointerOverGameObject(int pointerId) {
    return Impl.IsPointerOverGameObject(pointerId);
  }

  public override void Process() {
    UpdateImplProperties();
    Impl.Process();
  }

  protected override void Awake() {
    base.Awake();
    Impl = new GvrPointerInputModuleImpl();
    EventExecutor = new GvrEventExecutor();
    UpdateImplProperties();
  }

  public bool ShouldActivate() {
    return base.ShouldActivateModule();
  }

  public void Deactivate() {
    base.DeactivateModule();
  }

  public new GameObject FindCommonRoot(GameObject g1, GameObject g2) {
    return BaseInputModule.FindCommonRoot(g1, g2);
  }

  public new BaseEventData GetBaseEventData() {
    return base.GetBaseEventData();
  }

  public new RaycastResult FindFirstRaycast(List<RaycastResult> candidates) {
    return BaseInputModule.FindFirstRaycast(candidates);
  }

  private void UpdateImplProperties() {
    if (Impl == null) {
      return;
    }

    Impl.ScrollInput = scrollInput;
    Impl.VrModeOnly = vrModeOnly;
    Impl.ModuleController = this;
    Impl.EventExecutor = EventExecutor;
  }
}

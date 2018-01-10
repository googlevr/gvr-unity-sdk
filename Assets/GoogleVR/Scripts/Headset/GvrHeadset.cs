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

using UnityEngine;
using System;
using System.Collections;
using System.ComponentModel;

using Gvr.Internal;

/// Main entry point for Standalone headset APIs.
///
/// To use this API, use the GvrHeadset prefab. There can be only one
/// such prefab in a scene.
///
/// This is a singleton object.
public class GvrHeadset : MonoBehaviour {
  private static GvrHeadset instance;

  private IHeadsetProvider headsetProvider;
  private HeadsetState headsetState;
  private IEnumerator standaloneUpdate;
  private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

  // Delegates for GVR events.
  private OnSafetyRegionEvent safetyRegionDelegate;
  private OnRecenterEvent recenterDelegate;

  // Delegate definitions.
  /// This delegate is called when the headset crosses the safety region boundary.
  public delegate void OnSafetyRegionEvent(bool enter);
  /// This delegate is called after the headset is recentered.
  /// |recenterType| indicates the reason recentering occurred.
  /// |recenterFlags| are flags related to recentering.  See |GvrRecenterFlags|.
  /// |recenteredPosition| is the positional offset from the session start pose.
  /// |recenteredOrientation| is the rotational offset from the session start pose.
  public delegate void OnRecenterEvent(GvrRecenterEventType recenterType,
                                       GvrRecenterFlags recenterFlags,
                                       Vector3 recenteredPosition,
                                       Quaternion recenteredOrientation);

#region DELEGATE_HANDLERS
  public static event OnSafetyRegionEvent OnSafetyRegionChange {
    add {
      if (instance != null) {
        instance.safetyRegionDelegate += value;
      }
    }
    remove {
      if (instance != null) {
        instance.safetyRegionDelegate -= value;
      }
    }
  }

  public static event OnRecenterEvent OnRecenter {
    add {
      if (instance != null) {
        instance.recenterDelegate += value;
      }
    }
    remove {
      if (instance != null) {
        instance.recenterDelegate -= value;
      }
    }
  }
#endregion  // DELEGATE_HANDLERS

#region GVR_STANDALONE_PROPERTIES
  /// Returns |true| if the current headset supports positionally tracked, 6DOF head poses.
  /// Returns |false| if only rotation-based head poses are supported.
  public static bool SupportsPositionalTracking {
    get {
      if (instance == null) {
        return false;
      }
      try {
        return instance.headsetProvider.SupportsPositionalTracking;
      }
      catch(Exception e) {
        Debug.LogError("Error reading SupportsPositionalTracking: " + e.Message);
        return false;
      }
    }
  }

  /// If a floor is found, populates floorHeight with the detected height.
  /// Otherwise, leaves the value unchanged.
  /// Returns true if value retrieval was successful, false otherwise (depends on tracking state).
  public static bool TryGetFloorHeight(ref float floorHeight) {
    if (instance == null) {
      return false;
    }
    return instance.headsetProvider.TryGetFloorHeight(ref floorHeight);
  }

  /// If the last recentering transform is available, populates position and rotation with that
  /// transform.
  /// Returns true if value retrieval was successful, false otherwise (unlikely).
  public static bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation) {
    if (instance == null) {
      return false;
    }
    return instance.headsetProvider.TryGetRecenterTransform(ref position, ref rotation);
  }

  /// Populates safetyType with the available safety region feature on the
  /// currently-running device.
  /// Returns true if value retrieval was successful, false otherwise (unlikely).
  public static bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType) {
    if (instance == null) {
      return false;
    }
    return instance.headsetProvider.TryGetSafetyRegionType(ref safetyType);
  }

  /// If the safety region is of type GvrSafetyRegionType.Cylinder, populates innerRadius with the
  /// inner radius size (where fog starts appearing) of the safety cylinder in meters.
  /// Assumes the safety region type has been previously checked by the caller.
  /// Returns true if value retrieval was successful, false otherwise (if region type is
  /// GvrSafetyRegionType.Invalid).
  public static bool TryGetSafetyCylinderInnerRadius(ref float innerRadius) {
    if (instance == null) {
      return false;
    }
    return instance.headsetProvider.TryGetSafetyCylinderInnerRadius(ref innerRadius);
  }

  /// If the safety region is of type GvrSafetyRegionType.Cylinder, populates outerRadius with the
  /// outer radius size (where fog is 100% opaque) of the safety cylinder in meters.
  /// Assumes the safety region type has been previously checked by the caller.
  /// Returns true if value retrieval was successful, false otherwise (if region type is
  /// GvrSafetyRegionType.Invalid).
  public static bool TryGetSafetyCylinderOuterRadius(ref float outerRadius) {
    if (instance == null) {
      return false;
    }
    return instance.headsetProvider.TryGetSafetyCylinderOuterRadius(ref outerRadius);
  }
#endregion  // GVR_STANDALONE_PROPERTIES

  private GvrHeadset() {
    headsetState.Initialize();
  }

  void Awake() {
    if (instance != null) {
      Debug.LogError("More than one GvrHeadset instance was found in your scene. "
        + "Ensure that there is only one GvrHeadset.");
      this.enabled = false;
      return;
    }
    instance = this;
    if (headsetProvider == null) {
      headsetProvider = HeadsetProviderFactory.CreateProvider();
    }
  }

  void OnEnable() {
    if (!SupportsPositionalTracking) {
      return;
    }
    standaloneUpdate = EndOfFrame();
    StartCoroutine(standaloneUpdate);
  }

  void OnDisable() {
    if (!SupportsPositionalTracking) {
      return;
    }
    StopCoroutine(standaloneUpdate);
  }

  void OnDestroy() {
    if (!SupportsPositionalTracking) {
      return;
    }
    instance = null;
  }

  private void UpdateStandalone() {
    // Events are stored in a queue, so poll until we get Invalid.
    headsetProvider.PollEventState(ref headsetState);
    while (headsetState.eventType != GvrEventType.Invalid) {
      switch (headsetState.eventType) {
        case GvrEventType.Recenter:
          if (recenterDelegate != null) {
            recenterDelegate(headsetState.recenterEventType,
                             (GvrRecenterFlags) headsetState.recenterEventFlags,
                             headsetState.recenteredPosition,
                             headsetState.recenteredRotation);
          }
          break;
        case GvrEventType.SafetyRegionEnter:
          if (safetyRegionDelegate != null) {
            safetyRegionDelegate(true);
          }
          break;
        case GvrEventType.SafetyRegionExit:
          if (safetyRegionDelegate != null) {
            safetyRegionDelegate(false);
          }
          break;
        case GvrEventType.Invalid:
          throw new InvalidEnumArgumentException("Invalid headset event: " + headsetState.eventType);
        default:  // Fallthrough, should never get here.
          break;
      }
      headsetProvider.PollEventState(ref headsetState);
    }
  }

  IEnumerator EndOfFrame() {
    while (true) {
      // This must be done at the end of the frame to ensure that all GameObjects had a chance
      // to read transient state (e.g. events, etc) for the current frame before it gets reset.
      yield return waitForEndOfFrame;
      UpdateStandalone();
    }
  }
}

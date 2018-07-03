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

namespace GoogleVR.HelloVR {
  using UnityEngine;
  using System.Collections;

  /// Demonstrates the use of GvrHeadset events and APIs.
  public class HeadsetDemoManager : MonoBehaviour {
    public GameObject safetyRing;
    public bool enableDebugLog = false;
    private WaitForSeconds waitFourSeconds = new WaitForSeconds(4);

#region STANDALONE_DELEGATES
    public void OnSafetyRegionEvent(bool enter) {
      Debug.Log("SafetyRegionEvent: " + (enter ? "enter" : "exit"));
    }

    public void OnRecenterEvent(GvrRecenterEventType recenterType,
                                GvrRecenterFlags recenterFlags,
                                Vector3 recenteredPosition,
                                Quaternion recenteredOrientation) {
      Debug.Log(string.Format("RecenterEvent: Type {0}, flags {1}\nPosition: {2}, " +
            "Rotation: {3}", recenterType, recenterFlags, recenteredPosition, recenteredOrientation));
    }
#endregion  // STANDALONE_DELEGATES

    public void FindFloorHeight() {
      float floorHeight = 0.0f;
      bool success = GvrHeadset.TryGetFloorHeight(ref floorHeight);
      Debug.Log("Floor height success " + success + "; value " + floorHeight);
    }

    public void FindRecenterTransform() {
      Vector3 position = Vector3.zero;
      Quaternion rotation = Quaternion.identity;
      bool success = GvrHeadset.TryGetRecenterTransform(ref position, ref rotation);
      Debug.Log("Recenter transform success " + success + "; value " + position + "; " + rotation);
    }

    public void FindSafetyRegionType() {
      GvrSafetyRegionType safetyType = GvrSafetyRegionType.None;
      bool success = GvrHeadset.TryGetSafetyRegionType(ref safetyType);
      Debug.Log("Safety region type success " + success + "; value " + safetyType);
    }

    public void FindSafetyInnerRadius() {
      float innerRadius = -1.0f;
      bool success = GvrHeadset.TryGetSafetyCylinderInnerRadius(ref innerRadius);
      Debug.Log("Safety region inner radius success " + success + "; value " + innerRadius);
      // Don't activate the safety cylinder visual until the radius is a reasonable value.
      if (innerRadius > 0.1f && safetyRing != null) {
        safetyRing.SetActive(true);
        safetyRing.transform.localScale = new Vector3(innerRadius, 1, innerRadius);
      }
    }

    public void FindSafetyOuterRadius() {
      float outerRadius = -1.0f;
      bool success = GvrHeadset.TryGetSafetyCylinderOuterRadius(ref outerRadius);
      Debug.Log("Safety region outer radius success " + success + "; value " + outerRadius);
    }

    void OnEnable() {
      if (safetyRing != null) {
        safetyRing.SetActive(false);
      }
      if (!GvrHeadset.SupportsPositionalTracking) {
        return;
      }
      GvrHeadset.OnSafetyRegionChange += OnSafetyRegionEvent;
      GvrHeadset.OnRecenter += OnRecenterEvent;
      if (enableDebugLog) {
        StartCoroutine(StatusUpdateLoop());
      }
    }

    void OnDisable() {
      if (!GvrHeadset.SupportsPositionalTracking) {
        return;
      }
      GvrHeadset.OnSafetyRegionChange -= OnSafetyRegionEvent;
      GvrHeadset.OnRecenter -= OnRecenterEvent;
    }

    void Start() {
      if (GvrHeadset.SupportsPositionalTracking) {
        Debug.Log("Device supports positional tracking!");
      }
    }

    private IEnumerator StatusUpdateLoop() {
      while(true) {
        yield return waitFourSeconds;
        FindFloorHeight();
        FindRecenterTransform();
        FindSafetyOuterRadius();
        FindSafetyInnerRadius();
        FindSafetyRegionType();
      }
    }
  }
}

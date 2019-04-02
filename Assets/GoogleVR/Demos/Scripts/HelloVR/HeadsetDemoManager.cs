//-----------------------------------------------------------------------
// <copyright file="HeadsetDemoManager.cs" company="Google Inc.">
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

namespace GoogleVR.HelloVR
{
    using System.Collections;
    using UnityEngine;

    /// <summary>Demonstrates the use of GvrHeadset events and APIs.</summary>
    public class HeadsetDemoManager : MonoBehaviour
    {
        /// <summary>A visual representation of the Gvr Api's Safety Cylinder.</summary>
        public GameObject safetyRing;

        /// <summary>If `true`, this class logs status messages.  If `false`, it does not.</summary>
        public bool enableDebugLog = false;
        private WaitForSeconds waitFourSeconds = new WaitForSeconds(4);

#region STANDALONE_DELEGATES
        /// <summary>An event which triggers when the safety region is entered or exited.</summary>
        /// <param name="enter">
        /// Value `true` if the safety region is being entered, or `false` if the safety region is
        /// being exited.
        /// </param>
        public void OnSafetyRegionEvent(bool enter)
        {
            if (enableDebugLog)
            {
                Debug.Log("SafetyRegionEvent: " + (enter ? "enter" : "exit"));
            }
        }

        /// <summary>An event which triggers when a Recenter occurs.</summary>
        /// <param name="recenterType">The type of the last recenter.</param>
        /// <param name="recenterFlags">The flag context of the last recenter.</param>
        /// <param name="recenteredPosition">The positional reference of the last recenter.</param>
        /// <param name="recenteredOrientation">The rotation reference of the last recenter.</param>
        public void OnRecenterEvent(GvrRecenterEventType recenterType,
                                    GvrRecenterFlags recenterFlags,
                                    Vector3 recenteredPosition,
                                    Quaternion recenteredOrientation)
        {
            if (enableDebugLog)
            {
                Debug.Log(string.Format(
                    "RecenterEvent: Type {0}, flags {1}\nPosition: {2}, Rotation: {3}",
                    recenterType, recenterFlags, recenteredPosition, recenteredOrientation));
            }
        }
#endregion  // STANDALONE_DELEGATES
        /// <summary>Prints the floor height to console.</summary>
        public void FindFloorHeight()
        {
            float floorHeight = 0.0f;
            bool success = GvrHeadset.TryGetFloorHeight(ref floorHeight);
            if (enableDebugLog)
            {
                Debug.Log("Floor height success " + success + "; value " + floorHeight);
            }
        }

        /// <summary>
        /// Prints the reference transformation as of the last recenter to console.
        /// </summary>
        public void FindRecenterTransform()
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            bool success = GvrHeadset.TryGetRecenterTransform(ref position, ref rotation);
            if (enableDebugLog)
            {
                Debug.Log("Recenter transform success " + success + "; value " + position + "; "
                          + rotation);
            }
        }

        /// <summary>Prints the safety region's type to console.</summary>
        public void FindSafetyRegionType()
        {
            GvrSafetyRegionType safetyType = GvrSafetyRegionType.None;
            bool success = GvrHeadset.TryGetSafetyRegionType(ref safetyType);
            if (enableDebugLog)
            {
                Debug.Log("Safety region type success " + success + "; value " + safetyType);
            }
        }

        /// <summary>Prints the safety region's inner radius to console.</summary>
        public void FindSafetyInnerRadius()
        {
            float innerRadius = -1.0f;
            bool success = GvrHeadset.TryGetSafetyCylinderInnerRadius(ref innerRadius);
            if (enableDebugLog)
            {
                Debug.Log("Safety region inner radius success " + success + "; value "
                          + innerRadius);
            }

            // Don't activate the safety cylinder visual until the radius is a reasonable value.
            if (innerRadius > 0.1f && safetyRing != null)
            {
                safetyRing.SetActive(true);
                safetyRing.transform.localScale = new Vector3(innerRadius, 1, innerRadius);
            }
        }

        /// <summary>Prints the safety region's outer radius to console.</summary>
        public void FindSafetyOuterRadius()
        {
            float outerRadius = -1.0f;
            bool success = GvrHeadset.TryGetSafetyCylinderOuterRadius(ref outerRadius);
            if (enableDebugLog)
            {
                Debug.Log("Safety region outer radius success " + success + "; value " +
                          outerRadius);
            }
        }

        private void OnEnable()
        {
            if (safetyRing != null)
            {
                safetyRing.SetActive(false);
            }

            if (!GvrHeadset.SupportsPositionalTracking)
            {
                return;
            }

            GvrHeadset.OnSafetyRegionChange += OnSafetyRegionEvent;
            GvrHeadset.OnRecenter += OnRecenterEvent;
            if (enableDebugLog)
            {
                StartCoroutine(StatusUpdateLoop());
            }
        }

        private void OnDisable()
        {
            if (!GvrHeadset.SupportsPositionalTracking)
            {
                return;
            }

            GvrHeadset.OnSafetyRegionChange -= OnSafetyRegionEvent;
            GvrHeadset.OnRecenter -= OnRecenterEvent;
        }

        private void Start()
        {
            if (enableDebugLog)
            {
                Debug.Log("Device supports positional tracking: "
                          + GvrHeadset.SupportsPositionalTracking);
            }
        }

        private IEnumerator StatusUpdateLoop()
        {
            while (true)
            {
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

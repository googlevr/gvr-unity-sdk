//-----------------------------------------------------------------------
// <copyright file="GvrRecenterOnlyController.cs" company="Google Inc.">
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

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// Used to recenter only the controllers, required for scenes that have no clear forward direction.
/// Details: https://developers.google.com/vr/distribute/daydream/design-requirements#UX-D6
///
/// Works by offsetting the orientation of the transform when a recenter occurs to correct for the
/// orientation change caused by the recenter event.
///
/// Usage: Place on the parent of the camera that should have it's orientation corrected.
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrRecenterOnlyController")]
public class GvrRecenterOnlyController : MonoBehaviour
{
    private Quaternion lastAppliedYawCorrection = Quaternion.identity;
    private Quaternion yawCorrection = Quaternion.identity;

    void Update()
    {
        bool connected = false;
        foreach (var hand in Gvr.Internal.ControllerUtils.AllHands)
        {
            GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
            if (device.State == GvrConnectionState.Connected)
            {
                connected = true;
                break;
            }
        }

        if (!connected)
        {
            return;
        }

// Daydream is loaded only on deivce, not in editor.
#if UNITY_ANDROID && !UNITY_EDITOR
        if (XRSettings.loadedDeviceName != GvrSettings.VR_SDK_DAYDREAM)
        {
          return;
        }
#endif

        if (GvrControllerInput.Recentered)
        {
            ApplyYawCorrection();
            return;
        }

#if UNITY_EDITOR
        // Compatibility for Instant Preview.
        if (Gvr.Internal.InstantPreview.Instance != null &&
          Gvr.Internal.InstantPreview.Instance.enabled &&
          Gvr.Internal.ControllerUtils.AnyButton(GvrControllerButton.System))
          {
            return;
        }
#else  // !UNITY_EDITOR
        if (Gvr.Internal.ControllerUtils.AnyButton(GvrControllerButton.System))
        {
            return;
        }
#endif  // UNITY_EDITOR

        yawCorrection = GetYawCorrection();
    }

    void OnDisable()
    {
        yawCorrection = Quaternion.identity;
        RemoveLastYawCorrection();
    }

    private void ApplyYawCorrection()
    {
        RemoveLastYawCorrection();
        transform.localRotation = transform.localRotation * yawCorrection;
        lastAppliedYawCorrection = yawCorrection;
    }

    private void RemoveLastYawCorrection()
    {
        transform.localRotation =
      transform.localRotation * Quaternion.Inverse(lastAppliedYawCorrection);
        lastAppliedYawCorrection = Quaternion.identity;
    }

    private Quaternion GetYawCorrection()
    {
        Quaternion headRotation = GvrVRHelpers.GetHeadRotation();
        Vector3 euler = headRotation.eulerAngles;
        return lastAppliedYawCorrection * Quaternion.Euler(0.0f, euler.y, 0.0f);
    }
}

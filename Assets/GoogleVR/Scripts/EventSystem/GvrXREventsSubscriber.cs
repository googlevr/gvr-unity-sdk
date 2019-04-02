//-----------------------------------------------------------------------
// <copyright file="GvrXREventsSubscriber.cs" company="Google Inc.">
// Copyright 2018 Google Inc. All rights reserved.
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
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRDevice = UnityEngine.VR.VRDevice;
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// <summary>Handler for subscribing XR Unity actions to GVR Actions.</summary>
public class GvrXREventsSubscriber : MonoBehaviour
{
    private static GvrXREventsSubscriber instance;
    private string instanceLoadedDeviceName;

    /// <summary>Gets the name of the loaded GVR device.</summary>
    /// <remarks><para>
    /// This should be used in place of `XRSettings.loadedDeviceName`, which allocates small
    /// amounts of memory on every call.
    /// </para><para>
    /// When using 2018.3 and above, a cached copy of `XRSettings.loadedDeviceName` which updates
    /// whenever the `OnDeviceLoadAction` event triggers.
    /// </para><para>
    /// On 2018.2 and below, a one-time snapshot of the initial `XRSettings.loadedDeviceName` taken
    /// when this component is instantiated.  If `loadedDeviceName` is expected to change during
    /// runtime in 2018.2 or earlier, use the setter to assign `XRSettings.loadedDeviceName` when
    /// this is expected to happen.
    /// </para></remarks>
    /// <value>The name of the loaded GVR device.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("UnityRules.LegacyGvrStyleRules",
                                                     "VR1001:AccessibleNonConstantPropertiesMustBeUpperCamelCase",
                                                     Justification = "Legacy Public API.")]
    public static string loadedDeviceName
    {
        get { return GetInstance().instanceLoadedDeviceName; }
        private set { GetInstance().instanceLoadedDeviceName = value; }
    }

    private static void OnDeviceLoadAction(string newLoadedDeviceName)
    {
        loadedDeviceName = newLoadedDeviceName;
    }

    private static GvrXREventsSubscriber GetInstance()
    {
        if (instance == null)
        {
            GameObject gvrXREventsSubscriber = new GameObject("GvrXREventsSubscriber");
            gvrXREventsSubscriber.AddComponent<GvrXREventsSubscriber>();
        }

        return instance;
    }

    private void Awake()
    {
        instance = this;
        loadedDeviceName = XRSettings.loadedDeviceName;
#if UNITY_2018_3_OR_NEWER
        XRDevice.deviceLoaded += OnDeviceLoadAction;
#endif // UNITY_2018_3_OR_NEWER
    }
}

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

using UnityEngine;
using System.Collections;
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
    private string _loadedDeviceName;

    /// <summary>The device name loaded from settings.</summary>
    public static string loadedDeviceName
    {
        get { return GetInstance()._loadedDeviceName; }
        set { GetInstance()._loadedDeviceName = value; }
    }

    private static void OnDeviceLoadAction(string newLoadedDeviceName)
    {
        loadedDeviceName = newLoadedDeviceName;
    }

    void Awake()
    {
        instance = this;
        _loadedDeviceName = XRSettings.loadedDeviceName;
#if UNITY_2018_3_OR_NEWER
        XRDevice.deviceLoaded += OnDeviceLoadAction;
#endif // UNITY_2018_3_OR_NEWER
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
}

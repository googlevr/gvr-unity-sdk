//-----------------------------------------------------------------------
// <copyright file="GvrDaydreamApi.cs" company="Google Inc.">
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
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>Main entry point Daydream specific APIs.</summary>
/// <remarks>
/// This class automatically instantiates an instance when this API is used for the first time.
/// For explicit control over when the instance is created and the Java references are setup
/// call the provided `CreateAsync` method, for example when no UI is being displayed to the user.
/// </remarks>
public class GvrDaydreamApi : IDisposable
{
    private const string METHOD_CREATE = "create";
    private const string METHOD_LAUNCH_VR_HOMESCREEN = "launchVrHomescreen";
    private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
    private const string PACKAGE_DAYDREAM_API = "com.google.vr.ndk.base.DaydreamApi";

    private static GvrDaydreamApi instance;

    #if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject daydreamApiObject;
    private AndroidJavaClass daydreamApiClass = new AndroidJavaClass(PACKAGE_DAYDREAM_API);

    /// <summary>Gets an `AndroidJavaObject` associated with the Daydream app.</summary>
    /// <value>An `AndroidJavaObject` associated with the Daydream app.</value>
    public static AndroidJavaObject JavaInstance
    {
        get
        {
            EnsureCreated(null);
            return instance.daydreamApiObject;
        }
    }
    #endif  // UNITY_ANDROID && !UNITY_EDITOR

    /// <summary>Gets a value indicating whether the `GvrDaydreamApi` has been created.</summary>
    /// <value>Value `true` if the GvrDaydreamApi has been created, `false` otherwise.</value>
    public static bool IsCreated
    {
        get
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            return instance != null;
#else
            return instance != null && instance.daydreamApiObject != null;
#endif  // !UNITY_ANDROID || UNITY_EDITOR
        }
    }

    /// @deprecated Create() without arguments is deprecated. Use CreateAsync(callback) instead.
    /// <summary>Creates a generic asynchronous callback.</summary>
    [System.Obsolete(
        "Create() without arguments is deprecated. Use CreateAsync(callback) instead.")]
    public static void Create()
    {
        CreateAsync(null);
    }

    /// <summary>Asynchronously instantiates a `GvrDayreamApi`.</summary>
    /// <remarks>
    /// The provided callback will be called with a bool argument indicating whether instance
    /// creation was successful.
    /// </remarks>
    /// <param name="callback">A callback to make after creating a `GvrDaydreamApi`.</param>
    public static void CreateAsync(Action<bool> callback)
    {
        if (instance == null)
        {
            instance = new GvrDaydreamApi();
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        if (instance.daydreamApiObject != null)
        {
            return;
        }

        if (instance.daydreamApiClass == null)
        {
            Debug.LogErrorFormat("Failed to get DaydreamApi class, {0}", PACKAGE_DAYDREAM_API);
            return;
        }

        AndroidJavaObject activity = GvrActivityHelper.GetActivity();
        if (activity == null)
        {
            Debug.LogError("DaydreamApi.Create failed to get acitivty");
            return;
        }

        AndroidJavaObject context = GvrActivityHelper.GetApplicationContext(activity);
        if (context == null)
        {
            Debug.LogError("DaydreamApi.Create failed to get application context from activity");
            return;
        }

        activity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() =>
        {
            instance.daydreamApiObject =
                instance.daydreamApiClass.CallStatic<AndroidJavaObject>(METHOD_CREATE, context);
            bool success = instance.daydreamApiObject != null;
            if (!success)
            {
                Debug.LogErrorFormat("DaydreamApi.Create call to {0} failed to instantiate object",
                    METHOD_CREATE);
            }

            if (callback != null)
            {
                callback(success);
            }
        }));
#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }

    /// @deprecated Use `LaunchVrHomeAsync(callback)` instead.
    /// <summary>Launches a generic asynchronous VR Home call.</summary>
    [System.Obsolete("LaunchVrHome() deprecated. Use LaunchVrHomeAsync(callback) instead.")]
    public static void LaunchVrHome()
    {
        LaunchVrHomeAsync(null);
    }

    /// <summary>Asynchronously launches VR Home.</summary>
    /// <remarks><para>
    /// Instantiates an instance of GvrDaydreamApi if necessary. If successful, launches VR Home.
    /// </para><para>
    /// The provided callback will be called with a bool argument indicating whether instance
    /// creation and launch of VR Home was successful.
    /// </para></remarks>
    /// <param name="callback">A callback to make after launching the VrHome screen.</param>
    public static void LaunchVrHomeAsync(Action<bool> callback)
    {
        EnsureCreated((success) =>
        {
            if (success)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                instance.daydreamApiObject.Call(METHOD_LAUNCH_VR_HOMESCREEN);
#else
                Debug.LogWarning("Launching VR Home is only possible on Android devices.");
#endif  // UNITY_ANDROID && !UNITY_EDITOR
            }

            if (callback != null)
            {
                callback(success);
            }
        });
    }

    /// @cond
    /// <summary>Call Dispose to free up memory used by this API.</summary>
    public void Dispose()
    {
        instance = null;
    }

    /// @endcond
    /// <summary>Ensures that the Daydream Api has been created.</summary>
    /// <param name="callback">The callback to make upon completion.</param>
    private static void EnsureCreated(Action<bool> callback)
    {
        if (!IsCreated)
        {
            CreateAsync(callback);
        }
        else
        {
            callback(true);
        }
    }
}

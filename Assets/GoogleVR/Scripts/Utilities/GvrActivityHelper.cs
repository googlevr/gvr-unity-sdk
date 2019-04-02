//-----------------------------------------------------------------------
// <copyright file="GvrActivityHelper.cs" company="Google Inc.">
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

/// <summary>
/// Simple static class to abstract out several JNI calls that need to be shared between different
/// classes.
/// </summary>
public static class GvrActivityHelper
{
    /// @cond
#if UNITY_IOS
    /// <summary>The name of the GVR DLL.</summary>
    public const string GVR_DLL_NAME = "__Internal";
#else
    /// <summary>The name of the GVR DLL.</summary>
    public const string GVR_DLL_NAME = "gvr";

    /// <summary>The name of the GVR Shim DLL.</summary>
    public const string GVR_SHIM_DLL_NAME = "gvrshim";
#endif // UNITY_IOS
    /// @endcond
    /// <summary>Package name for the Android Unity player class.</summary>
    public const string PACKAGE_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Returns the Android Activity used by the Unity device player.
    /// </summary>
    /// <remarks>
    /// The caller is responsible for memory-managing the returned `AndroidJavaObject`.
    /// </remarks>
    /// <returns>An Android GVR activity.</returns>
    public static AndroidJavaObject GetActivity()
    {
        AndroidJavaClass jc = new AndroidJavaClass(PACKAGE_UNITY_PLAYER);
        if (jc == null)
        {
            Debug.LogErrorFormat("Failed to get class {0}", PACKAGE_UNITY_PLAYER);
            return null;
        }

        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        if (activity == null)
        {
            Debug.LogError("Failed to obtain current Android activity.");
            return null;
        }

        return activity;
    }

    /// <summary>Returns the application context of the current Android Activity.</summary>
    /// <param name="activity">The Android activity from which to get the context.</param>
    /// <returns>The application context from the associated GVR activity.</returns>
    public static AndroidJavaObject GetApplicationContext(AndroidJavaObject activity)
    {
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        if (context == null)
        {
            Debug.LogError("Failed to get application context from Activity.");
            return null;
        }

        return context;
    }
#endif // UNITY_ANDROID && !UNITY_EDITOR
}

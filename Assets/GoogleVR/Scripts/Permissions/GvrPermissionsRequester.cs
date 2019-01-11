//-----------------------------------------------------------------------
// <copyright file="GvrPermissionsRequester.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0(the "License");
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

#if UNITY_ANDROID || UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using Gvr.Internal;

// Requests dangerous permissions at runtime
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrPermissionsRequester")]
public class GvrPermissionsRequester
{
    // Permissions are requested via an Android Activity Fragment java object.
    private AndroidJavaObject permissionsFragment = null;

    // Constants used via JNI to access the permissions fragment.
    private const string FRAGMENT_CLASSNAME =
        "com.google.gvr.permissionsupport.PermissionsFragment";

    private const string CALLBACK_CLASSNAME = FRAGMENT_CLASSNAME + "$PermissionsCallback";

    // Singleton instance.
    private static GvrPermissionsRequester theInstance;

    /// The singleton instance of the PermissionsRequester class,
    /// lazily instantiated.
    public static GvrPermissionsRequester Instance
    {
        [SuppressMemoryAllocationError(IsWarning = false,
                                       Reason = "Lazy-loading getter is allowed to allocate sometimes.")]
        get
        {
            if (theInstance == null)
            {
                theInstance = new GvrPermissionsRequester();
                if (!theInstance.InitializeFragment())
                {
                    Debug.LogError("Cannot initialize fragment!");
                    theInstance = null;
                }
            }

            return theInstance;
        }
    }

    /// <summary>
    /// Initializes the fragment via JNI.
    /// </summary>
    /// <returns>True if fragment was initialized.</returns>
    protected bool InitializeFragment()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        Debug.LogWarning("GvrPermissionsRequester requires the Android runtime environment");
        return false;
#else
        AndroidJavaClass ajc = new AndroidJavaClass(FRAGMENT_CLASSNAME);

        if (ajc != null)
        {
            // Get the PermissionsRequesterFragment object
            permissionsFragment = ajc.CallStatic<AndroidJavaObject>("getInstance",
                GvrActivityHelper.GetActivity());
        }

        return permissionsFragment != null &&
        permissionsFragment.GetRawObject() != IntPtr.Zero;
#endif  // !UNITY_ANDROID || UNITY_EDITOR
    }

    [SuppressMemoryAllocationError(IsWarning = true)]
    public bool IsPermissionGranted(string permission)
    {
        return permissionsFragment.Call<bool>("hasPermission", permission);
    }

    [SuppressMemoryAllocationError(IsWarning = true)]
    public bool[] HasPermissionsGranted(string[] permissions)
    {
        Debug.Log("Calling HasPermissionsGranted: " + permissions);

        object[] args = { permissions };
        AndroidJavaObject resultArr =
            permissionsFragment.Call<AndroidJavaObject>("hasPermissions", args);

        if (resultArr.GetRawObject() != IntPtr.Zero)
        {
            return AndroidJNIHelper.ConvertFromJNIArray<bool[]>(
                resultArr.GetRawObject());
        }
        else
        {
            return new bool[0];
        }
    }

    public bool ShouldShowRational(string permission)
    {
        Debug.Log("GvrPermissionsRequester.ShouldShowRational()");
        return permissionsFragment.Call<bool>("shouldShowRational", permission);
    }

    public void RequestPermissions(string[] permissionArray,
                                   Action<PermissionStatus[]> callback)
    {
        PermissionsCallback cb = new PermissionsCallback(permissionArray, callback);
        permissionsFragment.Call("requestPermission", permissionArray, cb);
        Debug.Log("Calling requestPermission");
    }

    public class PermissionStatus
    {
        public PermissionStatus(string name, bool granted)
        {
            Name = name;
            Granted = granted;
        }

        public string Name { get; set; }

        public bool Granted { get; set; }
    }

    /// <summary>
    /// Permissions callback implementation.
    /// </summary>
    /// <remarks>Instances of this class are passed to the java fragment and then
    /// invoked once the request process is completed by the user.
    /// </remarks>
    class PermissionsCallback : AndroidJavaProxy
    {
        // permissions being requested.
        private string[] permissionNames;
        private Action<PermissionStatus[]> callback;

        internal PermissionsCallback(string[] requestedPermissions,
                                      Action<PermissionStatus[]> callback) :
            base(CALLBACK_CLASSNAME)
        {
            permissionNames = requestedPermissions;
            this.callback = callback;
        }

        /// <summary>
        /// Called when then permission request flow is completed.
        /// </summary>
        /// <param name="allPermissionsGranted">
        /// True if all permissions granted.</param>
        void onRequestPermissionResult(bool allPermissionsGranted)
        {
            List<PermissionStatus> permissionStatusList =
                new List<PermissionStatus>();
            if (allPermissionsGranted)
            {
                Debug.Log("onRequestPermissionResult(): all permissions granted");
                foreach (string p in permissionNames)
                {
                    permissionStatusList.Add(new PermissionStatus(p, true));
                }
            }
            else
            {
                Debug.Log("onRequestPermissionResult(): some permissions denied");

                bool[] grantResults = Instance.HasPermissionsGranted(permissionNames);
                Debug.Log("onRequestPermissionResult(): checking " + grantResults);
                int size = grantResults.Length;
                for (int i = 0; i < size; i++)
                {
                    // get the grant result
                    string name = permissionNames[i];
                    bool grantResult = grantResults[i];
                    permissionStatusList.Add(new PermissionStatus(name, grantResult));
                }
            }

            callback(permissionStatusList.ToArray());
        }
    }
}
#endif  // UNITY_ANDROID

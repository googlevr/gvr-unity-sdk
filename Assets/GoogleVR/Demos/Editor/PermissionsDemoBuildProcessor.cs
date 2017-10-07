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
// See the License for the specific language governing permissioßns and
// limitations under the License.

// Only invoke custom build processor when building for Android.
#if UNITY_ANDROID
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditorInternal.VR;

class PermissionsDemoBuildProcessor : IPreprocessBuild, IPostprocessBuild
{
    private const string SCENE_NAME_PERMISSIONS_DEMO = "PermissionsDemo";
    private const string VR_DEVICE_CARDBOARD = "cardboard";
    private const string VR_DEVICE_DAYDREAM = "daydream";

    private bool m_cardboardAddedFromCode = false;

    public int callbackOrder
    {
        get { return 0; }
    }

    // OnPreprocessBuild() is called right before the build process begins. If it
    // detects that the first enabled scene in the build arrays is the PermissionsDemo,
    // and Daydream is in the VR SDKs, it will add Cardboard to the VR SDKs. Because
    // the PermissionsDemo needs a perm statement in the Manifest while other demos don't.
    // Adding Cardboard to VR SDKs will merge in the Manifest-Cardboard which has perm
    // statement in it.
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        m_cardboardAddedFromCode = false;

        string[] androidVrSDKs = VREditor.GetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android);

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        // See if PermissionsDemo is the first enabled scene in the array of scenes to build.
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path.Contains(SCENE_NAME_PERMISSIONS_DEMO))
            {
                if (!scenes[i].enabled)
                {
                    return;
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (scenes[i].enabled)
                {
                    return;
                }
            }
        }

        bool hasCardboard = Array.Exists<string>(androidVrSDKs,
            element => element.Equals(VR_DEVICE_CARDBOARD));

        if (hasCardboard)
        {
            return;
        }

        bool hasDaydream = Array.Exists<string>(androidVrSDKs,
            element => element.Equals(VR_DEVICE_DAYDREAM));

        if (!hasDaydream)
        {
            return;
        }

        string[] androidVrSDKsAppended = new string[androidVrSDKs.Length+1];

        for (int i = 0; i < androidVrSDKs.Length; i++)
        {
            androidVrSDKsAppended[i] = androidVrSDKs[i];
        }

        androidVrSDKsAppended[androidVrSDKsAppended.Length - 1] = VR_DEVICE_CARDBOARD;

        VREditor.SetVREnabledOnTargetGroup(
            BuildTargetGroup.Android, true);
        VREditor.SetVREnabledDevicesOnTargetGroup(
            BuildTargetGroup.Android,
            androidVrSDKsAppended);

        m_cardboardAddedFromCode = true;
    }

    // OnPostprocessBuild() is called after the build process. It does appropriate cleanup
    // so that this script only affects build process for PermissionsDemo, not others.
    public void OnPostprocessBuild(BuildTarget target, string path)
    {
        if (!m_cardboardAddedFromCode)
            return;

        string[] androidVrSDKs = VREditor.GetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android);

        // The enabled devices are modified somehow, which shouldn't happen. Abort the post build process.
        if (androidVrSDKs.Length == 0 || androidVrSDKs[androidVrSDKs.Length - 1] != VR_DEVICE_CARDBOARD)
        {
            return;
        }

        string[] androidVrSDKsShortened = new string[androidVrSDKs.Length - 1];

        for (int i = 0; i < androidVrSDKsShortened.Length; i++)
        {
            androidVrSDKsShortened[i] = androidVrSDKs[i];
        }

        VREditor.SetVREnabledOnTargetGroup(
            BuildTargetGroup.Android, true);
        VREditor.SetVREnabledDevicesOnTargetGroup(
            BuildTargetGroup.Android,
            androidVrSDKsShortened);

        m_cardboardAddedFromCode = false;
    }
}
#endif  // UNITY_ANDROID

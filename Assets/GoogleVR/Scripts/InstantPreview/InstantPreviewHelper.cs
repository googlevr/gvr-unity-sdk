//-----------------------------------------------------------------------
// <copyright file="InstantPreviewHelper.cs" company="Google Inc.">
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

using System.IO;
using System.Runtime.InteropServices;
using Gvr.Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>Helper methods for Instant preview.</summary>
[ExecuteInEditMode]
[HelpURL("https://developers.google.com/vr/unity/reference/class/InstantPreviewHelper")]
public class InstantPreviewHelper : MonoBehaviour
{
    /// <summary>Path to `adb` executable.</summary>
    public static string adbPath;

    /// <summary>Path to `aapt` executable.</summary>
    public static string aaptPath;

#if UNITY_ANDROID && UNITY_EDITOR

#if UNITY_WINDOWS
    private const string CHECK_ANDROID_SDK_PATH =
        "Verify that your Android SDK path is configured correctly" +
        " (Edit > Preferences > External Tools > Android SDK).\n" +
        "See https://docs.unity3d.com/Manual/android-sdksetup.html for more information.";
#else
    private const string CHECK_ANDROID_SDK_PATH =
        "Verify that your Android SDK path is configured correctly" +
        " (Unity > Preferences > External Tools > Android SDK).\n" +
        "See https://docs.unity3d.com/Manual/android-sdksetup.html for more information.";
#endif // UNITY_WINDOWS

    [DllImport(InstantPreview.dllName)]
    private static extern bool SetAdbPathAndStart(string adbPath);

    private void Awake()
    {
        // Gets android SDK root from preferences.
        var sdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
        if (string.IsNullOrEmpty(sdkRoot))
        {
            Debug.LogError(CHECK_ANDROID_SDK_PATH);
            return;
        }

        // Gets adb path from known directory.
        adbPath = Path.Combine(Path.GetFullPath(sdkRoot),
                               "platform-tools" + Path.DirectorySeparatorChar + "adb");

        // Gets latest build-tools subdirectory.
        string LatestBuildToolsDir = GetLatestBuildToolsDir(sdkRoot);
        if (LatestBuildToolsDir != null)
        {
            // Gets aapt path from known directory.
            aaptPath = Path.Combine(Path.GetFullPath(LatestBuildToolsDir), "aapt");
        }
        else
        {
            Debug.LogError(string.Format("build-tools not found in \"{0}\". Please add " +
                                         "build-tools to your SDK path and restart the Unity " +
                                         "editor.", Path.GetFullPath(sdkRoot)));
            return;
        }
#if UNITY_EDITOR_WIN
        adbPath = Path.ChangeExtension(adbPath, "exe");
        aaptPath = Path.ChangeExtension(aaptPath, "exe");
#endif // UNITY_EDITOR_WIN

        if (!File.Exists(adbPath))
        {
            Debug.LogErrorFormat("\"{0}\" not found. {1}", adbPath, CHECK_ANDROID_SDK_PATH);
            return;
        }

        if (!File.Exists(aaptPath))
        {
            Debug.LogError(string.Format("aapt not found at \"{0}\". Please add aapt to your SDK " +
                                         "path and restart the Unity editor.", aaptPath));
            return;
        }

        // Try to start server.
        var started = SetAdbPathAndStart(adbPath);
        if (!started)
        {
            Debug.LogErrorFormat("Couldn't start Instant Preview server using \"{0}\".", adbPath);
        }
    }

#elif UNITY_EDITOR
    void Awake()
    {
        Debug.LogWarning("Instant Preview is disabled; set target platform to Android to use it.");
    }

#endif

    // Split vesion directory paths (eg "Path/To/Build-Tools/23.0.2") into an array of ints
    // (eg [23, 0, 2]).
    private int[] GetIntValuesFromString(string DirPath)
    {
        string DirName = Path.GetFileName(DirPath);
        string[] VersionValues = DirName.Split('.');
        int[] VersionInts = new int[VersionValues.Length];
        for (int j = 0; j < VersionValues.Length; ++j)
        {
            if (!int.TryParse(VersionValues[j], out VersionInts[j]))
            {
                VersionInts[j] = 0;
            }
        }

        return VersionInts;
    }

    // Get the numerically latest subdirectory within build-tools subdirectories, (eg select 101.0.1
    // rather than 99.5.3).
    // Returns the full path (eg /path/to/build-tools/101.0.1).
    private string GetLatestBuildToolsDir(string sdkRoot)
    {
        string[] BuildToolsDirs = Directory.GetDirectories(Path.Combine(
            Path.GetFullPath(sdkRoot), string.Format("build-tools")));
        if (BuildToolsDirs.Length == 0)
        {
            return null;
        }

        string LatestBuildToolsDir = BuildToolsDirs[0];
        int[] LatestVersionInts = GetIntValuesFromString(LatestBuildToolsDir);
        for (int i = 1; i < BuildToolsDirs.Length; ++i)
        {
            int[] CurrentVersionInts = GetIntValuesFromString(BuildToolsDirs[i]);

            // Compare ints sequentially.
            for (int j = 0; j < Mathf.Min(LatestVersionInts.Length, CurrentVersionInts.Length); ++j)
            {
                if (LatestVersionInts[j] > CurrentVersionInts[j])
                {
                    break;
                }
                else if (CurrentVersionInts[j] > LatestVersionInts[j] ||
                         j == LatestVersionInts.Length - 1)
                {
                    // If one string version string has more elements than the other and leading
                    // digits are the same, it's probably newer.
                    LatestVersionInts = CurrentVersionInts;
                    LatestBuildToolsDir = BuildToolsDirs[i];
                }
            }
        }

        return LatestBuildToolsDir;
    }
}

#if !UNITY_ANDROID && UNITY_EDITOR
[CustomEditor(typeof(InstantPreviewHelper))]
public class InstantPreviewHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(
            "Instant Preview is disabled; set target platform to Android to use it.");
    }
}
#endif

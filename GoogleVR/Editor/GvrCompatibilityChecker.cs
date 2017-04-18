// Copyright 2016 Google Inc. All rights reserved.
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

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using System.Linq;

// Disable unused variable warnings.
#pragma warning disable 414

/// <summary>
/// Updates non-native versions of Unity with additional GVR library imports if those files
/// do not exist. Otherwise, if this is Unity 5.4 and there are legacy libraries or
/// AndroidManifest.xml, they are removed.
/// </summary>
[InitializeOnLoad]
public class GvrCompatibilityChecker {
  // Asset subpaths.
  private static string PLUGINS_ANDROID_PATH = Application.dataPath + "/Plugins/Android/";
  private static string PLUGINS_IOS_PATH = Application.dataPath + "/Plugins/iOS/";
  private static string ARMEABI_PATH = "libs/armeabi-v7a/";
  private static string X86_PATH = "libs/x86/";
  private static string IGNORE_MANIFEST_MERGE_CHECK_PATH = "GvrIgnoreManifestMergeCheck.txt";
  private static string IGNORE_COMPATIBILITY_CHECK_PATH = "GvrIgnoreCompatibilityCheck.txt";

  // Files for backwards compatibility.
  private static string ANDROID_MANIFEST = "AndroidManifest.xml";
  private static string ANDROID_MANIFEST_CARDBOARD = "AndroidManifest-Cardboard.xml";
  private static string NATIVE_LIB = "libgvrunity.so";

  private static string[] BACK_COMPAT_FILE_PATHS = new string[] {
    PLUGINS_ANDROID_PATH + ARMEABI_PATH + NATIVE_LIB,
    PLUGINS_ANDROID_PATH + X86_PATH + NATIVE_LIB,
    PLUGINS_ANDROID_PATH + "gvr_android_common.aar",
    PLUGINS_ANDROID_PATH + "unitygvractivity.aar",
    PLUGINS_IOS_PATH + "libgvrunity.a",
    PLUGINS_IOS_PATH + "CardboardAppController.h",
    PLUGINS_IOS_PATH + "CardboardAppController.mm",
  };

  private static string[] BACK_COMPAT_DIR_PATHS =  new string[] {
    PLUGINS_IOS_PATH + "CardboardSDK.bundle",
    PLUGINS_IOS_PATH + "GoogleKitCore.bundle",
    PLUGINS_IOS_PATH + "GoogleKitDialogs.bundle",
    PLUGINS_IOS_PATH + "GoogleKitHUD.bundle",
    PLUGINS_IOS_PATH + "MaterialRobotoFontLoader.bundle",
  };

  // Files for native integration compatibility.
  private static string IOS_AUDIO_LIB = "libaudioplugingvrunity.a";

  // GVR backwards-compatible package.
  private static string BACK_COMPAT_PACKAGE_PATH =
    "/GoogleVR/GVRBackwardsCompatibility.unitypackage";

  // iOS native integration-compatible package.
  private static string IOS_NATIVE_COMPAT_PACKAGE_PATH =
    "/GoogleVR/GVRiOSNativeCompatibility.unitypackage";

  // Path elements.
  private static string ASSET_PATH_PREFIX = "Assets";
  private static string META_EXT = ".meta";

  // Dialog text.
  private static string BACK_COMPAT_FILES_FOUND_TITLE = "File Removal Required";
  private static string BACK_COMPAT_FILES_FOUND_MESSAGE =
    "Detected GVR libraries targeting a pre-5.4 build of Unity.\n\n" +
    "The following files must be removed to avoid project compilation discrepancies:\n\n";
  private static string IMPORT_REQUIRED_TITLE = "Package Import Required";
  private static string IMPORT_REQUIRED_MESSAGE =
    "Assets/GoogleVR/GVRBackwardsCompatibility.unitypackage must be imported for GVR to be " +
    "compatible with this version of Unity, which does not have the GVR native integration.\n" +
    "Please download this file from github.com/gvr-unity-sdk if it is not already in your project.";
  private static string MANIFEST_UPDATE_WARNING_TITLE = "AndroidManifest.xml Merge Required";
  private static string MERGE_MANIFEST_WARNING_MESSAGE =
    "Please merge the existing AndroidManifest.xml with AndroidManifest-Cardboard.xml.";
  private static string UNMERGE_MANIFEST_WARNING_MESSAGE =
    "Please remove all Cardboard and/or Daydream-specific attributes or tags from " +
    "AndroidManifest.xml. Delete this file if it consists only of a subset of " +
    "AndroidManifest-Cardboard.xml and/or AndroidManifest-Daydream.xml.";

  private static string PACKAGE_NOT_FOUND_TITLE = "Package not found";
  private static string REENABLE_COMPATIBILITY_CHECK_TITLE = "Skipping Compatibility Checks";
  private static string REENABLE_COMPATIBILITY_CHECK_MESSAGE =
    "Compatibility checks can be re-enabled by deleting " + IGNORE_COMPATIBILITY_CHECK_PATH;

  // Button text/
  private static string CANCEL_BUTTON = "Cancel";
  private static string CANCEL_DO_NOT_CHECK_AGAIN_BUTTON = "Cancel and Do Not Show Again";
  private static string IMPORT_PACKAGE_BUTTON = "Import Package";
  private static string OK_BUTTON = "OK";
  private static string REMOVE_FILES_BUTTON = "Remove Files";

// Only perform compatibility check if current build platform is Android or iOS.
#if UNITY_ANDROID || UNITY_IOS
  static GvrCompatibilityChecker() {
// No need to run the backwards compatibility checker GVR is natively integrated into Unity.
#if !UNITY_HAS_GOOGLEVR
    if (!IgnoreCompatibilityCheck() &&
        !AllBackwardsCompatibilityFilesExist()) {
      RemoveiOSNativeIntegrationFiles();
      ImportBackwardsCompatibilityPackage();
    }
#else
    RemoveAnyBackwardsCompatibleFiles();
    AndroidManifestCompatibilityUpdate();
#if UNITY_IOS
    ImportiOSNativeCompatibilityPackage();
#endif  // UNITY_IOS
#endif  // !UNITY_HAS_GOOGLEVR
  }
#endif  // UNITY_ANDROID || UNITY_IOS

  private static bool AllBackwardsCompatibilityFilesExist() {
    return !BACK_COMPAT_FILE_PATHS.Where(filePath => !File.Exists(filePath)).Any() &&
      !BACK_COMPAT_DIR_PATHS.Where(dirPath => !Directory.Exists(dirPath)).Any();
  }

  private static void ImportiOSNativeCompatibilityPackage() {
    string iOSAudioLib = PLUGINS_IOS_PATH + IOS_AUDIO_LIB;
    if (File.Exists(iOSAudioLib)) {
      return;
    }

    string packagePath = Application.dataPath + IOS_NATIVE_COMPAT_PACKAGE_PATH;
    AssetDatabase.ImportPackage(packagePath, true);
    AssetDatabase.Refresh();
  }

  private static void ImportBackwardsCompatibilityPackage() {
    int option = EditorUtility.DisplayDialogComplex(IMPORT_REQUIRED_TITLE,
      IMPORT_REQUIRED_MESSAGE,
      IMPORT_PACKAGE_BUTTON,
      CANCEL_DO_NOT_CHECK_AGAIN_BUTTON,
      CANCEL_BUTTON);

    switch (option) {
      case 0: // Import the package.
        string packagePath = Application.dataPath + BACK_COMPAT_PACKAGE_PATH;
        if (File.Exists(IGNORE_MANIFEST_MERGE_CHECK_PATH)) {
          File.Delete(IGNORE_MANIFEST_MERGE_CHECK_PATH);
        }

        if (!File.Exists(packagePath)) {
          EditorUtility.DisplayDialog(PACKAGE_NOT_FOUND_TITLE, null, OK_BUTTON);
          return;
        }
        AssetDatabase.ImportPackage(packagePath, true);
        AssetDatabase.Refresh();
        AndroidManifestCompatibilityUpdate();
        return;

      case 1: // Do not import, and do not check again.
        File.Create(IGNORE_COMPATIBILITY_CHECK_PATH);
        File.Create(IGNORE_MANIFEST_MERGE_CHECK_PATH);
        EditorUtility.DisplayDialog(REENABLE_COMPATIBILITY_CHECK_TITLE,
          REENABLE_COMPATIBILITY_CHECK_MESSAGE, OK_BUTTON);
        AndroidManifestCompatibilityUpdate();
        return;

      case 2: // Do not import.
        // Fall through.
      default:
        return;
    }
  }

  private static void RemoveAnyBackwardsCompatibleFiles() {
    IEnumerable<string> backCompatFiles = BACK_COMPAT_FILE_PATHS.AsEnumerable();
    backCompatFiles = backCompatFiles.Where(filePath => File.Exists(filePath));

    IEnumerable<string> iOSBackCompatDirs = BACK_COMPAT_DIR_PATHS.AsEnumerable();
    iOSBackCompatDirs = iOSBackCompatDirs.Where(dirPath => Directory.Exists(dirPath));

    if (backCompatFiles.Count() == 0 && iOSBackCompatDirs.Count() == 0) {
      return;
    }

    int dataPathLen = Application.dataPath.Length;
    string filesToRemove = "";
    foreach (string file in backCompatFiles) {
      filesToRemove += string.Format("\t{0}\n", file.Substring(dataPathLen + 1));
    }
    foreach (string dir in iOSBackCompatDirs) {
      filesToRemove += string.Format("\t{0}\n", dir.Substring(dataPathLen + 1));
    }

    bool removeBackwardsCompatibleFiles = EditorUtility.DisplayDialog(
      BACK_COMPAT_FILES_FOUND_TITLE,
      string.Format("{0}{1}", BACK_COMPAT_FILES_FOUND_MESSAGE, filesToRemove),
      REMOVE_FILES_BUTTON, CANCEL_BUTTON);
    if (!removeBackwardsCompatibleFiles) {
      return;
    }

    // Remove files.
    foreach (string file in backCompatFiles) {
      AssetDatabase.DeleteAsset(ASSET_PATH_PREFIX + file.Substring(dataPathLen));
      if (File.Exists(file)) {
        File.Delete(file);
      }
      if (File.Exists(file + META_EXT)) {
        File.Delete(file + META_EXT);
      }
      AssetDatabase.Refresh();
    }

    // Remove iOS bundles.
    foreach (string dir in iOSBackCompatDirs) {
      AssetDatabase.DeleteAsset(ASSET_PATH_PREFIX + dir.Substring(dataPathLen));
      // AssetDatabase may not fully delete files in versions < 5.6.
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
      if (File.Exists(dir + META_EXT)) {
        File.Delete(dir + META_EXT);
      }
      AssetDatabase.Refresh();
    }
  }

  private static void RemoveiOSNativeIntegrationFiles() {
    string iOSAudioLib = PLUGINS_IOS_PATH + IOS_AUDIO_LIB;
    if (!File.Exists(iOSAudioLib)) {
      return;
    }
    AssetDatabase.DeleteAsset(
        ASSET_PATH_PREFIX + iOSAudioLib.Substring(Application.dataPath.Length));
    File.Delete(iOSAudioLib);
    File.Delete(iOSAudioLib + META_EXT);
    AssetDatabase.Refresh();
  }

  private static bool IgnoreCompatibilityCheck() {
    return File.Exists(IGNORE_COMPATIBILITY_CHECK_PATH);
  }

  private static void AndroidManifestCompatibilityUpdate() {
#if !UNITY_HAS_GOOGLEVR
    if (File.Exists(PLUGINS_ANDROID_PATH + ANDROID_MANIFEST)) {
      // Show warning dialog.
      EditorUtility.DisplayDialog(MANIFEST_UPDATE_WARNING_TITLE,
          MERGE_MANIFEST_WARNING_MESSAGE, OK_BUTTON);
    } else {
      FileUtil.CopyFileOrDirectory(PLUGINS_ANDROID_PATH + ANDROID_MANIFEST_CARDBOARD,
          PLUGINS_ANDROID_PATH + ANDROID_MANIFEST);
    }
#else
    if (!File.Exists(IGNORE_MANIFEST_MERGE_CHECK_PATH) &&
        File.Exists(PLUGINS_ANDROID_PATH + ANDROID_MANIFEST)) {
      EditorUtility.DisplayDialog(MANIFEST_UPDATE_WARNING_TITLE,
          UNMERGE_MANIFEST_WARNING_MESSAGE, OK_BUTTON);
    }
#endif  // UNITY_HAS_GOOGLEVR
    File.Create(IGNORE_MANIFEST_MERGE_CHECK_PATH);
  }
}
#pragma warning restore 414

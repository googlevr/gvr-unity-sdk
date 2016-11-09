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
  private static string ARMEABI_PATH = "libs/armeabi-v7a/";
  private static string X86_PATH = "libs/x86/";
  private static string IGNORE_MANIFEST_MERGE_CHECK_PATH = "GvrIgnoreManifestMergeCheck.txt";
  private static string IGNORE_COMPATIBILITY_CHECK_PATH = "GvrIgnoreCompatibilityCheck.txt";

  // Files for backwards compatibility.
  private static string ANDROID_MANIFEST = "AndroidManifest.xml";
  private static string ANDROID_MANIFEST_CARDBOARD = "AndroidManifest-Cardboard.xml";
  private static string COMMON_AAR = "gvr_android_common.aar";
  private static string GVR_ACTIVITY_AAR = "unitygvractivity.aar";
  private static string NATIVE_LIB = "libgvrunity.so";

  // GVR backwards-compatible package.
  private static string BACK_COMPAT_PACKAGE_PATH =
    "/GoogleVR/compatibility-without-gvr-integration.unitypackage";

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
    "Additional libraries must be imported for GVR to be compatible with " +
    "this version of Unity, which does not have the GVR native integration.";
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

  static GvrCompatibilityChecker() {
// No need to run the backwards compatibility checker GVR is natively integrated into Unity.
#if !UNITY_HAS_GOOGLEVR
    if (!IgnoreCompatibilityCheck() &&
        !AllBackwardsCompatibilityFilesExist()) {
      ImportBackwardsCompatibilityPackage();
    }
#else
    RemoveAnyBackwardsCompatibleFiles();
    AndroidManifestCompatibilityUpdate();
#endif  // !UNITY_HAS_GOOGLEVR
  }

  private static bool AllBackwardsCompatibilityFilesExist() {
    return !GetBackCompatFilePaths().Where(filePath => !File.Exists(filePath)).Any();
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
    IEnumerable<string> backCompatFiles = GetBackCompatFilePaths().AsEnumerable();
    backCompatFiles = backCompatFiles.Where(filePath => File.Exists(filePath));
    if (backCompatFiles.Count() == 0) {
      return;
    }

    string filesToRemove = "";
    foreach (string file in backCompatFiles) {
      filesToRemove += string.Format("\t{0}\n", file);
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
      AssetDatabase.DeleteAsset(ASSET_PATH_PREFIX + file.Substring(Application.dataPath.Length));
      File.Delete(file);
      File.Delete(file + META_EXT);
      AssetDatabase.Refresh();
    }
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

  private static string[] GetBackCompatFilePaths() {
    return new string[] {
      PLUGINS_ANDROID_PATH + ARMEABI_PATH + NATIVE_LIB,
      PLUGINS_ANDROID_PATH + X86_PATH + NATIVE_LIB,
      PLUGINS_ANDROID_PATH + COMMON_AAR,
      PLUGINS_ANDROID_PATH + GVR_ACTIVITY_AAR,
    };
  }
}
#pragma warning restore 414

// Copyright 2014 Google Inc. All rights reserved.
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
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;

/// @ingroup EditorScripts
/// A custom editor for properties on the Cardboard script.  This appears in the
/// Inspector window of a Cardboard object.  Its purpose is to allow changing the
/// `Cardboard.SDK` object's properties from their default values.
[CustomEditor(typeof(Cardboard))]
[InitializeOnLoad]
public class CardboardEditor : Editor {
  GUIContent vrModeLabel = new GUIContent("VR Mode Enabled",
      "Sets whether VR mode is enabled.");

  GUIContent distortionCorrectionLabel = new GUIContent("Distortion Correction",
      "Whether distortion correction is performed the SDK.");

  GUIContent autoDriftCorrectionLabel = new GUIContent("Auto Drift Correction",
      "When enabled, drift in the gyro readings is estimated and removed.");

  GUIContent neckModelScaleLabel = new GUIContent("Neck Model Scale",
      "The scale factor of the builtin neck model [0..1].  To disable, set to 0.");

#if UNITY_IOS
  GUIContent syncWithCardboardLabel = new GUIContent("Sync with Cardboard App",
      "Enables the 'Sync with Google Cardboard' slider in the viewer settings dialog.");
#endif

  GUIContent alignmentMarkerLabel = new GUIContent("Alignment Marker",
      "Whether to draw the alignment marker. The marker is a vertical line that splits " +
      "the viewport in half, designed to help users align the screen with the Cardboard.");

  GUIContent settingsButtonLabel = new GUIContent("Settings Button",
      "Whether to draw the settings button. The settings button opens the " +
      "Google Cardboard app to allow the user to  configure their individual " +
      "settings and Cardboard headset parameters.");

  GUIContent tapIsTriggerLabel = new GUIContent("Tap Is Trigger",
      "Whether screen taps are treated as trigger events.");

  GUIContent editorSettingsLabel = new GUIContent("Unity Editor Emulation Settings",
      "Controls for the in-editor emulation of Cardboard.");

  GUIContent autoUntiltHeadLabel = new GUIContent("Auto Untilt Head",
      "When enabled, just release Ctrl to untilt the head.");

  GUIContent screenSizeLabel = new GUIContent("Screen Size",
      "The screen size to emulate.");

  GUIContent deviceTypeLabel = new GUIContent("Device Type",
      "The Cardboard device type to emulate.");

  public override void OnInspectorGUI() {
    GUI.changed = false;

    GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
    headingStyle.fontStyle = FontStyle.Bold;
    headingStyle.fontSize = 14;

    Cardboard cardboard = (Cardboard)target;

    EditorGUILayout.LabelField("General Settings", headingStyle);
    cardboard.VRModeEnabled =
        EditorGUILayout.Toggle(vrModeLabel, cardboard.VRModeEnabled);
    cardboard.DistortionCorrection =
        EditorGUILayout.Toggle(distortionCorrectionLabel, cardboard.DistortionCorrection);
    cardboard.AutoDriftCorrection =
        EditorGUILayout.Toggle(autoDriftCorrectionLabel, cardboard.AutoDriftCorrection);
    cardboard.NeckModelScale =
        EditorGUILayout.Slider(neckModelScaleLabel, cardboard.NeckModelScale, 0, 1);
    EditorGUILayout.Separator();

    EditorGUILayout.LabelField("Cardboard Settings", headingStyle);
#if UNITY_IOS
    cardboard.SyncWithCardboardApp =
        EditorGUILayout.Toggle(syncWithCardboardLabel, cardboard.SyncWithCardboardApp);
#endif
    cardboard.EnableAlignmentMarker =
        EditorGUILayout.Toggle(alignmentMarkerLabel, cardboard.EnableAlignmentMarker);
    cardboard.EnableSettingsButton =
        EditorGUILayout.Toggle(settingsButtonLabel, cardboard.EnableSettingsButton);
    cardboard.TapIsTrigger =
        EditorGUILayout.Toggle(tapIsTriggerLabel, cardboard.TapIsTrigger);
    EditorGUILayout.Separator();

    EditorGUILayout.LabelField(editorSettingsLabel, headingStyle);
    cardboard.autoUntiltHead =
        EditorGUILayout.Toggle(autoUntiltHeadLabel, cardboard.autoUntiltHead);
    cardboard.ScreenSize = (CardboardProfile.ScreenSizes)
        EditorGUILayout.EnumPopup(screenSizeLabel, cardboard.ScreenSize);
    cardboard.DeviceType = (CardboardProfile.DeviceTypes)
        EditorGUILayout.EnumPopup(deviceTypeLabel, cardboard.DeviceType);

    if (GUI.changed) {
      EditorUtility.SetDirty(cardboard);
    }
  }

  static CardboardEditor() {
    EditorUserBuildSettings.activeBuildTargetChanged += CheckGraphicsAPI;
  }

  [PostProcessBuild]
  public static void CheckGraphicsAPI(BuildTarget target, string path) {
    CheckGraphicsAPI();
  }

  private static void CheckGraphicsAPI() {
#if UNITY_IOS
#if UNITY_5 || UNITY_4_6 && !UNITY_4_6_1 && !UNITY_4_6_2
#if UNITY_5
    var iOSBuildTarget = BuildTarget.iOS;
    var iOSGraphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS);
    bool isOpenGL = true;
    foreach (var device in iOSGraphicsAPIs) {
      isOpenGL &= (device == GraphicsDeviceType.OpenGLES2 || device == GraphicsDeviceType.OpenGLES3);
    }
#else
    var iOSBuildTarget = BuildTarget.iPhone;
    bool isOpenGL = PlayerSettings.targetIOSGraphics == TargetIOSGraphics.OpenGLES_2_0
        || PlayerSettings.targetIOSGraphics == TargetIOSGraphics.OpenGLES_3_0;
#endif  // UNITY_5
    if (EditorUserBuildSettings.activeBuildTarget == iOSBuildTarget
        && !Application.isPlaying
        && Object.FindObjectOfType<Cardboard>() != null
        && !isOpenGL) {
      Debug.LogWarning("iOS Graphics API should be set to OpenGL for best " +
            "distortion-correction performance in Cardboard.");
    }
#endif  // UNITY_5 || UNITY_4_6 && !UNITY_4_6_1 && !UNITY_4_6_2
#endif  // UNITY_IOS
  }
}

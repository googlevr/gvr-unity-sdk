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

using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

[CustomEditor(typeof(Cardboard))]
[InitializeOnLoad]
public class CardboardEditor : Editor {
#if UNITY_IOS
  GUIContent syncWithCardboardLabel = new GUIContent("Sync with Cardboard App",
      "Enables the 'Sync with Google Cardboard' slider in the viewer settings dialog.");
#endif

  GUIContent distortionCorrectionLabel = new GUIContent("Distortion Correction",
      "Whether distortion correction is performed the SDK.");

  GUIContent vrModeLabel = new GUIContent("VR Mode Enabled",
      "Sets whether VR mode is enabled.");

  GUIContent alignmentMarkerLabel = new GUIContent("Alignment Marker",
      "Whether to draw the alignment marker. The marker is a vertical line that splits " +
      "the viewport in half, designed to help users align the screen with the Cardboard.");

  GUIContent settingsButtonLabel = new GUIContent("Settings Button",
      "Whether to draw the settings button. The settings button opens the " +
      "Google Cardboard app to allow the user to  configure their individual " +
      "settings and Cardboard headset parameters.");

  GUIContent autoDriftCorrectionLabel = new GUIContent("Auto Drift Correction",
      "When enabled, drift in the gyro readings is estimated and removed.");

  GUIContent tapIsTriggerLabel = new GUIContent("Tap Is Trigger",
      "Whether screen taps are treated as trigger events.");

  GUIContent neckModelScaleLabel = new GUIContent("Neck Model Scale",
      "The scale factor of the builtin neck model [0..1].  To disable, set to 0.");

  GUIContent editorSettingsLabel = new GUIContent("Editor Mock Settings",
      "Controls for the in-editor emulation of Cardboard.");

  GUIContent autoUntiltHeadLabel = new GUIContent("Auto Untilt Head",
      "When enabled, just release Ctrl to untilt the head.");

  GUIContent simulateDistortionLabel = new GUIContent("Simulate Distortion Correction",
      "Whether to perform distortion correction in the editor.");

  GUIContent screenSizeLabel = new GUIContent("Screen Size",
      "The screen size to emulate.");

  GUIContent deviceTypeLabel = new GUIContent("Device Type",
      "The Cardboard device type to emulate.");

  public override void OnInspectorGUI() {
    GUI.changed = false;

    Cardboard cardboard = (Cardboard)target;

#if UNITY_IOS
    cardboard.SyncWithCardboardApp =
        EditorGUILayout.Toggle(syncWithCardboardLabel, cardboard.SyncWithCardboardApp);
#endif
    cardboard.VRModeEnabled =
        EditorGUILayout.Toggle(vrModeLabel, cardboard.VRModeEnabled);
    cardboard.DistortionCorrection =
        EditorGUILayout.Toggle(distortionCorrectionLabel, cardboard.DistortionCorrection);
    cardboard.EnableAlignmentMarker =
        EditorGUILayout.Toggle(alignmentMarkerLabel, cardboard.EnableAlignmentMarker);
    cardboard.EnableSettingsButton =
        EditorGUILayout.Toggle(settingsButtonLabel, cardboard.EnableSettingsButton);
    cardboard.AutoDriftCorrection =
        EditorGUILayout.Toggle(autoDriftCorrectionLabel, cardboard.AutoDriftCorrection);
    cardboard.TapIsTrigger =
        EditorGUILayout.Toggle(tapIsTriggerLabel, cardboard.TapIsTrigger);
    cardboard.NeckModelScale =
        EditorGUILayout.Slider(neckModelScaleLabel, cardboard.NeckModelScale, 0, 1);

    EditorGUILayout.Separator();

    EditorGUILayout.LabelField(editorSettingsLabel);

    cardboard.autoUntiltHead =
        EditorGUILayout.Toggle(autoUntiltHeadLabel, cardboard.autoUntiltHead);
    cardboard.simulateDistortionCorrection =
        EditorGUILayout.Toggle(simulateDistortionLabel, cardboard.simulateDistortionCorrection);
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
    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone
        && !Application.isPlaying
        && Object.FindObjectOfType<Cardboard>() != null
        && PlayerSettings.targetIOSGraphics != TargetIOSGraphics.OpenGLES_2_0
        && PlayerSettings.targetIOSGraphics != TargetIOSGraphics.OpenGLES_3_0) {
      Debug.LogWarning("iOS Graphics API should be set to OpenGL for best distortion-"
        + "correction performance in Cardboard.");
    }
  }
}

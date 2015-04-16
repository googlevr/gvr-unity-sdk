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

[CustomEditor(typeof(Cardboard))]
public class CardboardEditor : Editor {
  GUIContent tapIsTriggerLabel = new GUIContent("Tap Is Trigger",
    "Allow screen taps and mouse clicks to emulate the magnet trigger.");

  GUIContent alignmentMarkerLabel = new GUIContent("Alignment Marker",
    "Whether to draw the alignment marker. The marker is a vertical line that splits " +
    "the viewport in half, designed to help users align the screen with the Cardboard.");

  GUIContent settingsButtonLabel = new GUIContent("Settings Button",
    "Whether to draw the settings button. The settings button opens the " +
    "Google Cardboard app to allow the user to  configure their individual " +
    "settings and Cardboard headset parameters.");

  GUIContent vrModeEnabledLabel = new GUIContent("VR Mode Enabled",
    "Explicitly set whether VR mode is enabled.  Clears the Auto Enable VR setting.");

  GUIContent neckModelScaleLabel = new GUIContent("Neck Model Scale",
    "The scale factor of the builtin neck model [0..1].  To disable, set to 0.");

  GUIContent backButtonExitsAppLabel = new GUIContent("Back Button Exits App",
    "Whether tapping the back button exits the app.");

  GUIContent autoDriftCorrectionLabel = new GUIContent("Auto Drift Correction",
    "When enabled, drift in the gyro readings is estimated and removed.  Currently only " +
    "works on Android.");

  GUIContent editorSettingsLabel = new GUIContent("Editor Mock Settings",
    "Controls for the in-editor emulation of Cardboard.");

  GUIContent autoUntiltHeadLabel = new GUIContent("Auto Untilt Head",
    "When enabled, just release Ctrl to untilt the head.");

  GUIContent screenSizeLabel = new GUIContent("Screen Size",
    "The screen size to emulate.");

  GUIContent deviceTypeLabel = new GUIContent("Device Type",
    "The Cardboard device type to emulate.");

  GUIContent simulateDistortionLabel = new GUIContent("Simulate Distortion Correction",
    "Whether to perform distortion correction in the editor.");

  public override void OnInspectorGUI() {
    GUI.changed = false;

    DrawDefaultInspector();

    Cardboard cardboard = (Cardboard)target;

    bool newTapIsTrigger = EditorGUILayout.Toggle(tapIsTriggerLabel, cardboard.TapIsTrigger);
    if (newTapIsTrigger != cardboard.TapIsTrigger) {
        cardboard.TapIsTrigger = newTapIsTrigger;
    }

    bool newEnableAlignmentMarkder =
      EditorGUILayout.Toggle(alignmentMarkerLabel, cardboard.EnableAlignmentMarker);
    if (newEnableAlignmentMarkder != cardboard.EnableAlignmentMarker) {
      cardboard.EnableAlignmentMarker = newEnableAlignmentMarkder;
    }

    bool newEnableSettingsButton =
      EditorGUILayout.Toggle(settingsButtonLabel, cardboard.EnableSettingsButton);
    if (newEnableSettingsButton != cardboard.EnableSettingsButton) {
      cardboard.EnableSettingsButton = newEnableSettingsButton;
    }

    bool newAutoDriftCorrection =
      EditorGUILayout.Toggle(autoDriftCorrectionLabel, cardboard.AutoDriftCorrection);
    if (newAutoDriftCorrection != cardboard.AutoDriftCorrection) {
      cardboard.AutoDriftCorrection = newAutoDriftCorrection;
    }

    bool newVRModeEnabled = EditorGUILayout.Toggle(vrModeEnabledLabel, cardboard.VRModeEnabled);
    if (newVRModeEnabled != cardboard.VRModeEnabled) {
      cardboard.VRModeEnabled = newVRModeEnabled;
    }

    float newNeckModelScale = EditorGUILayout.Slider(neckModelScaleLabel,
                                                     cardboard.NeckModelScale, 0, 1);
    if (!Mathf.Approximately(newNeckModelScale, cardboard.NeckModelScale)) {
      cardboard.NeckModelScale = newNeckModelScale;
    }

    cardboard.BackButtonExitsApp =
      EditorGUILayout.Toggle(backButtonExitsAppLabel, cardboard.BackButtonExitsApp);

    EditorGUILayout.Separator();

    EditorGUILayout.LabelField(editorSettingsLabel);

    cardboard.autoUntiltHead = EditorGUILayout.Toggle(autoUntiltHeadLabel,
                                                      cardboard.autoUntiltHead);

    cardboard.simulateDistortionCorrection =
        EditorGUILayout.Toggle(simulateDistortionLabel, cardboard.simulateDistortionCorrection);

    cardboard.screenSize = (CardboardProfile.ScreenSizes)
      EditorGUILayout.EnumPopup(screenSizeLabel, cardboard.screenSize);

    cardboard.deviceType = (CardboardProfile.DeviceTypes)
      EditorGUILayout.EnumPopup(deviceTypeLabel, cardboard.deviceType);

    if (GUI.changed) {
      EditorUtility.SetDirty(cardboard);
    }

    if (EditorApplication.isPlaying) {
      bool newInCardboard = EditorGUILayout.Toggle("Is In Cardboard", cardboard.InCardboard);
      if (newInCardboard != cardboard.InCardboard) {
        cardboard.SetInCardboard(newInCardboard); // Takes effect at end of frame.
      }
    }
  }
}

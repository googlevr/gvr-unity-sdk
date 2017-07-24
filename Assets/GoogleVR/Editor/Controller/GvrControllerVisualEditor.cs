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

using UnityEngine;
using UnityEditor;
using System.Collections;

/// Custom editor for GvrControllerVisual.
/// Enhances the visualization of the displayState and ensures that it can only be edited
/// if the application isn't playing or if readControllerState is turned off.
[CustomEditor(typeof(GvrControllerVisual)), CanEditMultipleObjects]
public class GvrControllerVisualEditor : Editor {
  private SerializedProperty attachmentPrefabs;
  private SerializedProperty touchPadColor;
  private SerializedProperty appButtonColor;
  private SerializedProperty systemButtonColor;
  private SerializedProperty readControllerState;
  private SerializedProperty displayState;

  private GUIStyle displayStateHeaderStyle;
  private GUIContent displayStateHeaderContent;
  private float displayStateHeaderHeight;

  private const string DISPLAY_STATE_HEADER_TEXT = "DisplayState:";
  private const string DISPLAY_STATE_ITEM_PREFIX = "• ";
  private const int DISPLAY_STATE_HEADER_FONT_SIZE_OFFSET = 2;

  private const string ATTACHMENT_PREFABS_PROP_NAME = "attachmentPrefabs";
  private const string TOUCH_PAD_COLOR_PROP_NAME = "touchPadColor";
  private const string APP_BUTTON_COLOR_PROP_NAME = "appButtonColor";
  private const string SYSTEM_BUTTON_COLOR_PROP_NAME = "systemButtonColor";
  private const string READ_CONTROLLER_STATE_PROP_NAME = "readControllerState";
  private const string DISPLAY_STATE_PROP_NAME = "displayState";

  void OnEnable() {
    attachmentPrefabs = serializedObject.FindProperty(ATTACHMENT_PREFABS_PROP_NAME);
    touchPadColor = serializedObject.FindProperty(TOUCH_PAD_COLOR_PROP_NAME);
    appButtonColor = serializedObject.FindProperty(APP_BUTTON_COLOR_PROP_NAME);
    systemButtonColor = serializedObject.FindProperty(SYSTEM_BUTTON_COLOR_PROP_NAME);
    readControllerState = serializedObject.FindProperty(READ_CONTROLLER_STATE_PROP_NAME);
    displayState = serializedObject.FindProperty(DISPLAY_STATE_PROP_NAME);
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();

    CreateStylesAndContent();

    // Show all properties except for display state.
    EditorGUILayout.PropertyField(attachmentPrefabs, true);
    EditorGUILayout.PropertyField(touchPadColor);
    EditorGUILayout.PropertyField(appButtonColor);
    EditorGUILayout.PropertyField(systemButtonColor);
    EditorGUILayout.PropertyField(readControllerState);

    // Determine if the display state can currently be edited in the inspector.
    bool allowEditDisplayState = !readControllerState.boolValue || !Application.isPlaying;

    if (!allowEditDisplayState) {
      // Prevents editing the display state in the inspector.
      GUI.enabled = false;
    }

    Rect displayStateRect = EditorGUILayout.BeginVertical();
    GUI.Box(displayStateRect, "");

    // Show the display state header.
    EditorGUILayout.LabelField(displayStateHeaderContent,
      displayStateHeaderStyle,
      GUILayout.Height(displayStateHeaderHeight));

    // Indent the display state properties.
    EditorGUI.indentLevel++;

    // Iterate through the child properties of the displayState property.
    SerializedProperty iter = displayState.Copy();
    SerializedProperty nextElement = displayState.Copy();
    bool hasNextElement = nextElement.Next(false);

    iter.NextVisible(true);
    do {
      // It iter is the same as nextElement, then the iter has moved beyond the children of the
      // display state which means it has finished showing the display state.
      if (hasNextElement && SerializedProperty.EqualContents(nextElement, iter)) {
        break;
      }

      GUIContent content = new GUIContent(DISPLAY_STATE_ITEM_PREFIX + iter.displayName);
      EditorGUILayout.PropertyField(iter, content);
    } while (iter.NextVisible(false));

    // End the vertical region and draw the box.
    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.EndVertical();

    // Reset GUI.enabled.
    if (!allowEditDisplayState) {
      GUI.enabled = true;
    }

    serializedObject.ApplyModifiedProperties();
  }

  private void CreateStylesAndContent() {
    if (displayStateHeaderContent == null) {
      displayStateHeaderContent = new GUIContent(DISPLAY_STATE_HEADER_TEXT);
    }

    if (displayStateHeaderStyle == null) {
      displayStateHeaderStyle = new GUIStyle(EditorStyles.boldLabel);

      displayStateHeaderStyle.fontSize =
        displayStateHeaderStyle.font.fontSize + DISPLAY_STATE_HEADER_FONT_SIZE_OFFSET;

      displayStateHeaderHeight = displayStateHeaderStyle.CalcSize(displayStateHeaderContent).y;
    }
  }
}

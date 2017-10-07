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

/// Custom editor for GvrLaserVisual.
/// Shows the relationship between the shrinkLaser property and other related properties.
[CustomEditor(typeof(GvrLaserVisual)), CanEditMultipleObjects]
public class GvrLaserVisualEditor : Editor {
  private SerializedProperty laserColor;
  private SerializedProperty laserColorEnd;
  private SerializedProperty maxLaserDistance;
  private SerializedProperty shrinkLaser;
  private SerializedProperty shrunkScale;
  private SerializedProperty beginShrinkAngleDegrees;
  private SerializedProperty endShrinkAngleDegrees;
  private SerializedProperty lerpSpeed;
  private SerializedProperty lerpThreshold;
  private SerializedProperty reticle;
  private SerializedProperty controller;

  public const string LASER_COLOR_PROP_NAME = "laserColor";
  public const string LASER_COLOR_END_PROP_NAME = "laserColorEnd";
  public const string MAX_LASER_DISTANCE_PROP_NAME = "maxLaserDistance";
  public const string SHRINK_LASER_PROP_NAME = "shrinkLaser";
  public const string SHURNK_SCALE_PROP_NAME = "shrunkScale";
  public const string BEGIN_SHRINKING_ANGLE_DEGREES_PROP_NAME = "beginShrinkAngleDegrees";
  public const string END_SHRINKING_ANGLE_DEGREES_PROP_NAME = "endShrinkAngleDegrees";
  public const string LERP_SPEED_PROP_NAME = "lerpSpeed";
  public const string LERP_THRESHOLD_PROP_NAME = "lerpThreshold";
  public const string RETICLE_PROP_NAME = "reticle";
  public const string CONTROLLER_PROP_NAME = "controller";

  private const string ITEM_PREFIX = "• ";

  void OnEnable() {
    laserColor = serializedObject.FindProperty(LASER_COLOR_PROP_NAME);
    laserColorEnd = serializedObject.FindProperty(LASER_COLOR_END_PROP_NAME);
    maxLaserDistance = serializedObject.FindProperty(MAX_LASER_DISTANCE_PROP_NAME);
    shrinkLaser = serializedObject.FindProperty(SHRINK_LASER_PROP_NAME);
    shrunkScale = serializedObject.FindProperty(SHURNK_SCALE_PROP_NAME);
    beginShrinkAngleDegrees = serializedObject.FindProperty(BEGIN_SHRINKING_ANGLE_DEGREES_PROP_NAME);
    endShrinkAngleDegrees = serializedObject.FindProperty(END_SHRINKING_ANGLE_DEGREES_PROP_NAME);
    lerpSpeed = serializedObject.FindProperty(LERP_SPEED_PROP_NAME);
    lerpThreshold = serializedObject.FindProperty(LERP_THRESHOLD_PROP_NAME);
    reticle = serializedObject.FindProperty(RETICLE_PROP_NAME);
    controller = serializedObject.FindProperty(CONTROLLER_PROP_NAME);
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();

    // Add clickable script field, as would have been provided by DrawDefaultInspector()
    MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup();

    // Show properties for the laser visual.
    EditorGUILayout.PropertyField(reticle);
    EditorGUILayout.PropertyField(controller);
    EditorGUILayout.PropertyField(laserColor);
    EditorGUILayout.PropertyField(laserColorEnd);
    EditorGUILayout.PropertyField(maxLaserDistance);
    EditorGUILayout.PropertyField(lerpSpeed);
    EditorGUILayout.PropertyField(lerpThreshold);
    EditorGUILayout.PropertyField(shrinkLaser);

    // Show properties for shrinking animation. Only enabled if shrinkLaser is enabled.
    if (!shrinkLaser.boolValue) {
      GUI.enabled = false;
    }

    EditorGUI.indentLevel++;
    Rect shrinkLaserRect = EditorGUILayout.BeginVertical();
    shrinkLaserRect = EditorGUI.IndentedRect(shrinkLaserRect);
    GUI.Box(shrinkLaserRect, "");
    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(shrunkScale,
      new GUIContent(ITEM_PREFIX + shrunkScale.displayName));

    EditorGUILayout.PropertyField(beginShrinkAngleDegrees,
      new GUIContent(ITEM_PREFIX + beginShrinkAngleDegrees.displayName));

    EditorGUILayout.PropertyField(endShrinkAngleDegrees,
      new GUIContent(ITEM_PREFIX + endShrinkAngleDegrees.displayName));

    EditorGUILayout.Space();
    EditorGUILayout.EndVertical();
    EditorGUI.indentLevel--;

    if (!shrinkLaser.boolValue) {
      GUI.enabled = true;
    }

    EditorGUILayout.Space();

    serializedObject.ApplyModifiedProperties();
  }
}

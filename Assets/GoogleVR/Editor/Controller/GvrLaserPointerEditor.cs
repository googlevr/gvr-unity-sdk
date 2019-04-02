//-----------------------------------------------------------------------
// <copyright file="GvrLaserPointerEditor.cs" company="Google Inc.">
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

using UnityEditor;
using UnityEngine;

/// <summary>Custom editor for `GvrLaserPointer`.</summary>
/// <remarks>
/// Adds buttons that allows user's to set the recommended default values for the different
/// raycast modes.
/// </remarks>
[CustomEditor(typeof(GvrLaserPointer)), CanEditMultipleObjects]
public class GvrLaserPointerEditor : Editor
{
    /// <summary>The name of the **raycast mode** property.</summary>
    public const string RAYCAST_MODE_PROP_NAME = "raycastMode";

    /// <summary>The name of the **override pointer camera** property.</summary>
    public const string OVERRIDE_POINTER_CAMERA_PROP_NAME = "overridePointerCamera";

    /// <summary>The name of the **max pointer distance** property.</summary>
    public const string MAX_POINTER_DISTANCE_PROP_NAME = "maxPointerDistance";

    /// <summary>The name of the **default reticle distance** property.</summary>
    public const string DEFAULT_RETICLE_DISTANCE_PROP_NAME = "defaultReticleDistance";

    /// <summary>The name of the **override camera ray intersection distance** property.</summary>
    public const string RAY_INTERSECTION_PROP_NAME = "overrideCameraRayIntersectionDistance";

    /// <summary>The name of the **draw debug rays** property.</summary>
    public const string DRAW_DEBUG_RAYS_PROP_NAME = "drawDebugRays";

    private SerializedProperty mode;
    private SerializedProperty overridePointerCamera;
    private SerializedProperty maxPointerDistance;
    private SerializedProperty defaultReticleDistance;
    private SerializedProperty rayIntersection;
    private SerializedProperty drawDebugRays;

    /// @cond
    /// <summary>A builtin method of the `Editor` class.</summary>
    /// <remarks>Implement this function to make a custom inspector.</remarks>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Add clickable script field, as would have been provided by DrawDefaultInspector()
        MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        Rect defaultsRect = EditorGUILayout.BeginVertical();
        GUI.Box(defaultsRect, /* No label. */"");

        GUILayout.Space(3.0f);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Hybrid"))
        {
            SetDefaultsForRaycastMode(GvrBasePointer.RaycastMode.Hybrid);
        }

        if (GUILayout.Button("Camera"))
        {
            SetDefaultsForRaycastMode(GvrBasePointer.RaycastMode.Camera);
        }

        if (GUILayout.Button("Direct"))
        {
            SetDefaultsForRaycastMode(GvrBasePointer.RaycastMode.Direct);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Use the above Raycast Mode buttons to reset the following " +
        "properties to their recommended values.\n\n" +
        "GvrLaserPointer:\n" +
        "   • " + mode.displayName + "\n" +
        "   • " + rayIntersection.displayName + "\n\n" +
        "GvrLaserVisual:\n" +
        "   • Max Laser Distance\n" +
        "   • Shrink Laser\n", MessageType.Info);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(maxPointerDistance);
        EditorGUILayout.PropertyField(defaultReticleDistance);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Advanced:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(overridePointerCamera);
        EditorGUILayout.PropertyField(rayIntersection);
        EditorGUILayout.PropertyField(drawDebugRays);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        mode = serializedObject.FindProperty(RAYCAST_MODE_PROP_NAME);
        overridePointerCamera = serializedObject.FindProperty(OVERRIDE_POINTER_CAMERA_PROP_NAME);
        maxPointerDistance = serializedObject.FindProperty(MAX_POINTER_DISTANCE_PROP_NAME);
        defaultReticleDistance = serializedObject.FindProperty(DEFAULT_RETICLE_DISTANCE_PROP_NAME);
        rayIntersection = serializedObject.FindProperty(RAY_INTERSECTION_PROP_NAME);
        drawDebugRays = serializedObject.FindProperty(DRAW_DEBUG_RAYS_PROP_NAME);
    }

    /// @endcond
    /// <summary>Returns a given raycast mode's settings to their default values.</summary>
    /// <param name="raycastMode">The `RaycastMode` to set default settings for.</param>
    private void SetDefaultsForRaycastMode(GvrBasePointer.RaycastMode raycastMode)
    {
        switch (raycastMode)
        {
            case GvrBasePointer.RaycastMode.Hybrid:
                mode.intValue = (int)raycastMode;
                rayIntersection.floatValue = GvrVRHelpers.GetRayIntersection(raycastMode);
                SetPropertiesForVisual(GvrVRHelpers.GetShrinkLaser(raycastMode),
                                       GvrVRHelpers.GetRecommendedMaxLaserDistance(raycastMode));
                break;
            case GvrBasePointer.RaycastMode.Camera:
                mode.intValue = (int)raycastMode;
                rayIntersection.floatValue = GvrVRHelpers.GetRayIntersection(raycastMode);
                SetPropertiesForVisual(GvrVRHelpers.GetShrinkLaser(raycastMode),
                                       GvrVRHelpers.GetRecommendedMaxLaserDistance(raycastMode));
                break;
            case GvrBasePointer.RaycastMode.Direct:
                mode.intValue = (int)raycastMode;
                rayIntersection.floatValue = GvrVRHelpers.GetRayIntersection(raycastMode);
                SetPropertiesForVisual(GvrVRHelpers.GetShrinkLaser(raycastMode),
                                       GvrVRHelpers.GetRecommendedMaxLaserDistance(raycastMode));
                break;
            default:
                Debug.LogError("Trying to set defaults for invalid Raycast Mode: " + raycastMode);
                return;
        }
    }

    private void SetPropertiesForVisual(bool shrinkLaser, float maxLaserDistance)
    {
        foreach (Object obj in serializedObject.targetObjects)
        {
            GvrLaserVisual laserVisual = (obj as MonoBehaviour).GetComponent<GvrLaserVisual>();
            if (laserVisual != null)
            {
                SerializedObject serializedLaserVisual = new SerializedObject(laserVisual);

                SerializedProperty serializedShrinkLaser =
                    serializedLaserVisual.FindProperty(GvrLaserVisualEditor.SHRINK_LASER_PROP_NAME);
                serializedShrinkLaser.boolValue = shrinkLaser;

                SerializedProperty serializedMaxLaserDistance =
                    serializedLaserVisual.FindProperty(
                        GvrLaserVisualEditor.MAX_LASER_DISTANCE_PROP_NAME);
                serializedMaxLaserDistance.floatValue = maxLaserDistance;

                serializedLaserVisual.ApplyModifiedProperties();
            }
        }
    }
}

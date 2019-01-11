//-----------------------------------------------------------------------
// <copyright file="GvrPointerScrollInputEditor.cs" company="Google Inc.">
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

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomPropertyDrawer(typeof(GvrPointerScrollInput), true)]
public class GvrPointerScrollInputEditor : PropertyDrawer
{
    private bool isExpanded = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        int rows = GetNumRows(property);
        float totalHeight = position.height;
        float rowHeight = totalHeight / rows;
        position.height = rowHeight;

        isExpanded = EditorGUI.Foldout(position, isExpanded, label);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;

            // Inertia property.
            SerializedProperty inertia =
                property.FindPropertyRelative(GvrPointerScrollInput.PROPERTY_NAME_INERTIA);

            position.y += rowHeight;
            EditorGUI.PropertyField(position, inertia);

            if (inertia.boolValue)
            {
                EditorGUI.indentLevel++;

                // Deceleration rate property.
                SerializedProperty decelerationRate =
                    property.FindPropertyRelative(GvrPointerScrollInput.PROPERTY_NAME_DECELERATION_RATE);

                position.y += rowHeight;
                EditorGUI.PropertyField(position, decelerationRate);

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * GetNumRows(property);
    }

    private int GetNumRows(SerializedProperty property)
    {
        SerializedProperty inertia =
            property.FindPropertyRelative(GvrPointerScrollInput.PROPERTY_NAME_INERTIA);

        if (!isExpanded)
        {
            return 1;
        }
        else if (!inertia.boolValue)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}

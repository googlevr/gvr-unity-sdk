//-----------------------------------------------------------------------
// <copyright file="GvrControllerTooltipsSimple.cs" company="Google Inc.">
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

// The controller is not available for versions of Unity without the
// GVR native integration.

using System.Collections;
using UnityEngine;

/// <summary>A lightweight tooltip designed to minimize draw calls.</summary>
[ExecuteInEditMode]
[HelpURL("https://developers.google.com/vr/reference/unity/class/GvrControllerTooltipsSimple")]
public class GvrControllerTooltipsSimple : MonoBehaviour, IGvrArmModelReceiver
{
    private MeshRenderer tooltipRenderer;
    private MaterialPropertyBlock materialPropertyBlock;
    private int colorId;

    /// <summary>Gets or sets the arm model used to position the controller.</summary>
    /// <value>The arm model used to position the controller.</value>
    public GvrBaseArmModel ArmModel { get; set; }

    /// <summary>Updates the tooltip visualation based on the arm model.</summary>
    protected void OnVisualUpdate()
    {
        float alpha = ArmModel != null ? ArmModel.TooltipAlphaValue : 1.0f;
        materialPropertyBlock.SetColor(colorId, new Color(1, 1, 1, alpha));
        tooltipRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
    }

    private void OnDisable()
    {
        GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            OnVisualUpdate();
        }
    }

    private void Initialize()
    {
        if (tooltipRenderer == null)
        {
            tooltipRenderer = GetComponent<MeshRenderer>();
        }

        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        colorId = Shader.PropertyToID("_Color");
    }

    private void OnPostControllerInputUpdated()
    {
        OnVisualUpdate();
    }
}

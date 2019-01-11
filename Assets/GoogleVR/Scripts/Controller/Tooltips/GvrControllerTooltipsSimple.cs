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

using UnityEngine;
using System.Collections;

/// A lightweight tooltip designed to minimize draw calls.
[ExecuteInEditMode]
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrControllerTooltipsSimple")]
public class GvrControllerTooltipsSimple : MonoBehaviour, IGvrArmModelReceiver
{
    private MeshRenderer tooltipRenderer;

    /// <summary>Arm model used to position the controller.</summary>
    public GvrBaseArmModel ArmModel { get; set; }

    private MaterialPropertyBlock materialPropertyBlock;
    private int colorId;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
    }

    void OnDisable()
    {
        GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
    }

    void OnValidate()
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

    /// <summary>Updates the tooltip visualation based on the arm model.</summary>
    protected void OnVisualUpdate()
    {
        float alpha = ArmModel != null ? ArmModel.TooltipAlphaValue : 1.0f;
        materialPropertyBlock.SetColor(colorId, new Color(1, 1, 1, alpha));
        tooltipRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}

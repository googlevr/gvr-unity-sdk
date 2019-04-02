//-----------------------------------------------------------------------
// <copyright file="GvrDropdown.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0(the "License");
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

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Dropdown UI component that works with the GvrRaycasters.</summary>
/// <remarks>
/// This is a workaround for the fact that the Dropdown component doesn't work with custom
/// raycasters because it internally adds two GraphicRaycasters.
/// </remarks>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrDropdown")]
public class GvrDropdown : Dropdown
{
    private GameObject currentBlocker;

    /// <inheritdoc/>
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        FixTemplateAndBlockerRaycasters();
    }

    /// <inheritdoc/>
    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        FixTemplateAndBlockerRaycasters();
    }

    /// <inheritdoc/>
    protected override GameObject CreateBlocker(Canvas rootCanvas)
    {
        currentBlocker = base.CreateBlocker(rootCanvas);
        return currentBlocker;
    }

    /// <inheritdoc/>
    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject dropdown = base.CreateDropdownList(template);
        FixRaycaster(dropdown, false);
        return dropdown;
    }

    private static T GetOrAddComponent<T>(GameObject go,
                                          out bool addedComponent) where T : Component
    {
        T comp = go.GetComponent<T>();
        addedComponent = false;
        if (!comp)
        {
            comp = go.AddComponent<T>();
            addedComponent = true;
        }

        return comp;
    }

    private void FixTemplateAndBlockerRaycasters()
    {
        if (template != null)
        {
            FixRaycaster(template.gameObject, false);
        }

        FixRaycaster(currentBlocker, true);
    }

    private void FixRaycaster(GameObject go, bool shouldCopyProperties)
    {
        if (go == null)
        {
            return;
        }

        GraphicRaycaster oldRaycaster = go.GetComponent<GraphicRaycaster>();
        Destroy(oldRaycaster);

        bool addedRaycaster;
        GvrPointerGraphicRaycaster raycaster;
        raycaster = GetOrAddComponent<GvrPointerGraphicRaycaster>(go, out addedRaycaster);

        if (shouldCopyProperties)
        {
            GvrPointerGraphicRaycaster templateRaycaster = GetTemplateRaycaster();
            if (addedRaycaster && templateRaycaster != null)
            {
                CopyRaycasterProperties(templateRaycaster, raycaster);
            }
        }
    }

    private GvrPointerGraphicRaycaster GetTemplateRaycaster()
    {
        if (template == null)
        {
            return null;
        }

        return template.GetComponent<GvrPointerGraphicRaycaster>();
    }

    private void CopyRaycasterProperties(GvrPointerGraphicRaycaster source,
                                         GvrPointerGraphicRaycaster dest)
    {
        if (source == null || dest == null)
        {
            return;
        }

        dest.blockingMask = source.blockingMask;
        dest.blockingObjects = source.blockingObjects;
        dest.ignoreReversedGraphics = source.ignoreReversedGraphics;
    }
}

//-----------------------------------------------------------------------
// <copyright file="ProfilingSessionSummaryUI.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI element that displays a summary of a profiling session.
/// </summary>
internal class ProfilingSessionSummaryUI : MonoBehaviour
{
    /// <summary>
    /// A prefab for displaying an individual profiling run summary from the profiling session.
    /// </summary>
    public GameObject m_profilingRunSummaryPrefab = null;

    /// <summary>
    /// The line height of a profiling run summary.
    /// </summary>
    public float m_runSummaryLineHeight = 50.0f;

    /// <summary>
    /// The x-offset of a profiling run summary.
    /// </summary>
    public float m_runSumaryXOffset = 20.0f;

#if UNITY_5_3_OR_NEWER
    /// <summary>
    /// The list of profiling run summaries to display.
    /// </summary>
    private List<MemoryProfilerApplicationController.MemoryProfilingRunSummary> m_profilingRunSummaryList;

    /// <summary>
    /// Displays a list of profiling summaries.
    /// </summary>
    /// <param name="profilingRunSummaryList">The profiling summary list to display.</param>
    public void DisplaySummaries(List<MemoryProfilerApplicationController.MemoryProfilingRunSummary>
        profilingRunSummaryList)
    {
        m_profilingRunSummaryList = profilingRunSummaryList;
        StartCoroutine(_DisplaySummariesCoroutine());
    }

    /// <summary>
    /// Displays the list of profling run summaries in the UI.
    /// </summary>
    /// <returns>An IEnumerator.</returns>
    private IEnumerator _DisplaySummariesCoroutine()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta =
            new Vector2(rectTransform.sizeDelta.x, m_runSummaryLineHeight * m_profilingRunSummaryList.Count);

        foreach (var summary in m_profilingRunSummaryList)
        {
            RectTransform statusTransform =
                (Instantiate(m_profilingRunSummaryPrefab) as GameObject).GetComponent<RectTransform>();
            statusTransform.SetParent(transform);
            statusTransform.localScale = Vector3.one;

            var sceneProfilingSummaryUI = statusTransform.GetComponent<ProfilingRunSummaryUI>();
            bool memoryViolation;
            bool gcViolation;

            summary.GetViolations(out memoryViolation, out gcViolation);
            ProfilingRunSummaryUI.ImageEnum statusImage = memoryViolation || gcViolation ?
                ProfilingRunSummaryUI.ImageEnum.FAILURE : ProfilingRunSummaryUI.ImageEnum.SUCCESS;

            sceneProfilingSummaryUI.SetState(
                statusImage,
                summary.MemoryProfilingRun.Name,
                summary.MemoryProfilingRun.TotalGarbageGeneratedBytes,
                summary.MemoryProfilingRun.GarbageCollectionCount);

            yield return new WaitForSeconds(.25f);
        }
    }
#endif // UNITY_5_3_OR_NEWER
}

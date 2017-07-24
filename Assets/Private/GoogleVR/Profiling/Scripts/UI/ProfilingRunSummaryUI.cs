//-----------------------------------------------------------------------
// <copyright file="ProfilingRunSummaryUI.cs" company="Google">
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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying the summary of a profiling run.
/// </summary>
public class ProfilingRunSummaryUI : MonoBehaviour
{
    /// <summary>
    /// The image indicating success or failure of the profiling run.
    /// </summary>
    public Image m_statusImage;

    /// <summary>
    /// The sprite for success.
    /// </summary>
    public Sprite m_successSprite;

    /// <summary>
    /// The sprite for failure.
    /// </summary>
    public Sprite m_failureSprite;

    /// <summary>
    /// The UI component that displays the profiling run summary.
    /// </summary>
    public Text m_runSummaryText;

    /// <summary>
    /// The duration of the text fade-in effect.
    /// </summary>
    public float m_fadeInDuration = 2.0f;

    /// <summary>
    /// An enumeration of the types of images ProfilingRunSummaryUI displays.
    /// </summary>
    public enum ImageEnum
    {
        NONE = 0,
        SUCCESS = 1,
        FAILURE = 2,
    }

#if UNITY_5_3_OR_NEWER
    /// <summary>
    /// The Unity start method.
    /// </summary>
    public void Start()
    {
        m_statusImage.color = new Color(m_statusImage.color.r, m_statusImage.color.b, m_statusImage.color.g, 0.01f);
        m_runSummaryText.color =
            new Color(m_runSummaryText.color.r, m_runSummaryText.color.b, m_runSummaryText.color.g, 0.01f);
        m_statusImage.CrossFadeAlpha(255.0f, m_fadeInDuration, true);
        m_runSummaryText.CrossFadeAlpha(255.0f, m_fadeInDuration, true);
    }

    /// <summary>
    /// Sets the state of the profiling run summary component.
    /// </summary>
    /// <param name="imageType">The type of image to display.</param>
    /// <param name="runName">The name of the profiling run.</param>
    /// <param name="garbageGenerated">The garbage generated in this run.</param>
    /// <param name="garbageCollectionCount">The garbage collection count in the run.</param>
    public void SetState(ImageEnum imageType, string runName, uint garbageGenerated, uint garbageCollectionCount)
    {
        if (imageType == ImageEnum.NONE)
        {
            m_statusImage.gameObject.SetActive(false);
        }
        else
        {
            m_statusImage.sprite = imageType == ImageEnum.SUCCESS ? m_successSprite : m_failureSprite;
        }

        m_runSummaryText.text = string.Format("{0} KB ({1} GC) {2}",
            System.Math.Round(garbageGenerated / 1000.0f, 2), garbageCollectionCount, runName);
    }
#endif // UNITY_5_3_OR_NEWER
}

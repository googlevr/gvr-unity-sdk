//-----------------------------------------------------------------------
// <copyright file="SwitchVideos.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleVR.VideoDemo
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A top-level container for different videos which can be loaded or switched between.
    /// </summary>
    public class SwitchVideos : MonoBehaviour
    {
        /// <summary>The local video sample.</summary>
        public GameObject localVideoSample;

        /// <summary>The dash video sample.</summary>
        public GameObject dashVideoSample;

        /// <summary>The pano video sample.</summary>
        public GameObject panoVideoSample;

        /// <summary>Text indicating a library is missing.</summary>
        public Text missingLibText;

        private GameObject[] videoSamples;

        /// <summary>Called by this instance's Awake step.</summary>
        public void Awake()
        {
            videoSamples = new GameObject[3];
            videoSamples[0] = localVideoSample;
            videoSamples[1] = dashVideoSample;
            videoSamples[2] = panoVideoSample;

            string NATIVE_LIBS_MISSING_MESSAGE =
                "Video Support libraries not found or could not be loaded!\n" +
                "Please add the <b>GVRVideoPlayer.unitypackage</b>\n to this project";

            if (missingLibText != null)
            {
                try
                {
                    IntPtr ptr = GvrVideoPlayerTexture.CreateVideoPlayer();
                    if (ptr != IntPtr.Zero)
                    {
                        GvrVideoPlayerTexture.DestroyVideoPlayer(ptr);
                        missingLibText.enabled = false;
                    }
                    else
                    {
                        missingLibText.text = NATIVE_LIBS_MISSING_MESSAGE;
                        missingLibText.enabled = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    missingLibText.text = NATIVE_LIBS_MISSING_MESSAGE;
                    missingLibText.enabled = true;
                }
            }
        }

        /// <summary>Shows the main menu.</summary>
        public void ShowMainMenu()
        {
            ShowSample(-1);
        }

        /// <summary>Called on the Flat Local event.</summary>
        public void OnFlatLocal()
        {
            ShowSample(0);
        }

        /// <summary>Called on the Dash event.</summary>
        public void OnDash()
        {
            ShowSample(1);
        }

        /// <summary>Called on the 360 Video event.</summary>
        public void On360Video()
        {
            ShowSample(2);
        }

        private void ShowSample(int index)
        {
            // If the libs are missing, always show the main menu.
            if (missingLibText != null && missingLibText.enabled)
            {
                index = -1;
            }

            for (int i = 0; i < videoSamples.Length; i++)
            {
                if (videoSamples[i] != null)
                {
                    if (i != index)
                    {
                        if (videoSamples[i].activeSelf)
                        {
                            videoSamples[i].GetComponentInChildren<GvrVideoPlayerTexture>()
                                           .CleanupVideo();
                        }
                    }
                    else
                    {
                        videoSamples[i].GetComponentInChildren<GvrVideoPlayerTexture>()
                            .ReInitializeVideo();
                    }

                    videoSamples[i].SetActive(i == index);
                }
            }

            GetComponent<Canvas>().enabled = index == -1;
        }
    }
}

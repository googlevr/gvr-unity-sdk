//-----------------------------------------------------------------------
// <copyright file="VideoControlsManager.cs" company="Google Inc.">
// Copyright 2016 Google Inc. All rights reserved.
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

namespace GoogleVR.VideoDemo
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Controls for the Video Player.</summary>
    public class VideoControlsManager : MonoBehaviour
    {
        private GameObject pauseSprite;
        private GameObject playSprite;
        private Slider videoScrubber;
        private Slider volumeSlider;
        private GameObject volumeWidget;
        private GameObject settingsPanel;
        private GameObject bufferedBackground;
        private Vector3 basePosition;
        private Text videoPosition;
        private Text videoDuration;

        /// <summary>Gets or sets the player.</summary>
        /// <value>The player.</value>
        public GvrVideoPlayerTexture Player { get; set; }

        /// <summary>Raises the volume up event.</summary>
        public void OnVolumeUp()
        {
            if (Player.CurrentVolume < Player.MaxVolume)
            {
                Player.CurrentVolume += 1;
            }
        }

        /// <summary>Raises the volume down event.</summary>
        public void OnVolumeDown()
        {
            if (Player.CurrentVolume > 0)
            {
                Player.CurrentVolume -= 1;
            }
        }

        /// <summary>Raises the toggle volume event.</summary>
        public void OnToggleVolume()
        {
            bool visible = !volumeWidget.activeSelf;
            volumeWidget.SetActive(visible);

            // close settings if volume opens.
            settingsPanel.SetActive(settingsPanel.activeSelf && !visible);
        }

        /// <summary>Raises the toggle settings event.</summary>
        public void OnToggleSettings()
        {
            bool visible = !settingsPanel.activeSelf;
            settingsPanel.SetActive(visible);

            // close settings if volume opens.
            volumeWidget.SetActive(volumeWidget.activeSelf && !visible);
        }

        /// <summary>Raises the play pause event.</summary>
        public void OnPlayPause()
        {
            bool isPaused = Player.IsPaused;
            if (isPaused)
            {
                Player.Play();
            }
            else
            {
                Player.Pause();
            }

            pauseSprite.SetActive(isPaused);
            playSprite.SetActive(!isPaused);
            CloseSubPanels();
        }

        /// <summary>Sets the volume to the given value.</summary>
        /// <param name="val">The new Volume value.</param>
        public void OnVolumePositionChanged(float val)
        {
            if (Player.VideoReady)
            {
                Debug.Log("Setting current volume to " + val);
                Player.CurrentVolume = (int)val;
            }
        }

        /// <summary>Closes the sub panels.</summary>
        public void CloseSubPanels()
        {
            volumeWidget.SetActive(false);
            settingsPanel.SetActive(false);
        }

        /// <summary>Fade this video canvas in or out.</summary>
        /// <param name="show">
        /// Value `true` if this video should appear, or `false` if it should fade.
        /// </param>
        public void Fade(bool show)
        {
            if (show)
            {
                StartCoroutine(DoAppear());
            }
            else
            {
                StartCoroutine(DoFade());
            }
        }

        private void Awake()
        {
            foreach (Text t in GetComponentsInChildren<Text>())
            {
                if (t.gameObject.name == "curpos_text")
                {
                    videoPosition = t;
                }
                else if (t.gameObject.name == "duration_text")
                {
                    videoDuration = t;
                }
            }

            foreach (RawImage raw in GetComponentsInChildren<RawImage>(true))
            {
                if (raw.gameObject.name == "playImage")
                {
                    playSprite = raw.gameObject;
                }
                else if (raw.gameObject.name == "pauseImage")
                {
                    pauseSprite = raw.gameObject;
                }
            }

            foreach (Slider s in GetComponentsInChildren<Slider>(true))
            {
                if (s.gameObject.name == "video_slider")
                {
                    videoScrubber = s;
                    videoScrubber.maxValue = 100;
                    videoScrubber.minValue = 0;
                    foreach (Image i in videoScrubber.GetComponentsInChildren<Image>())
                    {
                        if (i.gameObject.name == "BufferedBackground")
                        {
                            bufferedBackground = i.gameObject;
                        }
                    }
                }
                else if (s.gameObject.name == "volume_slider")
                {
                    volumeSlider = s;
                }
            }

            foreach (RectTransform obj in GetComponentsInChildren<RectTransform>(true))
            {
                if (obj.gameObject.name == "volume_widget")
                {
                    volumeWidget = obj.gameObject;
                }
                else if (obj.gameObject.name == "settings_panel")
                {
                    settingsPanel = obj.gameObject;
                }
            }
        }

        private void Start()
        {
            foreach (ScrubberEvents s in GetComponentsInChildren<ScrubberEvents>(true))
            {
                s.ControlManager = this;
            }

            if (Player != null)
            {
                Player.Init();
            }
        }

        private void Update()
        {
            if (!Player.VideoReady || Player.IsPaused)
            {
                pauseSprite.SetActive(false);
                playSprite.SetActive(true);
            }
            else if (Player.VideoReady && !Player.IsPaused)
            {
                pauseSprite.SetActive(true);
                playSprite.SetActive(false);
            }

            if (Player.VideoReady)
            {
                if (basePosition == Vector3.zero)
                {
                    basePosition = videoScrubber.handleRect.localPosition;
                }

                videoScrubber.maxValue = Player.VideoDuration;
                videoScrubber.value = Player.CurrentPosition;

                float pct = Player.BufferedPercentage / 100.0f;
                float sx = Mathf.Clamp(pct, 0, 1f);
                bufferedBackground.transform.localScale = new Vector3(sx, 1, 1);
                bufferedBackground.transform.localPosition =
                    new Vector3(basePosition.x - (basePosition.x * sx), 0, 0);

                videoPosition.text = FormatTime(Player.CurrentPosition);
                videoDuration.text = FormatTime(Player.VideoDuration);

                if (volumeSlider != null)
                {
                    volumeSlider.minValue = 0;
                    volumeSlider.maxValue = Player.MaxVolume;
                    volumeSlider.value = Player.CurrentVolume;
                }
            }
            else
            {
                videoScrubber.value = 0;
            }
        }

        private IEnumerator DoAppear()
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            while (cg.alpha < 1.0)
            {
                cg.alpha += Time.deltaTime * 2;
                yield return null;
            }

            cg.interactable = true;
            yield break;
        }

        private IEnumerator DoFade()
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            while (cg.alpha > 0)
            {
                cg.alpha -= Time.deltaTime;
                yield return null;
            }

            cg.interactable = false;
            CloseSubPanels();
            yield break;
        }

        private string FormatTime(long ms)
        {
            int sec = (int)(ms / 1000L);
            int mn = sec / 60;
            sec = sec % 60;
            int hr = mn / 60;
            mn = mn % 60;
            if (hr > 0)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", hr, mn, sec);
            }

            return string.Format("{0:00}:{1:00}", mn, sec);
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="DemoSeeThroughController.cs" company="Google LLC">
// Copyright 2019 Google LLC. All rights reserved.
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

namespace GoogleVR.Beta.Demos
{
    using GoogleVR.Beta;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A demo controller that cycles through all of the see-through modes
    /// as any controller app button is clicked.
    /// </summary>
    public class DemoSeeThroughController : MonoBehaviour
    {
        /// <summary>
        /// A Text component that shows information about how to cycle through see-through modes.
        /// </summary>
        public Text infoText;

        /// <summary>
        /// A Text component that shows the current see-through status.
        /// </summary>
        public Text statusText;

        /// <summary>
        /// A list of GameObjects that should be disabled while see-through
        /// is active.
        /// </summary>
        public GameObject[] hideDuringSeeThrough;

        private void Start()
        {
            UpdateInfoText();
            bool supported = GvrBetaSettings.IsFeatureSupported(GvrBetaFeature.SeeThrough);
            bool enabled = GvrBetaSettings.IsFeatureEnabled(GvrBetaFeature.SeeThrough);
            if (supported && !enabled)
            {
                GvrBetaFeature[] features = new GvrBetaFeature[] { GvrBetaFeature.SeeThrough };
                GvrBetaSettings.RequestFeatures(features, null);
            }
        }

        private void OnApplicationPause(bool didPause)
        {
            if (!didPause)
            {
                UpdateInfoText();
            }
        }

        private void Update()
        {
            foreach (var hand in Gvr.Internal.ControllerUtils.AllHands)
            {
                GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
                if (device.GetButtonUp(GvrControllerButton.App))
                {
                    CycleSeeThroughModes();
                }
            }
        }

        private void OnDestroy()
        {
            // Disable see-through when this scene ends.
            GvrBetaHeadset.SetSeeThroughConfig(GvrBetaSeeThroughCameraMode.Disabled,
                                               GvrBetaSeeThroughSceneType.Virtual);
        }

        private void UpdateInfoText()
        {
            if (GvrBetaSettings.IsFeatureEnabled(GvrBetaFeature.SeeThrough))
            {
                // SeeThrough enabled.
                infoText.text = "Press the App button to cycle see-through modes.";
                statusText.gameObject.SetActive(true);
            }
            else
            {
                // SeeThrough not enabled.
                if (GvrBetaSettings.IsFeatureSupported(GvrBetaFeature.SeeThrough))
                {
                    infoText.text = "See-through is not currently enabled. " +
                                    "Enable in Settings > Beta Features.";
                }
                else
                {
                    infoText.text = "See-through is not supported on this system.";
                }

                statusText.gameObject.SetActive(false);
            }
        }

        private void CycleSeeThroughModes()
        {
            if (!GvrBetaSettings.IsFeatureEnabled(GvrBetaFeature.SeeThrough))
            {
                return;
            }

            GvrBetaSeeThroughCameraMode camMode = GvrBetaHeadset.CameraMode;
            GvrBetaSeeThroughSceneType sceneType = GvrBetaSeeThroughSceneType.Augmented;

            switch (camMode)
            {
                case GvrBetaSeeThroughCameraMode.Disabled:
                    camMode = GvrBetaSeeThroughCameraMode.RawImage;
                    break;
                case GvrBetaSeeThroughCameraMode.RawImage:
                    camMode = GvrBetaSeeThroughCameraMode.ToneMap;
                    break;
                case GvrBetaSeeThroughCameraMode.ToneMap:
                    // When see-through is disabled, use scene type Virtual to restore headpose
                    // offset to 0.
                    sceneType = GvrBetaSeeThroughSceneType.Virtual;
                    camMode = GvrBetaSeeThroughCameraMode.Disabled;
                    break;
            }

            GvrBetaHeadset.SetSeeThroughConfig(camMode, sceneType);

            statusText.text = "Mode: " + camMode;

            bool seethruEnabled = camMode != GvrBetaSeeThroughCameraMode.Disabled;
            foreach (var go in hideDuringSeeThrough)
            {
                go.SetActive(!seethruEnabled);
            }
        }
    }
}

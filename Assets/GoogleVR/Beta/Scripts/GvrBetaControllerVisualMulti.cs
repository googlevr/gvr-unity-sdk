//-----------------------------------------------------------------------
// <copyright file="GvrBetaControllerVisualMulti.cs" company="Google LLC">
// Copyright 2018 Google LLC. All rights reserved.
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

namespace GoogleVR.Beta
{
    using GoogleVR.Beta;
    using UnityEngine;

    /// <summary>A beta library for multiple 6DoF controller visuals.</summary>
    public class GvrBetaControllerVisualMulti : GvrControllerVisual
    {
        /// <summary>
        /// An array of mesh and material pairs used to dynamically change the controller visual.
        /// </summary>
        [SerializeField]
        private VisualAssets[] visualsAssets;

        /// <inheritdoc/>
        public override float PreferredAlpha
        {
            get
            {
                float controllerAlpha = base.PreferredAlpha;
                if (ControllerInputDevice != null)
                {
                    switch (ControllerInputDevice.GetTrackingStatusFlags())
                    {
                        case GvrBetaControllerInput.TrackingStatusFlags.Occluded:
                        case GvrBetaControllerInput.TrackingStatusFlags.OutOfFov:
                            controllerAlpha *= 0.5f;
                            break;
                        }
                }

                return controllerAlpha;
            }
        }

        /// <inheritdoc/>
        protected override VisualAssets GetVisualAssets()
        {
            VisualAssets vizAssets = base.GetVisualAssets();

            int controllerVisualIndex = 0;
            if (ControllerInputDevice != null &&
                ControllerInputDevice.GetConfigurationType() ==
                    GvrBetaControllerInput.Configuration.Is6DoF)
            {
                controllerVisualIndex = 1;
            }

            // Check that visualsAssets exists and that the visual index is within range.
            if (visualsAssets != null &&
                controllerVisualIndex < visualsAssets.Length)
            {
                vizAssets.material = visualsAssets[controllerVisualIndex].material;
                vizAssets.mesh = visualsAssets[controllerVisualIndex].mesh;
            }

            return vizAssets;
        }
    }
}

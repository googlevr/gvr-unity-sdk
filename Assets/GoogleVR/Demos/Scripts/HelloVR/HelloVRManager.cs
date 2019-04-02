//-----------------------------------------------------------------------
// <copyright file="HelloVRManager.cs" company="Google Inc.">
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

namespace GoogleVR.HelloVR
{
    using GoogleVR.Demos;
    using UnityEngine;

    /// @deprecated
    /// <summary>
    /// Keeps tabs on the scene's associated DemoInputManager, and deactivates it if necessary.
    /// </summary>
    /// <remarks>
    /// Capable of piping calls to a deprecated launchVrHomeButton to the GvrDaydreamApi.
    /// </remarks>
    public class HelloVRManager : MonoBehaviour
    {
        /// @deprecated
        /// <summary>
        /// A VR Home button to activate or deactivate as devices connect to or disconnect from the
        /// app.
        /// </summary>
        public GameObject launchVrHomeButton;

        /// @deprecated
        /// <summary>A DemoInputManager instance which is managing the scene, if any.</summary>
        public DemoInputManager demoInputManager;

        /// @deprecated
        /// <summary>A method which launches the VR Home screen.</summary>
        public void LaunchVrHome()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GvrDaydreamApi.LaunchVrHomeAsync((success) =>
            {
                if (!success)
                {
                    // Unexpected. See GvrDaydreamApi log messages for details.
                    Debug.LogError("GvrDaydreamApi.LaunchVrHomeAsync() failed");
                }
            });
#endif  // UNITY_ANDROID && !UNITY_EDITOR
        }

        private void Start()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            if (launchVrHomeButton == null)
            {
                return;
            }

            launchVrHomeButton.SetActive(false);
#else
            GvrDaydreamApi.CreateAsync((success) =>
            {
                if (!success)
                {
                    // Unexpected. See GvrDaydreamApi log messages for details.
                    Debug.LogError("GvrDaydreamApi.CreateAsync() failed");
                }
            });
#endif  // !UNITY_ANDROID || UNITY_EDITOR
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void Update()
        {
            if (launchVrHomeButton == null || demoInputManager == null)
            {
                return;
            }

            launchVrHomeButton.SetActive(demoInputManager.IsCurrentlyDaydream());
        }
#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }
}

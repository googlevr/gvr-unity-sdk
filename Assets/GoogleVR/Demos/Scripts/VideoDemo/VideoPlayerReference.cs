//-----------------------------------------------------------------------
// <copyright file="VideoPlayerReference.cs" company="Google Inc.">
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
    using UnityEngine;

    /// <summary>
    /// A data class which finds and stores reference information for `GvrVideoPlayerTexture`
    /// instances.
    /// </summary>
    public class VideoPlayerReference : MonoBehaviour
    {
        /// <summary>The `GvrVideoPlayerTexture` instance this object refers to.</summary>
        public GvrVideoPlayerTexture player;

        private void Awake()
        {
#if !UNITY_5_2
            GetComponentInChildren<VideoControlsManager>(true).Player = player;
#else
            GetComponentInChildren<VideoControlsManager>().Player = player;
#endif
        }
    }
}

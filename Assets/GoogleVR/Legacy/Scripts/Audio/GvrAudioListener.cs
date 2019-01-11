//-----------------------------------------------------------------------
// <copyright file="GvrAudioListener.cs" company="Google Inc.">
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

using UnityEngine;
using System.Collections;

#pragma warning disable 0618 // Ignore GvrAudio* deprecation

/// GVR audio listener component that enhances AudioListener to provide advanced spatial audio
/// features.
///
/// There should be only one instance of this which is attached to the AudioListener's game object.
#if UNITY_2017_1_OR_NEWER
[System.Obsolete("Please upgrade to Resonance Audio (https://developers.google.com/resonance-audio/migrate).")]
#endif  // UNITY_2017_1_OR_NEWER
[AddComponentMenu("GoogleVR/Audio/GvrAudioListener")]
public class GvrAudioListener : MonoBehaviour
{
    /// Global gain in decibels to be applied to the processed output.
    public float globalGainDb = 0.0f;

    /// Global layer mask to be used in occlusion detection.
    public LayerMask occlusionMask = -1;

    /// Audio rendering quality of the system.
    [SerializeField]
    private GvrAudio.Quality quality = GvrAudio.Quality.High;

    void Awake()
    {
#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
    Debug.LogWarningFormat(gameObject,
        "Game object '{0}' uses deprecated {1} component.\nPlease upgrade to Resonance Audio ({2}).",
        name, GetType().Name, "https://developers.google.com/resonance-audio/migrate");
#endif  // UNITY_EDITOR && UNITY_2017_1_OR_NEWER
        GvrAudio.Initialize(this, quality);
    }

    void OnEnable()
    {
        GvrAudio.UpdateAudioListener(globalGainDb, occlusionMask);
    }

    void OnDestroy()
    {
        GvrAudio.Shutdown(this);
    }

    void Update()
    {
        GvrAudio.UpdateAudioListener(globalGainDb, occlusionMask);
    }
}

#pragma warning restore 0618 // Restore warnings

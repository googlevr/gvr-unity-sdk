//-----------------------------------------------------------------------
// <copyright file="GvrAudioRoom.cs" company="Google Inc.">
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

/// GVR audio room component that simulates environmental effects of a room with respect to the
/// properties of the attached game object.
#if UNITY_2017_1_OR_NEWER
[System.Obsolete("Please upgrade to Resonance Audio (https://developers.google.com/resonance-audio/migrate).")]
#endif  // UNITY_2017_1_OR_NEWER
[AddComponentMenu("GoogleVR/Audio/GvrAudioRoom")]
public class GvrAudioRoom : MonoBehaviour
{
    /// Material type that determines the acoustic properties of a room surface.
    public enum SurfaceMaterial
    {
        /// Transparent
        Transparent = 0,

        /// Acoustic ceiling tiles
        AcousticCeilingTiles = 1,

        /// Brick, bare
        BrickBare = 2,

        /// Brick, painted
        BrickPainted = 3,

        /// Concrete block, coarse
        ConcreteBlockCoarse = 4,

        /// Concrete block, painted
        ConcreteBlockPainted = 5,

        /// Curtain, heavy
        CurtainHeavy = 6,

        /// Fiberglass insulation
        FiberglassInsulation = 7,

        /// Glass, thin
        GlassThin = 8,

        /// Glass, thick
        GlassThick = 9,

        /// Grass
        Grass = 10,

        /// Linoleum on concrete
        LinoleumOnConcrete = 11,

        /// Marble
        Marble = 12,

        /// Galvanized sheet metal
        Metal = 13,

        /// Parquet on concrete
        ParquetOnConcrete = 14,

        /// Plaster, rough
        PlasterRough = 15,

        /// Plaster, smooth
        PlasterSmooth = 16,

        /// Plywood panel
        PlywoodPanel = 17,

        /// Polished concrete or tile
        PolishedConcreteOrTile = 18,

        /// Sheetrock
        Sheetrock = 19,

        /// Water or ice surface
        WaterOrIceSurface = 20,

        /// Wood ceiling
        WoodCeiling = 21,

        /// Wood panel
        WoodPanel = 22
    }

    /// Room surface material in negative x direction.
    public SurfaceMaterial leftWall = SurfaceMaterial.ConcreteBlockCoarse;

    /// Room surface material in positive x direction.
    public SurfaceMaterial rightWall = SurfaceMaterial.ConcreteBlockCoarse;

    /// Room surface material in negative y direction.
    public SurfaceMaterial floor = SurfaceMaterial.ParquetOnConcrete;

    /// Room surface material in positive y direction.
    public SurfaceMaterial ceiling = SurfaceMaterial.PlasterRough;

    /// Room surface material in negative z direction.
    public SurfaceMaterial backWall = SurfaceMaterial.ConcreteBlockCoarse;

    /// Room surface material in positive z direction.
    public SurfaceMaterial frontWall = SurfaceMaterial.ConcreteBlockCoarse;

    /// Reflectivity scalar for each surface of the room.
    public float reflectivity = 1.0f;

    /// Reverb gain modifier in decibels.
    public float reverbGainDb = 0.0f;

    /// Reverb brightness modifier.
    public float reverbBrightness = 0.0f;

    /// Reverb time modifier.
    public float reverbTime = 1.0f;

    /// Size of the room (normalized with respect to scale of the game object).
    public Vector3 size = Vector3.one;

    void Awake()
    {
#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
        Debug.LogWarningFormat(gameObject,
            "Game object '{0}' uses deprecated {1} component.\nPlease upgrade to Resonance Audio ({2}).",
            name, GetType().Name, "https://developers.google.com/resonance-audio/migrate");
#endif  // UNITY_EDITOR && UNITY_2017_1_OR_NEWER
    }

    void OnEnable()
    {
        GvrAudio.UpdateAudioRoom(this, GvrAudio.IsListenerInsideRoom(this));
    }

    void OnDisable()
    {
        GvrAudio.UpdateAudioRoom(this, false);
    }

    void Update()
    {
        GvrAudio.UpdateAudioRoom(this, GvrAudio.IsListenerInsideRoom(this));
    }

    void OnDrawGizmosSelected()
    {
        // Draw shoebox model wireframe of the room.
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}

#pragma warning restore 0618 // Restore warnings

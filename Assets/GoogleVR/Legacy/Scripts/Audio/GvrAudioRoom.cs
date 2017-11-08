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

using UnityEngine;
using System.Collections;

#pragma warning disable 0618 // Ignore GvrAudio* deprecation

/// GVR audio room component that simulates environmental effects of a room with respect to the
/// properties of the attached game object.
[System.Obsolete("GvrAudioRoom is deprecated. Please upgrade to Resonance Audio (https://developers.google.com/resonance-audio/migrate).")]
[AddComponentMenu("GoogleVR/Audio/Deprecated/GvrAudioRoom")]
public class GvrAudioRoom : MonoBehaviour {
  /// Material type that determines the acoustic properties of a room surface.
  public enum SurfaceMaterial {
    Transparent = 0,              ///< Transparent
    AcousticCeilingTiles = 1,     ///< Acoustic ceiling tiles
    BrickBare = 2,                ///< Brick, bare
    BrickPainted = 3,             ///< Brick, painted
    ConcreteBlockCoarse = 4,      ///< Concrete block, coarse
    ConcreteBlockPainted = 5,     ///< Concrete block, painted
    CurtainHeavy = 6,             ///< Curtain, heavy
    FiberglassInsulation = 7,     ///< Fiberglass insulation
    GlassThin = 8,                ///< Glass, thin
    GlassThick = 9,               ///< Glass, thick
    Grass = 10,                   ///< Grass
    LinoleumOnConcrete = 11,      ///< Linoleum on concrete
    Marble = 12,                  ///< Marble
    Metal = 13,                   ///< Galvanized sheet metal
    ParquetOnConcrete = 14,       ///< Parquet on concrete
    PlasterRough = 15,            ///< Plaster, rough
    PlasterSmooth = 16,           ///< Plaster, smooth
    PlywoodPanel = 17,            ///< Plywood panel
    PolishedConcreteOrTile = 18,  ///< Polished concrete or tile
    Sheetrock = 19,               ///< Sheetrock
    WaterOrIceSurface = 20,       ///< Water or ice surface
    WoodCeiling = 21,             ///< Wood ceiling
    WoodPanel = 22                ///< Wood panel
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

  void Awake() {
#if UNITY_EDITOR
    Debug.LogWarningFormat(gameObject,
        "Game object '{0}' uses deprecated {1} component.\nPlease upgrade to Resonance Audio ({2}).",
        name, GetType().Name, "https://developers.google.com/resonance-audio/migrate");
#endif  // UNITY_EDITOR
  }

  void OnEnable () {
    GvrAudio.UpdateAudioRoom(this, GvrAudio.IsListenerInsideRoom(this));
  }

  void OnDisable () {
    GvrAudio.UpdateAudioRoom(this, false);
  }

  void Update () {
    GvrAudio.UpdateAudioRoom(this, GvrAudio.IsListenerInsideRoom(this));
  }

  void OnDrawGizmosSelected () {
    // Draw shoebox model wireframe of the room.
    Gizmos.color = Color.yellow;
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawWireCube(Vector3.zero, size);
  }
}

#pragma warning restore 0618 // Restore warnings

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

using System;
using System.Collections.Generic;
using GVR.Input;
using UnityEngine;

namespace GVR.Samples.Magic {
  /// <summary>
  /// A projectile object thrown from a magic wand.
  /// </summary>
  public class MagicProjectile : MonoBehaviour {
    [Header("FX")]
    public ParticleSystem OrbParticles;

    [Tooltip("Curve controlling how the object grows from weak to strong")]
    public AnimationCurve GrowthCurve;

    [Tooltip("Size (units) of the projectile when fully charged")]
    public float FullyGrownSize = 2f;

    [Tooltip("Color of the projectile when initially spawned")]
    public Color DefaultColor = Color.white;

    [Tooltip("Color of the projectile when fully charged")]
    public Color GrownColor = Color.white;

    [Header("Audio Loop")]
    [Tooltip("Looping clip to play for weak projectiles")]
    public AudioClip Weak;

    [Tooltip("Looping clip to play for strong projectiles")]
    public AudioClip Strong;

    [Tooltip("Time (seconds) for audio loops to fade in")]
    public float RampTime = 0.08f;

    /// <summary>
    /// Gets the magical strength of this projectile.
    /// </summary>
    public MagicStrength Strength { get; private set; }

    /// <summary>
    /// Various strengths of magic that can be cast
    /// </summary>
    public enum MagicStrength {
      Weak,
      Strong
    }

    void Start() {
      Strength = MagicStrength.Weak;
      InitParticles();
      StartAudioLoop(Weak);
    }

    void Update() {
      RampAudio();
      if (_growing) {
        GrowProjectile();
      } else {
        if (GvrController.IsTouching)
          BeginGrowth();
      }
    }

    private void GrowProjectile() {
      if (!GvrController.IsTouching)
        return;

      if (transform.parent != null) {
        float sample = GrowthCurve.Evaluate(_time);
        _time += Time.deltaTime;
        ScaleParticleSystems(1f + sample * FullyGrownSize);
        if (sample >= 1f && Strength == MagicStrength.Weak) {
          SetToStrong();
        }
      } else {
        // Disallow growth after a projectile is thrown.
        StopGrowth();
      }
    }

    private void SetToStrong() {
      Strength = MagicStrength.Strong;
      StartAudioLoop(Strong);
      _allOrbSystems.ForEach(p => p.startColor = GrownColor);
    }

    private void StartAudioLoop(AudioClip clip) {
      if (_audioSource == null)
        _audioSource = GetComponentInChildren<GvrAudioSource>();

      _audioSource.clip = clip;
      if (!_audioSource.isPlaying) {
        _audioSource.loop = true;
        _audioSource.volume = 0;
        _audioSource.Play();
      }
    }

    private void RampAudio() {
      if (_audioSource.volume >= 1f)
        return;

      if (_audioRampTime < 0f)
        _audioRampTime = Time.realtimeSinceStartup;

      float elapsed = Time.realtimeSinceStartup - _audioRampTime;
      float step = elapsed / RampTime;
      _audioSource.volume = Mathf.Lerp(0, 1, step);
    }

    private void BeginGrowth() {
      if (Strength == MagicStrength.Weak) {
        _growing = true;
        _time = 0f;
      }
    }

    private void StopGrowth() {
      _growing = false;
    }

    private void InitParticles() {
      // Orb particles are made up of multiple nested particle systems. We need
      // to scale and color all of them every time, so we'll track them in a way
      // that allows us to work on them as a batch.

      // This dictionary records the initial state of each particle system
      _startInfo = new Dictionary<ParticleSystem, ParticleSystemInfo>();

      _allOrbSystems = new List<ParticleSystem> { OrbParticles };
      _allOrbSystems.AddRange(OrbParticles.GetComponentsInChildren<ParticleSystem>());

      _allOrbSystems.ForEach(p => {
        _startInfo[p] = new ParticleSystemInfo(p);
        p.startColor = DefaultColor;
      });
    }

    private void ScaleParticleSystems(float scale) {
      _allOrbSystems.ForEach(p => {
        ParticleSystemInfo starting = _startInfo[p];
        p.startSize = starting.Size * scale;
        p.startSpeed = starting.Speed * scale;
      });
    }

    /// <summary>
    /// Tracks the state of a particle system. This class allows
    /// us to modify particle systems relative to their state when
    /// the game starts.
    /// </summary>
    internal class ParticleSystemInfo {
      /// <summary>
      /// Initializes a new instance of the <see cref="ParticleSystemInfo"/> class.
      /// </summary>
      /// <param name="system">Particle sytem instance</param>
      public ParticleSystemInfo(ParticleSystem system) {
        _system = system;
        Size = system.startSize;
        Speed = system.startSpeed;
      }

      /// <summary>
      /// Gets the tracked particle system.
      /// </summary>
      public ParticleSystem System { get { return _system; } }

      /// <summary>
      /// Gets the starting size of the particle system.
      /// </summary>
      public float Size { get; private set; }

      /// <summary>
      /// Gets the starting speed of the particle system.
      /// </summary>
      public float Speed { get; private set; }

      private readonly ParticleSystem _system;
    }

    private float _audioRampTime = -1f;
    private bool _growing;
    private float _time;
    private GvrAudioSource _audioSource;

    private List<ParticleSystem> _allOrbSystems;
    private Dictionary<ParticleSystem, ParticleSystemInfo> _startInfo;
  }
}

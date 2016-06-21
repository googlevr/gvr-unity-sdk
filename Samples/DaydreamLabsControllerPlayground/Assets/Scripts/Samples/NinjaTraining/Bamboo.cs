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

using GVR.Entity;
using GVR.GUI;

using UnityEngine;
using UnityEngine.Events;

namespace GVR.Samples.NinjaTraining {
  /// <summary>
  /// A simple, object oriented class to handle the state of the Bamboo Obstacles in the Ninja Training demo.
  /// </summary>

  [RequireComponent(typeof(LinearMotor))]
  public class Bamboo : MonoBehaviour {
    [Tooltip("Event that fires when the Bamboo is explicitly Activated. Useful for SFX and hitbox Triggers.")]
    public UnityEvent OnActivate;

    [Tooltip("Event that fires when the Bamboo is explicitly Reset. Useful for SFX and hitbox Triggers.")]
    public UnityEvent OnReset;

    [Tooltip("Event that fires when the Bamboo is explicitly Disarmed. Useful for SFX and hitbox Triggers.")]
    public UnityEvent OnDisarm;

    private bool slashed = false;
    private bool active = false;
    private float currentLifetime = 0f;
    private float currentTimeDead = 0f;
    private float currentSpeed = 0f;

    private LinearMotor motor;
    private LevelSelectMenuListener levelSelectMenuListener;

    void Awake() {
      motor = GetComponent<LinearMotor>();
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
      levelSelectMenuListener.OnLevelSelectMenuClosed.AddListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.AddListener(OnMenuOpened);
    }

    void OnDestroy() {
      levelSelectMenuListener.OnLevelSelectMenuClosed.RemoveListener(OnMenuClosed);
      levelSelectMenuListener.OnLevelSelectMenuOpened.RemoveListener(OnMenuOpened);
    }

    private void OnMenuClosed() {
      motor.RatePerSecond = currentSpeed;
    }

    private void OnMenuOpened() {
      motor.RatePerSecond = 0.0f;
    }

    public float GetLifetime() {
      return currentLifetime;
    }

    public float GetDeadtime() {
      return currentTimeDead;
    }

    void Update() {
      if (active && !levelSelectMenuListener.IsMenuOpen) {
        if (slashed == false) {
          currentLifetime += Time.deltaTime;
        } else {
          currentTimeDead += Time.deltaTime;
        }
      }
    }

    public void SetSpeed(float speed) {
      currentSpeed = speed;
      if (motor) {
        motor.RatePerSecond = currentSpeed;
      }
    }

    public void Activate() {
      active = true;
      OnActivate.Invoke();
    }

    public void Disarm() {
      OnDisarm.Invoke();
    }

    public void Reset() {
      active = false;
      slashed = false;
      currentLifetime = 0f;
      currentTimeDead = 0f;
      currentSpeed = 0f;
      OnReset.Invoke();
    }
  }
}

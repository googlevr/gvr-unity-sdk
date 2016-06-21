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
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Samples.Adventure {
  /// <summary>
  /// A system that can respond differently to repeated events that happen
  /// within a specified time. Being triggered in succession will fire new
  /// events from the combo tier list. Having too much time between increment
  /// calls will reset the event index.
  /// </summary>
  public class ComboCounter : MonoBehaviour {
    #region -- Inspector Variables ----------------------------------------

    [Serializable]
    public class ComboTier {
      [Tooltip("Multiplier for the duration that this tier is active before it expires. " +
               "Multiplied by GlobalTierDuration.")]
      public float TierDurationMultiplier = 1.0f;

      [Tooltip("Event to raise when entering this tier.")]
      public UnityEvent OnTierEnter;

      [Tooltip("Event to raise when this tier expires.")]
      public UnityEvent OnTierFail;
    }

    [Tooltip("Duration that each tier has to trigger the next combo tier before expiring. " +
             "Each tier may apply a multiply on top of this.")]
    public float GlobalTierDuration = 1.0f;

    [Tooltip("Tiers of Combo data, definging cooldown times and responses..")]
    public ComboTier[] Tiers = new ComboTier[0];

    #endregion -- Inspector Variables -------------------------------------

    private int currentTier = -1;
    private float remainingTimeInTier;

    void Update() {
      if (currentTier >= 0) {
        remainingTimeInTier -= Time.deltaTime;
        if (remainingTimeInTier <= 0.0f) {
          Tiers[currentTier].OnTierFail.Invoke();
          currentTier = -1;
          remainingTimeInTier = 0.0f;
        }
      }
    }

    /// <summary>
    /// Increment to the next tier, triggering the Tier enter action. If
    /// on the last tier, it will enter it again.
    /// </summary>
    public void Increment() {
      currentTier = Mathf.Min(currentTier + 1, Tiers.Length - 1);
      remainingTimeInTier = Tiers[currentTier].TierDurationMultiplier * GlobalTierDuration;
      Tiers[currentTier].OnTierEnter.Invoke();
    }
  }
}

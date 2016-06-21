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
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Samples.NinjaTraining {
  //Simple class for holding difficulty settings for the Bamboo Hopper in the Ninja Training prototype.
  [System.Serializable]
  public class HopperSetting {
    public string Label = "New Difficulty Setting";
    public int MinScoreRequired = 0;
    public int SequenceLength = 5;
    public float PauseBetweenSequences = 3f;
    public float PauseBetweenBamboo = 1f;
    public float CleanupLifetime = 5f;
    public float CleanupDeadtime = 3f;
    public float BambooSpeed = 4f;
    public UnityEvent OnDifficultyEnter;
  }

  /// <summary>
  /// Simple Event-based State Manager for the Ninja Training demo. Based off of a List of Hopper
  /// Settings that are iterated through as a minimum score requirement is met. The Difficulty
  /// Ramper looks at the next Difficulty setting in sequence and waits for the score in that
  /// sequence to be met before pushing those values to the bamboo Hopper.
  /// </summary>
  public class HopperDifficultyRamper : MonoBehaviour {
    public HopperSetting[] HopperSettings;
    public BambooHopper Hopper;
    public Counter ScoreCounter;

    int currentDifficulty = 0;

    void Start() {
      PushDifficultySettings();
    }

    public void ResetDifficulty() {
      currentDifficulty = 0;
      PushDifficultySettings();
    }

    public void UpdateHopperSettings() {
      Debug.Log("Attempting to Update Difficulty.");
      int currentScore = ScoreCounter.GetCurrentCount();

      Debug.Log("Checking to See if Current Difficulty(" + currentDifficulty +
                ") plus one, is less than " + HopperSettings.Length);
      if (currentDifficulty + 1 < HopperSettings.Length) {
        Debug.Log("Checking to see if the Required Score of the NEXT difficulty in Sequence (" +
                  HopperSettings[currentDifficulty + 1].MinScoreRequired +
                  ") is less than or equal to the Current Score (" + currentScore + ")");
        if (HopperSettings[currentDifficulty + 1].MinScoreRequired <= currentScore) {
          Debug.Log("Checks Passed. Pushing new settings and Iterating on Difficulty.");
          currentDifficulty++;
          PushDifficultySettings();
        }
      }
    }

    public void PushDifficultySettings() {
      Hopper.SequenceLength = HopperSettings[currentDifficulty].SequenceLength;
      Hopper.PauseBetweenSequences = HopperSettings[currentDifficulty].PauseBetweenSequences;
      Hopper.PauseBetweenBamboo = HopperSettings[currentDifficulty].PauseBetweenBamboo;
      Hopper.CleanupLifetime = HopperSettings[currentDifficulty].CleanupLifetime;
      Hopper.CleanupDeadtime = HopperSettings[currentDifficulty].CleanupDeadtime;

      Hopper.SetBambooSpeed(HopperSettings[currentDifficulty].BambooSpeed);

      Debug.Log("Invoking On Difficulty Entered Events.");
      HopperSettings[currentDifficulty].OnDifficultyEnter.Invoke();
    }
  }
}

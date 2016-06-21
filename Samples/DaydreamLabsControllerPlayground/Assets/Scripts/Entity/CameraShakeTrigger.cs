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

namespace GVR.Entity {
  /// <summary>
  /// A Camera Shake Event Trigger meant to be used with the CameraShaker singleton class. When the
  /// "TriggerShake" public method is called, it will push a new Shake Event to the CameraShaker.
  /// </summary>
  public class CameraShakeTrigger : MonoBehaviour {
    [Header("Basic Shake Parameters:")]
    [Tooltip("The designer facing label for this Shake Trigger.")]
    public string ShakeLabel = "New Camera Shake Event.";

    [Tooltip("The Magnitude of this Shake.")]
    public float Magnitude = 5f;

    [Tooltip("The duration of this Shake.")]
    public float Duration = 1f;

    [Tooltip("The overall Distance Weight applied to this Shake. Values less than 1 will result " +
             "in a minimum shake value applied beyond the maximum distance.")]
    public float DistanceWeight = 1f;

    [Header("Advanced Settings: ")]
    [Tooltip("If true, custom falloff curves and Distance data will be pushed to the Camera " +
             "Shaker. If left false, the default values on the Shaker will be used instead.")]
    public bool UseCustomAdvancedData = false;

    [Tooltip("Custom max-distance on Shake events from this trigger.")]
    public float CustomMaxDistance = 10f;

    [Tooltip("Custom Duration falloff on shake events from this trigger.")]
    public AnimationCurve CustomDurationFalloff;

    [Tooltip("Custom Distance falloff on shake events from this trigger.")]
    public AnimationCurve CustomDistanceFalloff;

    public void TriggerShake() {
      ShakeData newData = new ShakeData();
      newData.Label = ShakeLabel + "_" + this.GetInstanceID();
      newData.ShakeMagnitude = Magnitude;
      newData.ShakeDuration = Duration;
      newData.Source = transform;
      newData.DistanceWeight = DistanceWeight;
      if (UseCustomAdvancedData) {
        newData.MaxShakeDistance = CustomMaxDistance;
        newData.DurationFalloff = CustomDurationFalloff;
        newData.DistanceFalloff = CustomDistanceFalloff;
      } else {
        newData.MaxShakeDistance = CameraShaker.current.DefaultMaxShakeDistance;
        newData.DurationFalloff = CameraShaker.current.DefaultDurationFalloff;
        newData.DistanceFalloff = CameraShaker.current.DefaultDistanceFalloff;
      }
      CameraShaker.current.StartNewShake(newData);
    }
  }
}

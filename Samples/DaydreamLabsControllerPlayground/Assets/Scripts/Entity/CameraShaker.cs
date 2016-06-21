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

using System.Collections.Generic;
using UnityEngine;

namespace GVR.Entity {
  /// <summary>
  /// Relevant data for a Camera Shake Event.
  /// </summary>
  public class ShakeData {
    [Tooltip("Designer-facing Label for this particular Shake Event.")]
    public string Label = "New Shake";

    [Tooltip("The distance used for the maximum X-axis value of the Distance Falloff animation curve.")]
    public float MaxShakeDistance = 20f;

    [Tooltip("The magnitude of the Shake in world units. Use sparingly.")]
    public float ShakeMagnitude = 1f;

    [Tooltip("The duration of the shake event. Used as a multiplier on the DurationFalloff animation curve.")]
    public float ShakeDuration = 1f;

    [Tooltip("The Transform of the source of the shake. This is important for doing " +
             "distance-based Shakes. As this transform gets closer or farther from the Shaker, " +
             "the DistanceFalloff animation curve will determine the magnitude of the shake.")]
    public Transform Source;

    [Tooltip("The falloff of the magnitude of the Shake due to time.")]
    public AnimationCurve DurationFalloff;

    [Tooltip("The falloff of the magnitude of the shake due to distance.")]
    public AnimationCurve DistanceFalloff;

    [Tooltip("The absolute weight of the Distance component of the falloff. At values less than " +
             "one, the shake will continue to have a magnitude even beyond the maximum distance.")]
    public float DistanceWeight = 1f;

    [Tooltip("The amount of time, in seconds, that this shake event has been ongoing.")]
    public float currentTimeShaking = 0f;
  }

  /// <summary>
  /// An all-purpose solution for Shaking the Camera (or any Transform hierarchy) along a local X-Y
  /// axis. On awake, this class will generate a new Container Object, push all of its children into
  /// the container, then apply camera shake activity in Local-space to the Container. It does so
  /// following a set of animation curves defined by Shake Data objects, and is typically triggered
  /// by the "Camera Shake Trigger" component. The goal is to create a fire-and-forget shake
  /// solution that can take any number of concurrent shake events.
  /// </summary>
  public class CameraShaker : MonoBehaviour {
    [HideInInspector]
    public static CameraShaker current;

    [Tooltip("The default maximum shake distance if one is not supplied by the Shake Trigger.")]
    public float DefaultMaxShakeDistance = 20f;

    [Tooltip("The default Duration falloff for this Camera if one is not supplied by the Shake Trigger.")]
    public AnimationCurve DefaultDurationFalloff;

    [Tooltip("The default Distance falloff for this Camera if one is not supplied by the Shake Trigger.")]
    public AnimationCurve DefaultDistanceFalloff;

    List<ShakeData> activeShakes = new List<ShakeData>();

    Transform shakeContainer;

    void Awake() {
      current = this;
      GenerateShakeContainer();
    }

    void Update() {
      ProcessAllShakeData();
    }

    public void StartNewShake(ShakeData newData) {
      activeShakes.Add(newData);
    }

    void ProcessAllShakeData() {
      float largestShakeThisFrame = 0f;
      for (int i = activeShakes.Count - 1; i >= 0; i--) {
        ShakeData data = activeShakes[i];
        if (data.currentTimeShaking < data.ShakeDuration) {
          data.currentTimeShaking += Time.deltaTime;
          float percentDuration = data.currentTimeShaking / data.ShakeDuration;
          float durationDamp = data.DurationFalloff.Evaluate(percentDuration);
          float distanceToShaker = Vector3.Distance(transform.position, data.Source.position);
          float percentDistance = Mathf.Clamp(distanceToShaker / DefaultMaxShakeDistance, 0f, 1f);
          float distanceDamp = data.DistanceFalloff.Evaluate(percentDistance);
          float weightedDistDamp = Mathf.Lerp(1.0f, distanceDamp, data.DistanceWeight);
          float shakeStrength = data.ShakeMagnitude * durationDamp * weightedDistDamp;
          if (shakeStrength > largestShakeThisFrame) {
            largestShakeThisFrame = shakeStrength;
            float x = Random.value * 2.0f - 1.0f;
            float y = Random.value * 2.0f - 1.0f;
            x *= shakeStrength;
            y *= shakeStrength;
            shakeContainer.localPosition = new Vector3(x, y, 0f);
          }
        } else {
          activeShakes.RemoveAt(i);
        }
      }
    }

    void GenerateShakeContainer() {
      List<Transform> children = new List<Transform>();
      for (int i = 0; i < transform.childCount; i++) {
        children.Add(transform.GetChild(i));
      }
      GameObject go = new GameObject();
      shakeContainer = go.transform;
      shakeContainer.SetParent(transform);
      shakeContainer.localPosition = Vector3.zero;
      shakeContainer.localEulerAngles = Vector3.zero;
      shakeContainer.gameObject.name = "Shake Container";
      for (int j = 0; j < children.Count; j++) {
        children[j].parent = shakeContainer;
      }
    }
  }
}

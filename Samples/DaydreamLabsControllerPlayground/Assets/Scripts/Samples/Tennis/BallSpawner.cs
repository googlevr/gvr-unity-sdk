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

using GVR.GUI;

using UnityEngine;

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component controls the ball spawning pattern.
  /// </summary>
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class BallSpawner : MonoBehaviour {
    public enum LaunchMethod {
      alternating,
      random
    };

    [Tooltip("The ObjectRecycler containing the ball objects.")]
    public ObjectRecycler BallRecycler;

    [Tooltip("The delay in seconds before the first ball is launched.")]
    public float InitialLaunchDelay;

    [Tooltip("The delay between subsequent ball launches.")]
    public float LaunchInterval;

    [Tooltip("The speed (m/s) at which the balls launch.")]
    public float LaunchSpeed;

    [Tooltip("The the cylinder from which the ball launches. Currently, its up vector is the vector used to aim the balls.")]
    public Transform LaunchTransform;

    [Tooltip("The variance in degrees of the ball launch trajectory in Y direction.")]
    public float LaunchVarianceY;

    [Tooltip("The variance in degrees of the ball launch trajectory in X direction.")]
    public float LaunchVarianceX;

    [Tooltip("The minimum angle in the X-direction that the ball will launch.")]
    public float MinXAngle;

    [Tooltip("Should the launch alternate between forehand and backhand, or be random?")]
    public LaunchMethod LaunchStyle;

    [Tooltip("The AudioSource that plays when you launch a ball.")]
    public GvrAudioSource LaunchAudio;

    private float intialLaunchCounter = 0;
    private float launchCounter = 0;
    private int ballCount = 0;
    private LevelSelectMenuListener levelSelectMenuListener;

    void Start() {
      intialLaunchCounter = InitialLaunchDelay;
      launchCounter = 0;
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
    }

    void Update() {
      if (levelSelectMenuListener.IsMenuOpen) {
        return;
      }

      if (intialLaunchCounter > 0) {
        intialLaunchCounter -= Time.deltaTime;
      } else if (launchCounter > 0) {
        launchCounter -= Time.deltaTime;
      } else {
        GameObject ball = BallRecycler.GetNextObject() as GameObject;
        ball.SetActive(true);
        ball.transform.position = LaunchTransform.position;
        ballCount++;
        int launchSide = 0;

        switch (LaunchStyle) {
          case LaunchMethod.alternating:
            launchSide = -1 + ((ballCount % 2) * 2);
            break;
          case LaunchMethod.random:
            launchSide = -1 + (Random.Range(0, 2) * 2);
            break;
        }

        ball.transform.eulerAngles = LaunchTransform.eulerAngles +
        new Vector3(Random.Range(0, LaunchVarianceY) * (-1 + (Random.Range(0, 2) * 2)),
                    0,
                    Random.Range(MinXAngle, LaunchVarianceX + MinXAngle) * launchSide);
        ball.GetComponent<Rigidbody>().velocity = ball.transform.up * LaunchSpeed;
        ball.GetComponent<TrailRenderer>().enabled = false;

        launchCounter = LaunchInterval;
        LaunchAudio.Play();
      }
    }
  }
}

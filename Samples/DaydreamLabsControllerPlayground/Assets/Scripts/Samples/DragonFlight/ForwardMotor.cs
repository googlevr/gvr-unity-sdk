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

namespace GVR.Samples.DragonFlight {
  /// <summary>
  /// A simple Forward Motor. Uses the Character Controller. Move method at a constant rate,
  /// driven by this motor.
  /// </summary>
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class ForwardMotor : MonoBehaviour {
    [Tooltip("The distance traveled in one second, in Unity Units.")]
    public float Rate = 2.0f;

    [Tooltip("The character controller driving the motion for this object.")]
    public CharacterController controller;

    // The dragon's maximum distance from the scene's origin.
    private const float MAX_RADIUS = 285.0f;

    private LevelSelectMenuListener levelSelectMenuListener;

    void Start() {
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
    }

    void Update() {
      // Pause the dragon if the menu is open.
      if (levelSelectMenuListener.IsMenuOpen) {
        return;
      }
      float radius = Mathf.Sqrt(Mathf.Pow(transform.position.x, 2.0f) +
                     Mathf.Pow(transform.position.y, 2.0f) +
                     Mathf.Pow(transform.position.z, 2.0f));
      bool withinBounds = (radius < MAX_RADIUS);
      bool isChangingDirection = 
          (Mathf.Sign(transform.position.x) != Mathf.Sign(transform.forward.x)) ||
          (Mathf.Sign(transform.position.y) != Mathf.Sign(transform.forward.y)) ||
          (Mathf.Sign(transform.position.z) != Mathf.Sign(transform.forward.z));
      // Move the dragon forwards if it is within limits, or if its coordinates is out of limits
      // but it is switching direction. This prevents the dragon from getting far away enough that 
      // the player loses track of its location.
      if (withinBounds || isChangingDirection) {
        controller.Move(transform.forward * Rate * Time.deltaTime);
      } 
    }
  }
}

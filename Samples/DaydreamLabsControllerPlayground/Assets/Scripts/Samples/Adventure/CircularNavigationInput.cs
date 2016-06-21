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

namespace GVR.Samples.Adventure {
  /// <summary>
  /// Gets the input from the controller's touchpad and transforms it such that "right" would point
  /// along the tangent of a circle around "Pivot" (or the origin) and "up" would move the player
  /// away from the pivot. This makes it such that holding right would make the player circle "Pivot".
  /// </summary>
  public class CircularNavigationInput : MonoBehaviour, IPlayerInputProvider {
    [Tooltip("The transform to pivot around. A 'down' input will move closer to this position. " +
             "A 'right' input will trace a circle around this point. If this is null, the origin is used.")]
    public Transform Pivot;

    public Vector3 GetMovementVector() {
      if (!GvrController.IsTouching)
        return Vector3.zero;

      // Translate to -1,-1 to 1,1
      Vector2 rawInput = (GvrController.TouchPos - new Vector2(0.5f, 0.5f)) * 2.0f;

      Vector3 center = Vector3.zero;
      if (Pivot != null)
        center = Pivot.position;
      Vector3 dif = center + Vector3.forward - transform.position;
      dif.y = 0.0f;

      Vector3 input = new Vector3(-rawInput.x, 0.0f, rawInput.y);
      input = Quaternion.Euler(0, Mathf.Atan2(dif.x, dif.z) * Mathf.Rad2Deg, 0) * input;
      return Vector3.ClampMagnitude(input, 1.0f);
    }

    public bool IsReady() {
      return GvrController.State == GvrConnectionState.Connected;
    }
  }
}

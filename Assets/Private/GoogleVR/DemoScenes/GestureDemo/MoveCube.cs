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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;
using System.Collections;

public class MoveCube : MonoBehaviour {
  public float translationSpeed = 1.0f;
  private float rotationSpeed = 0;
  private Vector3 rotationDirection = Vector3.zero;

  void Update() {
    GvrGesture.UpdateDetector();

    // Re-orient object at beginning of gesture.
    if (GvrGesture.DetectorState == GvrGesture.State.Start) {
      rotationSpeed = 0;
      transform.rotation = Quaternion.identity;
    }

    // Translate cube by scrolling.
    if (GvrGesture.DetectorState != GvrGesture.State.Idle) {
      Vector2 gestureDisplacement = GvrGesture.Displacement;
      TranslateCube(gestureDisplacement);
    }

    // Spin object with swipe.
    if (GvrGesture.DetectorState == GvrGesture.State.End && GvrGesture.SwipeDetected) {
      SetRotation();
    }

    // Long press on App button to recenter the object
    if (GvrGesture.LongPressButton == GvrGesture.Button.App) {
      transform.position = Vector3.zero;
      transform.rotation = Quaternion.identity;
      rotationSpeed = 0;
    }

    transform.Rotate(rotationDirection * rotationSpeed * 90 * Time.deltaTime);
  }

  void TranslateCube(Vector2 displacement) {
    Vector3 movement = new Vector3(displacement.x, displacement.y, 0);
    transform.position = transform.position + movement * translationSpeed;
  }

  void SetRotation() {
    Vector2 velocity = GvrGesture.Velocity;
    if (GvrGesture.GestureDirection == GvrGesture.Direction.Left ||
        GvrGesture.GestureDirection == GvrGesture.Direction.Right) {
      rotationSpeed = velocity.x;
      rotationDirection = Vector3.down;
    } else if (GvrGesture.GestureDirection == GvrGesture.Direction.Up ||
               GvrGesture.GestureDirection == GvrGesture.Direction.Down) {
      rotationSpeed = velocity.y;
      rotationDirection = Vector3.right;
    }
  }
}

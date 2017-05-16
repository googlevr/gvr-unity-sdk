// Copyright 2017 Google Inc. All rights reserved.
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
using UnityEngine.EventSystems;
using UnityEngine.VR;
using System.Collections;

/// Helper functions to perform common math operations for Gvr.
public static class GvrMathHelpers {
  private static Vector2 sphericalCoordinatesResult;

  public static Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult) {
    // Check for camera
    if (cam == null) {
      return Vector3.zero;
    }

    float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
    Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
    return intersectionPosition;
  }

  public static Vector2 GetViewportCenter() {
    int viewportWidth = Screen.width;
    int viewportHeight = Screen.height;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID
    // GVR native integration is supported.
    if (VRSettings.enabled) {
      viewportWidth = VRSettings.eyeTextureWidth;
      viewportHeight = VRSettings.eyeTextureHeight;
    }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID

    return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
  }

  public static Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
    cartCoords.Normalize();

    if (cartCoords.x == 0) {
      cartCoords.x = Mathf.Epsilon;
    }

    float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);

    if (cartCoords.x < 0) {
      outPolar += Mathf.PI;
    }

    float outElevation = Mathf.Asin(cartCoords.y);

    sphericalCoordinatesResult.x = outPolar;
    sphericalCoordinatesResult.y = outElevation;
    return sphericalCoordinatesResult;
  }
}

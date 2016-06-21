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

namespace GVR.TransformControl {
  /// <summary>
  /// Projects a ray on to a plane in space and places a selected transform
  /// at the location.
  /// </summary>
  public class TransformProjector : MonoBehaviour {
    [Tooltip("The transform where the ray comes from.")]
    public Transform RaycastSource;

    [Tooltip("Object to be moved to the raycast hit location.")]
    public Transform ObjectToMove;

    public Vector3 PlaneOrigin;

    public Vector3 PlaneNormal;

    private Plane plane;

    void Start() {
      RefreshPlane();
    }

    private void RefreshPlane() {
      plane = new Plane(PlaneNormal, PlaneOrigin);
    }

    public void SetPlaneValues(Vector3 origin, Vector3 normal) {
      PlaneOrigin = origin;
      PlaneNormal = normal;
      RefreshPlane();
    }

    void Update() {
      Ray ray = new Ray(RaycastSource.position, RaycastSource.forward);
      float distance = 0.0f;
      bool hit = plane.Raycast(ray, out distance);
      if (hit) {
        ObjectToMove.position = RaycastSource.position + ray.direction * distance;
      }
    }

  }
}

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

namespace GVR.TransformControl {
  /// <summary>
  /// Assumes the up vector of this transform and the transform's center
  /// represent a plane, this will project a list of other transforms to
  /// always be a fixed distance from that plane in the up direction. This also
  /// ensures that the distance of all listed objects from the center of this
  /// object never exceeds a maximum, resulting in a flat circle of valid
  /// positions on one side of the plane.
  /// </summary>
  public class SignedCircleConstraint : MonoBehaviour {
    [Tooltip("List of transforms locked to this constraint")]
    public List<Transform> LockedObjects = new List<Transform>();

    public float MaxDistance = 0.03f;

    public float MaxRadialDistance = 1.0f;

    [Tooltip("When enabled, the transform is locked to the circle. Otherwise, its just to the " +
             "positive side of the plane")]
    public bool RestrictWhileOutsideCircle;

    void OnDrawGizmosSelected() {
      int segments = 50;
      float inc = 360.0f / segments;
      Quaternion rotation = Quaternion.AngleAxis(inc, transform.up);
      Vector3 direction = transform.forward;
      for (int i = 0; i < segments; i++) {
        Vector3 oldPoint = transform.position + (direction * MaxRadialDistance);
        direction = rotation * direction;
        Gizmos.DrawLine(transform.position, transform.position + (direction * MaxRadialDistance));
        Gizmos.DrawLine(oldPoint, transform.position + (direction * MaxRadialDistance));
        Gizmos.DrawLine(oldPoint, oldPoint + (transform.up * MaxDistance));
      }
      Gizmos.DrawLine(transform.position, transform.position + (transform.up * MaxDistance));
    }

    void Update() {
      Plane plane = new Plane(transform.up, transform.position);
      for (int i = 0; i < LockedObjects.Count; i++) {
        float currentDistance = plane.GetDistanceToPoint(LockedObjects[i].position);
        if (currentDistance < 0) {
          Vector3 planarOffset = Vector3.ProjectOnPlane(LockedObjects[i].position, transform.up) -
                                 Vector3.ProjectOnPlane(transform.position, transform.up);
          if (!RestrictWhileOutsideCircle && planarOffset.magnitude > MaxRadialDistance) {
            Debug.DrawRay(transform.position, planarOffset);
            continue;
          }
          Vector3 currentoffset = transform.up * currentDistance;
          Vector3 offset = transform.up * MaxDistance;
          Debug.DrawLine(LockedObjects[i].position, LockedObjects[i].position - offset);
          LockedObjects[i].position = LockedObjects[i].position - currentoffset + offset;
          Vector3 centerOffset = LockedObjects[i].position - transform.position;
          centerOffset = Vector3.ClampMagnitude(centerOffset, MaxRadialDistance);
          LockedObjects[i].position = transform.position + centerOffset;
        }
      }
    }
  }
}

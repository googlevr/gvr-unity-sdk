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
  /// Assumes the up vector of this transform and the transform's center
  /// represent a plane, this will project a list of other transforms to
  /// always be a fixed distance from that plane. This also ensures that the
  /// distance of all listed objects from the center of this object never
  /// exceeds a maximum, resulting in a flat circle of valid positions.
  /// </summary>
  public class CircleConstraint : MonoBehaviour {
    public Transform[] LockedObjects = new Transform[0];
    public float MaxDistance = 0.03f;
    public float MaxRadialDistance = 1.0f;

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
      for (int i = 0; i < LockedObjects.Length; i++) {
        float currentDistance = plane.GetDistanceToPoint(LockedObjects[i].position);
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

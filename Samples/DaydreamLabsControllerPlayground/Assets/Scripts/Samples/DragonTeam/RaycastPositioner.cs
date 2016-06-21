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

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Positions a Cursor object in world space according to a raycast from a Transform along its
  /// local forward Z. This class includes special handling to preserve the Y axis of the Position
  /// Object when the Raycasthit does not collide with a legal surface. This makes it very useful
  /// as an all-purpose selection cursor.
  /// </summary>
  public class RaycastPositioner : MonoBehaviour {
    [Tooltip("Object to raycast from. Raycast originates along local forward Z.")]
    public Transform RaycastSource;

    [Tooltip("Cursor object to position according to the raycast.")]
    public GameObject ObjectToPosition;

    [Tooltip("Layermask to determine which objects should be casted to.")]
    public LayerMask Collidable;

    [Tooltip("If true, the line renderer referenced below will be updated to reflect  the raycast.")]
    public bool UseLineRenderer = false;

    [Tooltip("Line renderer visual used to show the raycast, if enabled.")]
    public LineRenderer Line;

    // Coordinate plane facing world Y up that the Position Object translates across when no legal
    // ground is found by the raycast. This helps keep the player from losing track of the Cursor
    // when it leaves legal ground.
    private Plane lastLegalPlane;

    private RaycastHit hit;

    public float MaxDistance = 50f;

    void Update() {
      Vector3 forward = RaycastSource.TransformDirection(Vector3.forward);
      bool didHit = Physics.Raycast(RaycastSource.position, forward, out hit, MaxDistance, Collidable);
      if (ObjectToPosition != null) {
        if (didHit) {
          ObjectToPosition.transform.position = hit.point;
          lastLegalPlane.SetNormalAndPosition(Vector3.up, 
                                              new Vector3(0, ObjectToPosition.transform.position.y, 0));
        } else {
          Ray castRay = new Ray(RaycastSource.transform.position, RaycastSource.transform.forward);
          float rayDistance;
          if (lastLegalPlane.Raycast(castRay, out rayDistance)) {
            ObjectToPosition.transform.position = castRay.GetPoint(rayDistance);
          }
        }
      }
      if (UseLineRenderer) {
        if (didHit) {
          if (Line.gameObject.activeSelf == false) {
            Line.gameObject.SetActive(true);
          }
          Line.SetPosition(0, RaycastSource.transform.position);
          Line.SetPosition(1, ObjectToPosition.transform.position);
        } else if (Line.gameObject.activeSelf == true)
          Line.gameObject.SetActive(false);
      }
    }
  }
}

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

namespace GVR.Samples.Maze {
  /// <summary>
  /// Handles the tilt controls for the maze, mapping rotation ranges of the
  /// input to a different range on the maze.
  /// </summary>
  public class MazeController : MonoBehaviour {
    [Tooltip("The object that will be rotated based on the specified ranges and this transforms rotation.")]
    public Rigidbody GameBoard;

    [Tooltip("The max angle that the board should tilt on any axis.")]
    public float MaxOutputAngle = 8.0f;

    [Tooltip("The tilt needed from the input device to reach the MaxOutputAngle.")]
    public float MaxInputAngle = 60.0f;

    private float prevMappedX, prevMappedZ;

    // Update is called once per frame
    void Update() {
      Quaternion newRot = Quaternion.Euler(transform.rotation.eulerAngles.x, 0.0f,
                                           transform.rotation.eulerAngles.z);

      //if tilted left/right
      float mappedX = 0;
      if (newRot.eulerAngles.x >= 0 && newRot.eulerAngles.x <= MaxInputAngle) {
        mappedX = Map(newRot.eulerAngles.x, 0, MaxInputAngle, 0, MaxOutputAngle);
        prevMappedX = mappedX;
      } else if (newRot.eulerAngles.x <= 360 && newRot.eulerAngles.x >= 360 - MaxInputAngle) {
        mappedX = Map(newRot.eulerAngles.x, 360 - MaxInputAngle, 360, -MaxOutputAngle, 0);
        prevMappedX = mappedX;
      } else {
        mappedX = prevMappedX;
      }

      //keep y as is
      float mappedY = newRot.eulerAngles.y;

      //if tilted forward
      float mappedZ = 0;
      if (newRot.eulerAngles.z >= 0 && newRot.eulerAngles.z <= MaxInputAngle) {
        mappedZ = Map(newRot.eulerAngles.z, 0, MaxInputAngle, 0, MaxOutputAngle);
        prevMappedZ = mappedZ;
      } else if (newRot.eulerAngles.z <= 360 && newRot.eulerAngles.z >= 360 - MaxInputAngle) {
        mappedZ = Map(newRot.eulerAngles.z, 360 - MaxInputAngle, 360, -MaxOutputAngle, 0);
        prevMappedZ = mappedZ;
      } else {
        mappedZ = prevMappedZ;
      }

      Quaternion mappedNewRot = Quaternion.Euler(mappedX, mappedY, mappedZ);

      //update rigidbody for floor/walls
      GameBoard.MoveRotation(mappedNewRot);
    }


    float Map(float value, float istart, float istop, float ostart, float ostop) {
      return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    }
  }
}

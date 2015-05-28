// Copyright 2014 Google Inc. All rights reserved.
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

public class Pose3D {
  // Right-handed to left-handed matrix converter (and vice versa).
  protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));

  public Vector3 Position { get; protected set; }
  public Quaternion Orientation { get; protected set; }
  public Matrix4x4 Matrix { get; protected set; }

  public Matrix4x4 RightHandedMatrix {
    get {
      return flipZ * Matrix * flipZ;
    }
  }

  public Pose3D() {
    Position = Vector3.zero;
    Orientation = Quaternion.identity;
    Matrix = Matrix4x4.identity;
  }

  public Pose3D(Vector3 position, Quaternion orientation) {
    Set(position, orientation);
  }

  public Pose3D(Matrix4x4 matrix) {
    Set(matrix);
  }

  protected void Set(Vector3 position, Quaternion orientation) {
    Position = position;
    Orientation = orientation;
    Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
  }

  protected void Set(Matrix4x4 matrix) {
    Matrix = matrix;
    Position = matrix.GetColumn(3);
    Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
  }
}

public class MutablePose3D : Pose3D {
  public new void Set(Vector3 position, Quaternion orientation) {
    base.Set(position, orientation);
  }

  public new void Set(Matrix4x4 matrix) {
    base.Set(matrix);
  }

  public void SetRightHanded(Matrix4x4 matrix) {
    Set(flipZ * matrix * flipZ);
  }
}

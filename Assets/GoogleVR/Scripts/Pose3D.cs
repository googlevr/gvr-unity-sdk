//-----------------------------------------------------------------------
// <copyright file="Pose3D.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Encapsulates a rotation and a translation.  This is a convenience class that allows
/// construction and value access either by Matrix4x4 or Quaternion + Vector3 types.
/// </summary>
public class Pose3D
{
    /// <summary>Right-handed to left-handed matrix converter (and vice versa).</summary>
    public static readonly Matrix4x4 FLIP_Z = Matrix4x4.Scale(new Vector3(1, 1, -1));

    /// <summary>Initializes a new instance of the <see cref="Pose3D" /> class.</summary>
    /// <remarks>
    /// Initializes position to the origin and orientation to the identity rotation.
    /// </remarks>
    public Pose3D()
    {
        Position = Vector3.zero;
        Orientation = Quaternion.identity;
        Matrix = Matrix4x4.identity;
    }

    /// <summary>Initializes a new instance of the <see cref="Pose3D" /> class.</summary>
    /// <param name="position">The position to initialize.</param>
    /// <param name="orientation">The orientation to initialize.</param>
    public Pose3D(Vector3 position, Quaternion orientation)
    {
        Set(position, orientation);
    }

    /// <summary>Initializes a new instance of the <see cref="Pose3D" /> class.</summary>
    /// <param name="matrix">The matrix to initialize.</param>
    public Pose3D(Matrix4x4 matrix)
    {
        Set(matrix);
    }

    /// <summary>Gets or sets the translation component of the pose.</summary>
    /// <value>The translation component of the pose.</value>
    public Vector3 Position { get; protected set; }

    /// <summary>Gets or sets the rotation component of the pose.</summary>
    /// <value>The rotation component of the pose.</value>
    public Quaternion Orientation { get; protected set; }

    /// <summary>Gets or sets the pose as a matrix in Unity gameobject convention.</summary>
    /// <remarks>GVR contention is right-handed, while Unity convention is left-handed.</remarks>
    /// <value>The pose as a matrix in Unity gameobject convention.</value>
    public Matrix4x4 Matrix { get; protected set; }

    /// <summary>Gets the pose as a matrix in right-handed coordinates.</summary>
    /// <value>The pose as a matrix in right-handed coordinates.</value>
    public Matrix4x4 RightHandedMatrix
    {
        get
        {
            return FlipHandedness(Matrix);
        }
    }

    /// <summary>Flip the handedness of a matrix.</summary>
    /// <param name="matrix">The Matrix4x4 to flip.</param>
    /// <returns>A handedness-flipped Matrix4x4.</returns>
    public static Matrix4x4 FlipHandedness(Matrix4x4 matrix)
    {
        return FLIP_Z * matrix * FLIP_Z;
    }

    /// <summary>Sets a Pose3D according to the provided values.</summary>
    /// <param name="position">The position to set.</param>
    /// <param name="orientation">The orientation to set.</param>
    protected void Set(Vector3 position, Quaternion orientation)
    {
        Position = position;
        Orientation = orientation;
        Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
    }

    /// <summary>Sets a Pose3D according to the provided values.</summary>
    /// <param name="matrix">The matrix to set.</param>
    protected void Set(Matrix4x4 matrix)
    {
        Matrix = matrix;
        Position = matrix.GetColumn(3);
        Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
    }
}

/// <summary>Mutable version of Pose3D.</summary>
public class MutablePose3D : Pose3D
{
    /// <summary>Sets the position and orientation from a Vector3 + Quaternion.</summary>
    /// <param name="position">The position to set.</param>
    /// <param name="orientation">The orientation to set.</param>
    public new void Set(Vector3 position, Quaternion orientation)
    {
        base.Set(position, orientation);
    }

    /// <summary>Sets the position and orientation from a Matrix4x4.</summary>
    /// <param name="matrix">The matrix to set.</param>
    public new void Set(Matrix4x4 matrix)
    {
        base.Set(matrix);
    }

    /// <summary>Sets the position and orientation from a right-handed Matrix4x4.</summary>
    /// <param name="matrix">The right-handed matrix to set.</param>
    public void SetRightHanded(Matrix4x4 matrix)
    {
        Set(FlipHandedness(matrix));
    }
}

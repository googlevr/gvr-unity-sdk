//-----------------------------------------------------------------------
// <copyright file="GvrMathHelpers.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

/// <summary>Helper functions to perform common math operations for Gvr.</summary>
public static class GvrMathHelpers
{
    // 3D pose instance to be used in ConvertFloatArrayToMatrix calls.
    private static MutablePose3D transientPose = new MutablePose3D();

    /// <summary>Gets the intersection position of the camera and the raycast result.</summary>
    /// <param name="cam">The camera to use.</param>
    /// <param name="raycastResult">The result of the raycast to intersect with the camera.</param>
    /// <returns>The position of the intersection.</returns>
    public static Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
    {
        // Check for camera
        if (cam == null)
        {
            return Vector3.zero;
        }

        float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
        Vector3 intersectionPosition =
            cam.transform.position + (cam.transform.forward * intersectionDistance);
        return intersectionPosition;
    }

    /// <summary>Normalizes a 3D Cartesian direction to a 2D spherical direction.</summary>
    /// <param name="cartCoords">The coordinates to normalize.</param>
    /// <returns>The spherical coordinates.</returns>
    public static Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
    {
        cartCoords.Normalize();

        if (cartCoords.x == 0)
        {
            cartCoords.x = Mathf.Epsilon;
        }

        float polar = Mathf.Atan(cartCoords.z / cartCoords.x);

        if (cartCoords.x < 0)
        {
            polar += Mathf.PI;
        }

        float elevation = Mathf.Asin(cartCoords.y);
        return new Vector2(polar, elevation);
    }

    /// <summary>A cubic easing function (https://easings.net/#easeOutCubic).</summary>
    /// <param name="min">The minimum output value.</param>
    /// <param name="max">The maximum output value.</param>
    /// <param name="value">The input to the easing function between 0 and 1.</param>
    /// <returns>The output of the easing function between (min) and (max).</returns>
    public static float EaseOutCubic(float min, float max, float value)
    {
        if (min > max)
        {
            Debug.LogError("Invalid values passed to EaseOutCubic, max must be greater than min. " +
            "min: " + min + ", max: " + max);
            return value;
        }

        value = Mathf.Clamp01(value);
        value -= 1.0f;
        float delta = max - min;
        float result = (delta * ((value * value * value) + 1.0f)) + min;
        return result;
    }

    /// <summary>Converts matrix from Google VR convention to Unity convention.</summary>
    /// <remarks>Google VR is row-major, RHS coordinates, and Unity is column-major, LHS.</remarks>
    /// <param name="gvrMatrix">The Google VR matrix data.</param>
    /// <param name="position">The position in Unity space based on the Google VR matrix.</param>
    /// <param name="orientation">The orientation in Unity space.</param>
    public static void GvrMatrixToUnitySpace(Matrix4x4 gvrMatrix,
                                             out Vector3 position,
                                             out Quaternion orientation)
    {
        // Invert the matrix to go from row-major (GVR) to column-major (Unity).
        Matrix4x4 unityMatrix = Matrix4x4.Transpose(gvrMatrix);

        // Change from RHS to LHS coordinates.
        transientPose.SetRightHanded(unityMatrix);

        position = transientPose.Position;
        orientation = transientPose.Orientation;
    }

    /// <summary>Converts a float array of length 16 into a column-major 4x4 matrix.</summary>
    /// <param name="floatArray">The array to convert to a matrix.</param>
    /// <returns>A column-major 4x4 matrix.</returns>
    public static Matrix4x4 ConvertFloatArrayToMatrix(float[] floatArray)
    {
        Matrix4x4 result = new Matrix4x4();

        if (floatArray == null || floatArray.Length != 16)
        {
            throw new System.ArgumentException(
                "floatArray must not be null and have a length of 16.");
        }

        result[0, 0] = floatArray[0];
        result[1, 0] = floatArray[1];
        result[2, 0] = floatArray[2];
        result[3, 0] = floatArray[3];
        result[0, 1] = floatArray[4];
        result[1, 1] = floatArray[5];
        result[2, 1] = floatArray[6];
        result[3, 1] = floatArray[7];
        result[0, 2] = floatArray[8];
        result[1, 2] = floatArray[9];
        result[2, 2] = floatArray[10];
        result[3, 2] = floatArray[11];
        result[0, 3] = floatArray[12];
        result[1, 3] = floatArray[13];
        result[2, 3] = floatArray[14];
        result[3, 3] = floatArray[15];

        return result;
    }
}

using UnityEngine;

public class TestUtils
{
  public static Matrix4x4 parseMatrix4x4(float[,] matrixArray) {
    Matrix4x4 matrix = new Matrix4x4();
    matrix.m00 = matrixArray[0, 0];
    matrix.m01 = matrixArray[0, 1];
    matrix.m02 = matrixArray[0, 2];
    matrix.m03 = matrixArray[0, 3];
    matrix.m10 = matrixArray[1, 0];
    matrix.m11 = matrixArray[1, 1];
    matrix.m12 = matrixArray[1, 2];
    matrix.m13 = matrixArray[1, 3];
    matrix.m20 = matrixArray[2, 0];
    matrix.m21 = matrixArray[2, 1];
    matrix.m22 = matrixArray[2, 2];
    matrix.m23 = matrixArray[2, 3];
    matrix.m30 = matrixArray[3, 0];
    matrix.m31 = matrixArray[3, 1];
    matrix.m32 = matrixArray[3, 2];
    matrix.m33 = matrixArray[3, 3];
    return matrix;
  }

  public static bool RectsAreEqual(Rect a, Rect b) {
    return Mathf.Approximately(a.x, b.x)
      && Mathf.Approximately(a.y, b.y)
      && Mathf.Approximately(a.width, b.width)
      && Mathf.Approximately(a.height, b.height);
  }

  public static bool Matrix4x4sAreEqual(Matrix4x4 a, Matrix4x4 b) {
    return Mathf.Approximately(a.m00, b.m00)
      && Mathf.Approximately(a.m01, b.m01)
      && Mathf.Approximately(a.m02, b.m02)
      && Mathf.Approximately(a.m03, b.m03)
      && Mathf.Approximately(a.m10, b.m10)
      && Mathf.Approximately(a.m11, b.m11)
      && Mathf.Approximately(a.m12, b.m12)
      && Mathf.Approximately(a.m13, b.m13)
      && Mathf.Approximately(a.m20, b.m20)
      && Mathf.Approximately(a.m21, b.m21)
      && Mathf.Approximately(a.m22, b.m22)
      && Mathf.Approximately(a.m23, b.m23)
      && Mathf.Approximately(a.m30, b.m30)
      && Mathf.Approximately(a.m31, b.m31)
      && Mathf.Approximately(a.m32, b.m32)
      && Mathf.Approximately(a.m33, b.m33);
  }
}



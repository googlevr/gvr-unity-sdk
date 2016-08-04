// Copyright 2015 Google Inc. All rights reserved.
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


// To use in a surface shader, add the following text to the code:
//
// #pragma surface ... vertex:vert                 <-- add "vertex:vert" to this line
// #pragma multi_compile __ GVR_DISTORTION         <-- copy the next 5 lines
// #include "GvrDistortion.cginc"
// void vert (inout appdata_base v) {
//   v.vertex = undistortSurface(v.vertex);
// }

// To use in a vertex shader, modify it as follows:
//
// #pragma multi_compile __ GVR_DISTORTION         <-- add these 2 lines
// #include "GvrDistortion.cginc"
//
// v2f vert (appdata_blah v) {
//   v2f o;
//   o.vertex = undistortVertex(v.vertex);    <-- replace "mul(UNITY_MATRIX_MVP, v.vertex)"
//   ...
//   return o;
// }

#if defined(GVR_DISTORTION)

float4x4  _Undistortion;
float     _MaxRadSq;
float     _NearClip;
float4x4  _RealProjection;
float4x4  _FixProjection;

float distortionFactor(float rSquared) {
  float ret = 0.0;
  ret = rSquared * (ret + _Undistortion[1][1]);
  ret = rSquared * (ret + _Undistortion[0][1]);
  ret = rSquared * (ret + _Undistortion[3][0]);
  ret = rSquared * (ret + _Undistortion[2][0]);
  ret = rSquared * (ret + _Undistortion[1][0]);
  ret = rSquared * (ret + _Undistortion[0][0]);
  return ret + 1.0;
}

// Convert point from world space to undistorted camera space.
float4 undistort(float4 pos) {
  // Go to camera space.
  pos = mul(UNITY_MATRIX_MV, pos);
  if (pos.z <= -_NearClip) {  // Reminder: Forward is -Z.
    // Undistort the point's coordinates in XY.
    float r2 = clamp(dot(pos.xy, pos.xy) / (pos.z*pos.z), 0, _MaxRadSq);
    pos.xy *= distortionFactor(r2);
  }
  return pos;
}

// Multiply by no-lens projection matrix after undistortion.
float4 undistortVertex(float4 pos) {
  return mul(_RealProjection, undistort(pos));
}

// Surface shader hides away the MVP multiplication, so we have
// to multiply by _FixProjection = inverse(VP)*_RealProjection
// and then by inverse(M), in order to cancel it out and leave our
// own transform in place.
float4 undistortSurface(float4 pos) {
  float4 proj = mul(_FixProjection, undistort(pos));
  return mul(_World2Object, proj);
}

#else
// Distortion disabled.

// Just do the standard MVP transform.
float4 undistortVertex(float4 pos) {
  return mul(UNITY_MATRIX_MVP, pos);
}

// Surface shader hides away the MVP multiplication, so just return pos.
float4 undistortSurface(float4 pos) {
  return pos;
}

#endif

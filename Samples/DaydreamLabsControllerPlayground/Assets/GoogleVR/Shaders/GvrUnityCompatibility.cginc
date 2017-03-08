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

// Required for compatibility between Unity 5.2, 5.3.3 and 5.4.

// Tranforms position from object to homogenous space
inline float4 GvrUnityObjectToClipPos(in float3 pos) {
#if defined(UNITY_5_4_OR_NEWER)
    return UnityObjectToClipPos(pos);
#else

#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
    // More efficient than computing M*VP matrix product
    return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#else
    return mul(UNITY_MATRIX_MVP, float4(pos, 1.0));
#endif  // defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)

#endif  // defined(UNITY_5_4_OR_NEWER)
}

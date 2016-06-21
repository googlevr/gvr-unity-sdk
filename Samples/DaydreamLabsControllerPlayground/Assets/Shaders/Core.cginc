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

#ifndef GVR_CORE_INCLUDED
#define GVR_CORE_INCLUDED

//////////// Includes and Pragmas ////////////
#pragma multi_compile NATIVE_OR_OFF GVR_DISTORTION
#include "Assets/GoogleVR/Distortion/GvrDistortion.cginc" // absolute path to plugin

//////////// Color Macros ////////////
#define WHITE fixed4(1.0,1.0,1.0,1.0)
#define BLACK fixed4(0.0,0.0,0.0,1.0)
#define RED fixed4(1.0,0.0,0.0,1.0)
#define GREEN fixed4(0.0,1.0,0.0,1.0)
#define BLUE fixed4(0.0,0.0,1.0,1.0)
#define CLEAR fixed4(0.0,0.0,0.0,0.0)

//////////// Vector Macros ////////////
#define ORIGIN float4(0.0,0.0,0.0,1.0)
#define RIGHT float4(1.0,0.0,0.0,0.0)
#define UP float4(0.0,1.0,0.0,0.0)
#define FORWARD float4(0.0,0.0,1.0,0.0)

//////////// Properties ////////////
#define SAMPLER_ST(texSampler) \
sampler2D texSampler; \
float4 texSampler##_ST

//////////// A2V Properties ////////////
#define A2V_VERTEX float4 vertex : POSITION
#define A2V_TEXCOORD(index) float4 texcoord##index : TEXCOORD##index
#define A2V_NORMAL float3 normal : NORMAL
#define A2V_TANGENT float4 tangent : TANGENT
#define A2V_COLOR float4 color : COLOR

//////////// Vertex Macros ////////////
#if defined(LIGHTMAP_ON) || !defined(LIGHTMAP_OFF)
	#define LIGHTMAP_TEXCOORDS v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw
#else
	#define LIGHTMAP_TEXCOORDS v.texcoord1
#endif
#define WORLD_POSITION mul(_Object2World,v.vertex)
#define WORLD_NORMAL normalize(mul(_Object2World,float4(v.normal,0.0)).xyz)
#define WORLD_VIEW(worldVert) (worldVert.xyz - _WorldSpaceCameraPos)
#define MVP_VERTEX undistortVertex(v.vertex)

#define PAN_TEX_COORD(time,speed,texcoord) texcoord = frac(time * speed)

//////////// Fragment Macros ////////////
#define SAMPLE_LIGHTMAP(texcoord) DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap,texcoord))

#endif
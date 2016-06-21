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

Shader "Stencil/Mask"
{
    Properties
    {
        __StencilRef("Stencil ID [0-255]", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]
        __StencilComp ("Stencil Comparison", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]
        __StencilOp ("Stencil Operation", Float) = 0
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100"}
        ColorMask 0
        ZWrite off
        Stencil 
        {
            Ref [__StencilRef]
            Comp [__StencilComp]
            Pass [__StencilOp] 
        }
        
        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata 
            {
                float4 vertex : POSITION;
            };
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
            };
            
            v2f vert(appdata v) 
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            
            half4 frag(v2f i) : COLOR 
            {
                return half4(1,0,0,1); // this doesn't matter, we don't write to the color buffer anyway
            }
        ENDCG
        }
    }
}

// <copyright file="InsideShader.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

// Shader that renders a "left on top" stereo texture
Shader "GoogleVR/Demos/VideoDemo StereoShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader {
        Pass {
            Tags { "RenderType"="Opaque" }

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "../../../Shaders/GvrUnityCompatibility.cginc"

                float4 _MainTex_ST;
                sampler2D _MainTex;

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert (appdata_base v) {
                    v2f o;

                    o.pos = GvrUnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                    o.uv.y *= 0.5f;

                    if(unity_StereoEyeIndex == 0) {
                        o.uv.y += 0.5f;
                    }

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target {
                    return tex2D(_MainTex, i.uv);
                }
            ENDCG
        }
    }
    Fallback "Mobile/VertextLit"
}

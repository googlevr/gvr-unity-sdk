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

//
// This shader switches the culling to the front side and inverts the normal so
// textures are drawn on the inside or back of the object.
//
Shader "GoogleVR/Demos/VideoDemo InsideShader" {
    Properties {
        _Gamma ("Video gamma", Range(0.01,3.0)) = 1.0
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _StereoVideo ("Render Stereo Video", Int) = 1
    }

    SubShader {
        Pass {
            Tags { "RenderType" = "Opaque" }

            // cull the outside, since we want to draw on the inside of the mesh.
            Cull Front

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "../../../Shaders/GvrUnityCompatibility.cginc"

                float4 _MainTex_ST;
                sampler2D _MainTex;
                int _StereoVideo;
                float _Gamma;

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                float3 gammaCorrect(float3 v)
                {
                    return pow(v, 1.0/_Gamma);
                }

                float3 gammaCorrectApprox(float3 v)
                {
                    return rsqrt(v);
                }

                // Apply the gamma correction.  One possible optimization that could
                // be applied is if _Gamma == 2.0, then use gammaCorrectApprox since sqrt will be faster.
                // Also, if _Gamma == 1.0, then there is no effect, so this call could be skipped all together.
                float4 gammaCorrect(float4 v)
                {
                    return float4( gammaCorrect(v.xyz), v.w );
                }

                v2f vert (appdata_base v) {
                    v2f o;
                    // invert the normal of the vertex
                    v.normal.xyz = v.normal * -1;
                    o.pos = GvrUnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                    if (_StereoVideo > 0) {
                        o.uv.y *= 0.5f;
                        if(unity_StereoEyeIndex == 0) {
                            o.uv.y += 0.5f;
                        }
                    }
                    o.uv.x = 1 - o.uv.x;
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target {
                    return gammaCorrect(tex2D(_MainTex, i.uv));
                }
            ENDCG
        }
    }
    Fallback "Mobile/VertextLit"
}

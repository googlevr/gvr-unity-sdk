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

Shader "GVR/Particles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        [Header(Blend Mode)]
        [Enum(UnityEngine.Rendering.BlendMode)]
        __BlendSrc ("Blend Src", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        __BlendDst ("Blend Dst", Float) = 10
        [Toggle]
        _Premultiply_Alpha("Premultiply Alpha",Float) = 0

        [Header(Fog Override Settings)]
        [Toggle]
        _OverrideFog("Override Fog",Float) = 0
        _FogDistanceOverride("Fog Distance Override",Float) = 1
        _FogColor("Fog Color",Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Blend [__BlendSrc] [__BlendDst]
        
        Pass
        {
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature __ _OVERRIDEFOG_ON
            #pragma shader_feature __ _PREMULTIPLY_ALPHA_ON
            
            #include "UnityCG.cginc"
            #include "Core.cginc"
            #include "Env.cginc"
            struct a2v
            {
                A2V_VERTEX;
                A2V_TEXCOORD(0);
                A2V_COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 fog : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _FogColor;

            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = MVP_VERTEX;
                o.uv = TRANSFORM_TEX(v.texcoord0, _MainTex);
                o.color = v.color;
                
                float4 worldPos = WORLD_POSITION;
                float3 worldView = WORLD_VIEW(worldPos);
                float distance = length(worldView);
                float3 viewDir = worldView / (distance + 0.0001); 

                o.fog = fog(distance,viewDir);
                #if defined(_OVERRIDEFOG_ON)
                    o.fog.rgb = _FogColor.rgb;
                #endif

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color * _Color;

                col.rgb = BLEND_FOG(col.rgb,i.fog);
                #if defined(_PREMULTIPLY_ALPHA_ON)
                    col.rgb *= col.a;
                #endif
                #if defined(_OVERRIDEFOG_ON)
                    col.a *= 1.0-saturate(i.fog.a*_FogDistanceOverride);
                #else
                    col.a *= 1.0-i.fog.a;
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}

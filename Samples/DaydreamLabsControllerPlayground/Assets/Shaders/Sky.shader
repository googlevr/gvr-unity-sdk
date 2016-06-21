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

Shader "GVR/Sky"
{
    Properties
    {
        [OptionalTexture(SKYTEX)]
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Background" "IgnoreProjector"="True" "PreviewType"="Skybox" "ForceNoShadowCasting"="True" }
        LOD 100

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature __ SKYTEX_ON
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SkyTexture;

            v2f vert (appdata v)
            {
                v2f o;
                
                float4 worldPos = mul(_Object2World,v.vertex);
                float3 viewDir = normalize(worldPos.xyz-_WorldSpaceCameraPos.xyz);
                
                o.uv = viewDir.y;

                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                #if defined (SKYTEX_ON)
                    fixed4 col = tex2D(_MainTex,i.uv);
                #else
                    fixed4 col = tex2D(_SkyTexture, i.uv);
                #endif
                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
}

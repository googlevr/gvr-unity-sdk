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

Shader "GVR/Dynamic Objects"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        [NoScaleOffset]
        _AOTex("Ambient Occlusion",2D) = "white"{}
        _RimPower("Rim Power",Range(0.001,3)) = 1

        [Space]
        [Header(Fog Override Settings)]
        [Toggle]
        _OverrideFog("Override Fog",Float) = 0
        _FogDistanceOverride("Fog Distance Override",Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry+30" "RenderType"="Opaque" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature __ _OVERRIDEFOG_ON
            #include "UnityCG.cginc"
            #include "Core.cginc"
            #include "Env.cginc"

            // A2V macros found in Core.cginc
            struct a2v
            {
                A2V_VERTEX;
                A2V_TEXCOORD(0);
                A2V_NORMAL;
                A2V_COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0; // sky
                half2 uv2 : TEXCOORD1; // ao map
                fixed4 rim : TEXCOORD2;
                fixed4 fogColor : TEXCOORD3;
                fixed3 shColor : TEXCOORD4; // light probes
                fixed4 color : COLOR0;
            };

            fixed4 _Color;
            fixed _RimPower;
            
            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = MVP_VERTEX;
                o.uv2 = v.texcoord0;
                
                float4 worldPos = WORLD_POSITION;
                float3 worldNormal = WORLD_NORMAL;
                float3 worldView = WORLD_VIEW(worldPos);
                float distance = length(worldView);
                float3 viewDir = worldView / (distance + 0.0001);

                o.fogColor = fog(distance,viewDir);

                o.uv = dot(worldNormal,UP.xyz);

                float4 rim = saturate(0.5 * dot(worldNormal, _WorldSpaceLightPos0.xyz) + 0.5);
                rim.w = pow(0.5 + 0.5 * dot(worldNormal,normalize(worldView)),_RimPower);
                o.rim = rim;
                o.rim.xyz = rim.xyz * rim.w;
                o.shColor = ShadeSH9(float4 (worldNormal, 1.0));
                o.color = v.color * _Color;
                return o;
            }

            sampler2D _AOTex;
            
            fixed4 frag (v2f i) : SV_Target 
            {
                fixed4 col = i.color;
                fixed3 sky = tex2D(_SkyTexture,i.uv);
                fixed3 ao = tex2D(_AOTex,i.uv2).rgb;
                ao *= i.shColor;
                
                fixed3 lighting = i.rim;
                
                col.rgb *= ao;
                col.rgb += lighting*sky;
                col.a = 1.0;
                
                col.rgb = BLEND_FOG(col.rgb,i.fogColor);
                    
                return col;
            }
            ENDCG
        }
    }
}

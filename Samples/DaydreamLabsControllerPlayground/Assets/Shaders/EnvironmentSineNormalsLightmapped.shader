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

Shader "GVR/Sine Wave Normals (Lightmapped)"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _RimPower("Rim Power",Range(0.001,3)) = 1

        [Space]
        [Header(Wave Settings)]
        _WaveFreq("Wave Frequency",Vector) = (0,0,0,0)
        _WaveSpeed("Wave Speed",Vector) = (0,0,0,0)
        _WaveAmp("Wave Amplitude",Vector) = (0,0,0,0)

        [Space]
        [Header(Fog Override Settings)]
        [Toggle]
        _OverrideFog("Override Fog",Float) = 0
        _FogDistanceOverride("Fog Distance Override",Float) = 1

    }
    SubShader
    {
        Tags { "Queue"="Geometry+40" "RenderType"="Opaque" "IgnoreProjector"="True" } 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature __ _OVERRIDEFOG_ON
            #include "UnityCG.cginc"
            #include "Core.cginc"
            #include "Env.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest 

            fixed4 _Color;
            fixed _RimPower;

            // A2V macros found in Core.cginc
            struct a2v
            {
                A2V_VERTEX;
                A2V_TEXCOORD(1);
                A2V_NORMAL;
                A2V_COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0; // sky
                half2 uv2 : TEXCOORD1; // lightmap
                fixed4 rim : TEXCOORD2;
                fixed4 fogColor : TEXCOORD3;
                fixed4 color : COLOR;
            };

            uniform float4 _WaveFreq; // The frequency of the sine wave for all axis, w unused
            uniform float4 _WaveSpeed; // The speed at which the wave pans for all axis, w unused
            uniform float4 _WaveAmp; // The amount to which the raw vertex data is affected on all axis, w unused

            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = MVP_VERTEX;
                o.uv2 = LIGHTMAP_TEXCOORDS;

                float4 worldPos = WORLD_POSITION;
                float3 worldNormal = WORLD_NORMAL;

                float3 wt = _Time.x * _WaveSpeed.xyz;
                float3 sine = sin(wt.xyz + (worldNormal * _WaveFreq.xyz));
                worldNormal += (sine.x + sine.y + sine.z) * _WaveAmp.xyz * v.color.a;
                worldNormal = normalize(worldNormal);

                float3 worldView = WORLD_VIEW(worldPos);
                float distance = length(worldView);
                float3 viewDir = worldView / (distance + 0.0001);

                o.fogColor = fog(distance,viewDir);

                o.uv = dot(worldNormal,UP.xyz);

                float4 rim = saturate(0.5 * dot(worldNormal, _WorldSpaceLightPos0.xyz) + 0.5);
                rim.w = pow(0.5 + 0.5 * dot(worldNormal,normalize(worldView)),_RimPower);
                o.rim = rim;
                o.rim.xyz = rim.xyz * rim.w;
                
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                fixed3 sky = tex2D(_SkyTexture,i.uv);
                fixed3 lighting = i.rim;

                #ifndef LIGHTMAP_OFF
                    fixed3 lm = SAMPLE_LIGHTMAP(i.uv2);
                    col.rgb *= lm;
                #endif

                col.rgb += lighting*sky;
                col.a = 1.0;

                col.rgb = BLEND_FOG(col.rgb,i.fogColor);
                return col;
            }

            ENDCG
        }
    }
}

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

Shader "GVR/Image FX/Screen-Space Outline"
{
    Properties
    {
        [Header(Controlled via OutlineObjectFX Script)]
        _MainTex ("Texture", 2D) = "white" {}
        _Thickness ("Thickness",Float) = 0.05
        _OutlineColor("Outline Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Core.cginc"

            struct a2v
            {
                A2V_VERTEX;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _Thickness;
            float4 _OutlineColor;

            v2f vert (a2v v)
            {
                v2f o;
                
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

                float2 uv = ComputeScreenPos(o.vertex);
                #if defined(UNITY_UV_STARTS_AT_TOP)
                    uv.y = 1-uv.y;
                #endif

                o.uv = uv;
                o.uv1 = uv + float2(-_Thickness,-_Thickness);
                o.uv2 = uv + float2(-_Thickness,_Thickness);
                o.uv3 = uv + float2(_Thickness,-_Thickness);
                o.uv4 = uv + float2(_Thickness,_Thickness);

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col1 = tex2D(_MainTex, i.uv1);
                fixed4 col2 = tex2D(_MainTex, i.uv2);
                fixed4 col3 = tex2D(_MainTex, i.uv3);
                fixed4 col4 = tex2D(_MainTex, i.uv4);
                
                col = saturate(col1 + col2 + col3 + col4) * (1.0-col);
                return fixed4(_OutlineColor.rgb,col.a);
            }
            ENDCG
        }
    }
}

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

Shader "Hidden/GVR/Outline Replacement"
{
    Properties{}

    CGINCLUDE

        #include "UnityCG.cginc"
        #include "Core.cginc"

        struct a2v_min
        {
            float4 vertex : POSITION;
        };

        struct v2f_min
        {
            float4 vertex : SV_POSITION;
        };

        v2f_min vert_min (a2v_min v)
        {
            v2f_min o;
            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
            return o;
        }

        fixed4 frag_min (v2f_min i) : SV_Target
        {
            return WHITE;
        }

    ENDCG

    SubShader
    {
        Tags { "Outline"="Opaque" }
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
                #pragma vertex vert_min
                #pragma fragment frag_min
            ENDCG
        }
    }
}

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

Shader "GoogleVR/Unlit/Controller Reticle" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }

  SubShader {
    Tags {
      "Queue"="Transparent"
      "IgnoreProjector"="True"
      "RenderType"="TransparentCutout"
    }

    LOD 100

    Cull Back
    Lighting Off
    Offset -150, -150

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0

      #include "UnityCG.cginc"

      struct appdata_t {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half2 texcoord : TEXCOORD0;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata_t v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        fixed4 col = tex2D(_MainTex, i.texcoord);
        clip(col.a - 0.5);
        return col;
      }
      ENDCG
    }
  }
}

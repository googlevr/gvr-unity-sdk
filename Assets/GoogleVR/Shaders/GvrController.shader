// Copyright 2017 Google Inc. All rights reserved.
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

Shader "GoogleVR/Unlit/Controller" {
  Properties {
    _Color ("Color", COLOR) = (1, 1, 1, 1)
    _MainTex ("Texture", 2D) = "white" {}
    /// The center of the touchpad in UV space
    /// Only change this value if you also change the UV layout of the mesh.
    _GvrTouchpadCenterX ("GVR Touchpad Center UV.X", Float) = 0.15
    _GvrTouchpadCenterY ("GVR Touchpad Center UV.Y", Float) = 0.85
    /// The radius of the touchpad in UV space, based on the geometry
    /// Only change this value if you also change the UV layout of the mesh.
    _GVRTouchPadRadius("GVRTouchPadRadius", Range(0.0, 1.0)) = 0.139
  }
  SubShader {
    Tags {
      "Queue" = "Overlay+100"
      "IgnoreProjector" = "True"
      "RenderType"="Transparent"
    }
    LOD 100

    ZWrite On
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      /// The size of the touch display.  A value of 1 sets the radius to equal the touchpad radius
      #define _GVR_DISPLAY_RADIUS .25

      // How opaque is the battery indicator when illuminated
      #define _GVR_BATTERY_ACTIVE_ALPHA 0.9

      //How opaque is the battery indicator when not illuminated
      #define _GVR_BATTERY_OFF_ALPHA 0.25

      // How much do the app and system buttons depress when pushed
      #define _BUTTON_PRESS_DEPTH 0.001

      // Larger values tighten the feather
      #define _TOUCH_FEATHER 8

      /// The center of the touchpad in UV space
      /// Only change this value if you also change the UV layout of the mesh
      #define _GVR_TOUCHPAD_CENTER half2(_GvrTouchpadCenterX, _GvrTouchpadCenterY)

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
      };

      struct v2f {
        half2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        half4 color : TEXCOORD1;
        half2 touchVector : TEXCOORD2;
        half alpha : TEXCOORD3;
      };

      sampler2D _MainTex;
      half4 _GvrControllerAlpha;
      float4 _MainTex_ST;

      half4 _Color;
      half4 _GvrTouchPadColor;
      half4 _GvrAppButtonColor;
      half4 _GvrSystemButtonColor;
      half4 _GvrBatteryColor;
      half4 _GvrTouchInfo;//xy position, z touch duration, w battery info
      float _GvrTouchpadCenterX;
      float _GvrTouchpadCenterY;
      float _GVRTouchPadRadius;

      v2f vert (appdata v) {
        v2f o;
        float4 vertex4;
        vertex4.xyz = v.vertex;
        vertex4.w = 1.0;

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = half4(0,0,0,0);
        o.touchVector = half2(0,0);


        half batteryOrController = saturate( 10.0 * (v.color.a - 0.6) );
        half batteryMask = saturate( 10.0 * (1 - v.color.a) );
        half batteryLevelMask = saturate( 20.0 * (v.color.a - _GvrTouchInfo.w) );
        o.alpha = batteryOrController;
        o.color.a = _GvrBatteryColor.a * batteryMask * (batteryLevelMask * _GVR_BATTERY_ACTIVE_ALPHA + (1-batteryLevelMask)*_GVR_BATTERY_OFF_ALPHA);
        o.color.rgb = batteryMask * (batteryLevelMask * _GvrBatteryColor.rgb);

        // v.color.r = Touchpad, v.color.g = AppButton, v.color.b = SystemButton, v.color.a = BatteryIndicator
        // Update touch vector info, but only if in the touchpad region.

        //This is the distance between the scaled center of the touchpad in UV space, and the input coords
        half2 touchPosition = ((v.uv - _GVR_TOUCHPAD_CENTER)/_GVRTouchPadRadius - _GvrTouchInfo.xy);

        // the duration of a press + minimum radius
        half scaledInput = _GvrTouchInfo.z + _GVR_DISPLAY_RADIUS;

        // Apply a cubic function, but make sure when press duration =1 , we cancel out the min radius
        half bounced = 2 * (2 * scaledInput - scaledInput*scaledInput ) -
          (1 - 2.0*_GVR_DISPLAY_RADIUS*_GVR_DISPLAY_RADIUS);

        o.touchVector = v.color.r * ((2-bounced)*( (1 - _GvrControllerAlpha.y)/_GVR_DISPLAY_RADIUS ) *touchPosition);

        // Apply colors based on masked values.
        o.color.rgb += v.color.r * _GvrTouchInfo.z * _GvrTouchPadColor.rgb +
          v.color.g * _GvrControllerAlpha.z * _GvrAppButtonColor.rgb +
          v.color.b * _GvrControllerAlpha.w * _GvrSystemButtonColor.rgb;

        o.color.a += v.color.r * _GvrTouchInfo.z +
          v.color.g * _GvrControllerAlpha.z +
          v.color.b * _GvrControllerAlpha.w;

        // Animate position based on masked values.
        vertex4.y -= v.color.g * _BUTTON_PRESS_DEPTH*_GvrControllerAlpha.z +
          v.color.b *  _BUTTON_PRESS_DEPTH*_GvrControllerAlpha.w;

        o.vertex = UnityObjectToClipPos(vertex4);

        return o;
      }

      fixed4 frag (v2f i) : SV_Target {

        // Compute the length from a touchpoint, scale the value to control the edge sharpness.
        half len = saturate(_TOUCH_FEATHER*(1-length(i.touchVector)) );
        i.color = i.color *len;

        half4 texcol =  tex2D(_MainTex, i.uv);
        half3 tintColor = (i.color.rgb + (1-i.color.a) * _Color.rgb);

        // Tint the controller based on luminance
        half luma = Luminance(tintColor);
        tintColor = texcol.rgb *(tintColor + .25*(1-luma));

        /// Battery indicator.
        texcol.a = i.alpha * texcol.a + (1-i.alpha)*(texcol.r)* i.color.a;
        texcol.rgb = i.alpha * tintColor + (1-i.alpha)*i.color.rgb;

        texcol.a *= _GvrControllerAlpha.x;
        return texcol;
      }
      ENDCG
    }
  }
  FallBack "Unlit/Transparent"
}

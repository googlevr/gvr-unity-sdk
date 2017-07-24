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

      /// The color constants for the battery
      #define _GVR_BATTERY_CRITICAL_COLOR half3(1,0,0)
      #define _GVR_BATTERY_LOW_COLOR half3(1,0.6823,0)
      #define _GVR_BATTERY_MED_COLOR half3(0,1,0.588)
      #define _GVR_BATTERY_HIGH_COLOR half3(0,1,0.588)
      #define _GVR_BATTERY_FULL_COLOR half3(0,1,0.588)
      #define _GVR_BATTERY_CHARGING_COLOR half3(0,1,0.588)

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
      #define _GVR_TOUCHPAD_CENTER half2(.15, .85)

      /// The radius of the touchpad in UV space, based on the geometry
      /// Only change this value if you also change the UV layout of the mesh
      #define _GVR_TOUCHPAD_RADIUS .139

      /// Battery upper threshold values.  These correspond to the values passed from C#
      #define _BATTERY_CHARGING 0
      #define _BATTERY_UNKNOWN 0.1
      #define _BATTERY_CRITICAL 0.3
      #define _BATTERY_LOW 0.5
      #define _BATTERY_MED 0.7
      #define _BATTERY_HIGH 0.9


      // These are the UV coordinates that define the various controller regions
      // Only change these if you are modifying the UV layout,
      // and comfortable changing the shader logic below
      #define _BATTERY_UV_X_MIN 0.3
      #define _BATTERY_UV_Y_MAX 0.11

      #define _BATTERY_UV_X_MAX_1 0.4
      #define _BATTERY_UV_X_MAX_2 0.5
      #define _BATTERY_UV_X_MAX_3 0.6
      #define _BATTERY_UV_X_MAX_4 0.7
      #define _BATTERY_UV_X_MAX_5 0.8
      #define _BATTERY_UV_X_CHARGE_OFFSET 0.3

      #define _SYSTEM_UV_X_MAX 0.2
      #define _SYSTEM_UV_Y_MAX 0.5

      #define _APP_UV_X_MAX 0.2
      #define _APP_UV_Y_MAX 0.7

      #define _TOUCHPAD_UV_X_MAX 0.295

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
      half4 _GvrTouchInfo;//xy position, z touch duration, w battery info

      v2f vert (appdata v) {
        v2f o;
        float4 vertex4;
        vertex4.xyz = v.vertex;
        vertex4.w = 1.0;

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = half4(0,0,0,0);
        o.touchVector = half2(0,0);
        o.alpha = 1;

        /// Battery
        if(v.uv.y < _BATTERY_UV_Y_MAX && v.uv.x > _BATTERY_UV_X_MIN){
          o.alpha =0;

          //charging
          if(_GvrTouchInfo.w < _BATTERY_CHARGING){
            o.color.rgb = _GVR_BATTERY_CHARGING_COLOR;
            o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
            if(v.uv.x > _BATTERY_UV_X_MAX_2 && v.uv.x < _BATTERY_UV_X_MAX_3){
              o.uv.x += _BATTERY_UV_X_CHARGE_OFFSET;
            }
          }
          //unknown
          else if(_GvrTouchInfo.w < _BATTERY_UNKNOWN){

          }
          //critical low
          else if(_GvrTouchInfo.w < _BATTERY_CRITICAL){
            if(v.uv.x < _BATTERY_UV_X_MAX_1){
              o.color.rgb = _GVR_BATTERY_CRITICAL_COLOR;
              o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
            }
            else{
              o.color.rgb = 0;
              o.color.a = _GVR_BATTERY_OFF_ALPHA;
            }
          }
          //low
          else if(_GvrTouchInfo.w < _BATTERY_LOW){
            if(v.uv.x < _BATTERY_UV_X_MAX_2){
              o.color.rgb = _GVR_BATTERY_LOW_COLOR;
              o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
            }
            else{
              o.color.rgb = 0;
              o.color.a = _GVR_BATTERY_OFF_ALPHA;
            }
          }
          //med
          else if(_GvrTouchInfo.w < _BATTERY_MED){
            if(v.uv.x < _BATTERY_UV_X_MAX_3){
              o.color.rgb = _GVR_BATTERY_MED_COLOR;
              o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
            }
            else{
              o.color.rgb = 0;
              o.color.a = _GVR_BATTERY_OFF_ALPHA;
            }
          }
          //high
          else if(_GvrTouchInfo.w < _BATTERY_HIGH){
            if(v.uv.x < _BATTERY_UV_X_MAX_4){
              o.color.rgb = _GVR_BATTERY_HIGH_COLOR;
              o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
            }
            else{
              o.color.rgb = 0;
              o.color.a = _GVR_BATTERY_OFF_ALPHA;
            }
          }
          //full
          else{
            o.color.rgb = _GVR_BATTERY_FULL_COLOR;
            o.color.a = _GVR_BATTERY_ACTIVE_ALPHA;
          }

        }
        /// System Button
        else if( v.uv.y < _SYSTEM_UV_Y_MAX){
          if(v.uv.x < _SYSTEM_UV_X_MAX){
            o.color = _GvrSystemButtonColor;
            o.color.rgb = _GvrControllerAlpha.w * o.color.rgb;
            o.color.a = ( _GvrControllerAlpha.w);

            vertex4.y -= _BUTTON_PRESS_DEPTH*_GvrControllerAlpha.w;
          }
        }
        /// App Button
        else if(v.uv.y < _APP_UV_Y_MAX){
          if(v.uv.x < _APP_UV_X_MAX){
            o.color = _GvrAppButtonColor;
            o.color.rgb = _GvrControllerAlpha.z * o.color.rgb;
            o.color.a = ( _GvrControllerAlpha.z);
            vertex4.y -= _BUTTON_PRESS_DEPTH*_GvrControllerAlpha.z;
          }
        }
        /// Touchpad
        else{
          if(v.uv.x < _TOUCHPAD_UV_X_MAX){
            half2 touchPosition = ((v.uv - _GVR_TOUCHPAD_CENTER)/_GVR_TOUCHPAD_RADIUS - _GvrTouchInfo.xy);

            half scaledInput = _GvrTouchInfo.z + .25;
            half bounced = 2 * (2 * scaledInput - scaledInput*scaledInput -.4375);
            o.touchVector = (2-bounced)*( (1 - _GvrControllerAlpha.y)/_GVR_DISPLAY_RADIUS ) *touchPosition;
            o.color = _GvrTouchPadColor;
            o.color.rgb = _GvrTouchInfo.z *o.color.rgb;
            o.color.a = (_GvrTouchInfo.z);
          }
        }
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

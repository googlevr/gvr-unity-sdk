// <copyright file="VideoUnlitShader.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
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
// This shader renders from OES_external_image textures which require special
// OpenGLES extensions and a special texture sampler.
//
Shader "GoogleVR/Video Unlit Shader" {
  Properties {
    _Gamma ("Video gamma", Range(0.01,3.0)) = 1.0
    _MainTex ("Base (RGB)", 2D) = "white" {}
    [KeywordEnum(None, TopBottom, LeftRight)] _StereoMode ("Stereo mode", Float) = 0
    [Toggle(FLIP_X)] _FlipX ("Flip X", Float) = 0
  }

  SubShader {
    Pass {
      Tags { "RenderType" = "Opaque" }

      Lighting Off
      Cull Off

      GLSLPROGRAM
        #pragma only_renderers gles gles3
        #extension GL_OES_EGL_image_external : require
        #extension GL_OES_EGL_image_external_essl3 : enable

        #pragma multi_compile ___ _STEREOMODE_TOPBOTTOM _STEREOMODE_LEFTRIGHT
        #pragma multi_compile ___ FLIP_X

        precision mediump int;
        precision mediump float;

        #ifdef VERTEX
          uniform vec4 _MainTex_ST;
          uniform int unity_StereoEyeIndex;
          varying vec2 uv;

          void main() {
            gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            uv = gl_MultiTexCoord0.xy;
            #ifdef FLIP_X
              uv.x = 1.0 - uv.x;
            #endif  // FLIP_X
            #ifdef _STEREOMODE_TOPBOTTOM
              uv.y *= 0.5;
              if (unity_StereoEyeIndex == 0) {
                uv.y += 0.5;
              }
            #endif  // _STEREOMODE_TOPBOTTOM
            #ifdef _STEREOMODE_LEFTRIGHT
              uv.x *= 0.5;
              if (unity_StereoEyeIndex != 0) {
                uv.x += 0.5;
              }
            #endif  // _STEREOMODE_LEFTRIGHT
            // Apply video texture transform from the video decoder.
            uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
          }
        #endif  // VERTEX

        #ifdef FRAGMENT
          vec3 gammaCorrect(vec3 v, float gamma) {
            return pow(v, vec3(1.0/gamma));
          }

          // Apply the gamma correction.  One possible optimization that could
          // be applied is if _Gamma == 2.0, then use gammaCorrectApprox since sqrt will be faster.
          // Also, if _Gamma == 1.0, then there is no effect, so this call could be skipped all together.
          vec4 gammaCorrect(vec4 v, float gamma) {
            return vec4(gammaCorrect(v.xyz, gamma), v.w);
          }

          uniform float _Gamma;
          uniform samplerExternalOES _MainTex;
          varying vec2 uv;

          void main() {
            gl_FragColor = gammaCorrect(texture2D(_MainTex, uv), _Gamma);
          }
        #endif  // FRAGMENT
      ENDGLSL
    }
  }
  Fallback "Unlit/Texture"
}

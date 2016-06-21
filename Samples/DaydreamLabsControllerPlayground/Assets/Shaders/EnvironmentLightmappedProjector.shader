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

Shader "GVR/Environment (Lightmapped + Projector)"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _RimPower("Rim Power",Range(0.001,3)) = 1
        
        [Space]
        [Header(Fog Override Settings)]
        [Toggle]
        _OverrideFog("Override Fog",Float) = 0
        _FogDistanceOverride("Fog Distance Override",Float) = 1

        [Header(Projector Data)]
        [OptionalTexture(PROJECTOR)][NoScaleOffset]
        _ProjectorCookie("Projector Cookie", 2D) = "black" {}
        _CustomProjectorCutoff("Projector Cutoff",Range(-1,1)) = 0.75
    }
    SubShader
    {
        Tags { "Queue"="Geometry+20" "RenderType"="Opaque" "IgnoreProjector"="True" } 
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM

            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma shader_feature __ _OVERRIDEFOG_ON
            #pragma shader_feature __ PROJECTOR_ON
            #include "UnityCG.cginc"
            #include "Core.cginc"
            #define ENV_BASE
            #include "Env.cginc"
            #pragma vertex vert_env
            #pragma fragment frag_env
            #pragma fragmentoption ARB_precision_hint_fastest 

            ENDCG
        }
    }
}

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

#ifndef GVR_ENV_INCLUDED
#define GVR_ENV_INCLUDED

float4 _SkyApexColor;
float4 _SkyHorizonColor;
float4 _FogApexColor;
float4 _FogHorizonColor;
float4 _FogParams;
float _FogDistanceOverride;
sampler2D _SkyTexture;

//////////// Fog  ////////////

#if defined(_OVERRIDEFOG_ON)
    #define FOG_OVERRIDE_VALUE(input) \
        saturate(input*_FogDistanceOverride)
#else
    #define FOG_OVERRIDE_VALUE(input) \
        input
#endif

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #define BLEND_FOG(col,fogCol) lerp(col,fogCol.rgb,FOG_OVERRIDE_VALUE(fogCol.a))
#else
    #define BLEND_FOG(col,fogCol) col
#endif

half4 fog(half distanceToCamera, half3 viewVector)
{
    half fogVertical = saturate(viewVector.y);

    half horizonFogDistance = distanceToCamera * _SkyHorizonColor.a;
    half apexFogDistance = distanceToCamera * _SkyApexColor.a;
    half3 horizonColor;
    half3 apexColor;
    half density;

    // here we can do something other than a linear fog - add support for exp & exp2
    half fogAlpha;
    #if defined(FOG_LINEAR)
        
        fogAlpha = saturate(horizonFogDistance + fogVertical * apexFogDistance * apexFogDistance ); // normalized view distance black to white

        horizonFogDistance = saturate( (horizonFogDistance - _FogHorizonColor.a)*_FogParams.x );
        horizonColor = horizonFogDistance * _SkyHorizonColor.rgb + (1.0-horizonFogDistance) * _FogHorizonColor.rgb;
        apexFogDistance = saturate( (apexFogDistance - _FogApexColor.a) * _FogParams.y );
        apexColor = apexFogDistance * _SkyApexColor.rgb + (1.0 - apexFogDistance) * _FogApexColor.rgb;

    #elif defined(FOG_EXP)
        density = unity_FogParams.y;
    #elif defined(FOG_EXP2)
        density = unity_FogParams.x;
    #else // fog unsupported
        return half4(1,1,0,1);
    #endif

    // These are not correct. However we are only using Linear in our scenes.
    #if defined(FOG_EXP) || defined(FOG_EXP2)
        fogAlpha = saturate(fogVertical + density);
        horizonColor = density * _SkyHorizonColor.rgb + density * _FogHorizonColor.rgb;
        apexColor = density * _SkyApexColor.rgb + (1.0 - density) * _FogApexColor.rgb;
    #endif

    //return half4(fogVertical,fogVertical,0,fogAlpha); // returning this will give us a texcoord version if we wanted to pack this into a texture.

    half3 fogColor = (1.0 - fogVertical) * horizonColor + fogVertical * apexColor;
    
    return half4(fogColor,fogAlpha);
}

//////////// Base Environment ////////////

#if defined(ENV_BASE)
    fixed4 _Color;
    fixed _RimPower;

    #if defined(PROJECTOR_ON)
        float4x4 _CustomProjector;
        sampler2D _ProjectorCookie;
        float4 _CustomProjectorWorldDir;
        float _CustomProjectorCutoff;
    #endif

    // A2V macros found in Core.cginc
    struct a2v_env
    {
        A2V_VERTEX;
        //A2V_TEXCOORD(0); // We don't care about the uv1 channel, only lightmap
        A2V_TEXCOORD(1);
        A2V_NORMAL;
        A2V_COLOR;
    };

    struct v2f_env
    {
        float4 vertex : SV_POSITION;
        half2 uv : TEXCOORD0; // sky
        half2 uv2 : TEXCOORD1; // lightmap
        fixed4 rim : TEXCOORD2;
        fixed4 fogColor : TEXCOORD3;
        fixed4 color : COLOR;
        #if defined(PROJECTOR_ON)
            fixed4 projUV : TEXCOORD4;
            fixed projOcc : TEXCOORD5;
        #endif
    };

    v2f_env vert_env (a2v_env v)
    {
        v2f_env o;
        o.vertex = MVP_VERTEX;
        o.uv2 = LIGHTMAP_TEXCOORDS;
                
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
                
        o.color = v.color * _Color;

        #if defined(PROJECTOR_ON)
            o.projUV = mul(_CustomProjector,worldPos);
            float3 dir = _CustomProjectorWorldDir.xyz;
            float np = dot(worldNormal, -dir);
            o.projOcc = step(_CustomProjectorCutoff,np);
        #endif

        return o;
    }

    fixed4 frag_env (v2f_env i) : SV_Target 
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
                
        #if defined(PROJECTOR_ON)
            fixed2 projUV = i.projUV.xy / max(0.0,i.projUV.w) / 2.0f + 0.5f;
            fixed4 projCol = tex2D(_ProjectorCookie, projUV);
            col.rgb = lerp(col.rgb, projCol.rgb, projCol.a*i.projOcc);
        #endif

        return col;
    }
#endif

#endif
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

using UnityEngine;

namespace GVR.Gfx {
  [ExecuteInEditMode]
  public class EnvironmentProperties : MonoBehaviour {
    private static EnvironmentProperties _instance;

    public static EnvironmentProperties Instance { get { return _instance; } }

    [Header("Fog Properties")]
    public Color fogApexColor;
    public Color fogHorizonColor;
    public Color horizonColor;
    public Color apexColor;
    public float apexFogDistance;
    public float apexFogPoint;
    public float apexFogBlendDistance;
    public float horizonFogDistance;
    public float horizonFogPoint;
    public float horizonFogBlendDistance;
    public float fogDensity;

    private int id_fogHorColor;
    private int id_fogApexColor;
    private int id_params;
    private int id_skyHorColor;
    private int id_skyApexColor;

    public Texture2D skyTexture;

    [SerializeField]
    [HideInInspector]
    Shader skyShader;

    void Awake() {
      if (_instance) {
        throw new System.Exception("Singleton Exception.");
      }
      _instance = this;
      id_fogHorColor = Shader.PropertyToID(ShaderLib.Variables.VECTOR_FOG_HORIZON);
      id_fogApexColor = Shader.PropertyToID(ShaderLib.Variables.VECTOR_FOG_APEX);
      id_skyHorColor = Shader.PropertyToID(ShaderLib.Variables.VECTOR_SKY_HORIZON);
      id_skyApexColor = Shader.PropertyToID(ShaderLib.Variables.VECTOR_SKY_APEX);
      id_params = Shader.PropertyToID(ShaderLib.Variables.VECTOR_FOG_PARAMS);
#if !UNITY_EDITOR
      ValidateSkybox();
#endif
    }

    void Reset() {
      ValidateSkybox();
    }

    void OnValidate() {
      ValidateSkybox();
    }

    void ValidateSkybox() {
      if (skyShader == null) {
        skyShader = Shader.Find("GVR/Sky");
      }
      if (RenderSettings.skybox == null || RenderSettings.skybox.shader != skyShader) {
        Debug.LogWarning("Auto-Generated Sky Material.");
        Material tmp = new Material(skyShader);
        tmp.name = "Auto-Generated Sky Material";
        RenderSettings.skybox = tmp;
      }
    }

    void OnEnable() {
      PushProperties();
    }

#if UNITY_EDITOR
    void Update() {
      ValidateSkybox();
      PushProperties();
    }
#endif

    public void PushProperties() {
      Shader.SetGlobalTexture("_SkyTexture", skyTexture);
      // sky properties
      Shader.SetGlobalVector(id_skyHorColor,
                             new Vector4(horizonColor.r, horizonColor.g, horizonColor.b,
                                         1f / (0.1f + horizonFogDistance)));
      Shader.SetGlobalVector(id_skyApexColor,
                             new Vector4(apexColor.r, apexColor.g, apexColor.b,
                                         1f / (0.1f + apexFogDistance)));
      // fog properties
      Shader.SetGlobalVector(id_fogHorColor,
                             new Vector4(fogHorizonColor.r, fogHorizonColor.g, fogHorizonColor.b,
                                         horizonFogPoint / (0.1f + horizonFogDistance)));
      Shader.SetGlobalVector(id_fogApexColor,
                             new Vector4(fogApexColor.r, fogApexColor.g, fogApexColor.b,
                                         apexFogPoint / (0.1f + apexFogDistance)));
      // fog params
      Shader.SetGlobalVector(id_params,
          new Vector4(1f / (0.1f + horizonFogBlendDistance / (0.1f + horizonFogDistance)),
                      1f / (0.1f + apexFogBlendDistance / (0.1f + apexFogDistance)),
                      fogDensity, 0f));
    }



  }
}

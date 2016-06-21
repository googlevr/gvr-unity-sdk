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

namespace GVR.Gfx {
  public static class ShaderLib {
    /// <summary>
    /// Holds const accessors for Shader variables.
    /// </summary>
    public static class Variables {
      public const string VECTOR_FOG_APEX = "_FogApexColor";
      public const string VECTOR_FOG_HORIZON = "_FogHorizonColor";
      public const string VECTOR_FOG_PARAMS = "_FogParams";
      public const string VECTOR_SKY_APEX = "_SkyApexColor";
      public const string VECTOR_SKY_HORIZON = "_SkyHorizonColor";
      public const string VECTOR_CUSTOM_PROJECTOR_WORLD_DIR = "_CustomProjectorWorldDir";
      public const string VECTOR_OUTLINE_COLOR = "_OutlineColor";
      public const string SAMPLER2D_MAINTEX = "_MainTex";
      public const string SAMPLER2D_BLENDTEX = "_BlendTex";
      public const string FLOAT_OUTLINE_THICKNESS = "_Thickness";
      public const string MATRIX_CUSTOM_PROJECTOR = "_CustomProjector";
    }

    /// <summary>
    /// Holds const accessors for Shader keywords.
    /// </summary>
    public static class Keywords {
      public const string AO_TEX = "AO_TEX_ON";
    }
  }
}

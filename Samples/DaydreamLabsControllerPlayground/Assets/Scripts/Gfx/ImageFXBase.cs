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
  public abstract class ImageFXBase : MonoBehaviour {
    [Header("RenderTexture Properties")]
    [SerializeField]
    protected int width;
    [SerializeField]
    protected int height;
    [SerializeField]
    protected int depth = OffscreenBuffer.DEPTH_ZERO;

    [Header("Camera Properties")]
    [SerializeField]
    protected LayerMask cullingMask;
    [SerializeField]
    protected DepthTextureMode depthMode = DepthTextureMode.None;
    [SerializeField]
    protected CameraClearFlags clearFlags = CameraClearFlags.Color;
    [SerializeField]
    protected Color clearColor = Color.black;

    [Header("Shader Replacement")]
    [SerializeField]
    protected Shader replacementShader;
    [SerializeField]
    protected string replacementTag = "RenderType";

    protected OffscreenBuffer bufferObj;
    protected RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;

    protected virtual void Awake() {
      Assert.SupportsRenderTextures(this);
      Assert.NotNull<Shader>(this, replacementShader, "replacementShader");
      if (!Assert.SupportsRenderTextureFormats(this, rtFormat)) {
        rtFormat = RenderTextureFormat.Default;
      }
    }

    protected virtual void OnEnable() {
      // buffer object setup
      if (bufferObj == null) {
        bufferObj = OffscreenBuffer.Create(width, height, depth, rtFormat, depthMode, cullingMask,
                                           clearFlags, clearColor, replacementShader, replacementTag);
      }
    }

    protected virtual void OnDisable() {
      bufferObj.enabled = false; // disable the bufferObj, will destroy RenderTextures
    }

    public OffscreenBuffer GetBufferObject() {
      return bufferObj;
    }

    /// <summary>
    /// Do the rendering required for this ImageFX
    /// </summary>
    /// <param name="cam">The Camera that the Offscreen Buffer will use as a reference.</param>
    public virtual void Render(Camera cam) {
      bufferObj.Render(cam);
    }
  }
}

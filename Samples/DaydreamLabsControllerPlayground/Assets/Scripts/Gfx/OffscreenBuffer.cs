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
  /// <summary>
  /// General use component for managing a RenderTexture.
  /// </summary>
  [RequireComponent(typeof(Camera))]
  public class OffscreenBuffer : MonoBehaviour {
    public const int DEPTH_ZERO = 0;
    public const int DEPTH_SIXTEEN = 16;
    public const int DEPTH_TWENTY_FOUR = 24;

    #region Inspector Variables

    [Header("Render Texture Properties")]
    [SerializeField]
    int width = 32;
    [SerializeField]
    int height = 32;
    [SerializeField]
    int depth = DEPTH_ZERO;
    [SerializeField]
    RenderTextureFormat format = RenderTextureFormat.Default;

    [Header("Camera Properties")]
    [SerializeField]
    LayerMask cullingMask;
    [SerializeField]
    DepthTextureMode depthMode = DepthTextureMode.None;
    [SerializeField]
    CameraClearFlags clearFlags = CameraClearFlags.Color;
    [SerializeField]
    Color clearColor = Color.black;

    [Header("Shader Replacement")]
    [SerializeField]
    Shader replacementShader;
    [SerializeField]
    string replacementTag = "RenderType";

    [Tooltip("If TRUE the attached Camera will be enabled and render as a normal Camera, otherwise it will be controlled via script.")]
    [SerializeField]
    bool autoUpdate = false;

    #endregion

    #region Class Variables

    private Camera cam;
    public Camera Camera { get { return cam; } }

    private RenderTexture buffer;
    public RenderTexture Buffer { get { return buffer; } }

    [System.NonSerialized]
    public bool useCameraPixelRect = true;
    [System.NonSerialized]
    public Rect pixelRect;
    [System.NonSerialized]
    public bool useOcclusionCulling = false;
    [System.NonSerialized]
    public bool hdr = false;

    #endregion

    /// <summary>
    /// Creates a GameObject with an OffscreenBuffer and required Camera with the specified properties.
    /// </summary>
    public static OffscreenBuffer Create(int width, int height, int depth,
                                         RenderTextureFormat format, DepthTextureMode depthMode,
                                         LayerMask layerMask, CameraClearFlags clearFlags,
                                         Color clearColor, Shader replacementShader,
                                         string replacementTag) {
      GameObject tmp = new GameObject("Offscreen Buffer Camera");
      // we want to do some setup before we activate so things adhere to Monobehaviour execution order.
      tmp.SetActive(false);
      tmp.AddComponent<Camera>();
      OffscreenBuffer c = tmp.AddComponent<OffscreenBuffer>();
      c.width = width;
      c.height = height;
      c.depth = depth;
      c.format = format;
      c.cullingMask = layerMask.value;
      c.clearFlags = clearFlags;
      c.clearColor = clearColor;
      if (replacementShader != null) {
        c.replacementShader = replacementShader;
        c.replacementTag = replacementTag;
      }
      tmp.SetActive(true);
      return c;
    }

    #region Unity Messages

    void Awake() {
      cam = GetComponent<Camera>();
    }

    void OnEnable() {
      if (buffer == null) {
        buffer = new RenderTexture(width, height, depth, format, RenderTextureReadWrite.Default);
      }
      cam.enabled = autoUpdate;
      ApplyCameraProperties();
    }

    void OnDisable() {
      if (buffer != null) {
        DestroyImmediate(buffer);
      }
    }

    #endregion

    #region Class Methods

    /// <summary>
    /// Applies properties that shouldn't be taken from the reference Camera.
    /// </summary>
    public void ApplyCameraProperties() {
      cam.clearFlags = clearFlags;
      cam.backgroundColor = clearColor;
      cam.cullingMask = cullingMask.value;
      cam.depthTextureMode = depthMode;
      cam.SetReplacementShader(replacementShader, replacementTag);
      cam.targetTexture = buffer;
      if (!useCameraPixelRect) {
        cam.pixelRect = new Rect(0, 0, width, height);
      }
      cam.useOcclusionCulling = useOcclusionCulling;
      cam.hdr = hdr;
    }

    public void Render(Camera src) {
      cam.CopyFrom(src); // copy positional data from the camera
      ApplyCameraProperties();
      cam.Render();
    }

    #endregion

  }
}

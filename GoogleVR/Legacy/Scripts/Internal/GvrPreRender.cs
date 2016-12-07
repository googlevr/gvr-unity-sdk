// Copyright 2015 Google Inc. All rights reserved.
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

/// This class is defined only the editor does not natively support GVR, or if the current
/// VR player is the in-editor emulator.
#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

using UnityEngine;

/// Clears the entire screen.  This script and GvrPostRender work together
/// to draw the whole screen in VR Mode.  There should be exactly one of each
/// component in any GVR-enabled scene.  It is part of the _GvrCamera_
/// prefab, which is included in _GvrMain_.  The GvrViewer script will
/// create one at runtime if the scene doesn't already have it, so generally
/// it is not necessary to manually add it unless you wish to edit the _Camera_
/// component that it controls.
[RequireComponent(typeof(Camera))]
[AddComponentMenu("GoogleVR/GvrPreRender")]
public class GvrPreRender : MonoBehaviour {

  public Camera cam { get; private set; }

  void Awake() {
    cam = GetComponent<Camera>();
  }

  void Start() {
    // Ensure distortion shader variables are initialized, because we can't count on
    // getting a ProfileChanged event on the first frame rendered.
    SetShaderGlobals();
  }

  void Reset() {
#if UNITY_EDITOR
    // Member variable 'cam' not always initialized when this method called in Editor.
    // So, we'll just make a local of the same name.
    var cam = GetComponent<Camera>();
#endif
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = Color.black;
    cam.cullingMask = 0;
    cam.useOcclusionCulling = false;
    cam.depth = -100;
  }

  void OnPreCull() {
    GvrViewer.Instance.UpdateState();
    if (GvrViewer.Instance.ProfileChanged) {
      SetShaderGlobals();
    }
    cam.clearFlags = GvrViewer.Instance.VRModeEnabled ?
        CameraClearFlags.SolidColor : CameraClearFlags.Nothing;
  }

  private void SetShaderGlobals() {
    // For any shaders that want to use these numbers for distortion correction.  But only
    // if distortion correction is needed, yet not already being handled by another method.
   if (GvrViewer.Instance.VRModeEnabled
        && GvrViewer.Instance.DistortionCorrection == GvrViewer.DistortionCorrectionMethod.None) {
      GvrProfile p = GvrViewer.Instance.Profile;
      // Distortion vertex shader currently setup for only 6 coefficients.
      if (p.viewer.inverse.Coef.Length > 6) {
        Debug.LogWarning("Inverse distortion correction has more than 6 coefficents. "
                         + "Shader only supports 6.");
      }
      Matrix4x4 mat = new Matrix4x4() {};
      for (int i=0; i<p.viewer.inverse.Coef.Length; i++) {
        mat[i] = p.viewer.inverse.Coef[i];
      }
      Shader.SetGlobalMatrix("_Undistortion", mat);
      float[] rect = new float[4];
      p.GetLeftEyeVisibleTanAngles(rect);
      float r = GvrProfile.GetMaxRadius(rect);
      Shader.SetGlobalFloat("_MaxRadSq", r*r);
    }
  }
}

#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

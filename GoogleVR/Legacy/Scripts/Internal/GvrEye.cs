// Copyright 2014 Google Inc. All rights reserved.
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

/// This class is defined only if the editor does not natively support GVR, or if the current
/// VR player is the in-editor emulator.

using UnityEngine;

/// Controls one camera of a stereo pair.  Each frame, it mirrors the settings of
/// the parent mono Camera, and then sets up side-by-side stereo with
/// the view and projection matrices from the GvrViewer.EyeView and GvrViewer.Projection.
/// The render output is directed to the GvrViewer.StereoScreen render texture, either
/// to the left half or right half depending on the chosen eye.
///
/// To enable a stereo camera pair, enable the parent mono camera and set
/// GvrViewer.vrModeEnabled = true.
///
/// @note If you programmatically change the set of GvrEyes belonging to a
/// StereoController, be sure to call StereoController::InvalidateEyes on it
/// in order to reset its cache.
[RequireComponent(typeof(Camera))]
[AddComponentMenu("GoogleVR/GvrEye")]
public class GvrEye : MonoBehaviour {
  /// Whether this is the left eye or the right eye.
  /// Determines which stereo eye to render, that is, which `EyeOffset` and
  /// `Projection` matrix to use and which half of the screen to render to.
  public GvrViewer.Eye eye;

  /// Allows you to flip on or off specific culling mask layers for just this
  /// eye.  The mask is a toggle:  The eye's culling mask is first copied from
  /// the parent mono camera, and then the layers specified here are flipped.
  /// Each eye has its own toggle mask.
  [Tooltip("Culling mask layers that this eye should toggle relative to the parent camera.")]
  public LayerMask toggleCullingMask = 0;

  /// The StereoController in charge of this eye (and whose mono camera
  /// we will copy settings from).
  public StereoController Controller {
    // This property is set up to work both in editor and in player.
    get {
      if (transform.parent == null) { // Should not happen.
        return null;
      }
      if ((Application.isEditor && !Application.isPlaying) || controller == null) {
        // Go find our controller.
        return transform.parent.GetComponentInParent<StereoController>();
      }
      return controller;
    }
  }

  /// Returns the closest ancestor GvrHead.
  /// @note Uses GetComponentInParent(), so the result will be null if no active ancestor is found.
  public GvrHead Head {
    get {
      return GetComponentInParent<GvrHead>();
    }
  }

// C# stereo rendering is not used when UNITY_HAS_GOOGLEVR is true and this is running on a device.
// Disable variable warnings in this case.
#if UNITY_HAS_GOOGLEVR && !UNITY_EDITOR
#pragma warning disable 649
#pragma warning disable 414
#endif  // UNITY_HAS_GOOGLEVR && !UNITY_EDITOR

  private StereoController controller;
  private StereoRenderEffect stereoEffect;
  private Camera monoCamera;
  private Matrix4x4 realProj;
  private float interpPosition = 1;

#if UNITY_HAS_GOOGLEVR && !UNITY_EDITOR
#pragma warning restore 414
#pragma warning restore 649
#endif  // UNITY_HAS_GOOGLEVR && !UNITY_EDITOR

  // Convenient accessor to the camera component used throughout this script.
  public Camera cam { get; private set; }

#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
  void Awake() {
    cam = GetComponent<Camera>();
  }

  void Start() {
    var ctlr = Controller;
    if (ctlr == null) {
      Debug.LogError("GvrEye must be child of a StereoController.");
      enabled = false;
      return;
    }
    // Save reference to the found controller and it's camera.
    controller = ctlr;
    monoCamera = controller.GetComponent<Camera>();
    SetupStereo(/*forceUpdate=*/true);
  }

  public void UpdateStereoValues() {
    Matrix4x4 proj = GvrViewer.Instance.Projection(eye);
    realProj = GvrViewer.Instance.Projection(eye, GvrViewer.Distortion.Undistorted);

    CopyCameraAndMakeSideBySide(controller, proj[0, 2], proj[1, 2]);

    // Fix aspect ratio and near/far clipping planes.
    float nearClipPlane = monoCamera.nearClipPlane;
    float farClipPlane = monoCamera.farClipPlane;

    GvrCameraUtils.FixProjection(cam.rect, nearClipPlane, farClipPlane, ref proj);
    GvrCameraUtils.FixProjection(cam.rect, nearClipPlane, farClipPlane, ref realProj);

    // Zoom the stereo cameras if requested.
    float monoProj11 = monoCamera.projectionMatrix[1, 1];
    GvrCameraUtils.ZoomStereoCameras(controller.matchByZoom, controller.matchMonoFOV,
                                     monoProj11, ref proj);

    // Set the eye camera's projection for rendering.
    cam.projectionMatrix = proj;
    if (Application.isEditor) {
      // So you can see the approximate frustum in the Scene view when the camera is selected.
      cam.fieldOfView = 2 * Mathf.Atan(1 / proj[1, 1]) * Mathf.Rad2Deg;
    }

    // Draw to the mono camera's target, or the stereo screen.
    cam.targetTexture = monoCamera.targetTexture ?? GvrViewer.Instance.StereoScreen;
    if (cam.targetTexture == null) {
      // When drawing straight to screen, account for lens FOV limits.
      // Note: do this after all calls to FixProjection() which needs the unfixed rect.
      Rect viewport = GvrViewer.Instance.Viewport(eye);
      bool isRightEye = eye == GvrViewer.Eye.Right;
      cam.rect = GvrCameraUtils.FixViewport(cam.rect, viewport, isRightEye);

      // The game window's aspect ratio may not match the device profile parameters.
      if (Application.isEditor) {
        GvrProfile.Screen profileScreen = GvrViewer.Instance.Profile.screen;
        float profileAspect = profileScreen.width / profileScreen.height;
        float windowAspect = (float)Screen.width / Screen.height;
        cam.rect = GvrCameraUtils.FixEditorViewport(cam.rect, profileAspect, windowAspect);
      }
    }
  }

  private void SetupStereo(bool forceUpdate) {
    GvrViewer.Instance.UpdateState();

    bool updateValues = forceUpdate  // Being called from Start(), most likely.
        || controller.keepStereoUpdated  // Parent camera may be animating.
        || GvrViewer.Instance.ProfileChanged  // New QR code.
        || cam.targetTexture == null
            && GvrViewer.Instance.StereoScreen != null ;  // Need to (re)assign targetTexture.
    if (updateValues) {
      // Set projection, viewport and targetTexture.
      UpdateStereoValues();
    }

    // Will need to update view transform if there is a COI, or if there is a remnant of
    // prior stereo-adjustment smoothing to finish off.
    bool haveCOI = controller.centerOfInterest != null
        && controller.centerOfInterest.gameObject.activeInHierarchy;
    if (updateValues || haveCOI || interpPosition < 1) {
      // Set view transform.
      float proj11 = cam.projectionMatrix[1, 1];
      float zScale = transform.lossyScale.z;
      Vector3 eyePos = controller.ComputeStereoEyePosition(eye, proj11, zScale);
      // Apply smoothing only if updating position every frame.
      interpPosition = controller.keepStereoUpdated || haveCOI ?
          Time.deltaTime / (controller.stereoAdjustSmoothing + Time.deltaTime) : 1;
      transform.localPosition = Vector3.Lerp(transform.localPosition, eyePos, interpPosition);
    }

    // Pass necessary information to any shaders doing distortion correction.
    if (GvrViewer.Instance.DistortionCorrection == GvrViewer.DistortionCorrectionMethod.None) {
      // Correction matrix for use in surface shaders that do vertex warping for distortion.
      // Have to compute it every frame because cameraToWorldMatrix is changing constantly.
      var fixProj = cam.cameraToWorldMatrix *
                    Matrix4x4.Inverse(cam.projectionMatrix) *
                    realProj;
      Shader.SetGlobalMatrix("_RealProjection", realProj);
      Shader.SetGlobalMatrix("_FixProjection", fixProj);
      Shader.EnableKeyword("GVR_DISTORTION");
    }
    Shader.SetGlobalFloat("_NearClip", cam.nearClipPlane);
  }

  void OnPreCull() {
    if (!GvrViewer.Instance.VRModeEnabled || !monoCamera.enabled) {
      // Keep stereo enabled flag in sync with parent mono camera.
      cam.enabled = false;
      return;
    }
    SetupStereo(/*forceUpdate=*/false);
    bool doStereoEffect = GvrViewer.Instance.StereoScreen != null;
#if UNITY_IOS
    doStereoEffect &= !controller.directRender;
#endif  // UNITY_IOS
    if (doStereoEffect) {
      // Some image effects clobber the whole screen.  Add a final image effect to the chain
      // which restores side-by-side stereo.
      stereoEffect = GetComponent<StereoRenderEffect>();
      if (stereoEffect == null) {
        stereoEffect = gameObject.AddComponent<StereoRenderEffect>();
      }
      stereoEffect.enabled = true;
    } else if (stereoEffect != null) {
      // Don't need the side-by-side image effect.
      stereoEffect.enabled = false;
    }
  }

  void OnPostRender() {
    Shader.DisableKeyword("GVR_DISTORTION");
  }

  /// Helper to copy camera settings from the controller's mono camera.  Used in SetupStereo() and
  /// in the custom editor for StereoController.  The parameters parx and pary, if not left at
  /// default, should come from a projection matrix returned by the SDK.  They affect the apparent
  /// depth of the camera's window.  See SetupStereo().
  public void CopyCameraAndMakeSideBySide(StereoController controller,
                                          float parx = 0, float pary = 0) {
#if UNITY_EDITOR
    // Member variable 'cam' not always initialized when this method called in Editor.
    // So, we'll just make a local of the same name.
    var cam = GetComponent<Camera>();
#endif
    // Same for controller's camera, but it can happen at runtime too (via AddStereoRig on
    // StereoController).
    var monoCamera =
        controller == this.controller ? this.monoCamera : controller.GetComponent<Camera>();

    float ipd = GvrProfile.Default.viewer.lenses.separation * controller.stereoMultiplier;
    Vector3 localPosition = Application.isPlaying ?
        transform.localPosition : (eye == GvrViewer.Eye.Left ? -ipd/2 : ipd/2) * Vector3.right;;

    // Sync the camera properties.
    cam.CopyFrom(monoCamera);
    cam.cullingMask ^= toggleCullingMask.value;

    // Not sure why we have to do this, but if we don't then switching between drawing to
    // the main screen or to the stereo rendertexture acts very strangely.
    cam.depth = monoCamera.depth;

    // Reset transform, which was clobbered by the CopyFrom() call.
    // Since we are a child of the mono camera, we inherit its transform already.
    transform.localPosition = localPosition;
    transform.localRotation = Quaternion.identity;
    transform.localScale = Vector3.one;

    Skybox monoCameraSkybox = monoCamera.GetComponent<Skybox>();
    Skybox customSkybox = GetComponent<Skybox>();
    if(monoCameraSkybox != null) {
      if (customSkybox == null) {
        customSkybox = gameObject.AddComponent<Skybox>();
      }
      customSkybox.material = monoCameraSkybox.material;
    } else if (customSkybox != null) {
      Destroy(customSkybox);
    }

    // Set up side-by-side stereo.
    // Note: The code is written this way so that non-fullscreen cameras
    // (PIP: picture-in-picture) still work in stereo.  Even if the PIP's content is
    // not going to be in stereo, the PIP itself still has to be rendered in both eyes.
    Rect rect = cam.rect;

    // Move away from edges if padding requested.  Some HMDs make the edges of the
    // screen a bit hard to see.
    Vector2 center = rect.center;
    center.x = Mathf.Lerp(center.x, 0.5f, Mathf.Clamp01(controller.stereoPaddingX));
    center.y = Mathf.Lerp(center.y, 0.5f, Mathf.Clamp01(controller.stereoPaddingY));
    rect.center = center;

    // Semi-hacky aspect ratio adjustment because the screen is only half as wide due
    // to side-by-side stereo, to make sure the PIP width fits.
    float width = Mathf.SmoothStep(-0.5f, 0.5f, (rect.width + 1) / 2);
    rect.x += (rect.width - width) / 2;
    rect.width = width;

    // Divide the outside region of window proportionally in each half of the screen.
    rect.x *= (0.5f - rect.width) / (1 - rect.width);
    if (eye == GvrViewer.Eye.Right) {
      rect.x += 0.5f; // Move to right half of the screen.
    }

    // Adjust the window for requested parallax.  This affects the apparent depth of the
    // window in the main camera's screen.  Useful for PIP windows only, where rect.width < 1.
    float parallax = Mathf.Clamp01(controller.screenParallax);
    if (monoCamera.rect.width < 1 && parallax > 0) {
      // Note: parx and pary are signed, with opposite signs in each eye.
      rect.x -= parx / 4 * parallax; // Extra factor of 1/2 because of side-by-side stereo.
      rect.y -= pary / 2 * parallax;
    }

    cam.rect = rect;
  }
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
}

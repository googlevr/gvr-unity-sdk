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

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// @ingroup Scripts
/// The Cardboard object communicates with the head-mounted display in order to:
/// -  Query the device for viewing parameters
/// -  Retrieve the latest head tracking data
/// -  Provide the rendered scene to the device for distortion correction
///
/// There should only be one of these in a scene.  An instance will be generated
/// at runtime if needed, or you can add one via the Editor if you wish to customize
/// its starting properties.
public class Cardboard : MonoBehaviour {
  /// The singleton instance of the Cardboard class.
  /// Not null: the instance is created on demand if not already present.
  public static Cardboard SDK {
    get {
      if (sdk == null) {
        sdk = UnityEngine.Object.FindObjectOfType<Cardboard>();
      }
      if (sdk == null) {
        Debug.Log("Creating Cardboard object");
        var go = new GameObject("Cardboard");
        sdk = go.AddComponent<Cardboard>();
        go.transform.localPosition = Vector3.zero;
      }
      return sdk;
    }
  }
  private static Cardboard sdk = null;

  /// Whether distortion correction is performed by the plugin.  If disabled, then it is
  /// up to the app to perform whatever distortion correction it would like, if any.
  public bool DistortionCorrection {
    get {
      return distortionCorrection;
    }
    set {
      if (value != distortionCorrection && device != null) {
        device.SetDistortionCorrectionEnabled(value && NativeDistortionCorrectionSupported);
      }
      distortionCorrection = value;
    }
  }
  [SerializeField]
  private bool distortionCorrection = true;

  /// Determine whether the scene renders in stereo or mono.  Assign to this field
  /// to control stereo/mono mode yourself.
  public bool VRModeEnabled {
    get {
      return vrModeEnabled;
    }
    set {
      if (value != vrModeEnabled && device != null) {
        device.SetVRModeEnabled(value);
      }
      vrModeEnabled = value;
    }
  }
  [SerializeField]
  private bool vrModeEnabled = true;

  /// Enables or disables the vertical line rendered between the stereo views to
  /// help the user align the Cardboard to the phone's screen.
  public bool EnableAlignmentMarker {
    get {
      return enableAlignmentMarker;
    }
    set {
      if (value != enableAlignmentMarker && device != null) {
        device.SetAlignmentMarkerEnabled(value && NativeUILayerSupported);
      }
      enableAlignmentMarker = value;
    }
  }
  [SerializeField]
  private bool enableAlignmentMarker = true;

  /// Enables or disables the Cardboard settings button.  It appears as a gear icon
  /// in the blank space between the stereo views.  The settings button opens the
  /// Google Cardboard app to allow the user to configure their individual settings
  /// and Cardboard headset parameters.
  public bool EnableSettingsButton {
    get {
      return enableSettingsButton;
    }
    set {
      if (value != enableSettingsButton && device != null) {
        device.SetSettingsButtonEnabled(value);
      }
      enableSettingsButton = value;
    }
  }
  [SerializeField]
  private bool enableSettingsButton = true;

  /// When enabled, Cardboard treats a screen tap the same as a trigger pull.
  public bool TapIsTrigger {
    get {
      return tapIsTrigger;
    }
    set {
      if (value != tapIsTrigger && device != null) {
        device.SetTapIsTrigger(value);
      }
      tapIsTrigger = value;
    }
  }
  [SerializeField]
  private bool tapIsTrigger = true;

  /// The native SDK will apply a neck offset to the head tracking, resulting in
  /// a more realistic model of a person's head position.  This control determines
  /// the scale factor of the offset.  To turn off the neck model, set it to 0, and
  /// to turn it all on, set to 1.  Intermediate values can be used to animate from
  /// on to off or vice versa.
  public float NeckModelScale {
    get {
      return neckModelScale;
    }
    set {
      value = Mathf.Clamp01(value);
      if (!Mathf.Approximately(value, neckModelScale) && device != null) {
        device.SetNeckModelScale(value);
      }
      neckModelScale = value;
    }
  }
  [SerializeField]
  private float neckModelScale = 0.0f;

  /// When enabled, drift in the gyro readings is estimated and removed.
  public bool AutoDriftCorrection {
    get {
      return autoDriftCorrection;
    }
    set {
      if (value != autoDriftCorrection && device != null) {
        device.SetAutoDriftCorrectionEnabled(value);
      }
      autoDriftCorrection = value;
    }
  }
  [SerializeField]
  private bool autoDriftCorrection = true;

#if UNITY_IOS
  public bool SyncWithCardboardApp {
    get {
      return syncWithCardboardApp;
    }
    set {
      if (value && value != syncWithCardboardApp) {
        Debug.LogWarning("Remember to enable iCloud capability in Xcode, "
            + "and set the 'iCloud Documents' checkbox. "
            + "Not doing this may cause the app to crash if the user tries to sync.");
      }
      syncWithCardboardApp = value;
    }
  }
  [SerializeField]
  private bool syncWithCardboardApp = false;
#endif

#if UNITY_EDITOR
  /// Mock settings for in-editor emulation of Cardboard while playing.
  public bool autoUntiltHead = true;

  /// Use unity remote as the input source.
  [HideInInspector]
  public bool UseUnityRemoteInput = false;

  public CardboardProfile.ScreenSizes ScreenSize {
    get {
      return screenSize;
    }
    set {
      if (value != screenSize) {
        screenSize = value;
        if (device != null) {
          device.UpdateScreenData();
        }
      }
    }
  }
  [SerializeField]
  private CardboardProfile.ScreenSizes screenSize = CardboardProfile.ScreenSizes.Nexus5;

  public CardboardProfile.DeviceTypes DeviceType {
    get {
      return deviceType;
    }
    set {
      if (value != deviceType) {
        deviceType = value;
        if (device != null) {
          device.UpdateScreenData();
        }
      }
    }
  }
  [SerializeField]
  public CardboardProfile.DeviceTypes deviceType = CardboardProfile.DeviceTypes.CardboardJun2014;
#endif

  /// The VR device that will be providing input data.
  private static BaseVRDevice device;

  public bool NativeDistortionCorrectionSupported { get; private set; }

  public bool NativeUILayerSupported { get; private set; }

  /// The texture that Unity renders the scene to. This is sent to the VR device,
  /// which renders it to screen, correcting for lens distortion.
  public RenderTexture StereoScreen {
    get {
      // Don't need it except for distortion correction.
      if (!distortionCorrection || !vrModeEnabled) {
        return null;
      }
      if (stereoScreen == null && NativeDistortionCorrectionSupported) {
        StereoScreen = CreateStereoScreen();  // Note: use set{}
      }
      return stereoScreen;
    }
    set {
      if (value == stereoScreen) {
        return;
      }
      if (!NativeDistortionCorrectionSupported && value != null) {
        Debug.LogError("Can't set StereoScreen: native distortion correction is not supported.");
        return;
      }
      if (stereoScreen != null) {
        stereoScreen.Release();
      }
      stereoScreen = value;
      if (stereoScreen != null && !stereoScreen.IsCreated()) {
        stereoScreen.Create();
      }
      if (device != null) {
        device.SetStereoScreen(stereoScreen);
      }
    }
  }
  private static RenderTexture stereoScreen = null;

  public bool UseDistortionEffect {
    get {
      return !NativeDistortionCorrectionSupported && distortionCorrection && vrModeEnabled
          && SystemInfo.supportsRenderTextures;
    }
  }

  /// Describes the current device, including phone screen.
  public CardboardProfile Profile {
    get {
      return device.Profile;
    }
  }

  /// Distinguish the stereo eyes.
  public enum Eye {
    Left,
    Right,
    Center
  }

  /// When retrieving the _Projection_ and _Viewport_ properties, specifies
  /// whether you want the values as seen through the Cardboard lenses (`Distorted`) or
  /// as if no lenses were present (`Undistorted`).
  public enum Distortion {
    Distorted,   // Viewing through the lenses
    Undistorted  // No lenses
  }

  /// The transformation of head from origin in the tracking system.
  public Pose3D HeadPose {
    get {
      return device.GetHeadPose();
    }
  }

  /// The transformation from head to eye.
  public Pose3D EyePose(Eye eye) {
    return device.GetEyePose(eye);
  }

  /// The projection matrix for a given eye.
  /// This matrix is an off-axis perspective projection with near and far
  /// clipping planes of 1m and 1000m, respectively.  The CardboardEye script
  /// takes care of adjusting the matrix for its particular camera.
  public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted) {
    return device.GetProjection(eye, distortion);
  }

  /// The screen space viewport that the camera for the specified eye should render into.
  /// In the _Distorted_ case, this will be either the left or right half of the `StereoScreen`
  /// render texture.  In the _Undistorted_ case, it refers to the actual rectangle on the
  /// screen that the eye can see.
  public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted) {
    return device.GetViewport(eye, distortion);
  }

  /// The distance range from the viewer in user-space meters where objects
  /// may be viewed comfortably in stereo.  If the center of interest falls
  /// outside this range, the stereo eye separation should be adjusted to
  /// keep the onscreen disparity within the limits set by this range.
  public Vector2 ComfortableViewingRange {
    get {
      return defaultComfortableViewingRange;
    }
  }
  private readonly Vector2 defaultComfortableViewingRange = new Vector2(1.0f, 100000.0f);

  /// @hide
  public Uri DefaultDeviceProfile = null;

  private void InitDevice() {
    if (device != null) {
      device.Destroy();
    }
    device = BaseVRDevice.GetDevice();
    device.Init();

    List<string> diagnostics = new List<string>();
    NativeDistortionCorrectionSupported = device.SupportsNativeDistortionCorrection(diagnostics);
    if (diagnostics.Count > 0) {
      Debug.LogWarning("Built-in distortion correction disabled. Causes: ["
                       + String.Join("; ", diagnostics.ToArray()) + "]");
    }
    diagnostics.Clear();
    NativeUILayerSupported = device.SupportsNativeUILayer(diagnostics);
    if (diagnostics.Count > 0) {
      Debug.LogWarning("Built-in UI layer disabled. Causes: ["
                       + String.Join("; ", diagnostics.ToArray()) + "]");
    }

    if (DefaultDeviceProfile != null) {
      device.SetDefaultDeviceProfile(DefaultDeviceProfile);
    }

    device.SetVRModeEnabled(vrModeEnabled);
    device.SetDistortionCorrectionEnabled(distortionCorrection
        && NativeDistortionCorrectionSupported);
    device.SetAlignmentMarkerEnabled(enableAlignmentMarker
        && NativeUILayerSupported);
    device.SetSettingsButtonEnabled(enableSettingsButton);
    device.SetTapIsTrigger(tapIsTrigger);
    device.SetNeckModelScale(neckModelScale);
    device.SetAutoDriftCorrectionEnabled(autoDriftCorrection);

    device.UpdateScreenData();
  }

  /// @note Each scene load causes an OnDestroy of the current SDK, followed
  /// by and Awake of a new one.  That should not cause the underlying native
  /// code to hiccup.  Exception: developer may call Application.DontDestroyOnLoad
  /// on the SDK if they want it to survive across scene loads.
  void Awake() {
    if (sdk == null) {
      sdk = this;
    }
    if (sdk != this) {
      Debug.LogWarning("Cardboard SDK object should be a singleton.");
      enabled = false;
      return;
    }
#if UNITY_IOS
    Application.targetFrameRate = 60;
#endif
    // Prevent the screen from dimming / sleeping
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    InitDevice();
    AddDummyCamera();
    StereoScreen = null;
  }

  /// Emitted whenever a trigger pull occurs.  If #TapIsTrigger is set, then it is also
  /// emitted when a screen tap occurs.
  public event Action OnTrigger;

  /// Emitted whenever the viewer is tilted on its side.  If #TapIsTrigger is set, the
  /// Escape key issues this as well.
  /// @note On Android, Unity sees the Back button as an Escape key.
  public event Action OnTilt;

  /// Whether the Cardboard trigger was pulled. True for exactly one complete frame
  /// after each pull.
  public bool Triggered { get; private set; }

  /// Whether the Cardboard viewer was tilted on its side. True for exactly one complete frame
  /// after each tilt.  Some apps treat this as a "go back" or "exit scene" action.
  public bool Tilted { get; private set; }

  private bool updated = false;

  private CardboardUILayer uiLayer = null;

  /// Reads the latest tracking data from the phone.  This must be
  /// Called before accessing any of the poses and matrices above.
  ///
  /// Multiple invocations per frame are OK:  Subsequent calls merely yield the
  /// cached results of the first call.  To minimize latency, it should be first
  /// called later in the frame (for example, in `LateUpdate`) if possible.
  public void UpdateState() {
    if (!updated) {
      device.UpdateState();
      updated = true;
    }
  }

  private void DispatchEvents() {
    Triggered = device.triggered || TapIsTrigger && Input.GetMouseButtonDown(0);
    Tilted = device.tilted || TapIsTrigger && Input.GetKeyDown(KeyCode.Escape);
    device.triggered = false;
    device.tilted = false;

    if (Tilted) {
      if (OnTilt != null) {
        OnTilt();
      }
    }
    if (Triggered) {
      if (OnTrigger != null) {
        OnTrigger();
      }
    }
  }

  private void AddDummyCamera() {
    var go = gameObject;
    if (go.GetComponent<Camera>()) {
      go = new GameObject("CardboardDummy");
      go.transform.parent = gameObject.transform;
    }
    var cam = go.AddComponent<Camera>();
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = Color.black;
    cam.cullingMask = 0;
    cam.useOcclusionCulling = false;
    cam.depth = -100;
  }

  IEnumerator EndOfFrame() {
    while (true) {
      yield return new WaitForEndOfFrame();
      UpdateState();
      device.PostRender(vrModeEnabled);
      if (vrModeEnabled && !NativeUILayerSupported) {
        if (uiLayer == null) {
          uiLayer = new CardboardUILayer();
        }
        uiLayer.Draw();
      }
      DispatchEvents();
      updated = false;
    }
  }

  /// Return a StereoScreen with sensible default values.
  public RenderTexture CreateStereoScreen() {
    return device.CreateStereoScreen();
  }

  /// Reset the tracker so that the user's current direction becomes forward.
  public void Recenter() {
    device.Recenter();
  }

  /// Set the screen coordinates of the mouse/touch event.
  public void SetTouchCoordinates(int x, int y) {
    device.SetTouchCoordinates(x, y);
  }

  /// Launch the device pairing and setup dialog.
  public void ShowSettingsDialog() {
    device.ShowSettingsDialog();
  }

  void OnEnable() {
    device.OnPause(false);
    StartCoroutine("EndOfFrame");
  }

  void OnDisable() {
    StopCoroutine("EndOfFrame");
    device.OnPause(true);
  }

  void OnApplicationPause(bool pause) {
    device.OnPause(pause);
  }

  void OnApplicationFocus(bool focus) {
    device.OnFocus(focus);
  }

  void OnLevelWasLoaded(int level) {
    device.Reset();
  }

  void OnDestroy() {
    VRModeEnabled = false;
    if (device != null) {
      device.Destroy();
    }
    if (sdk == this) {
      sdk = null;
    }
  }

  void OnApplicationQuit() {
    device.OnApplicationQuit();
  }

  //********* OBSOLETE ACCESSORS *********

  /// @deprecated Use #DistortionCorrection instead.
  [System.Obsolete("Use DistortionCorrection instead.")]
  public bool nativeDistortionCorrection {
    get { return DistortionCorrection; }
    set { DistortionCorrection = value; }
  }

  /// @deprecated
  [System.Obsolete("InCardboard is deprecated.")]
  public bool InCardboard { get { return true; } }

  /// @deprecated Use #Triggered instead.
  [System.Obsolete("Use Triggered instead.")]
  public bool CardboardTriggered { get { return Triggered; } }

  /// @deprecated Use #HeadPose instead.
  [System.Obsolete("Use HeadPose instead.")]
  public Matrix4x4 HeadView { get { return HeadPose.Matrix; } }

  /// @deprecated Use #HeadPose instead.
  [System.Obsolete("Use HeadPose instead.")]
  public Quaternion HeadRotation { get { return HeadPose.Orientation; } }

  /// @deprecated Use #HeadPose instead.
  [System.Obsolete("Use HeadPose instead.")]
  public Vector3 HeadPosition { get { return HeadPose.Position; } }

  /// @deprecated Use #EyePose instead.
  [System.Obsolete("Use EyePose() instead.")]
  public Matrix4x4 EyeView(Eye eye) {
    return EyePose(eye).Matrix;
  }

  /// @deprecated Use #EyePose instead.
  [System.Obsolete("Use EyePose() instead.")]
  public Vector3 EyeOffset(Eye eye) {
    return EyePose(eye).Position;
  }

  /// @deprecated Use #Projection instead.
  [System.Obsolete("Use Projection() instead.")]
  public Matrix4x4 UndistortedProjection(Eye eye) {
    return Projection(eye, Distortion.Undistorted);
  }

  /// @deprecated Use #Viewport instead.
  [System.Obsolete("Use Viewport() instead.")]
  public Rect EyeRect(Eye eye) {
    return Viewport(eye, Distortion.Distorted);
  }

  /// @deprecated Use #ComfortableViewingRange instead.
  [System.Obsolete("Use ComfortableViewingRange instead.")]
  public float MinimumComfortDistance { get { return ComfortableViewingRange.x; } }

  /// @deprecated Use #ComfortableViewingRange instead.
  [System.Obsolete("Use ComfortableViewingRange instead.")]
  public float MaximumComfortDistance { get { return ComfortableViewingRange.y; } }
}

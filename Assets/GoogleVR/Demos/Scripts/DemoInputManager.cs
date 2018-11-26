// Copyright 2017 Google Inc. All rights reserved.
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

#if UNITY_ANDROID && !UNITY_EDITOR
#define RUNNING_ON_ANDROID_DEVICE
#endif  // UNITY_ANDROID && !UNITY_EDITOR

namespace GoogleVR.Demos {
  using UnityEngine;
  using UnityEngine.UI;
  using System;

#if UNITY_2017_2_OR_NEWER
  using UnityEngine.XR;
#else
  using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

  public class DemoInputManager : MonoBehaviour {
    private const string MESSAGE_CANVAS_NAME = "MessageCanvas";
    private const string MESSAGE_TEXT_NAME = "MessageText";
    private const string LASER_GAMEOBJECT_NAME = "Laser";

    private const string CONTROLLER_CONNECTING_MESSAGE = "Controller connecting...";
    private const string CONTROLLER_DISCONNECTED_MESSAGE = "Controller disconnected";
    private const string CONTROLLER_SCANNING_MESSAGE =  "Controller scanning...";
    private const string NON_GVR_PLATFORM =
      "Please select a supported Google VR platform via 'Build Settings > Android | iOS > Switch Platform'\n";
    private const string VR_SUPPORT_NOT_CHECKED =
      "Please make sure 'Player Settings > Virtual Reality Supported' is checked\n";
    private const string EMPTY_VR_SDK_WARNING_MESSAGE =
      "Please add 'Daydream' or 'Cardboard' under 'Player Settings > Virtual Reality SDKs'\n";

    // Java class, method, and field constants.
    private const int ANDROID_MIN_DAYDREAM_API = 24;
    private const string FIELD_SDK_INT = "SDK_INT";
    private const string PACKAGE_BUILD_VERSION = "android.os.Build$VERSION";
    private const string PACKAGE_DAYDREAM_API_CLASS = "com.google.vr.ndk.base.DaydreamApi";
    private const string METHOD_IS_DAYDREAM_READY = "isDaydreamReadyPlatform";

    private bool isDaydream = false;
    private int activeControllerPointer = 0;
    private static readonly GvrControllerHand[] AllHands = {
      GvrControllerHand.Right,
      GvrControllerHand.Left,
    };
    // Buttons that can trigger pointer switching.
    private const GvrControllerButton pointerButtonMask =
            GvrControllerButton.App |
            GvrControllerButton.TouchPadButton |
            GvrControllerButton.Trigger |
            GvrControllerButton.Grip;

    [Tooltip("Reference to GvrControllerMain")]
    public GameObject controllerMain;
    public static string CONTROLLER_MAIN_PROP_NAME = "controllerMain";

    [Tooltip("Reference to GvrControllerPointers")]
    public GameObject[] controllerPointers;
    public static string CONTROLLER_POINTER_PROP_NAME = "controllerPointers";

    [Tooltip("Reference to GvrReticlePointer")]
    public GameObject reticlePointer;
    public static string RETICLE_POINTER_PROP_NAME = "reticlePointer";

    public GameObject messageCanvas;
    public Text messageText;

#if !RUNNING_ON_ANDROID_DEVICE
    public enum EmulatedPlatformType {
      Daydream,
      Cardboard
    }
    [Tooltip("Emulated GVR Platform")]
    public EmulatedPlatformType gvrEmulatedPlatformType = EmulatedPlatformType.Daydream;
    public static string EMULATED_PLATFORM_PROP_NAME = "gvrEmulatedPlatformType";
#else
    // Running on an Android device.
    private GvrSettings.ViewerPlatformType viewerPlatform;
#endif  // !RUNNING_ON_ANDROID_DEVICE

    void Start() {
      if (messageCanvas == null) {
        messageCanvas = transform.Find(MESSAGE_CANVAS_NAME).gameObject;
        if (messageCanvas != null) {
          messageText = messageCanvas.transform.Find(MESSAGE_TEXT_NAME).GetComponent<Text>();
        }
      }
      // Message canvas will be enabled later when there's a message to display.
      messageCanvas.SetActive(false);
#if !RUNNING_ON_ANDROID_DEVICE
      if (playerSettingsHasDaydream() || playerSettingsHasCardboard()) {
        // The list is populated with valid VR SDK(s), pick the first one.
        gvrEmulatedPlatformType =
          (XRSettings.supportedDevices[0] == GvrSettings.VR_SDK_DAYDREAM) ?
          EmulatedPlatformType.Daydream :
          EmulatedPlatformType.Cardboard;
      }
      isDaydream = (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream);
#else
      // Running on an Android device.
      viewerPlatform = GvrSettings.ViewerPlatform;
      // First loaded device in Player Settings.
      string vrDeviceName = XRSettings.loadedDeviceName;
      if (vrDeviceName != GvrSettings.VR_SDK_CARDBOARD &&
          vrDeviceName != GvrSettings.VR_SDK_DAYDREAM) {
        Debug.LogErrorFormat("Loaded device was '{0}', must be one of '{1}' or '{2}'",
              vrDeviceName, GvrSettings.VR_SDK_DAYDREAM, GvrSettings.VR_SDK_CARDBOARD);
        return;
      }

      // On a non-Daydream ready phone, fall back to Cardboard if it's present in the list of
      // enabled VR SDKs.
      // On a Daydream-ready phone, go into Cardboard mode if it's the currently-paired viewer.
      if ((!IsDeviceDaydreamReady() && playerSettingsHasCardboard()) ||
          (IsDeviceDaydreamReady() && playerSettingsHasCardboard() &&
           GvrSettings.ViewerPlatform == GvrSettings.ViewerPlatformType.Cardboard)) {
        vrDeviceName = GvrSettings.VR_SDK_CARDBOARD;
      }
      isDaydream = (vrDeviceName == GvrSettings.VR_SDK_DAYDREAM);
#endif  // !RUNNING_ON_ANDROID_DEVICE
      SetVRInputMechanism();
    }

    // Runtime switching enabled only in-editor.
    void Update() {
      UpdateStatusMessage();

      // Scan all devices' buttons for button down, and switch the singleton pointer
      // to the controller the user last clicked.
      int newPointer = activeControllerPointer;

      if (controllerPointers.Length > 1 && controllerPointers[1] != null) {
        GvrTrackedController trackedController1 = controllerPointers[1].GetComponent<GvrTrackedController>();
        foreach (var hand in AllHands) {
          GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
          if (device.GetButtonDown(pointerButtonMask)) {
            // Match the button to our own controllerPointers list.
            if (device == trackedController1.ControllerInputDevice) {
              newPointer = 1;
            } else {
              newPointer = 0;
            }
            break;
          }
        }
      }
      if (newPointer != activeControllerPointer) {
        activeControllerPointer = newPointer;
        SetVRInputMechanism();
      }

#if !RUNNING_ON_ANDROID_DEVICE
      UpdateEmulatedPlatformIfPlayerSettingsChanged();
      if ((isDaydream && gvrEmulatedPlatformType == EmulatedPlatformType.Daydream) ||
          (!isDaydream && gvrEmulatedPlatformType == EmulatedPlatformType.Cardboard)) {
        return;
      }
      isDaydream = (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream);
      SetVRInputMechanism();
#else
      // Running on an Android device.
      // Viewer type switched at runtime.
      if (!IsDeviceDaydreamReady() || viewerPlatform == GvrSettings.ViewerPlatform) {
        return;
      }
      isDaydream = (GvrSettings.ViewerPlatform == GvrSettings.ViewerPlatformType.Daydream);
      viewerPlatform = GvrSettings.ViewerPlatform;
      SetVRInputMechanism();
#endif  // !RUNNING_ON_ANDROID_DEVICE
    }

    public bool IsCurrentlyDaydream() {
      return isDaydream;
    }

    public static bool playerSettingsHasDaydream() {
      string[] playerSettingsVrSdks = XRSettings.supportedDevices;
      return Array.Exists<string>(playerSettingsVrSdks,
          element => element.Equals(GvrSettings.VR_SDK_DAYDREAM));
    }

    public static bool playerSettingsHasCardboard() {
      string[] playerSettingsVrSdks = XRSettings.supportedDevices;
      return Array.Exists<string>(playerSettingsVrSdks,
          element => element.Equals(GvrSettings.VR_SDK_CARDBOARD));
    }

#if !RUNNING_ON_ANDROID_DEVICE
    private void UpdateEmulatedPlatformIfPlayerSettingsChanged() {
      if (!playerSettingsHasDaydream() && !playerSettingsHasCardboard()) {
        return;
      }

      // Player Settings > VR SDK list may have changed at runtime. The emulated platform
      // may not have been manually updated if that's the case.
      if (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream &&
          !playerSettingsHasDaydream()) {
        gvrEmulatedPlatformType = EmulatedPlatformType.Cardboard;
      } else if (gvrEmulatedPlatformType == EmulatedPlatformType.Cardboard &&
                 !playerSettingsHasCardboard()) {
        gvrEmulatedPlatformType = EmulatedPlatformType.Daydream;
      }
    }
#endif  // !RUNNING_ON_ANDROID_DEVICE

#if RUNNING_ON_ANDROID_DEVICE
    // Running on an Android device.
    private static bool IsDeviceDaydreamReady() {
      // Check API level.
      using (var version = new AndroidJavaClass(PACKAGE_BUILD_VERSION)) {
        if (version.GetStatic<int>(FIELD_SDK_INT) < ANDROID_MIN_DAYDREAM_API) {
          return false;
        }
      }
      // API level > 24, check whether the device is Daydream-ready..
      AndroidJavaObject androidActivity = null;
      try {
        androidActivity = GvrActivityHelper.GetActivity();
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception while connecting to the Activity: " + e);
        return false;
      }
      AndroidJavaClass daydreamApiClass = new AndroidJavaClass(PACKAGE_DAYDREAM_API_CLASS);
      if (daydreamApiClass == null || androidActivity == null) {
        return false;
      }
      return daydreamApiClass.CallStatic<bool>(METHOD_IS_DAYDREAM_READY, androidActivity);
    }
#endif  // RUNNING_ON_ANDROID_DEVICE

    private void UpdateStatusMessage() {
      if (messageText == null || messageCanvas == null) {
        return;
      }

#if !UNITY_ANDROID && !UNITY_IOS
      messageText.text = NON_GVR_PLATFORM;
      messageCanvas.SetActive(true);
      return;
#else
#if UNITY_EDITOR
      if (!UnityEditor.PlayerSettings.virtualRealitySupported) {
        messageText.text = VR_SUPPORT_NOT_CHECKED;
        messageCanvas.SetActive(true);
        return;
      }
#endif  // UNITY_EDITOR

      bool isVrSdkListEmpty = !playerSettingsHasCardboard() && !playerSettingsHasDaydream();
      if (!isDaydream) {
        if (messageCanvas.activeSelf) {
          messageText.text = EMPTY_VR_SDK_WARNING_MESSAGE;
          messageCanvas.SetActive(isVrSdkListEmpty);
        }
        return;
      }

      string vrSdkWarningMessage = isVrSdkListEmpty ? EMPTY_VR_SDK_WARNING_MESSAGE : "";
      string controllerMessage = "";
      GvrPointerGraphicRaycaster graphicRaycaster =
        messageCanvas.GetComponent<GvrPointerGraphicRaycaster>();
      GvrControllerInputDevice dominantDevice = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
      GvrConnectionState connectionState = dominantDevice.State;
      // This is an example of how to process the controller's state to display a status message.
      switch (connectionState) {
        case GvrConnectionState.Connected:
          break;
        case GvrConnectionState.Disconnected:
          controllerMessage = CONTROLLER_DISCONNECTED_MESSAGE;
          messageText.color = Color.white;
          break;
        case GvrConnectionState.Scanning:
          controllerMessage = CONTROLLER_SCANNING_MESSAGE;
          messageText.color = Color.cyan;
          break;
        case GvrConnectionState.Connecting:
          controllerMessage = CONTROLLER_CONNECTING_MESSAGE;
          messageText.color = Color.yellow;
          break;
        case GvrConnectionState.Error:
          controllerMessage = "ERROR: " + dominantDevice.ErrorDetails;
          messageText.color = Color.red;
          break;
        default:
          // Shouldn't happen.
          Debug.LogError("Invalid controller state: " + connectionState);
          break;
      }
      messageText.text = string.Format("{0}\n{1}", vrSdkWarningMessage, controllerMessage);
      if (graphicRaycaster != null) {
        graphicRaycaster.enabled =
          !isVrSdkListEmpty || connectionState != GvrConnectionState.Connected;
      }
      messageCanvas.SetActive(isVrSdkListEmpty ||
                              (connectionState != GvrConnectionState.Connected));
#endif  // !UNITY_ANDROID && !UNITY_IOS
    }

    private void SetVRInputMechanism() {
      SetGazeInputActive(!isDaydream);
      SetControllerInputActive(isDaydream);
    }

    private void SetGazeInputActive(bool active) {
      if (reticlePointer == null) {
        return;
      }
      reticlePointer.SetActive(active);

      // Update the pointer type only if this is currently activated.
      if (!active) {
        return;
      }

      GvrReticlePointer pointer =
          reticlePointer.GetComponent<GvrReticlePointer>();
      if (pointer != null) {
        GvrPointerInputModule.Pointer = pointer;
      }
    }

    private void SetControllerInputActive(bool active) {
      if (controllerMain != null) {
        controllerMain.SetActive(active);
      }
      if (controllerPointers == null || controllerPointers.Length <= activeControllerPointer) {
        return;
      }
      controllerPointers[activeControllerPointer].SetActive(active);

      // Update the pointer type only if this is currently activated.
      if (!active) {
        return;
      }
      GvrLaserPointer pointer =
          controllerPointers[activeControllerPointer].GetComponentInChildren<GvrLaserPointer>(true);
      if (pointer != null) {
        GvrPointerInputModule.Pointer = pointer;
      }
    }
  }
}

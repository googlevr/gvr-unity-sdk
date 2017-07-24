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

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using System.Collections;

/// Provides visual feedback for the daydream controller.
[RequireComponent(typeof(Renderer))]
public class GvrControllerVisual : GvrBaseControllerVisual {

  [System.Serializable]
  public struct ControllerDisplayState {

    public GvrControllerBatteryLevel batteryLevel;
    public bool batteryCharging;

    public bool clickButton;
    public bool appButton;
    public bool homeButton;
    public bool touching;
    public Vector2 touchPos;
  }

  /// An array of prefabs that will be instantiated and added as children
  /// of the controller visual when the controller is created. Used to
  /// attach tooltips or other additional visual elements to the control dynamically.
  [SerializeField]
  private GameObject[] attachmentPrefabs;
  [SerializeField] private Color touchPadColor =
      new Color(200f / 255f, 200f / 255f, 200f / 255f, 1);
  [SerializeField] private Color appButtonColor =
      new Color(200f / 255f, 200f / 255f, 200f / 255f, 1);
  [SerializeField] private Color systemButtonColor =
      new Color(20f / 255f, 20f / 255f, 20f / 255f, 1);

  /// Determines if the displayState is set from GvrControllerInput.
  public bool readControllerState = true;

  /// Used to set the display state of the controller visual.
  /// This can be used for tutorials that visualize the controller or other use-cases that require
  /// displaying the controller visual without the state being determined by controller input.
  /// Additionally, it can be used to preview the controller visual in the editor.
  /// NOTE: readControllerState must be disabled to set the display state.
  public ControllerDisplayState displayState;

  public Color TouchPadColor {
    get {
      return touchPadColor;
    }
    set {
      touchPadColor = value;
      if(materialPropertyBlock != null) {
        materialPropertyBlock.SetColor(touchPadId, touchPadColor);
      }
    }
  }

  public Color AppButtonColor {
    get {
      return appButtonColor;
    }
    set {
      appButtonColor = value;
      if(materialPropertyBlock != null){
        materialPropertyBlock.SetColor(appButtonId, appButtonColor);
      }
    }
  }

  public Color SystemButtonColor {
    get {
      return systemButtonColor;
    }
    set {
      systemButtonColor = value;
      if(materialPropertyBlock != null) {
        materialPropertyBlock.SetColor(systemButtonId, systemButtonColor);
      }
    }
  }

  private Renderer controllerRenderer;
  private MaterialPropertyBlock materialPropertyBlock;

  private int alphaId;
  private int touchId;
  private int touchPadId;
  private int appButtonId;
  private int systemButtonId;

  private bool wasTouching;
  private float touchTime;

  // Data passed to shader, (xy) touch position, (z) touch duration, (w) battery state.
  private Vector4 controllerShaderData;
  // Data passed to shader, (x) overall alpha, (y) touchpad click duration,
  //  (z) app button click duration, (w) system button click duration.
  private Vector4 controllerShaderData2;

  // These values control animation times for the controller buttons
  public const float APP_BUTTON_ACTIVE_DURATION_SECONDS = 0.111f;
  public const float APP_BUTTON_RELEASE_DURATION_SECONDS = 0.0909f;

  public const float SYSTEM_BUTTON_ACTIVE_DURATION_SECONDS = 0.111f;
  public const float SYSTEM_BUTTON_RELEASE_DURATION_SECONDS = 0.0909f;

  public const float TOUCHPAD_CLICK_DURATION_SECONDS = 0.111f;
  public const float TOUCHPAD_RELEASE_DURATION_SECONDS = 0.0909f;

  public const float TOUCHPAD_CLICK_SCALE_DURATION_SECONDS = 0.075f;
  public const float TOUCHPAD_POINT_SCALE_DURATION_SECONDS = 0.15f;

  // These values are used by the shader to control battery display
  // Only modify these values if you are also modifying the shader.
  private const float BATTERY_FULL = 1;
  private const float BATTERY_ALMOST_FULL = .8f;
  private const float BATTERY_MEDIUM = .6f;
  private const float BATTERY_LOW = .4f;
  private const float BATTERY_CRITICAL = .2f;
  private const float BATTERY_HIDDEN = 0;

  // How much time to use as an 'immediate update'.
  // Any value large enough to instantly update all visual animations.
  private const float IMMEDIATE_UPDATE_TIME = 10f;

  protected override void Awake() {
    base.Awake();
    Initialize();
    CreateAttachments();
  }

  private void OnValidate() {
    if (!Application.isPlaying) {
      Initialize();
      OnVisualUpdate(true);
    }
  }

  private void CreateAttachments() {
    if (attachmentPrefabs == null) {
      return;
    }

    for (int i = 0; i < attachmentPrefabs.Length; i++) {
      GameObject prefab = attachmentPrefabs[i];
      GameObject attachment = Instantiate(prefab);
      attachment.transform.SetParent(transform, false);
    }
  }

  private void Initialize() {
    if(controllerRenderer == null) {
      controllerRenderer = GetComponent<Renderer>();
    }
    if(materialPropertyBlock == null) {
      materialPropertyBlock = new MaterialPropertyBlock();
    }

    alphaId = Shader.PropertyToID("_GvrControllerAlpha");
    touchId = Shader.PropertyToID("_GvrTouchInfo");
    touchPadId = Shader.PropertyToID("_GvrTouchPadColor");
    appButtonId = Shader.PropertyToID("_GvrAppButtonColor");
    systemButtonId = Shader.PropertyToID("_GvrSystemButtonColor");

    materialPropertyBlock.SetColor(appButtonId, appButtonColor);
    materialPropertyBlock.SetColor(systemButtonId, systemButtonColor);
    materialPropertyBlock.SetColor(touchPadId, touchPadColor);
    controllerRenderer.SetPropertyBlock(materialPropertyBlock);
  }

  public void UpdateControllerState() {
    // Return early when the application isn't playing to ensure that the serialized displayState
    // is used to preview the controller visual instead of the default GvrControllerInput values.
#if UNITY_EDITOR
    if (!Application.isPlaying) {
      return;
    }
#endif

    displayState.batteryLevel = GvrControllerInput.BatteryLevel;
    displayState.batteryCharging = GvrControllerInput.IsCharging;

    displayState.clickButton = GvrControllerInput.ClickButton;
    displayState.appButton = GvrControllerInput.AppButton;
    displayState.homeButton = GvrControllerInput.HomeButtonState;
    displayState.touching = GvrControllerInput.IsTouching;
    displayState.touchPos = GvrControllerInput.TouchPosCentered;
  }

  public override void OnVisualUpdate(bool updateImmediately = false) {

    // Update the visual display based on the controller state
    if(readControllerState) {
      UpdateControllerState();
    }

    float deltaTime = Time.deltaTime;

    // If flagged to update immediately, set deltaTime to an arbitrarily large value
    // This is particularly useful in editor, but also for resetting state quickly
    if(updateImmediately) {
      deltaTime = IMMEDIATE_UPDATE_TIME;
    }

    if (displayState.clickButton) {
      controllerShaderData2.y = Mathf.Min(1, controllerShaderData2.y + deltaTime / TOUCHPAD_CLICK_DURATION_SECONDS);
    } else{
      controllerShaderData2.y = Mathf.Max(0, controllerShaderData2.y - deltaTime / TOUCHPAD_RELEASE_DURATION_SECONDS);
    }

    if (displayState.appButton) {
      controllerShaderData2.z = Mathf.Min(1, controllerShaderData2.z + deltaTime / APP_BUTTON_ACTIVE_DURATION_SECONDS);
    } else{
      controllerShaderData2.z = Mathf.Max(0, controllerShaderData2.z - deltaTime / APP_BUTTON_RELEASE_DURATION_SECONDS);
    }

    if (displayState.homeButton) {
      controllerShaderData2.w = Mathf.Min(1, controllerShaderData2.w + deltaTime / SYSTEM_BUTTON_ACTIVE_DURATION_SECONDS);
    } else {
      controllerShaderData2.w = Mathf.Max(0, controllerShaderData2.w - deltaTime / SYSTEM_BUTTON_RELEASE_DURATION_SECONDS);
    }

    // Set the material's alpha to the multiplied preferred alpha.
    controllerShaderData2.x = maximumAlpha * PreferredAlpha;
    materialPropertyBlock.SetVector(alphaId, controllerShaderData2);

    controllerShaderData.x = displayState.touchPos.x;
    controllerShaderData.y = displayState.touchPos.y;

    if (displayState.touching || displayState.clickButton) {
      if (!wasTouching) {
        wasTouching = true;
      }
      if(touchTime < 1) {
        touchTime = Mathf.Min(touchTime + deltaTime / TOUCHPAD_POINT_SCALE_DURATION_SECONDS, 1);
      }
    } else {
      wasTouching = false;
      if(touchTime > 0) {
        touchTime = Mathf.Max(touchTime - deltaTime / TOUCHPAD_POINT_SCALE_DURATION_SECONDS, 0);
      }
    }

    controllerShaderData.z = touchTime;

    UpdateBatteryIndicator();

    materialPropertyBlock.SetVector(touchId, controllerShaderData);

    // Update the renderer
    controllerRenderer.SetPropertyBlock(materialPropertyBlock);
  }

  private void UpdateBatteryIndicator() {

    GvrControllerBatteryLevel level = displayState.batteryLevel;
    bool charging = displayState.batteryCharging;

    switch (level) {
      case GvrControllerBatteryLevel.Full:
        controllerShaderData.w = BATTERY_FULL;
      break;
      case GvrControllerBatteryLevel.AlmostFull:
        controllerShaderData.w = BATTERY_ALMOST_FULL;
      break;
      case GvrControllerBatteryLevel.Medium:
        controllerShaderData.w = BATTERY_MEDIUM;
      break;
      case GvrControllerBatteryLevel.Low:
        controllerShaderData.w = BATTERY_LOW;
      break;
      case GvrControllerBatteryLevel.CriticalLow:
        controllerShaderData.w = BATTERY_CRITICAL;
      break;
      default:
        controllerShaderData.w = BATTERY_HIDDEN;
      break;
    }

    if (charging) {
      controllerShaderData.w = -controllerShaderData.w;
    }
  }

}

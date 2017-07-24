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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// A tooltip for displaying control schemes overlaying the controller visual using a Unity Canvas.
/// Automatically changes what side of the controller the tooltip is shown on depending
/// on the handedness setting for the player.
/// Automatically fades out when the controller visual is too close or too far
/// away from the player's head.
/// Look at the prefab GvrControllerPointer to see an example of how to use this script.
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class GvrTooltip : MonoBehaviour, IGvrArmModelReceiver {
  /// Rotation for a tooltip when it is displayed on the right side of the controller visual.
  protected static readonly Quaternion RIGHT_SIDE_ROTATION = Quaternion.Euler(0.0f, 0.0f, 0.0f);

  /// Rotation for a tooltip when it is displayed on the left side of the controller visual.
  protected static readonly Quaternion LEFT_SIDE_ROTATION = Quaternion.Euler(0.0f, 0.0f, 180.0f);

  /// Anchor point for a tooltip, used for controlling what side the tooltip is on.
  protected static readonly Vector2 SQUARE_CENTER = new Vector2(0.5f, 0.5f);

  /// Pivot point for a tooltip, used for controlling what side the tooltip is on.
  protected static readonly Vector2 PIVOT = new Vector2(-0.5f, 0.5f);

  /// Y Position for touch pad tooltips based on the standard controller visual.
  protected const float TOUCH_PAD_Y_POSITION_METERS = 0.0385f;

  /// Y position for app button tooltips based on the standard controller visual.
  protected const float APP_BUTTON_Y_POSITION_METERS = 0.0105f;

  /// Z position for all tooltips based on the standard controller visual.
  protected const float TOOLTIP_Z_POSITION_METERS = 0.0098f;

  /// Options for where the controller should be displayed.
  /// If set to custom, then the manually set localPosition of the tooltip is used.
  /// This is useful when displaying a tooltip for a non-standard controller visual.
  enum Location {
    TouchPadOutside,
    TouchPadInside,
    AppButtonOutside,
    AppButtonInside,
    Custom
  };

  [Tooltip("The location to display the tooltip at relative to the controller visual.")]
  [SerializeField]
  private Location location;

  [Tooltip("The text field for this tooltip.")]
  [SerializeField]
  private Text text;

  [Tooltip("Determines if the tooltip is always visible regardless of the controller's location.")]
  [SerializeField]
  private bool alwaysVisible;

  private bool isOnLeft = false;
  private RectTransform rectTransform;
  private CanvasGroup canvasGroup;

  /// The text field for this tooltip.
  public Text TooltipText {
    get {
      return text;
    }
  }

  public GvrBaseArmModel ArmModel { get; set; }

  void Awake() {
    rectTransform = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    isOnLeft = IsTooltipOnLeft();
    RefreshTooltip();
  }

  void OnEnable() {
    // Update using OnPostControllerInputUpdated.
    // This way, the position and rotation will be correct for the entire frame
    // so that it doesn't matter what order Updates get called in.
    if (Application.isPlaying) {
      GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
    }
  }

  void OnDisable() {
    GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
  }

  private void OnPostControllerInputUpdated() {
    CheckTooltipSide();

    if (canvasGroup != null && ArmModel != null) {
      canvasGroup.alpha = alwaysVisible ? 1.0f : ArmModel.TooltipAlphaValue;
    }
  }

  void OnValidate() {
    rectTransform = GetComponent<RectTransform>();
    RefreshTooltip();
  }

#if UNITY_EDITOR
  void OnRenderObject() {
    if (!Application.isPlaying) {
      CheckTooltipSide();
    }
  }
#endif  // UNITY_EDITOR

  /// Returns true if this tooltip is set to display on the inside of the controller.
  public bool IsTooltipInside() {
    switch (location) {
      case Location.TouchPadInside:
      case Location.AppButtonInside:
      case Location.Custom:
        return true;
      case Location.TouchPadOutside:
      case Location.AppButtonOutside:
      default:
        return false;
    }
  }

  /// Returns true if the tooltip should display on the left side of the controller.
  /// This will change based on the handedness of the controller, as well as if the
  /// tooltip is set to display inside or outside.
  public bool IsTooltipOnLeft() {
    bool isInside = IsTooltipInside();
    GvrSettings.UserPrefsHandedness handedness = GvrSettings.Handedness;

    if (handedness == GvrSettings.UserPrefsHandedness.Left) {
      return !isInside;
    } else {
      return isInside;
    }
  }

  /// Refreshes how the tooltip is being displayed based on what side it is being shown on.
  /// Override to add custom display functionality.
  protected virtual void OnSideChanged(bool IsLocationOnLeft) {
    transform.localRotation = (isOnLeft ? LEFT_SIDE_ROTATION : RIGHT_SIDE_ROTATION);

    if (text != null) {
      text.transform.localRotation = (IsLocationOnLeft ? LEFT_SIDE_ROTATION : RIGHT_SIDE_ROTATION);
      text.alignment = (IsLocationOnLeft ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
    }
  }

  protected float GetMetersToCanvasScale() {
    return GvrUIHelpers.GetMetersToCanvasScale(transform);
  }

  private Vector3 GetLocalPosition() {
    float metersToCanvasScale = GetMetersToCanvasScale();

    // Return early if we didn't find a valid metersToCanvasScale.
    if (metersToCanvasScale == 0.0f) {
      return rectTransform.anchoredPosition3D;
    }

    float tooltipZPosition = TOOLTIP_Z_POSITION_METERS / metersToCanvasScale;
    switch (location) {
      case Location.TouchPadOutside:
      case Location.TouchPadInside:
        float touchPadYPosition = TOUCH_PAD_Y_POSITION_METERS / metersToCanvasScale;
        return new Vector3(0.0f, touchPadYPosition, tooltipZPosition);
      case Location.AppButtonOutside:
      case Location.AppButtonInside:
        float appButtonYPosition = APP_BUTTON_Y_POSITION_METERS / metersToCanvasScale;
        return new Vector3(0.0f, appButtonYPosition, tooltipZPosition);
      case Location.Custom:
      default:
        return rectTransform.anchoredPosition3D;
    }
  }

  private void CheckTooltipSide() {
    // If handedness changes, the tooltip will switch sides.
    bool newIsOnLeft = IsTooltipOnLeft();
    if (newIsOnLeft != isOnLeft) {
      isOnLeft = newIsOnLeft;
      RefreshTooltip();
    }
  }

  private void RefreshTooltip() {
    rectTransform.anchorMax = SQUARE_CENTER;
    rectTransform.anchorMax = SQUARE_CENTER;
    rectTransform.pivot = PIVOT;
    rectTransform.anchoredPosition3D = GetLocalPosition();
    OnSideChanged(isOnLeft);
  }
}

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

// Tooltips are not available for versions of Unity without the
// GVR native integration.
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.VR;

/// Creates the ToolTip text around the controller and controls its animation.
[RequireComponent(typeof(CanvasGroup))]
public class GvrToolTips : MonoBehaviour {

  private static readonly Quaternion RIGHT_SIDE_ROTATION = Quaternion.Euler(0.0f, 0.0f, 0.0f);
  private static readonly Quaternion LEFT_SIDE_ROTATION = Quaternion.Euler(0.0f, 0.0f, 180.0f);
  private static readonly Vector2 SQUARE_CENTER = new Vector2(0.5f, 0.5f);

  /// Amount of normalized alpha transparency to change per second.
  private const float DELTA_ALPHA = 4.0f;

  private bool bVisible = true;
  private GvrSettings.UserPrefsHandedness handedness = GvrSettings.UserPrefsHandedness.Error;

  private CanvasGroup canvasGroup;

  public GameObject touchPadOutsideTooltip;
  public GameObject touchPadInsideTooltip;
  public GameObject appButtonOutsideTooltip;
  public GameObject appButtonInsideTooltip;

  public GameObject touchPadOutsideText;
  public GameObject touchPadInsideText;
  public GameObject appButtonOutsideText;
  public GameObject appButtonInsideText;

  private Vector3 GetHeadForward() {
#if UNITY_EDITOR
    return GvrViewer.Instance.HeadPose.Orientation * Vector3.forward;
#else
    return InputTracking.GetLocalRotation(VRNode.Head) * Vector3.forward;
#endif // UNITY_EDITOR
  }

  void Start() {
    canvasGroup = GetComponent<CanvasGroup>();
  }

  void Update () {
    // If handedness changed, place Tooltips on the correct side of the controller.
    if (handedness != GvrSettings.Handedness) {
      handedness = GvrSettings.Handedness;
      ShowRightLeft();
    }

    // Show tooltips if the controller is in the FOV or if the controller angle is high enough.
    float controllerAngleToFront = Vector3.Angle(GvrController.Orientation * Vector3.down, GetHeadForward());
    bVisible = (controllerAngleToFront < 50.0f);

    float currentAlpha = canvasGroup.alpha;
    if (bVisible) {
      currentAlpha = Mathf.Min(1.0f, currentAlpha + DELTA_ALPHA * Time.deltaTime);
    } else {
      currentAlpha = Mathf.Max(0.0f, currentAlpha - DELTA_ALPHA * Time.deltaTime);
    }

    currentAlpha = Mathf.Min(currentAlpha, GvrArmModel.Instance.alphaValue);
    canvasGroup.alpha = currentAlpha;
  }

  /// Forces the text to be on a particular side of the controller.
  private static void ForceSide(GameObject obj, GameObject objText, bool left) {
    obj.transform.localRotation = (left ? LEFT_SIDE_ROTATION : RIGHT_SIDE_ROTATION);
    objText.transform.localRotation = (left ? LEFT_SIDE_ROTATION : RIGHT_SIDE_ROTATION);
    objText.GetComponent<Text>().alignment = (left ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
  }

  private void ShowRightLeft() {
    // Place the pivot on the center.
    touchPadOutsideText.GetComponent<RectTransform>().pivot = SQUARE_CENTER;
    touchPadInsideText.GetComponent<RectTransform>().pivot = SQUARE_CENTER;
    appButtonOutsideText.GetComponent<RectTransform>().pivot = SQUARE_CENTER;
    appButtonInsideText.GetComponent<RectTransform>().pivot = SQUARE_CENTER;

    if (handedness == GvrSettings.UserPrefsHandedness.Right) {
      // Place the tooltips for right hand.
      ForceSide(touchPadOutsideTooltip, touchPadOutsideText, false);
      ForceSide(appButtonOutsideTooltip, appButtonOutsideText, false);
      ForceSide(touchPadInsideTooltip, touchPadInsideText, true);
      ForceSide(appButtonInsideTooltip, appButtonInsideText, true);
    } else {
      // Place the tooltips for left hand.
      ForceSide(touchPadOutsideTooltip, touchPadOutsideText, true);
      ForceSide(appButtonOutsideTooltip, appButtonOutsideText, true);
      ForceSide(touchPadInsideTooltip, touchPadInsideText, false);
      ForceSide(appButtonInsideTooltip, appButtonInsideText, false);
    }
  }
}

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

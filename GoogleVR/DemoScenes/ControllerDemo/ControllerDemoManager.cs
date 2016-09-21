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
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;
using UnityEngine.UI;

public class ControllerDemoManager : MonoBehaviour {
  public GameObject controllerPivot;
  public GameObject messageCanvas;
  public Text messageText;

  public Material cubeInactiveMaterial;
  public Material cubeHoverMaterial;
  public Material cubeActiveMaterial;

  private Renderer controllerCursorRenderer;

  // Currently selected GameObject.
  private GameObject selectedObject;

  // True if we are dragging the currently selected GameObject.
  private bool dragging;

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
  void Awake() {
  }

  void Update() {
    UpdatePointer();
    UpdateStatusMessage();
  }

  private void UpdatePointer() {
    if (GvrController.State != GvrConnectionState.Connected) {
      controllerPivot.SetActive(false);
    }
    controllerPivot.SetActive(true);
    controllerPivot.transform.rotation = GvrController.Orientation;

    if (dragging) {
      if (GvrController.TouchUp) {
        EndDragging();
      }
    } else {
      RaycastHit hitInfo;
      Vector3 rayDirection = GvrController.Orientation * Vector3.forward;
      if (Physics.Raycast(Vector3.zero, rayDirection, out hitInfo)) {
        if (hitInfo.collider && hitInfo.collider.gameObject) {
          SetSelectedObject(hitInfo.collider.gameObject);
        }
      } else {
        SetSelectedObject(null);
      }
      if (GvrController.TouchDown && selectedObject != null) {
        StartDragging();
      }
    }
  }

  private void SetSelectedObject(GameObject obj) {
    if (null != selectedObject) {
      selectedObject.GetComponent<Renderer>().material = cubeInactiveMaterial;
    }
    if (null != obj) {
      obj.GetComponent<Renderer>().material = cubeHoverMaterial;
    }
    selectedObject = obj;
  }

  private void StartDragging() {
    dragging = true;
    selectedObject.GetComponent<Renderer>().material = cubeActiveMaterial;

    // Reparent the active cube so it's part of the ControllerPivot object. That will
    // make it move with the controller.
    selectedObject.transform.SetParent(controllerPivot.transform, true);
  }

  private void EndDragging() {
    dragging = false;
    selectedObject.GetComponent<Renderer>().material = cubeHoverMaterial;

    // Stop dragging the cube along.
    selectedObject.transform.SetParent(null, true);
  }

  private void UpdateStatusMessage() {
    // This is an example of how to process the controller's state to display a status message.
    switch (GvrController.State) {
      case GvrConnectionState.Connected:
        messageCanvas.SetActive(false);
        break;
      case GvrConnectionState.Disconnected:
        messageText.text = "Controller disconnected.";
        messageText.color = Color.white;
        messageCanvas.SetActive(true);
        break;
      case GvrConnectionState.Scanning:
        messageText.text = "Controller scanning...";
        messageText.color = Color.cyan;
        messageCanvas.SetActive(true);
        break;
      case GvrConnectionState.Connecting:
        messageText.text = "Controller connecting...";
        messageText.color = Color.yellow;
        messageCanvas.SetActive(true);
        break;
      case GvrConnectionState.Error:
        messageText.text = "ERROR: " + GvrController.ErrorDetails;
        messageText.color = Color.red;
        messageCanvas.SetActive(true);
        break;
      default:
        // Shouldn't happen.
        Debug.LogError("Invalid controller state: " + GvrController.State);
        break;
    }
  }
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
}

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

using UnityEngine;
using System.Linq;

/// Class that can perform gaze-based selection, as a simple alternative to the
/// more complicated path of using _GazeInputModule_ and the rest of **uGUI**.
[RequireComponent(typeof(Camera))]
public class GvrGaze : MonoBehaviour {
  /// The active Gaze Pointer for this camera. Must have IGvrPointer.
  /// The IGvrPointer responds to events from this class.
  public GameObject PointerObject {
    get {
      return pointerObject;
    }
    set {
      if (value != null) {
        // Retrieve the IGvrPointer component.
        var ptr = value.GetComponents<MonoBehaviour>()
            .Select(c => c as IGvrPointer)
            .Where(c => c != null)
            .FirstOrDefault();

        if (ptr != null) {
          if (pointer != null) {
            if (isTriggered) {
              pointer.OnPointerClickUp();
            }
            if (currentGazeObject != null) {
              pointer.OnPointerExit(currentGazeObject);
            }
            pointer.OnInputModuleDisabled();
          }
          pointerObject = value;
          pointer = ptr;
          pointer.OnInputModuleEnabled();
          if (currentGazeObject != null) {
            pointer.OnPointerEnter(currentGazeObject, lastIntersectPosition,
                lastIntersectionRay, currentTarget != null);
          }
          if (isTriggered) {
            pointer.OnPointerClickDown();
          }
        } else {
          Debug.LogError("Object must have component which implements IGvrPointer.");
        }
      } else {
        if (pointer != null) {
          if (isTriggered) {
            pointer.OnPointerClickUp();
          }
          if (currentTarget != null) {
            pointer.OnPointerExit(currentGazeObject);
          }
        }
        pointer = null;
        pointerObject = null;
      }
    }
  }
  [SerializeField][HideInInspector]
  private GameObject pointerObject;
  private IGvrPointer pointer;

  // Convenient accessor to the camera component used throughout this script.
  public Camera cam { get; private set; }

  /// The layers to use for finding objects which intersect the user's gaze.
  public LayerMask mask = -1;

  // Current target detected the user is "gazing" at.
  private IGvrGazeResponder currentTarget;
  private GameObject currentGazeObject;

  private Vector3 lastIntersectPosition;
  private Ray lastIntersectionRay;

  // Trigger state.
  private bool isTriggered;

  void Awake() {
    cam = GetComponent<Camera>();
    PointerObject = pointerObject;
  }

  void OnEnable() {
    if (pointer != null) {
      pointer.OnInputModuleEnabled();
    }
  }

  void OnDisable() {
    // Is there a current target?
    if (currentTarget != null) {
      currentTarget.OnGazeExit();
    }
    // Tell pointer to exit target.
    if (pointer != null) {
      // Is there a pending trigger?
      if (isTriggered) {
        pointer.OnPointerClickUp();
      }
      if (currentGazeObject != null) {
        pointer.OnPointerExit(currentGazeObject);
      }
      pointer.OnInputModuleDisabled();
    }
    currentGazeObject = null;
    currentTarget = null;
    isTriggered = false;
  }

  void LateUpdate () {
    GvrViewer.Instance.UpdateState();
    HandleGaze();
    HandleTrigger();
  }

  private void HandleGaze() {
    // Retrieve GazePointer radius.
    float innerRadius = 0.0f;
    float outerRadius = 0.0f;
    if (pointer != null) {
      pointer.GetPointerRadius(out innerRadius, out outerRadius);
    }

    // Find what object the user is looking at.
    Vector3 intersectPosition;
    IGvrGazeResponder target = null;
    Ray intersectionRay;
    GameObject targetObject = FindGazeTarget(innerRadius, out target, out intersectPosition, out intersectionRay);

    // Found a target?
    if (targetObject != null) {
      lastIntersectPosition = intersectPosition;
      lastIntersectionRay = intersectionRay;

      // Is the object new?
      if (targetObject != currentGazeObject) {
        if (pointer != null) {
          pointer.OnPointerExit(currentGazeObject);
        }
        if (currentTarget != null) {
          // Replace with current object.
          currentTarget.OnGazeExit();
        }

        // Save new object.
        currentTarget = target;
        currentGazeObject = targetObject;

        // Inform pointer and target of gaze.
        if (pointer != null) {
          pointer.OnPointerEnter(currentGazeObject, intersectPosition,
             intersectionRay, currentTarget != null);
        }
        if (currentTarget != null) {
          currentTarget.OnGazeEnter();
        }
      } else {
        // Same object, inform pointer of new intersection.
        if (pointer != null) {
          pointer.OnPointerHover(currentGazeObject, intersectPosition,
              intersectionRay, currentTarget != null);
        }
      }
    } else {
      // Failed to find an object by inner radius.
      if (currentGazeObject != null) {
        // Already gazing an object? Check against outer radius.
        if (IsGazeNearObject(outerRadius, currentGazeObject, out intersectPosition)) {
          // Still gazing.
          if (pointer != null) {
            pointer.OnPointerHover(currentGazeObject, intersectPosition,
               intersectionRay, currentTarget != null);
          }
        } else {
          // No longer gazing any object.
          if (pointer != null) {
            pointer.OnPointerExit(currentGazeObject);
          }
          if (currentTarget != null) {
            currentTarget.OnGazeExit();
          }
          currentTarget = null;
          currentGazeObject = null;
        }
      }
    }
  }

  private GameObject FindGazeTarget(float radius, out IGvrGazeResponder responder,
    out Vector3 intersectPosition, out Ray intersectionRay) {
    RaycastHit hit;
    GameObject targetObject = null;
    bool hitResult = false;
    intersectionRay = GetRay();

    // Use Raycast or SphereCast?
    if (radius > 0.0f) {
      // Cast a sphere against the scene.
      hitResult = Physics.SphereCast(intersectionRay.origin,
        radius, intersectionRay.direction, out hit, cam.farClipPlane, mask);
    } else {
      // Cast a Ray against the scene.
      hitResult = Physics.Raycast(intersectionRay, out hit, cam.farClipPlane, mask);
    }

    // Found anything?
    if (hitResult) {
      // Set object and IGvrGazeResponder if any.
      targetObject = hit.collider.gameObject;
      responder = targetObject.GetComponent(typeof(IGvrGazeResponder))
          as IGvrGazeResponder;
      intersectPosition = transform.position + transform.forward * hit.distance;
    } else {
      // Nothing? Reset variables.
      intersectPosition = Vector3.zero;
      responder = null;
    }

    return targetObject;
  }

  private bool IsGazeNearObject(float radius, GameObject target, out Vector3 intersectPosition) {
    RaycastHit[] hits;

    // Use Raycast or SphereCast?
    if (radius > 0.0f) {
      // Cast a sphere against the scene.
      hits = Physics.SphereCastAll(transform.position,
          radius, transform.forward, cam.farClipPlane, mask);
    } else {
      // Cast a Ray against the object.
      RaycastHit hitInfo;
      Ray ray = new Ray(transform.position, transform.forward);

      if (target.GetComponent<Collider>().Raycast(ray, out hitInfo, cam.farClipPlane)) {
        hits = new RaycastHit[1];
        hits[0] = hitInfo;
      } else {
        hits = new RaycastHit[0];
      }
    }

    // Iterate all intersected objects to find the object we are looking for.
    foreach (RaycastHit hit in hits) {
      if (hit.collider.gameObject == target) {
        // Found our object, save intersection position.
        intersectPosition = transform.position + transform.forward * hit.distance;

        return true;
      }
    }

    // Desired object was not intersected.
    intersectPosition = Vector3.zero;
    return false;
  }

  private void HandleTrigger() {
    // If trigger isn't already held.
    if (!isTriggered) {
      if (GvrViewer.Instance.Triggered || Input.GetMouseButtonDown(0)) {
        // Trigger started.
        isTriggered = true;
        if (pointer != null) {
          pointer.OnPointerClickDown();
        }
      }
    } else if (!GvrViewer.Instance.Triggered && !Input.GetMouseButton(0)) {
      // Trigger ended.
      if (pointer != null) {
        pointer.OnPointerClickUp();
      }
      if (currentTarget != null) {
        currentTarget.OnGazeTrigger();
      }
      isTriggered = false;
    }
  }

  private Ray GetRay() {
    return new Ray(transform.position, transform.forward);
  }
}

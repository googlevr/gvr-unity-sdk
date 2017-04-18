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

/// This abstract class should be implemented for pointer based input, and used with
/// the GvrPointerInputModule script.
///
/// It provides methods called on pointer interaction with in-game objects and UI,
/// trigger events, and 'BaseInputModule' class state changes.
///
/// To have the methods called, an instance of this (implemented) class must be
/// registered with the **GvrPointerManager** script on 'OnEnable' by calling
/// GvrPointerManager.OnPointerCreated.
/// A registered instance should also un-register itself at 'OnDisable' calls
/// by setting the **GvrPointerManager.Pointer** static property
/// to null.
///
/// This abstract class should be implemented by pointers doing 1 of 2 things:
/// 1. Responding to movement of the users head (Cardboard gaze-based-pointer).
/// 2. Responding to the movement of the daydream controller (Daydream 3D pointer).
public abstract class GvrBasePointer {

  /// This is used by GvrBasePointerRaycaster to determine if the
  /// enterRadius or the exitRadius should be used for the raycast.
  /// It is set by GvrPointerInputModule and doesn't need to be controlled manually.
  public bool ShouldUseExitRadiusForRaycast { get; set; }

  /// Returns the transform that represents this pointer.
  /// It is used by GvrBasePointerRaycaster as the origin of the ray.
  public Transform PointerTransform { get; set; }

  /// Returns the point that represents the reticle position
  /// It is used by the keyboard as the end of the ray.
  public abstract Vector3 LineEndPoint { get; }

  /// Returns the max distance this pointer will be rendered at from the camera.
  /// This is used by GvrBasePointerRaycaster to calculate the ray when using
  /// the default "Camera" RaycastMode. See GvrBasePointerRaycaster.cs for details.
  public abstract float MaxPointerDistance { get; }

  public virtual void OnStart() {
    GvrPointerManager.OnPointerCreated(this);
  }

  /// This is called when the 'BaseInputModule' system should be enabled.
  public abstract void OnInputModuleEnabled();

  /// This is called when the 'BaseInputModule' system should be disabled.
  public abstract void OnInputModuleDisabled();

  /// Called when the pointer is facing a valid GameObject. This can be a 3D
  /// or UI element.
  ///
  /// The targetObject is the object the user is pointing at.
  /// The intersectionPosition is where the ray intersected with the targetObject.
  /// The intersectionRay is the ray that was cast to determine the intersection.
  public abstract void OnPointerEnter(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive);

  /// Called every frame the user is still pointing at a valid GameObject. This
  /// can be a 3D or UI element.
  ///
  /// The targetObject is the object the user is pointing at.
  /// The intersectionPosition is where the ray intersected with the targetObject.
  /// The intersectionRay is the ray that was cast to determine the intersection.
  public abstract void OnPointerHover(GameObject targetObject, Vector3 intersectionPosition,
      Ray intersectionRay, bool isInteractive);

  /// Called when the pointer no longer faces an object previously
  /// intersected with a ray projected from the camera.
  /// This is also called just before **OnInputModuleDisabled** and may have have any of
  /// the values set as **null**.
  public abstract void OnPointerExit(GameObject targetObject);

  /// Called when a click is initiated.
  public abstract void OnPointerClickDown();

  /// Called when click is finished.
  public abstract void OnPointerClickUp();

  /// Return the radius of the pointer. It is used by GvrPointerPhysicsRaycaster
  /// and GvrGaze when searching for valid pointer targets. If a radius is 0, then
  /// a ray is used to find a valid pointer target. Otherwise it will use a SphereCast.
  /// The *enterRadius* is used for finding new targets while the *exitRadius*
  /// is used to see if you are still nearby the object currently pointed at
  /// to avoid a flickering effect when just at the border of the intersection.
  ///
  /// NOTE: This is only works with GvrPointerPhysicsRaycaster. To use it with uGUI,
  /// add 3D colliders to your canvas elements.
  public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);
}

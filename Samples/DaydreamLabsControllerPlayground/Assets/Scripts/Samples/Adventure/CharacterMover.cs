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

using GVR.GUI;

using System;
using UnityEngine;

namespace GVR.Samples.Adventure {
  [RequireComponent(typeof(LevelSelectMenuListener))]
  [RequireComponent(typeof(Rigidbody))]
  public class CharacterMover : MonoBehaviour {

#if UNITY_EDITOR
    public void OnValidate() {
      MaxSlope = Mathf.Clamp(MaxSlope, 0.0f, 90.0f);

      if (InputProvider != null) {
        if (!(InputProvider is IPlayerInputProvider)) {
          Debug.LogError("Input must implement IPlayerInputProvider");
          InputProvider = null;
        }
      }

      if (CharacterAnimationController != null) {
        if (!(CharacterAnimationController is ICharacterAnimatorController)) {
          Debug.LogError("Input must implement ICharacterAnimatorController");
          CharacterAnimationController = null;
        }
      }
    }
#endif

    #region -- Inspector Variables ----------------------------------------

    [Header("Movement")]
    [Tooltip("The max speed that the character will move.")]
    public float MovementSpeed = 5.0f;

    [Tooltip("The input provider that will be polled for a direction vector each frame. " +
             "This must implement IPlayerInputProvider.")]
    public MonoBehaviour InputProvider;

    [Tooltip("The animation controller to drive animation and movement on this character. " +
             "If this is not null, input will be sent to this animation controller to drive an " +
             "animation. Root motion of the animation will be processed as world movement. " +
             "If this is null, input will directly move the player.")]
    public MonoBehaviour CharacterAnimationController;

    [Header("Ground Handling")]
    [Tooltip("The max slope that the character can climb.")]
    [Range(0.0f, 90.0f)]
    public float MaxSlope = 30.0f;

    [Tooltip("How high off of the player origin to start the ground raycast.")]
    public float RaycastDownHeight = 0.2f;

    [Tooltip("The length of the raycast to find the ground.")]
    public float RaycastDownLength = 0.4f;

    [Header("Collision")]
    [Tooltip("Top point for the collision capsule cast.")]
    public float CapsuleCastTop;

    [Tooltip("Bottom point for the collision capsule cast.")]
    public float CapsuleCastBottom;

    [Tooltip("Radius for the collision capsule cast.")]
    public float CapsuleRadius = 0.6f;

    [Tooltip("How many attempts to make to resolve a valid position before giving up and " +
             "placing at an invalid position.")]
    public float MaxCollisionIterations = 20;

    #endregion -- Inspector Variables -------------------------------------

    #region -- Private Variables ------------------------------------------

    private Rigidbody rigidBody;
    private GameObject surface;
    private Vector3 lastSurfacePosition;
    private bool sameSurface;
    private IPlayerInputProvider inputProvider;
    private ICharacterAnimatorController characterAnimationController;
    private LevelSelectMenuListener levelSelectMenuListener;

    #endregion -- Private Variables ---------------------------------------

    #region -- MonoBehaviour Functions ------------------------------------

    void Start() {
      rigidBody = GetComponent<Rigidbody>();
      inputProvider = InputProvider as IPlayerInputProvider;
      characterAnimationController = CharacterAnimationController as ICharacterAnimatorController;
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
    }

    void Update() {
      if (levelSelectMenuListener.IsMenuOpen) {
        return;
      }
      UpdateContactedSurface();
      ProcessMovementRequests();
      HandleGround();
    }

    #endregion -- MonoBehaviour Functions ---------------------------------

    public void OnAnimatorMove() {
      if (characterAnimationController != null) {
        transform.position = ResolveNewPosition(characterAnimationController.GetAnimationDelta(), 0);
      }
    }

    private void UpdateContactedSurface() {
      if (surface != null) {
        if (sameSurface) {
          Vector3 surfaceMovement = surface.transform.position - lastSurfacePosition;
          if (surfaceMovement.magnitude >= 0.01f) {
            surface.GetComponent<Collider>().enabled = false;
            Vector3 idealMove = surfaceMovement;
            transform.position = ResolveNewPosition(idealMove, 0);
            surface.GetComponent<Collider>().enabled = true;
          }
        }
        lastSurfacePosition = surface.transform.position;
      }
    }

    private void ProcessMovementRequests() {
      if (inputProvider.IsReady()) {
        Vector3 direction = inputProvider.GetMovementVector();
        if (direction.sqrMagnitude > 0.001f) {
          if (characterAnimationController != null) {
            characterAnimationController.ProcessInput(direction, MovementSpeed, Time.deltaTime);
          } else {
            Vector3 idealMove = direction * MovementSpeed * Time.deltaTime;
            transform.LookAt(transform.position + idealMove);
            transform.position = ResolveNewPosition(idealMove, 0);
          }
        } else {
          if (characterAnimationController != null) {
            characterAnimationController.ProcessInput(Vector3.zero, MovementSpeed, Time.deltaTime);
          }
        }
      }
    }

    void OnDrawGizmosSelected() {
      Vector3 bottom = transform.position + Vector3.up * CapsuleCastBottom;
      Vector3 top = transform.position + Vector3.up * CapsuleCastTop;
      Gizmos.DrawWireSphere(bottom, CapsuleRadius);
      Gizmos.DrawWireSphere(top, CapsuleRadius);
      Gizmos.DrawLine(top + transform.forward * CapsuleRadius, bottom + transform.forward * CapsuleRadius);
      Gizmos.DrawLine(top + transform.right * CapsuleRadius, bottom + transform.right * CapsuleRadius);
      Gizmos.DrawLine(top + transform.forward * -CapsuleRadius, bottom + transform.forward * -CapsuleRadius);
      Gizmos.DrawLine(top + transform.right * -CapsuleRadius, bottom + transform.right * -CapsuleRadius);
      Gizmos.color = Color.white;
      Matrix4x4 m = Gizmos.matrix;
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawCube(Vector3.zero, new Vector3(CapsuleRadius * 2.0f, 0.01f, CapsuleRadius * 2.0f));
      Gizmos.matrix = m;
      Gizmos.color = Color.red;
      Vector3 raycastStart = transform.position + Vector3.up * RaycastDownHeight;
      Vector3 raycastEnd = raycastStart + Vector3.down * RaycastDownLength;
      Gizmos.DrawSphere(raycastStart, 0.05f);
      Gizmos.DrawSphere(raycastEnd, 0.05f);
      Gizmos.DrawLine(raycastStart, raycastEnd);
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.color = Color.yellow;
      Vector3 slopeStart = transform.forward;
      Vector3 slopeEnd = slopeStart + 2.0f * new Vector3(0.0f, Mathf.Sin(Mathf.Deg2Rad * (MaxSlope)),
                                                         Mathf.Cos(Mathf.Deg2Rad * (MaxSlope)));
      Gizmos.DrawLine(slopeStart, slopeEnd);
    }

    private void HandleGround() {
      Ray down = new Ray(transform.position + Vector3.up * RaycastDownHeight, Vector3.down);
      RaycastHit downHit;
      bool didHit = Physics.Raycast(down, out downHit, RaycastDownLength,
                                    int.MaxValue, QueryTriggerInteraction.Ignore);
      sameSurface = false;
      if (didHit) {
        Vector3 p = transform.position;
        p.y = downHit.point.y;
        transform.position = p;
        sameSurface = surface == downHit.collider.gameObject;
        surface = downHit.collider.gameObject;
        rigidBody.isKinematic = true;
      } else {
        surface = null;
        rigidBody.isKinematic = false;
      }
    }

    private Vector3 ResolveNewPosition(Vector3 idealMove, int iteration) {
      if (iteration >= MaxCollisionIterations) {
        Debug.LogWarning("Maximum collision iterations reached. Rejecting movement.");
        return transform.position;
      }
      RaycastHit hit;
      Vector3 bottom = transform.position + Vector3.up * CapsuleCastBottom;
      Vector3 top = transform.position + Vector3.up * CapsuleCastTop;
      bool didHit = Physics.CapsuleCast(bottom, top, CapsuleRadius, idealMove.normalized, out hit,
                                        idealMove.magnitude, Int32.MaxValue,
                                        QueryTriggerInteraction.Ignore);
      Vector3 normal = new Vector3(hit.normal.x, 0.0f, hit.normal.z).normalized;
      if (didHit) {
        bool upwards = Vector3.Dot(hit.normal, Vector3.up) > 0.0f;
        Vector3 newMove;
        if (upwards) {
          float angle = 90.0f - Vector3.Angle(hit.normal, normal);
          if (angle <= MaxSlope) {
            newMove = idealMove - Vector3.Project(idealMove, hit.normal);
          } else {
            newMove = idealMove - Vector3.Project(idealMove, normal);
          }
        } else {
          newMove = idealMove - Vector3.Project(idealMove, normal);
        }
        return ResolveNewPosition(newMove, iteration + 1);
      }
      return transform.position + idealMove;
    }
  }
}

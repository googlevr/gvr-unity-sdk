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
using GVR.Input;
using UnityEngine;

namespace GVR.Samples.Archae {
  /// <summary>
  ///  This component maps the hmd and controller to an in-game implement.
  ///  When in dig mode, the hmd's forward vector determines the position
  ///  of the brush. The brush's orientation matches that of the controller.
  ///  The brush will rest close to the player unless the hmd is looking at
  ///  a brushable surface. When the hmd is looking at a brushable surface,
  ///  it extends out to rest against it.
  ///  When in examine mode, the artifact's orientation is mapped to that
  ///  of the controller.
  /// </summary>
  [RequireComponent(typeof(LevelSelectMenuListener))]
  public class RemoteController : MonoBehaviour {
    [Tooltip("Ref to the rigidbody of the Remote transform.")]
    public Rigidbody RemoteRigidbody;

    [Tooltip("Head transform located in GVRCardboardMain.")]
    public Transform HeadTransform;

    [Tooltip("BodyMain transform.")]
    public Transform BodyTransform;

    [Tooltip("Ref to the transform that denotes the tip of the brush.")]
    public Transform BrushTip;

    [Tooltip("Max distance the brush can reach when targeting a brushable surface.")]
    public float MaxReachDistance;

    [Tooltip("Distance that brush hangs back at when not in front of a brushable surface.")]
    public float RestingReachDistance;

    [Tooltip("Transform of the Artifact.")]
    public Transform ArtifactTransform;

    private Quaternion initialRotOffset = Quaternion.identity;
    private Vector3 lookTarget = Vector3.zero;
    private bool digMode = true;
    private bool examineMode = false;

    private LevelSelectMenuListener levelSelectMenuListener;

    void Start() {
      initialRotOffset = BodyTransform.rotation;
      levelSelectMenuListener = GetComponent<LevelSelectMenuListener>();
    }

    void Update() {
      if (levelSelectMenuListener.IsMenuOpen) {
        return;
      }
      if (digMode) {
        UpdateRemoteOrientation();
      }
      if (examineMode) {
        UpdateArtifactOrientation();
      }
      BodyTransform.eulerAngles = new Vector3(BodyTransform.eulerAngles.x,
        HeadTransform.eulerAngles.y, BodyTransform.eulerAngles.z);
    }

    void FixedUpdate() {
      if (digMode) {
        UpdateGaze();
      }
    }

    private void UpdateGaze() {
      Ray ray = new Ray(HeadTransform.position, HeadTransform.forward);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit, MaxReachDistance)) {
        lookTarget = hit.point - (HeadTransform.forward * BrushTip.localPosition.z) - Vector3.up * .25f;
      } else {
        lookTarget = BodyTransform.position + HeadTransform.forward * RestingReachDistance;
      }
      RemoteRigidbody.transform.position = lookTarget;
      RemoteRigidbody.velocity = Vector3.zero;
    }

    private void UpdateRemoteOrientation() {
      float initOffset = initialRotOffset.eulerAngles.y;
      RemoteRigidbody.transform.rotation = GvrController.Orientation;
      RemoteRigidbody.transform.eulerAngles += Vector3.up * initOffset;
    }

    private void UpdateArtifactOrientation() {
      ArtifactTransform.localRotation = GvrController.Orientation;
    }

    public void DisableDigMode() {
      Destroy(RemoteRigidbody.gameObject);
      digMode = false;
    }

    public void EnableExamineMode() {
      examineMode = true;
    }
  }
}

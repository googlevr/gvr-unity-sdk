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

using System;
using System.Collections;
using GVR.Utils;
using UnityEngine;

namespace GVR.Samples.Archae {
  /// <summary>
  ///  This component describes conditions for uncovering the artifact and
  ///  its behavior once it is released.
  ///  The artifact is considered buried until the joints of the dig site's
  ///  skinned mesh are pushed back according to the DiscoveryJoint data.
  ///  Once uncovered, the artifact will float to PresentationPosition and
  ///  then trigger the remote to take control of its orientation.
  /// </summary>
  public class Artifact : MonoBehaviour {
    [Tooltip("List of joints in your skinned mesh and their target z-values to count as 'brushed away'.")]
    public DiscoveryJoint[] RequisiteJoints;

    [Tooltip("Where the artifact will travel after it is totally uncovered.")]
    public Vector3 PresentationPosition;

    [Tooltip("Travel time for artifact from discover location to inspection location.")]
    public float TimeToPresent;

    [Tooltip("The RemoteController in the BodyMain object.")]
    public RemoteController Remote;

    [Tooltip("The Effect that plays upon discovery of this artifact.")]
    public EffectPlayer Effects;

    private bool digCompleted = false;

    public void Update() {
      if (!digCompleted) {
        CheckDigStatus();
      }
    }

    private void CheckDigStatus() {
      bool complete = true;
      for (int i = 0; i < RequisiteJoints.Length; i++) {
        if (RequisiteJoints[i].MeshJoint.localPosition.z < RequisiteJoints[i].RequisiteZDepth) {
          complete = false;
          break;
        }
      }
      if (complete) {
        digCompleted = true;
        Effects.Play();
        StartCoroutine(Presentation());
      }
    }

    private IEnumerator Presentation() {
      float timer = 0;
      Vector3 startPos = transform.position;
      Remote.DisableDigMode();
      Quaternion startRot = transform.rotation;
      Quaternion endRot = Quaternion.LookRotation(PresentationPosition - Remote.HeadTransform.position, Vector3.up);
      while (timer < TimeToPresent) {
        timer += Time.deltaTime;
        transform.position = Vector3.Lerp(startPos, PresentationPosition, timer / TimeToPresent);
        transform.localRotation = Quaternion.Lerp(startRot, endRot, timer / TimeToPresent);
        Remote.ArtifactTransform.localRotation =
            Quaternion.Lerp(Remote.ArtifactTransform.localRotation,
                            GvrController.Orientation, timer / TimeToPresent);
        yield return null;
      }
      transform.position = PresentationPosition;
      Remote.EnableExamineMode();
    }
  }

  /// <summary>
  ///  This class is a serialized contatiner for data about how far a skinned mesh joint needs
  ///  to be pushed in before it is completed.
  /// </summary>
  [Serializable]
  public class DiscoveryJoint {
    [Tooltip("Transform of the joint in your skinned mesh.")]
    public Transform MeshJoint;

    [Tooltip("The z-value of this joint required to discover the artifact.")]
    public float RequisiteZDepth;
  }
}

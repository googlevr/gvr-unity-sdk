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

using GVR.Input;
using UnityEngine;

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component primarily rotates the remote gameoject to match the physical remote's orientation.
  ///  Additionally, it allows the distance of the remote from it's base to shift inward or outwards
  ///  depending on the head's tilt. Tilting the head towards the remote will essentially push the
  ///  remote out, and tilting the head away from the remote will draw it in.
  /// </summary>
  public enum AnchorAxis {
    x,
    y,
    z
  }

  public class RemoteParent : MonoBehaviour {
    [Tooltip("Model of remote substitute. Will slide forward and backward depending on head tilt.")]
    public GameObject RemoteModel;

    [Tooltip("The 'Head' transform under the 'CardboardMain' object in the scene.")]
    public Transform CardboardHead;

    [Tooltip("Axis over which tilting the head affects the model slide distance.")]
    public AnchorAxis anchorAxis;

    [Tooltip("Min slide distance.")]
    public float MinDistance;

    [Tooltip("Max slide distance.")]
    public float MaxDistance;

    [Tooltip("Max head tilt angle required to utilize the max slide.")]
    public float MaxHeadTilt;

    [Tooltip("Point that is the tip of the remote model.")]
    public Transform ModelTip;

    [Tooltip("Point that is the base of the remote model.")]
    public Transform ModelBase;

    private float averageDistance = 0;
    private float maxModelDifference = 0;

    void Start() {
      maxModelDifference = Vector3.Distance(ModelTip.position, ModelBase.position);
      averageDistance = (MaxDistance + MinDistance) / 2f;
    }

    void Update() {
      // This is valid regardless of whether the controller is currently connected or not, because
      // when the controller is disconnected GvrController.Orientation will still report the last
      // orientation received (or "forward", if the controller was never connected).
      transform.rotation = GvrController.Orientation;

      float headTilt = 0;
      float modelDirection = 0;
      Vector3 offsetDirection = Vector3.zero;

      switch (anchorAxis) {
        case AnchorAxis.x:
          headTilt = CardboardHead.localEulerAngles.x;
          modelDirection = (ModelTip.position.z - ModelBase.position.z) / maxModelDifference;
          offsetDirection = Vector3.right;
          break;
        case AnchorAxis.y:
          headTilt = CardboardHead.localEulerAngles.y;
          modelDirection = (ModelTip.position.y - ModelBase.position.y) / maxModelDifference;
          offsetDirection = Vector3.up;
          break;
        case AnchorAxis.z:
          headTilt = CardboardHead.localEulerAngles.z;
          modelDirection = (ModelTip.position.x - ModelBase.position.x) / maxModelDifference;
          offsetDirection = Vector3.forward;
          break;

        headTilt = Mathf.Clamp(headTilt > 180 ? headTilt - 360 : headTilt, MaxHeadTilt * -1, MaxHeadTilt);
        float distanceScale = (Mathf.Abs(headTilt) / MaxHeadTilt) * Mathf.Abs(modelDirection);

        if (Mathf.Approximately(Mathf.Sign(headTilt), Mathf.Sign(modelDirection))) {
          RemoteModel.transform.localPosition =
              offsetDirection * (averageDistance - distanceScale * (averageDistance - MinDistance));
        } else {
          RemoteModel.transform.localPosition =
              offsetDirection * (distanceScale * (MaxDistance - averageDistance) + averageDistance);
        }
      }
    }
  }
}

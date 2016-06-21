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

using System.Collections.Generic;
using UnityEngine;
using GVR.Utils;

namespace GVR.Samples.Archae {
  /// <summary>
  ///  This component detects brush strokes, plays the appropriate effects,
  ///  and then pushes in the joints on the brushed surface.
  /// </summary>
  public class DustKicker : MonoBehaviour {
    [Tooltip("Reference to Object Pool containing all the dust particle effects.")]
    public ObjectPool DustPool;

    [Tooltip("Max reach of the brush to a valid brushable surface. Same as in Remote Controller.")]
    public float MaxReachDistance;

    [Tooltip("Min change of angle in the brush to be considered a brush swipe.")]
    public float MinSwipeAngle;

    [Tooltip("Distance that the brushable dirt mesh vert is pushed back when swiped.")]
    public float PushDistance;

    [Tooltip("Ref to Head transform in the GVRCardboardMain.")]
    public Transform HeadTransform;

    [Tooltip("Transform that denots the tip of the brush.")]
    public Transform BrushTipTransform;

    [Tooltip("Gameobject that contains the skinned mesh of the brushable surface.")]
    public GameObject BrushTarget;

    [Tooltip("Gameobject that contains the list of transforms that are the skinned mesh's joints.")]
    public GameObject JointGroup;

    [Tooltip("Array of AudioSources containing dust sounds.")]
    public GvrAudioSource[] DustAudio;

    private KeyValuePair<float, Vector3>[] brushFrames = new KeyValuePair<float, Vector3>[40];
    private int index = 0;
    private Transform[] joints = null;

    void Start() {
      joints = JointGroup.GetComponentsInChildren<Transform>();
    }

    void Update() {
      DetectBrushStroke();
    }

    void FixedUpdate() {
      bool brushOnTarget = false;
      RaycastHit hit;
      if (Physics.Raycast(new Ray(HeadTransform.position, HeadTransform.forward), out hit, MaxReachDistance)) {
        brushOnTarget = hit.collider.gameObject.Equals(BrushTarget);
      }
      if (!brushOnTarget) {
        ResetBrushFrames();
        return;
      }
      index = GetNextIndex();
      brushFrames[index] = new KeyValuePair<float,Vector3>(
          Vector3.Angle((transform.position - HeadTransform.position).normalized,
                        (BrushTipTransform.position - HeadTransform.position).normalized),
          BrushTipTransform.position);
    }

    // stroke is defined as a change in brush angle by at least MinSwipeAngle degrees
    // two strokes are required to count as a brush action
    private void DetectBrushStroke() {
      float startAngle = brushFrames[0].Key;
      float offsetAngle = 0;
      int strokeCount = 0;
      Vector3 averagePosition = Vector3.zero;
      for (int i = 0; i < brushFrames.Length; i++) {
        averagePosition += brushFrames[i].Value;
        float offset = Mathf.Abs(brushFrames[i].Key - startAngle);
        if (offset > offsetAngle) {  // continuing current swipe
          offsetAngle = offset;
        } else if (offset > MinSwipeAngle) {  // reversing direction
          startAngle = brushFrames[i].Key;
          offsetAngle = 0;
          strokeCount++;
        }
        if (strokeCount >= 2) {
          averagePosition /= i;
          EngageStroke(averagePosition);
          ResetBrushFrames();
          GameObject dust = DustPool.GetFreeObject();
          if (dust) {
            dust.transform.position = averagePosition;
            dust.transform.LookAt(HeadTransform);
            dust.GetComponent<DustSpawn>().Play();
          }
          DustAudio[i % DustAudio.Length].Play();
          return;
        }
      }
    }

    private void EngageStroke(Vector3 position) {
      int bestIndex = 0;
      float bestDistance = float.MaxValue;
      for (int i = 0; i < joints.Length; i++) {
        float distance = Vector3.Distance(position, joints[i].position);
        if (distance < bestDistance) {
          bestDistance = distance;
          bestIndex = i;
        }
      }
      joints[bestIndex].localPosition += Vector3.forward * PushDistance;
    }

    private void ResetBrushFrames() {
      if (index == -1) {
        return;
      }
      for (int i = 0; i < brushFrames.Length; i++) {
        brushFrames[i] = new KeyValuePair<float,Vector3>(0, Vector3.zero);
      }
      index = -1;
    }

    private int GetNextIndex() {
      int nextIndex = index + 1;
      if (nextIndex >= brushFrames.Length) {
        nextIndex = 0;
      }
      return nextIndex;
    }
  }
}

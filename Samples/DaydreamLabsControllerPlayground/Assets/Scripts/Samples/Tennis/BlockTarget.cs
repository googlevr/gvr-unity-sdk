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

using System.Collections;
using UnityEngine;

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component describes the behavior of the block targets. When a ball
  ///  hits the target for the first time, it will crack. The second hit causes
  ///  the target to shatter.
  /// </summary>
  [RequireComponent(typeof(Rigidbody))]
  public class BlockTarget : MonoBehaviour {
    [Tooltip("The gameobject for the uncracked-looking block.")]
    public GameObject FullBlock;

    [Tooltip("The gameobject for the cracked block.")]
    public GameObject CrackedBlock;

    [Tooltip("An array of gameobjects that make up the individual pieces once a block is shattered.")]
    public GameObject[] BlockPieces;

    [Tooltip("The target the ball will move towards when after hitting this block target.")]
    public Transform ReflectTarget;

    [Tooltip("The audio source that plays when a ball cracks this block.")]
    public GvrAudioSource CrackedAudio;

    [Tooltip("The audio source that plays when a ball shatters this block.")]
    public GvrAudioSource ShatterAudio;

    protected int hitCount = 0;

    public void OnCollisionEnter(Collision collision) {
      if (!collision.collider.CompareTag("TennisBall")) {
        return;
      }
      if (hitCount == 0) {
        FullBlock.SetActive(false);
        CrackedBlock.SetActive(true);
        CrackedAudio.Play();
        hitCount++;
      } else {
        Shatter(collision.impulse.magnitude, collision.contacts[0].point);
      }
    }

    public void OnCollisionExit(Collision collision) {
      if (!collision.collider.CompareTag("TennisBall")) {
        return;
      }

      Rigidbody r = collision.collider.attachedRigidbody;
      float magnitude = Mathf.Min(r.velocity.magnitude, 10f);

      StartCoroutine(SetVelocity(r, (ReflectTarget.position - r.transform.position).normalized * magnitude));
    }

    protected virtual void Shatter(float explodeMag, Vector3 explodePoint) {
      Destroy(GetComponent<Collider>());
      for (int i = 0; i < BlockPieces.Length; i++) {
        BlockPieces[i].transform.parent = null;
        BlockPieces[i].GetComponent<MeshCollider>().enabled = true;
        Rigidbody r = BlockPieces[i].GetComponent<Rigidbody>();
        r.isKinematic = false;
        r.AddExplosionForce(explodeMag, explodePoint, .3f, 0, ForceMode.VelocityChange);
        ShatterAudio.Play();
      }
    }

    private IEnumerator SetVelocity(Rigidbody r, Vector3 velocity) {
      yield return new WaitForEndOfFrame();
      r.velocity = velocity;
    }
  }
}

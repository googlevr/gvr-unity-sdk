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

using GVR.Utils;
using UnityEngine;

namespace GVR.Samples.Pancakes {
  /// <summary>
  ///  This component contains the logic for how the pancake reacts to
  ///  anthing it touches. If a pancake contacts the pan, or another
  ///  pancake on the pan, its underside cooks. If it touches a plate,
  ///  it gets removed from the pan circle constraint. If it hits the
  ///  floor it starts a timer to return to the pancake pool.
  /// </summary>
  public class PancakeCookedDetector : MonoBehaviour {
    [Tooltip("Material used to show an uncooked pancake side")]
    public Material UncookedSideMaterial;

    [Tooltip("Material used to show a cooked pancake side")]
    public Material CookedSideMaterial;

    [Tooltip("Reference to the pancake model's renderer")]
    public Renderer PancakeRenderer;

    [Tooltip("The Effect that is triggered when the pancake is finished cooking on both sides")]
    public EffectPlayer CookedEffect;

    [Tooltip("LayerMask for the layer of the pan")]
    public LayerMask PanMask;

    [Tooltip("LayerMask for the layer of the pancake")]
    public LayerMask PancakeMask;

    [Tooltip("LayerMask for the layer of the plate")]
    public LayerMask PlateMask;

    [Tooltip("How long it takes for a pancake on the floor to return to the pool")]
    public float TimeToExpire;

    [Tooltip("Reference to the PancakeDispenser in the scene")]
    public PancakeDispenser Dispenser;

    [Tooltip("Reference to this object's rigidbody")]
    public Rigidbody ActiveRigidbody;

    public bool IsOnPan { get; private set; }
    public bool IsCooked { get; private set; }
    public bool IsOnPlate { get; private set; }

    private bool topSideCooked = false;
    private bool bottomSideCooked = false;
    private float expireTimer = 0;
    private bool aboutToExpire = false;

    public void DeActivate() {
      IsOnPan = false;
      topSideCooked = false;
      bottomSideCooked = false;
      aboutToExpire = false;
      IsCooked = false;
      PancakeRenderer.materials = new Material[] { UncookedSideMaterial, UncookedSideMaterial };
      CookedEffect.Stop();
      Dispenser.PancakePool.ReturnObject(gameObject);
    }

    void Update() {
      if (aboutToExpire) {
        expireTimer -= Time.deltaTime;
        if (expireTimer < 0) {
          DeActivate();
        }
      }
    }

    void OnCollisionEnter(Collision collision) {
      Rigidbody r = collision.collider.attachedRigidbody;
      // hit floor
      if (r == null) {
        StartDisableTimer();
        return;
      }
      bool topBottom = false;
      Vector3 projectedUp = Vector3.Project(transform.up, Vector3.up);
      topBottom = projectedUp.y < 0;
      // hit pan
      if (((1 << collision.collider.gameObject.layer) & PanMask.value) > 0) {
        IsOnPan = true;
        CookSide(topBottom);
      }
      // hit plate
      else if (((1 << collision.collider.gameObject.layer) & PlateMask.value) > 0) {
        IsOnPlate = true;
        Dispenser.PanConstraints.LockedObjects.Remove(transform);
      }
      // hit pancake
      else if (((1 << collision.collider.gameObject.layer) & PancakeMask.value) > 0) {
        PancakeCookedDetector otherPancake = r.GetComponent<PancakeCookedDetector>();
        if (otherPancake.IsOnPan) {
          CookSide(topBottom);
        }
      }
    }

    void OnCollisionExit(Collision collision) {
      Rigidbody r = collision.collider.attachedRigidbody;
      if (r == null) {
        return;
      }
      // hit pan
      if (((1 << collision.collider.gameObject.layer) & PanMask.value) > 0) {
        IsOnPan = false;
      }
    }

    private void CookSide(bool topBottom) {
      if (topBottom && !topSideCooked) {
        topSideCooked = true;
        PancakeRenderer.materials = new Material[] {
          CookedSideMaterial,
          PancakeRenderer.materials[1]
        };
        if (bottomSideCooked) {
          CookedEffect.Play();
          IsCooked = true;
        }
      } else if (!topBottom && !bottomSideCooked) {
        bottomSideCooked = true;
        PancakeRenderer.materials = new Material[] {
          PancakeRenderer.materials[0],
          CookedSideMaterial
        };
        if (topSideCooked) {
          CookedEffect.Play();
          IsCooked = true;
        }
      }
    }

    private void StartDisableTimer() {
      expireTimer = TimeToExpire;
      aboutToExpire = true;
      Dispenser.PanConstraints.LockedObjects.Remove(transform);
    }
  }
}

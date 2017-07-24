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

// The controller is not available for versions of Unity without the
// GVR native integration.

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using NUnit.Framework;

namespace GvrPointerTests {

  [TestFixture]
  internal class GvrReticlePointerTest {
    private const float TOLERANCE_DELTA = 0.00001f;

    GvrReticlePointer pointer;

    [SetUp]
    public void Setup() {
      GameObject reticleObj = new GameObject();
      pointer = ComponentHelpers.AddComponentInvokeLifecycle<GvrReticlePointer>(reticleObj);
    }

    [Test]
    public void OnPointerEnter_NullBaseTransform() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 2);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);


      pointer.OnPointerEnter(raycastResult, false);

      // Base pointer transform was null, nothing should have changed.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);
    }

    [Test]
    public void OnPointerEnter_NotInteractable() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 2);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      pointer.OnPointerEnter(raycastResult, false);

      // Base pointer transform was not null, and this was not interactive.
      Assert.AreEqual(raycastResult.worldPosition.z, pointer.ReticleDistanceInMeters,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE, pointer.ReticleInnerAngle,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE, pointer.ReticleOuterAngle,
          TOLERANCE_DELTA);
    }

    [Test]
    public void OnPointerEnter_Interactable() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 1000000000);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      pointer.OnPointerEnter(raycastResult, true);

      // Base pointer transform was not null, and this was interactive.
      Assert.AreEqual(GvrReticlePointer.RETICLE_DISTANCE_MAX, pointer.ReticleDistanceInMeters,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE
          + GvrReticlePointer.RETICLE_GROWTH_ANGLE, pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE
          + GvrReticlePointer.RETICLE_GROWTH_ANGLE, pointer.ReticleOuterAngle, TOLERANCE_DELTA);
    }

    [Test]
    public void OnPointerHoverAndExit_NullBaseTransform() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 2);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      pointer.OnPointerHover(raycastResult, false);

      // Base pointer transform was null, nothing should have changed.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      // Pointer exits.
      pointer.OnPointerExit(new GameObject());

      // Reticle angles are back to their minimum limits.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_DISTANCE_MAX,
          pointer.ReticleDistanceInMeters, TOLERANCE_DELTA);
    }

    [Test]
    public void OnPointerHover_NotInteractable() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 2);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      pointer.OnPointerHover(raycastResult, false);

      // Base pointer transform was not null, and this was not interactive.
      Assert.AreEqual(raycastResult.worldPosition.z, pointer.ReticleDistanceInMeters,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE, pointer.ReticleInnerAngle,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE, pointer.ReticleOuterAngle,
          TOLERANCE_DELTA);
    }

    [Test]
    public void OnPointerHoverAndExit_Interactable() {
      RaycastResult raycastResult = new RaycastResult();
      raycastResult.worldPosition = new Vector3(0, 1, 1000000000);

      // Reticle inner and outer angle are zero to start with.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      pointer.OnPointerHover(raycastResult, true);

      // Base pointer transform was not null, and this was interactive.
      Assert.AreEqual(GvrReticlePointer.RETICLE_DISTANCE_MAX, pointer.ReticleDistanceInMeters,
          TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE
          + GvrReticlePointer.RETICLE_GROWTH_ANGLE, pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE
          + GvrReticlePointer.RETICLE_GROWTH_ANGLE, pointer.ReticleOuterAngle, TOLERANCE_DELTA);

      // Pointer exits.
      pointer.OnPointerExit(new GameObject());

      // Reticle angles are back to their minimum limits.
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_INNER_ANGLE,
          pointer.ReticleInnerAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_MIN_OUTER_ANGLE,
          pointer.ReticleOuterAngle, TOLERANCE_DELTA);
      Assert.AreEqual(GvrReticlePointer.RETICLE_DISTANCE_MAX,
          pointer.ReticleDistanceInMeters, TOLERANCE_DELTA);
    }

    [Test]
    public void GetPointerRadius() {
      float enterRadius = 0;
      float exitRadius = 0;
      float expectedEnterRadius =
        2.0f * Mathf.Tan(Mathf.Deg2Rad * GvrReticlePointer.RETICLE_MIN_INNER_ANGLE);
      float expectedExitRadius =
        2.0f * Mathf.Tan(Mathf.Deg2Rad * (GvrReticlePointer.RETICLE_MIN_INNER_ANGLE +
              GvrReticlePointer.RETICLE_GROWTH_ANGLE));
      pointer.GetPointerRadius(out enterRadius, out exitRadius);
      Assert.AreEqual(expectedEnterRadius, enterRadius, TOLERANCE_DELTA);
      Assert.AreEqual(expectedExitRadius, exitRadius, TOLERANCE_DELTA);
    }
  }
}
#endif  // UNITY_5_3_OR_NEWER

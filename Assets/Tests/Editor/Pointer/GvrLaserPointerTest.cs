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

#if UNITY_ANDROID || UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

namespace GvrPointerTests {

  [TestFixture]
  internal class GvrLaserPointerTest {
    private const float TOLERANCE_DELTA = 0.00001f;

    private GvrLaserVisual laserVisual;
    private GvrLaserPointer pointer;
    private GameObject reticle;

    private RaycastResult raycastResult;

    private const float MAX_RETICLE_DISTANCE = 20.0f;
    private readonly Vector3 DEFAULT_RETICLE_POS = new Vector3(0.0f, 0.0f, 20.0f);
    private readonly Vector3 CLOSE_HIT_POS = new Vector3(0.0f, 0.0f, 1.5f);
    private readonly Vector3 FAR_HIT_POS = new Vector3(0.0f, 0.0f, 4.5f);

    [SetUp]
    public void Setup() {
      GameObject laserObj = new GameObject();
      laserObj.AddComponent<LineRenderer>();
      laserVisual = ComponentHelpers.AddComponentInvokeLifecycle<GvrLaserVisual>(laserObj);
      laserVisual.lerpThreshold = 0.0f;

      pointer = ComponentHelpers.AddComponentInvokeLifecycle<GvrLaserPointer>(laserObj);
      pointer.raycastMode = GvrBasePointer.RaycastMode.Direct;
      pointer.maxPointerDistance = MAX_RETICLE_DISTANCE;
      pointer.defaultReticleDistance = MAX_RETICLE_DISTANCE;
      ComponentHelpers.CallStart(pointer);

      reticle = new GameObject();
      laserVisual.Reticle = reticle.transform;

      raycastResult.worldPosition = CLOSE_HIT_POS;
      raycastResult.distance = CLOSE_HIT_POS.z;
    }

    [Test]
    public void OnPointerEnterAndExit() {
      ComponentHelpers.CallLateUpdate(laserVisual);
      Assert.AreEqual(reticle.transform.position, DEFAULT_RETICLE_POS);

      pointer.OnPointerEnter(raycastResult, true);
      ComponentHelpers.CallLateUpdate(laserVisual);

      Assert.AreEqual(reticle.transform.position, CLOSE_HIT_POS);

      pointer.OnPointerExit(new GameObject());
      ComponentHelpers.CallUpdate(pointer);
      ComponentHelpers.CallLateUpdate(laserVisual);

      Assert.AreEqual(reticle.transform.position, DEFAULT_RETICLE_POS);
    }

    [Test]
    public void OnPointerHover() {
      ComponentHelpers.CallLateUpdate(laserVisual);
      Assert.AreEqual(reticle.transform.position, DEFAULT_RETICLE_POS);

      pointer.OnPointerHover(raycastResult, true);
      ComponentHelpers.CallLateUpdate(laserVisual);


      Assert.AreEqual(reticle.transform.position, CLOSE_HIT_POS);
    }

    [Test]
    public void OnPointerEnterFar() {
      ComponentHelpers.CallLateUpdate(laserVisual);
      Assert.AreEqual(reticle.transform.position, DEFAULT_RETICLE_POS);

      raycastResult.worldPosition = FAR_HIT_POS;
      raycastResult.distance = FAR_HIT_POS.z;
      pointer.OnPointerEnter(raycastResult, true);
      ComponentHelpers.CallLateUpdate(laserVisual);

      Assert.AreEqual(reticle.transform.position, FAR_HIT_POS);
    }

    [Test]
    public void GetPointerRadius_NullReticle() {
      laserVisual.Reticle = null;

      float enterRadius = 0;
      float exitRadius = 0;
      pointer.GetPointerRadius(out enterRadius, out exitRadius);

      Assert.AreEqual(0, enterRadius, TOLERANCE_DELTA);
      Assert.AreEqual(0, exitRadius, TOLERANCE_DELTA);
    }

    [Test]
    public void GetPointerRadius_WithReticle() {
      laserVisual.Reticle.transform.localScale = new Vector3(1, 0, 0);
      float enterRadius = 0;
      float exitRadius = 0;

      pointer.GetPointerRadius(out enterRadius, out exitRadius);

      Assert.AreEqual(0.005f, enterRadius, TOLERANCE_DELTA);
      Assert.AreEqual(0.1f, exitRadius, TOLERANCE_DELTA);
    }

    // TODO(miraleung): Add more tests.
  }
}
#endif  // UNITY_ANDROID || UNITY_EDITOR

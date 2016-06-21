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

namespace GVR.Samples.SkyShip {
  /// <summary>
  ///  This class is responsible for spawning and launching obstacles.
  ///  Every SpawnInterval, grab an obstacle from the pool. Wait for the
  ///  obstacle to become unblocked, then wait an additional
  ///  MinTimeBetweenSpawns before activating it. Then, launch the obstacle.
  /// </summary>
  public class ObstacleManager : MonoBehaviour {
    [Tooltip("Interval at which a new obstacle will spawn")]
    public float SpawnInterval;

    [Tooltip("If a spawn is blocked, wait for this long after becoming unblocked. This prevents obstacles from touching each other on spawn")]
    public float MinTimeBetweenSpawns;

    [Tooltip("Radius of allowed spawn locations in this tranforms's local X direction")]
    public float LevelWidthRadius;

    [Tooltip("Radius of allowed spawn locations in this tranforms's local Y direction")]
    public float LevelHeightRadius;

    [Tooltip("Speed that obstacles will travel forward in this transform's Z direction")]
    public float ObstacleSpeed;

    [Tooltip("Obstacles will spawn with a random varience in orientation with a max value set here.")]
    public float ObstacleRotationVariance;

    [Tooltip("Reference to obstacle recycler for all obstacles with coins (will spawn every 3rd obstacle)")]
    public ObstacleCycler ObstaclePoolCoin;

    [Tooltip("Reference to obstacle recycler for all obstacles with no coins.")]
    public ObstacleCycler ObstaclePoolNoCoin;

    private int prevQuadrant = -1;
    private Obstacle curObstacle = null;
    private bool isWaiting = false;
    private float spawnTimer = 0;
    private int spawnCount = 0;

    void Update() {
      if (isWaiting) {
        return;
      }
      spawnTimer -= Time.deltaTime;
      if (spawnTimer <= 0) {
        SpawnObstacle();
      }
    }

    private Vector2 PickSpawnLocation() {
      //pick a quadrant
      int quadrant = Random.Range(0, 4);
      if (quadrant == prevQuadrant) {
        quadrant++;
        if (quadrant == 4) {
          quadrant = 0;
        }
      }
      //pick location within that quadrant
      float quadX = Random.Range(0, LevelWidthRadius);
      float quadY = Random.Range(0, LevelHeightRadius);
      if (quadrant == 1 || quadrant == 2) {
        quadX *= -1;
      }
      if (quadrant == 2 || quadrant == 3) {
        quadY *= -1;
      }
      return new Vector2(quadX, quadY);
    }

    private void SpawnObstacle() {
      spawnCount++;
      ObstacleCycler pool = null;
      if (spawnCount % 3 == 0) {
        pool = ObstaclePoolCoin;
      } else {
        pool = ObstaclePoolNoCoin;
      }
      GameObject go = pool.GetRandomFreeObject();

      if (go == null) {
        return;
      }
      Obstacle ob = go.GetComponent<Obstacle>();
      ob.ObjectPool = pool;
      Vector2 spawnPos = PickSpawnLocation();
      ob.transform.position = transform.position + new Vector3(spawnPos.x, spawnPos.y, 0);
      ob.transform.eulerAngles = ObstacleRotationVariance *
          new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
      ob.ActiveRigidbody.velocity = Vector3.zero;
      ob.ActiveRigidbody.angularVelocity = Vector3.zero;
      ob.gameObject.SetActive(true);
      ob.DeActivate();
      curObstacle = ob;
      StartCoroutine(WaitForClearSpawn());
    }

    private IEnumerator WaitForClearSpawn() {
      isWaiting = true;
      yield return new WaitForSeconds(.1f);
      while (curObstacle.IsObstacleBlocked) {
        yield return null;
      }
      yield return new WaitForSeconds(MinTimeBetweenSpawns);
      curObstacle.ActiveRigidbody.velocity = transform.forward * ObstacleSpeed;
      curObstacle.Activate();
      spawnTimer = SpawnInterval;
      isWaiting = false;
    }
  }
}

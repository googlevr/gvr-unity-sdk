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

using UnityEngine;
using System.Collections.Generic;

public class CannonballPool : MonoBehaviour {
  public GameObject prefab_;
  private static List<GameObject> pool_ = new List<GameObject>();
  private static List<GameObject> active_pool_ = new List<GameObject>();

  public static CannonballPool instance_;

  void Awake() {
    if (instance_ == null) {
      instance_ = this;
    }

    if (pool_.Count == 0 && active_pool_.Count == 0) {
      for (int i = 0; i < 30; ++i) {
        GameObject instance = GameObject.Instantiate(instance_.prefab_);
        instance.SetActive(false);
        pool_.Add(instance);
      }
    }
  }

  void OnDestroy() {
    if (instance_ == this) {
      instance_ = null;
    }
  }

  public static GameObject Create(Transform at) {
    if (pool_.Count > 0) {
      GameObject instance = pool_[0];
      active_pool_.Add(instance);
      pool_.RemoveAt(0);

      instance.transform.position = at.position;
      instance.transform.rotation = at.rotation;
      instance.transform.localScale = instance_.prefab_.transform.localScale;
      instance.SetActive(true);

      return instance;
    }

    return null;
  }

  public static void Destroy(GameObject obj) {
    if (obj.GetComponent<CannonballBehaviour>() != null) {
      obj.SetActive(false);
      pool_.Add(obj);
      active_pool_.Remove(obj);
    }
  }
}

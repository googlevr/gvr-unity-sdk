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
using UnityEngine.Events;

namespace GVR.Events {
  /// <summary>
  /// Initializes handedness and responds to changing handedness.
  /// </summary>
  public class HandednessListener : MonoBehaviour {
    private const string PREFS_HANDEDNESS_LEFTY = "LeftHanded";
    private const int LEFT_HANDED = 1;
    private const int RIGHT_HANDED = 0;

    public UnityEvent OnLeftHandSelected;
    public UnityEvent OnRightHandSelected;

    public static void SetHandedness(bool lefty) {
      PlayerPrefs.SetInt(PREFS_HANDEDNESS_LEFTY, lefty ? LEFT_HANDED : RIGHT_HANDED);
      HandednessListener[] linsteners = FindObjectsOfType<HandednessListener>();
      for (int i = 0; i < linsteners.Length; i++) {
        if (lefty) {
          linsteners[i].OnLeftHandSelected.Invoke();
        } else {
          linsteners[i].OnRightHandSelected.Invoke();
        }
      }
    }

    void Start() {
      int prefs = GetStoredPref();
      if (prefs == LEFT_HANDED) {
        OnLeftHandSelected.Invoke();
      } else {
        OnRightHandSelected.Invoke();
      }
    }

    public static bool IsLeftHanded {
      get { return GetStoredPref() == LEFT_HANDED; }
    }

    public static bool IsRightHanded {
      get { return GetStoredPref() == RIGHT_HANDED; }
    }

    private static int GetStoredPref() {
      return PlayerPrefs.GetInt(PREFS_HANDEDNESS_LEFTY, RIGHT_HANDED);
    }
  }
}

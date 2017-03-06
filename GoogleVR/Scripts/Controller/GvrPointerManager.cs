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
using System.Collections;

/// GvrPointerManager is a standard interface for
/// controlling which GvrBasePointer is being used
/// for user input affordance.
///
public class GvrPointerManager : MonoBehaviour {
  private static GvrPointerManager instance;

  /// Change the GvrBasePointer that is currently being used.
  public static GvrBasePointer Pointer
  {
    get {
      return instance == null ? null : instance.pointer;
    }
    set {
      if (instance == null || instance.pointer == value) {
        return;
      }

      instance.pointer = value;
    }
  }

  /// GvrBasePointer calls this when it is created.
  /// If a pointer hasn't already been assigned, it
  /// will assign the newly created one by default.
  ///
  /// This simplifies the common case of having only one
  /// GvrBasePointer so is can be automatically hooked up
  /// to the manager.  If multiple GvrGazePointers are in
  /// the scene, the app has to take responsibility for
  /// setting which one is active.
  public static void OnPointerCreated(GvrBasePointer createdPointer) {
    if (instance != null && GvrPointerManager.Pointer == null) {
      GvrPointerManager.Pointer = createdPointer;
    }
  }

  private GvrBasePointer pointer;

  void Awake() {
    if (instance != null) {
      Debug.LogError("More than one GvrPointerManager instance was found in your scene. "
        + "Ensure that there is only one GvrPointerManager.");
      this.enabled = false;
      return;
    }

    instance = this;
  }

  void OnDestroy() {
    if (instance == this) {
      instance = null;
    }
  }
}

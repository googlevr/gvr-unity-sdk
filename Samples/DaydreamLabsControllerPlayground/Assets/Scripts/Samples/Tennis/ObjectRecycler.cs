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

namespace GVR.Samples.Tennis {
  /// <summary>
  ///  This component tracks an array of (ideally) pre-loaded objects in the scene.
  ///  It is primarily used to cycle through each object.
  /// </summary>
  public class ObjectRecycler : MonoBehaviour {
    [Tooltip("Array of existing objects in scene. They should all be the same type (ie: GameObject)")]
    public Object[] ObjectArray;

    private int currentIndex = 0;

    public Object GetNextObject() {
      currentIndex++;
      if (currentIndex >= ObjectArray.Length) {
        currentIndex = 0;
      }
      return ObjectArray[currentIndex];
    }
  }
}

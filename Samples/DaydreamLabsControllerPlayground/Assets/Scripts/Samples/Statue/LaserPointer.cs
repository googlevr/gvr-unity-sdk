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

namespace GVR.Samples.Statue {
  /// <summary>
  ///  This component modifies the points in a line renderer based on a target.
  /// </summary>
  public class LaserPointer : MonoBehaviour {
    [Tooltip("Reference to your line renderer.")]
    public LineRenderer LineRenderer;

    public Vector3 PointerTarget { get; set; }

    private int vertexCount = 10;
    private Vector3[] recentTargets = new Vector3[10];
    private int index = 0;

    void Update() {
      index = GetNextIndex();
      recentTargets[index] = PointerTarget;
      DrawRigidLine();
    }

    private void DrawRigidLine() {
      LineRenderer.SetVertexCount(vertexCount);
      for (int i = 0; i < vertexCount; i++) {
        Vector3 position = Vector3.Lerp(transform.position, PointerTarget, i / (float)vertexCount);
        LineRenderer.SetPosition(i, position);
      }
    }

    private int GetNextIndex() {
      int nextIndex = index + 1;
      if (nextIndex >= recentTargets.Length) {
        nextIndex = 0;
      }
      return nextIndex;
    }

    private int GetPrevIndex(int numStepsBack) {
      int prevIndex = index - numStepsBack;
      while (prevIndex < 0) {
        prevIndex += recentTargets.Length;
      }
      if (prevIndex == 10) {
        prevIndex = 0;
      }
      return prevIndex;
    }
  }
}

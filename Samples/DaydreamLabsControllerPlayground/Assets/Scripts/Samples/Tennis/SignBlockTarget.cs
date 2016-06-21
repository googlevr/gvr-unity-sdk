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
  ///  This component does the same thing as the Block Target, except it will shatter after
  ///  the first hit, and destroy a UI canvas attached to the target.
  /// </summary>
  public class SignBlockTarget : BlockTarget {
    [Tooltip("The Canvas attached to this block target.")]
    public Canvas Sign;

    void Start() {
      FullBlock.SetActive(false);
      CrackedBlock.SetActive(true);
      hitCount++;
    }

    protected override void Shatter(float explodeMag, Vector3 explodePoint) {
      base.Shatter(explodeMag, explodePoint);
      Destroy(Sign.gameObject);
    }
  }
}

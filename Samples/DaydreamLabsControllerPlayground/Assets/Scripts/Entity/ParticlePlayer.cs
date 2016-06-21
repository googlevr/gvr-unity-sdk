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

namespace GVR.Entity {
  public class ParticlePlayer : ArtPlayerBase<ParticleSystem> {
    [Header("Particle System Settings")]
    public bool StopOnLoad = true;
    public Color StartColor = Color.white;

    protected override void OnAfterStart() {
      if (Player != null) {
        Player.startColor = StartColor;
        if (StopOnLoad) {
          Player.Stop();
        }
      }
    }

    protected override void FireAction() {
      if (Player != null) {
        Player.Play();
      }
    }

    public override void Reset() {
      if (Player != null) {
        Player.Stop();
      }
    }
  }
}

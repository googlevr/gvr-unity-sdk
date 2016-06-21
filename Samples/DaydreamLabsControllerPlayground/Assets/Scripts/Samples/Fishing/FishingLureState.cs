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

using UnityEngine.Events;

namespace GVR.Samples.Fishing {
  /// <summary>
  /// The states used to control fishing lure behavior.
  /// </summary>
  [System.Serializable]
  public enum FishingLureState {
    /// <summary>
    /// The lure is reeled all the way in and
    /// attached to its attach point.
    /// </summary>
    ReeledIn,

    /// <summary>
    /// The lure is in the air flying away from the player
    /// </summary>
    Casting,

    /// <summary>
    /// The lure is being reeled back in to the attach point
    /// </summary>
    Reeling,

    /// <summary>
    /// The lure is at rest in the water.
    /// </summary>
    InWater,

    /// <summary>
    /// The lure is at rest on the ground (not water).
    /// </summary>
    OnGround
  }

  /// <summary>
  /// This event is used to communicate the lure's current state
  /// to listening game objects.
  /// </summary>
  [System.Serializable]
  public class FishingLureStateEvent : UnityEvent<FishingLureState> {
  }
}

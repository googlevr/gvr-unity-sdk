// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the MIT License, you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//     http://www.opensource.org/licenses/mit-license.php
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Interface for a mathematical model that uses the orientation and location
/// of the physical controller, and predicts the location of the controller and pointer
/// to determine where to place the controller model within the scene.
public abstract class GvrBaseArmModel : MonoBehaviour {
  /// Vector to represent the controller's location relative to
  /// the user's head position.
  public abstract Vector3 ControllerPositionFromHead { get; }

  /// Quaternion to represent the controller's rotation relative to
  /// the user's head position.
  public abstract Quaternion ControllerRotationFromHead { get; }

  /// Vector to represent the pointer's location relative to
  /// the controller.
  public abstract Vector3 PointerPositionFromController { get; }

  /// Quaternion to represent the pointer's rotation relative to
  /// the controller.
  public abstract Quaternion PointerRotationFromController { get; }

  /// The suggested rendering alpha value of the controller.
  /// This is to prevent the controller from intersecting the face.
  /// The range is always 0 - 1 but can be scaled by individual
  /// objects when using the GvrBaseControllerVisual script.
  public abstract float PreferredAlpha { get; }

  /// The suggested rendering alpha value of the controller tooltips.
  /// This is to only display the tooltips when the player is looking
  /// at the controller, and also to prevent the tooltips from intersecting the
  /// player's face.
  public abstract float TooltipAlphaValue { get; }
}

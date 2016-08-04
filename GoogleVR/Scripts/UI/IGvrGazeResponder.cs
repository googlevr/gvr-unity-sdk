// Copyright 2015 Google Inc. All rights reserved.
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

/// This script provides an interface for gaze based responders used with
/// the GvrGaze script.
public interface IGvrGazeResponder {
  /// Called when the user is looking on a GameObject with this script,
  /// as long as it is set to an appropriate layer (see GvrGaze).
  void OnGazeEnter();

  /// Called when the user stops looking on the GameObject, after OnGazeEnter
  /// was already called.
  void OnGazeExit();

  /// Called when the trigger is used, between OnGazeEnter and OnGazeExit.
  void OnGazeTrigger();
}

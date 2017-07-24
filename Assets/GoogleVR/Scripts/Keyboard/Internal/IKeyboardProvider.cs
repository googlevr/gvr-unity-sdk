// Copyright 2017 Google Inc. All rights reserved.
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

namespace Gvr.Internal {
  /// Internal interface that abstracts an implementation of a keyboard.
  ///
  /// Each platform has a different concrete implementation of a Keyboard Provider.
  /// For example, if running on the Unity Editor, we use an implementation that
  /// emulates the keyboard behaviour. If running on a real Android device,
  /// we use an implementation that uses the underlying Daydream keyboard API.
  interface IKeyboardProvider {
    /// Notifies the controller provider that the application has paused.
    void OnPause();

    /// Notifies the controller provider that the application has resumed.
    void OnResume();

    /// Reads the controller's current state and stores it in outState.
    void ReadState(KeyboardState outState);

    bool Create(GvrKeyboard.KeyboardCallback keyboardEvent);

    void UpdateData();

    void Render(int eye, Matrix4x4 modelview, Matrix4x4 projection, Rect viewport);

    void Hide();

    void Show(Matrix4x4 controllerMatrix, bool useRecommended, float distance, Matrix4x4 model);

    void SetInputMode(GvrKeyboardInputMode mode);

    string EditorText { get; set; }
  }
}

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
  /// Factory that provides a concrete implementation of IKeyboardProvider for the
  /// current platform.
  static class KeyboardProviderFactory {
    static internal IKeyboardProvider CreateKeyboardProvider(GvrKeyboard owner)
    {
#if UNITY_EDITOR
      return new EmulatorKeyboardProvider();
#elif UNITY_ANDROID && UNITY_HAS_GOOGLEVR
      return new AndroidNativeKeyboardProvider();
#else
      // Other platforms not supported, including iOS and Unity versions w/o the native integraiton.
      Debug.LogWarning("Platform not supported");
      return new DummyKeyboardProvider();
#endif  // UNITY_EDITOR
    }
  }
}

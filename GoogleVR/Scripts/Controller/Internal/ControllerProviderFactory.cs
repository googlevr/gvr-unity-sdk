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
// See the License for the specific language governing permissioßns and
// limitations under the License.

// The controller is not available for versions of Unity without the
// // GVR native integration.
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;

/// @cond
namespace Gvr.Internal {
  /// Factory that provides a concrete implementation of IControllerProvider for the
  /// current platform.
  static class ControllerProviderFactory {
    /// Provides a concrete implementation of IControllerProvider appropriate for the current
    /// platform. This method never returns null. In the worst case, it might return a dummy
    /// provider if the platform is not supported.
    static internal IControllerProvider CreateControllerProvider(GvrController owner) {
#if UNITY_EDITOR || UNITY_STANDALONE
      // Use the Controller Emulator.
      return new EmulatorControllerProvider(owner.emulatorConnectionMode);
#elif UNITY_ANDROID
      // Use the GVR C API.
      return new AndroidNativeControllerProvider();
#else
      // Platform not supported.
      Debug.LogWarning("No controller support on this platform.");
      return new DummyControllerProvider();
#endif  // UNITY_EDITOR || UNITY_STANDALONE
    }
  }
}
/// @endcond

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

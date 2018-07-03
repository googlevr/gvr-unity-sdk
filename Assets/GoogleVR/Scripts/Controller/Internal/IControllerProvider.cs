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

using System;

/// @cond
namespace Gvr.Internal {
  /// Internal interface that abstracts an implementation of a controller.
  ///
  /// Each platform has a different concrete implementation of a Controller Provider.
  /// For example, if running on the Unity Editor, we use an implementation that
  /// communicates with the controller emulator via USB or WiFi. If running on a real
  /// Android device, we use an implementation that uses the underlying Daydream controller API.
  interface IControllerProvider : IDisposable {
    /// True if controller has battery status support.
    bool SupportsBatteryStatus { get; }

    /// Reads the number of controllers the system is configured to use.  This does not
    /// indicate the number of currently connected controllers.
    int MaxControllerCount { get; }

    /// Notifies the controller provider that the application has paused.
    void OnPause();

    /// Notifies the controller provider that the application has resumed.
    void OnResume();

    /// Reads the controller's current state and stores it in outState.
    void ReadState(ControllerState outState, int controller_id);
  }
}
/// @endcond


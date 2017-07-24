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
// See the License for the specific language governing permissioßns and
// limitations under the License.

// This provider is only available in the editor.
#if UNITY_EDITOR

using Gvr;

namespace Gvr.Internal {
  /// Controller provider used when playing in the Unity Editor.
  /// Supports the Controller Emulator and Mouse input to mock the controller.
  class EditorControllerProvider : IControllerProvider {
    private EmulatorControllerProvider emulatorControllerProvider;
    private MouseControllerProvider mouseControllerProvider;

    ControllerState emulatorState = new ControllerState();
    ControllerState mouseState = new ControllerState();

    public bool SupportsBatteryStatus {
      get { return emulatorControllerProvider.SupportsBatteryStatus; }
    }

    internal EditorControllerProvider(GvrControllerInput.EmulatorConnectionMode connectionMode) {
      emulatorControllerProvider = new EmulatorControllerProvider(connectionMode);
      mouseControllerProvider = new MouseControllerProvider();
    }

    public void ReadState(ControllerState outState) {
      emulatorControllerProvider.ReadState(emulatorState);
      mouseControllerProvider.ReadState(mouseState);

      // Defaults to mouse state if the emulator isn't available.
      if (emulatorState.connectionState != GvrConnectionState.Connected
          && mouseState.connectionState == GvrConnectionState.Connected) {
        outState.CopyFrom(mouseState);
      } else {
        outState.CopyFrom(emulatorState);
      }
    }

    public void OnPause() {
      emulatorControllerProvider.OnPause();
      mouseControllerProvider.OnPause();
    }

    public void OnResume() {
      emulatorControllerProvider.OnResume();
      mouseControllerProvider.OnResume();
    }
  }
}

#endif  // UNITY_EDITOR

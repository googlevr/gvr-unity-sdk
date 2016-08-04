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

using Gvr;

/// @cond
namespace Gvr.Internal {
  /// Dummy controller provider.
  /// Used in platforms that do not support controllers.
  class DummyControllerProvider : IControllerProvider {
    private ControllerState dummyState = new ControllerState();
    internal DummyControllerProvider() {}
    public void ReadState(ControllerState outState) {
      outState.CopyFrom(dummyState);
    }
    public void OnPause() {}
    public void OnResume() {}
  }
}
/// @endcond

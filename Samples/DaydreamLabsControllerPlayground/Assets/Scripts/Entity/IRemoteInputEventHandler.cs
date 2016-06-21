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
using UnityEngine.EventSystems;

namespace GVR.Entity {
  public interface IRemoteInputEventHandler : IEventSystemHandler {
    void OnRemoteTouchDown(out bool handled, Vector2 touchPos, Transform remoteTransform);

    void OnRemoteTouchDelta(out bool handled, Vector2 touchPos, Transform remoteTransform);

    void OnRemoteTouchUp(out bool handled, Vector2 touchPos, Transform remoteTransform);

    void OnRemotePressDown(out bool handled);

    void OnRemotePressDelta(out bool handled);

    void OnRemotePressUp(out bool handled);

    void OnRemoteOrientation(out bool handled, Transform remoteTransform);

    void OnRemotePointEnter(out bool handled);

    void OnRemotePointExit(out bool handled);
  }
}

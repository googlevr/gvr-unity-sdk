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

using GVR.Input;
using UnityEngine;

namespace GVR.Events.Trigger {
  public abstract class TriggerListenerBase<T> : MonoBehaviour where T : TriggerObject {
    public TriggerContract TriggerContract;

    protected abstract void HandleEnter(T t);
    protected abstract void HandleExit(T t);

    void OnTriggerEnter(Collider other) {
      T[] tos = other.GetComponents<T>();
      for (int i = 0; i < tos.Length; i++) {
        tos[i].TriggerEnter();
        if (TriggerContract == tos[i].TriggerContract) {
          HandleEnter(tos[i]);
          tos[i].TriggerEnterAccepted();
        } else {
          tos[i].TriggerEnterRejected();
        }
      }
    }

    void OnTriggerExit(Collider other) {
      T[] tos = other.GetComponents<T>();
      for (int i = 0; i < tos.Length; i++) {
        tos[i].TriggerExit();
        if (TriggerContract == tos[i].TriggerContract) {
          HandleExit(tos[i]);
        }
      }
    }
  }

  public class TriggerListenerGameObject : TriggerListenerBase<TriggerObjectGameObject> {
    public GameObjectEvent OnEnter;
    public GameObjectEvent OnExit;

    protected override void HandleEnter(TriggerObjectGameObject t) {
      OnEnter.Invoke(t.Target);
    }

    protected override void HandleExit(TriggerObjectGameObject t) {
      OnEnter.Invoke(t.Target);
    }
  }
}

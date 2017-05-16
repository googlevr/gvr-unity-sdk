// Copyright 2017 Google Inc. All rights reserved.
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using UnityEngine;
using System;

// This is an abstract class instead of an interface so that it can be exposed in Unity's
// editor. It inherits from MonoBehaviour so that it can be directly used as a game object.
public abstract class GvrKeyboardDelegateBase : MonoBehaviour {

  public abstract void OnKeyboardShow();

  public abstract void OnKeyboardHide();

  public abstract void OnKeyboardUpdate(string edit_text);

  public abstract void OnKeyboardEnterPressed(string edit_text);

  public abstract void OnKeyboardError(GvrKeyboardError errorCode);

  public abstract event EventHandler KeyboardHidden;
  public abstract event EventHandler KeyboardShown;
}


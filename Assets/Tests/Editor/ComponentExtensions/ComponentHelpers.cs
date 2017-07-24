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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

/// Provides functions for calling Unity lifecycle methods via reflection to use
/// when unit testing components.
public static class ComponentHelpers {
  private const string ON_ENABLE_METHOD_NAME = "OnEnable";
  private const string AWAKE_METHOD_NAME = "Awake";
  private const string START_METHOD_NAME = "Start";
  private const string UPDATE_METHOD_NAME = "Update";
  private const string LATE_UPDATE_METHOD_NAME = "LateUpdate";
  private const string ON_DISABLE_METHOD_NAME = "OnDisable";
  private const string ON_DESTROY_METHOD_NAME = "OnDestroy";

  public static T AddComponentInvokeLifecycle<T>(GameObject obj)
    where T : MonoBehaviour {
    T result = obj.AddComponent<T>();

    CallOnEnable(result);
    CallAwake(result);

    return result;
  }

  public static void CallOnEnable<T>(T obj) {
    CallMethod(obj, ON_ENABLE_METHOD_NAME);
  }

  public static void CallAwake<T>(T obj) {
    CallMethod(obj, AWAKE_METHOD_NAME);
  }

  public static void CallStart<T>(T obj) {
    CallMethod(obj, START_METHOD_NAME);
  }

  public static void CallUpdate<T>(T obj) {
    CallMethod(obj, UPDATE_METHOD_NAME);
  }

  public static void CallLateUpdate<T>(T obj) {
    CallMethod(obj, LATE_UPDATE_METHOD_NAME);
  }

  public static void CallOnDisable<T>(T obj) {
    CallMethod(obj, ON_DISABLE_METHOD_NAME);
  }

  public static void CallOnDestroy<T>(T obj) {
    CallMethod(obj, ON_DESTROY_METHOD_NAME);
  }

  public static void CallMethod<T>(T obj, string methodName) {
    Type type = obj.GetType();

    MethodInfo method = type.GetMethod(methodName,
                          BindingFlags.Instance |
                          BindingFlags.Public |
                          BindingFlags.NonPublic);

    if (method != null) {
      method.Invoke(obj, null);
    }
  }
}

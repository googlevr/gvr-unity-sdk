// Copyright 2015 Google Inc. All rights reserved.
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
#if UNITY_ANDROID

using UnityEngine;
using System.Collections.Generic;

public abstract class AndroidBaseVRDevice : BaseVRDevice {
  protected AndroidJavaObject cardboardActivity;

  public override bool SupportsNativeDistortionCorrection(List<string> diagnostics) {
    bool support = base.SupportsNativeDistortionCorrection(diagnostics);
    if (cardboardActivity == null) {
      diagnostics.Add("Cannot access Activity");
    }
    return support;
  }

  public override void Destroy() {
    if (cardboardActivity != null) {
      cardboardActivity.Dispose();
      cardboardActivity = null;
    }
    base.Destroy();
  }

  protected virtual void ConnectToActivity() {
    try {
      using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
        cardboardActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
      }
    } catch (AndroidJavaException e) {
      cardboardActivity = null;
      Debug.LogError("Exception while connecting to the Activity: " + e);
    }
  }

  protected virtual bool CallActivityMethod(string name, params object[] args) {
    return CallObjectMethod(cardboardActivity, name, args);
  }

  protected virtual bool CallActivityMethod<T>(ref T result, string name, params object[] args) {
    return CallObjectMethod(ref result, cardboardActivity, name, args);
  }

  protected AndroidJavaObject Create(string className, params object[] args) {
    try {
      return new AndroidJavaObject(className, args);
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception creating object " + className + ": " + e);
      return null;
    }
  }

  protected static bool CallObjectMethod(AndroidJavaObject jo, string name, params object[] args) {
    if (jo == null) {
      Debug.LogError("Object is null when calling method " + name);
      return false;
    }
    try {
      jo.Call(name, args);
      return true;
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception calling method " + name + ": " + e);
      return false;
    }
  }

  protected static bool CallObjectMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                            params object[] args) {
    if (jo == null) {
      Debug.LogError("Object is null when calling method " + name);
      return false;
    }
    try {
      result = jo.Call<T>(name, args);
      return true;
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception calling method " + name + ": " + e);
      return false;
    }
  }
}

#endif

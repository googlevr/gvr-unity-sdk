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
using System.Collections;

public class GVRDemoManager : MonoBehaviour {
  public GameObject m_launchVrHomeButton;
  public DemoInputManager m_demoInputManager;

  void Start() {
#if !UNITY_HAS_GOOGLEVR || !UNITY_ANDROID || UNITY_EDITOR
    if (m_launchVrHomeButton == null) {
      return;
    }
    m_launchVrHomeButton.SetActive(false);
#else
    GvrDaydreamApi.Create();
#endif  // !UNITY_HAS_GOOGLEVR || !UNITY_ANDROID || UNITY_EDITOR
  }

#if UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR
  void Update() {
    if (m_launchVrHomeButton == null || m_demoInputManager == null) {
      return;
    }
    m_launchVrHomeButton.SetActive(m_demoInputManager.IsCurrentlyDaydream());
  }
#endif  // UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR

  public void LaunchVrHome() {
#if UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR
    GvrDaydreamApi.LaunchVrHome();
#endif  // UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR
  }
}

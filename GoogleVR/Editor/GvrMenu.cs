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

using UnityEngine;
using UnityEditor;

public class GvrMenu {
  [MenuItem("GoogleVR/Documentation/Developers Site", false, 100)]
  private static void OpenDocumentation() {
    Application.OpenURL("https://developers.google.com/vr");
  }

  [MenuItem("GoogleVR/Documentation/Unity Guide", false, 100)]
  private static void OpenUnityGuide() {
    Application.OpenURL("https://developers.google.com/vr/unity/guide");
  }

  [MenuItem("GoogleVR/Documentation/Release Notes", false, 100)]
  private static void OpenReleaseNotes() {
    Application.OpenURL("https://developers.google.com/vr/unity/release-notes");
  }

  [MenuItem("GoogleVR/Documentation/Known Issues", false, 100)]
  private static void OpenKnownIssues() {
    Application.OpenURL("https://developers.google.com/vr/unity/release-notes#known_issues");
  }

  [MenuItem("GoogleVR/Report Bug", false, 100)]
  private static void OpenReportBug() {
    Application.OpenURL("https://github.com/googlesamples/cardboard-unity/issues");
  }

  [MenuItem("GoogleVR/About Google VR", false, 200)]
  private static void OpenAbout() {
    EditorUtility.DisplayDialog("Google VR SDK for Unity",
        "Version: " + GvrViewer.GVR_SDK_VERSION + "\n\n"
        + "License: Apache 2.0\n"
        + "Copyright: ©2015 Google Inc. All rights reserved.\n"
        + "See LICENSE for additional license information.",
        "OK");
  }
}
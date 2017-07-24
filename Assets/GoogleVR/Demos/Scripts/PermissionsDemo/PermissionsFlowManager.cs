// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0(the "License");
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

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// Manages the permission flow in PermissionsDemo.
#if UNITY_ANDROID || UNITY_EDITOR
public class PermissionsFlowManager : MonoBehaviour {
  private static string[] permissionNames = { "android.permission.READ_EXTERNAL_STORAGE" };

  public Text statusText;

  private static List<GvrPermissionsRequester.PermissionStatus> permissionList =
      new List<GvrPermissionsRequester.PermissionStatus>();

  public void CheckPermission() {
    statusText.text = "Checking permission....";
    GvrPermissionsRequester permissionRequester = GvrPermissionsRequester.Instance;
    if (permissionRequester != null) {
      bool granted = permissionRequester.IsPermissionGranted(permissionNames[0]);
      statusText.text = permissionNames[0] + ": " + (granted ? "Granted" : "Denied");
    } else {
      statusText.text = "Permission requester cannot be initialized.";
    }
  }

  public void RequestPermissions() {
    if (statusText != null) {
      statusText.text = "Requesting permission....";
    }
    GvrPermissionsRequester permissionRequester = GvrPermissionsRequester.Instance;
    if (permissionRequester == null) {
      statusText.text = "Permission requester cannot be initialized.";
      return;
    }
    Debug.Log("Permissions.RequestPermisions: Check if permission has been granted");
    if (!permissionRequester.IsPermissionGranted(permissionNames[0])) {
      Debug.Log("Permissions.RequestPermisions: Permission has not been previously granted");
      if (permissionRequester.ShouldShowRational(permissionNames[0])) {
        statusText.text = "This game needs to access external storage.  Please grant permission when prompted.";
        statusText.color = Color.red;
      }
      permissionRequester.RequestPermissions(permissionNames,
          (GvrPermissionsRequester.PermissionStatus[] permissionResults) =>
          {
            statusText.color = Color.cyan;
            permissionList.Clear();
            permissionList.AddRange(permissionResults);
            string msg = "";
            foreach (GvrPermissionsRequester.PermissionStatus p in permissionList) {
              msg += p.Name + ": " + (p.Granted ? "Granted" : "Denied") + "\n";
            }
            statusText.text = msg;
          });
    }
    else {
      statusText.text = "ExternalStorage permission already granted!";
    }
  }
}
#endif  // (UNITY_ANDROID || UNITY_EDITOR)

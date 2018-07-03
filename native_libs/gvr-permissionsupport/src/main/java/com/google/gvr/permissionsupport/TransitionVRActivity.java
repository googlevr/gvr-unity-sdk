/*
 * Copyright 2016 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.google.gvr.permissionsupport;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

import com.google.vr.ndk.base.DaydreamApi;

/**
 * Intermediate activity used to work around calling exitFromVR from a fragment. This is done by
 * having the fragment start this activity, then this activity exists from VR, and then invokes the
 * permission reuqest activity.
 */
public class TransitionVRActivity extends Activity {
  public static final String PERMISSION_EXTRA = "permissions.PermissionArray";

  private static final int RC_EXIT_VR = 777;
  private static final int RC_ASK_PERMISSION = 778;
  private static final String TAG = "TransitionVRActivity";

  private String[] permissionArray;

  public void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    setImmersiveSticky();
    getWindow()
        .getDecorView()
        .setOnSystemUiVisibilityChangeListener(
            new View.OnSystemUiVisibilityChangeListener() {
              @Override
              public void onSystemUiVisibilityChange(int visibility) {
                if ((visibility & View.SYSTEM_UI_FLAG_FULLSCREEN) == 0) {
                  setImmersiveSticky();
                }
              }
            });

    Intent myIntent = getIntent();
    permissionArray = null;
    if (myIntent.hasExtra(PERMISSION_EXTRA)) {
      permissionArray = myIntent.getStringArrayExtra(PERMISSION_EXTRA);
    }

    if (permissionArray == null || permissionArray.length == 0) {
      Log.w(TAG, "No permissions requested!");
      PermissionsFragment.setPermissionResult(true);
      finish();
      return;
    }

    // Use this code when vrcore supports exitFromVr()
    // Prompt the user to remove the headset and get out of VR mode.
    DaydreamApi daydreamApi = DaydreamApi.create(this);
    daydreamApi.exitFromVr(this, RC_EXIT_VR, null);
    daydreamApi.close();
  }

  @Override
  public void onActivityResult(int requestCode, int resultCode, Intent data) {

    // If we exited successfully, then start the permission intent, otherwise finish.
    if (requestCode == RC_EXIT_VR) {
      if (resultCode == Activity.RESULT_OK) {
        requestPermissions(permissionArray, RC_ASK_PERMISSION);
      } else {
        Log.w(TAG, "exitFromVR returned " + resultCode + ", finishing");
        PermissionsFragment.setPermissionResult(false);
        finish();
      }
    } else {
      super.onActivityResult(requestCode, resultCode, data);
    }
  }

  @Override
  public void onRequestPermissionsResult(
      int requestCode, String[] permissions, int[] grantResults) {
    if (requestCode == RC_ASK_PERMISSION) {

      boolean allPermissionsGranted = true;
      for (int result : grantResults) {
        if (result != PackageManager.PERMISSION_GRANTED) {
          allPermissionsGranted = false;
          break;
        }
      }

      PermissionsFragment.setPermissionResult(allPermissionsGranted);
      finish();
    } else {
      super.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }
  }

  private void setImmersiveSticky() {
    getWindow()
        .getDecorView()
        .setSystemUiVisibility(
            View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_FULLSCREEN
                | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
  }
}

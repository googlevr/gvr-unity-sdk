/*
 * Copyright 2017 Google Inc. All Rights Reserved.
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

package com.google.gvr.keyboardsupport;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

import com.google.vr.ndk.base.DaydreamApi;

/**
 * Intermediate activity used to work around calling exitFromVR from a fragment. This is done by
 * having the fragment start this activity, then this activity exists from VR, and then invokes the
 * Keyboard play store intent.
 */

public class TransitionVRActivity extends Activity {
  private static final int RC_EXIT_VR = 777;
  private static final String TAG = "TransitionVRActivity";
  private static final String DD_KEYBOARD_BUNDLE_ID = "com.google.android.vr.inputmethod";

  public void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    // Use this code when vrcore supports exitFromVr()
    // Prompt the user to remove the headset and get out of VR mode.
    DaydreamApi daydreamApi = DaydreamApi.create(this);
    daydreamApi.exitFromVr(this, RC_EXIT_VR, null);
    daydreamApi.close();
  }

  @Override
  public void onActivityResult(int requestCode, int resultCode, Intent data) {

    // If we exited successfully, then start the intent, otherwise finish.
    if (requestCode == RC_EXIT_VR) {
      if (resultCode == Activity.RESULT_OK) {
        // Launch Play Store intent
        Intent intent = new Intent(Intent.ACTION_VIEW,
            Uri.parse("market://details?id=" + DD_KEYBOARD_BUNDLE_ID));
        startActivity(intent);
      } else {
        Log.w(TAG, "exitFromVR returned " + resultCode + ", finishing");
        finish();
      }
    } else {
      super.onActivityResult(requestCode, resultCode, data);
    }
    KeyboardFragment.callBackCall();
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

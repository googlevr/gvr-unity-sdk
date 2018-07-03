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
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.ComponentName;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import com.google.vr.ndk.base.DaydreamApi;

/**
 * Fragment suitable to add to a Unity player activity. Contains static methods to call from the
 * game engine code to start the process of requesting the user to install/update DD Keyboard app.
 * This process includes exiting VR mode.
 */
public class KeyboardFragment extends Fragment {

  private static final String TAG = "KeyboardFragment";
  public static final String FRAGMENT_TAG = "avr_KeyboardFragment";

  // Callback object passed in by the caller.
  // This callback is invoked when the user
  // has completed the process.
  private static KeyboardCallback keyboardCallback = null;

  /**
   * Obtains the instance of the fragment. If the fragment does not exist, it is created and added
   * to the parentActivity.
   *
   * @param parentActivity - The activity to attach the fragment to.
   * @return The instance of the fragment, or null if the fragment could not be created.
   */
  public static KeyboardFragment getInstance(Activity parentActivity) {

    KeyboardFragment fragment =
        (KeyboardFragment) parentActivity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);

    if (fragment == null) {
      try {
        Log.i(TAG, "Creating PlayGamesFragment");
        fragment = new KeyboardFragment();
        FragmentTransaction trans = parentActivity.getFragmentManager().beginTransaction();
        trans.add(fragment, FRAGMENT_TAG);
        trans.commitAllowingStateLoss();
      } catch (Throwable th) {
        Log.e(TAG, "Cannot launch KeyboardFragment:" + th.getMessage(), th);
        return null;
      }
    }

    return fragment;
  }

  @Override
  public void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);

    // Retain this fragment in the event of activity re-configuration.
    setRetainInstance(true);
  }

  /**
   * Starts the launch play store flow. This includes prompting the user to remove the phone from
   * the headset, then presenting the 2D play store.
   */
  public void launchPlayStore(KeyboardCallback callback) {
    keyboardCallback = callback;
    getActivity().runOnUiThread(new Runnable() {
      @Override
      public void run() {
        ComponentName componentName = new ComponentName(getContext(), TransitionVRActivity.class);
        Intent intent = DaydreamApi.createVrIntent(componentName);
        DaydreamApi.create(getContext()).launchInVr(intent);
      }
    });
  }

  public static void callBackCall() {
    if (keyboardCallback != null) {
      keyboardCallback.onPlayStoreResult();
    } else {
      Log.w(TAG, "Keyboard callback object is null!");
    }
  }

  /** Defines the callback that will be invoked when the flow has completed. */
  public interface KeyboardCallback {
    void onPlayStoreResult();
  }
}

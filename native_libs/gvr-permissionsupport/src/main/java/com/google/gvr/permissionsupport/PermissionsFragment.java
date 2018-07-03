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
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.ComponentName;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import com.google.vr.ndk.base.DaydreamApi;

/**
 * Fragment suitable to add to a Unity player activity. Contains static methods to call from the
 * game engine code to start the process of requesting the user to grant dangerous permissions. This
 * process includes exiting VR mode and then returning when complete.
 */
public class PermissionsFragment extends Fragment {

  private static final String TAG = "PermissionsFragment";
  public static final String FRAGMENT_TAG = "avr_PermissionsFragment";

  // Callback object passed in by the caller.  This callback is invoked when the user
  // has completed the permission review process.
  private static PermissionsCallback permissionsCallback = null;

  /**
   * Obtains the instance of the fragment. If the fragment does not exist, it is created and added
   * to the parentActivity.
   *
   * @param parentActivity - The activity to attach the fragment to.
   * @return The instance of the fragment, or null if the fragment could not be created.
   */
  public static PermissionsFragment getInstance(Activity parentActivity) {

    PermissionsFragment fragment =
        (PermissionsFragment) parentActivity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);

    if (fragment == null) {
      try {
        Log.i(TAG, "Creating PlayGamesFragment");
        fragment = new PermissionsFragment();
        FragmentTransaction trans = parentActivity.getFragmentManager().beginTransaction();
        trans.add(fragment, FRAGMENT_TAG);
        trans.commitAllowingStateLoss();
      } catch (Throwable th) {
        Log.e(TAG, "Cannot launch PermissionsFragment:" + th.getMessage(), th);
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
   * Checks if the permission specified has been granted or not.
   *
   * @param permission - The permission in question.
   * @return true if the permission is granted.
   */
  public boolean hasPermission(String permission) {
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
      Log.d(TAG, "Checking permission " + permission);
      return getActivity().checkSelfPermission(permission) == PackageManager.PERMISSION_GRANTED;
    } else {
      return true;
    }
  }

  /**
   * Checks an array of permissions for being granted or not.
   *
   * @param permissions - The names of the permissions to check.
   * @return array of boolean corresponding to the permission array.
   */
  public boolean[] hasPermissions(String[] permissions) {
    if (permissions == null) {
      Log.w(TAG, "No permission asked, no permissions returned");
      return new boolean[0];
    }

    int length = permissions.length;
    boolean[] grantResults = new boolean[length];
    for (int i = 0; i < length; i++) {
      Log.d(TAG, "Checking permission for " + permissions[i]);
      grantResults[i] = hasPermission(permissions[i]);
    }

    return grantResults;
  }

  /**
   * Gets whether you should show UI with rationale for requesting a permission. You should do this
   * only if you do not have the permission and the context in which the permission is requested
   * does not clearly communicate to the user what would be the benefit from granting this
   * permission.
   */
  public boolean shouldShowRational(String permission) {
    return Build.VERSION.SDK_INT >= Build.VERSION_CODES.M
        && getActivity().shouldShowRequestPermissionRationale(permission);
  }

  /**
   * Starts the permission request flow. This includes prompting the user to remove the phone from
   * the headset, then presenting the 2D prompts requesting permission.
   *
   * @param permissionArray - the names of the permissions requesting.
   */
  public void requestPermission(final String[] permissionArray, PermissionsCallback callback) {
    permissionsCallback = callback;
    getActivity().runOnUiThread(new Runnable() {
      @Override
      public void run() {
        ComponentName componentName = new ComponentName(getContext(), TransitionVRActivity.class);
        Intent intent = DaydreamApi.createVrIntent(componentName);
        intent.putExtra(TransitionVRActivity.PERMISSION_EXTRA, permissionArray);
        DaydreamApi.create(getContext()).launchInVr(intent);
      }
    });
   }

  /**
   * Called by the other activities in the permission request flow to indicate the flow has
   * completed. This has the side effect of calling the permissions callback.
   *
   * @param allGranted - True if all permissions are granted.
   */
  public static void setPermissionResult(final boolean allGranted) {

    if (permissionsCallback != null) {
      permissionsCallback.onRequestPermissionResult(allGranted);
    } else {
      Log.w(TAG, "Permission callback object is null!");
    }
  }

  /** Defines the callback that will be invoked when the permission request flow has completed. */
  public interface PermissionsCallback {
    void onRequestPermissionResult(boolean allAccepted);
  }
}

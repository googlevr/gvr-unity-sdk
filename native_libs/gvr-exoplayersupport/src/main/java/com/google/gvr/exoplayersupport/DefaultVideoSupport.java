/*
 * Copyright (C) 2016 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.google.gvr.exoplayersupport;

import android.app.Activity;
import android.util.Log;

import com.google.gvr.exoplayersupport.sample.HlsVideoFactory;
import com.google.gvr.exoplayersupport.sample.LocalVideoFactory;

/**
 * Support class used to locate the default video player factory. If a custom video player is needed
 * for an application, then a class which contains these same public static methods should be
 * created and registered with the plugin.
 */
public final class DefaultVideoSupport {
  private static final String TAG = "DefaultVideoSupport";
  private static DefaultVideoPlayerFactory dashfactory;
  private static LocalVideoFactory localfactory;
  private static HlsVideoFactory hlsfactory;

  /**
   * initializes the factory or factory provider. This method is called before the first time
   * getPlayerFactory is called. It is intended to provide a notification that the factory is about
   * to be accessed, allowing for more complex initialization.
   *
   * @param unityPlayerActivity - the activity using the video player. From the name you can guess
   *     that the Unity plugin is the primary use case for this method.
   */
  public static void initializePlayerFactory(Activity unityPlayerActivity) {
    dashfactory = null;
    localfactory = null;
    hlsfactory = null;
  }

  /**
   * Returns a video player instance. It is assumed that the player factory is thread safe, and each
   * thread will call getPlayerFactory(). The return value is not necessarily cached, meaning there
   * could be multiple calls to this method.
   *
   * @param type - the type of factory requested. The values are defined in VideoPlayerFactory
   *     interface.
   * @return the factory for the given type of null if no factory can satisfy the type requested.
   */
  public static VideoPlayerFactory getPlayerFactory(int type) {
    switch (type) {
      case VideoPlayerFactory.DashType:
        if (dashfactory == null) {
          dashfactory = new DefaultVideoPlayerFactory();
        }
        return dashfactory;
      case VideoPlayerFactory.HLSType:
        if (hlsfactory == null) {
          hlsfactory = new HlsVideoFactory();
        }
        return hlsfactory;
      case VideoPlayerFactory.OtherType:
        if (localfactory == null) {
          localfactory = new LocalVideoFactory();
        }
        return localfactory;
      default:
        Log.e(TAG, "Cannot make factory for type: " + type);
        return null;
    }
  }
}

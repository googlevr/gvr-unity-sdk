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
package com.google.gvr.exoplayersupport.impl;

import com.google.gvr.exoplayersupport.VideoPlayer;

/** VideoPlayer.Listener implementation that connects to the native video layer. */
public class NativeVideoCallbacks implements VideoPlayer.Listener {

  static {
    System.loadLibrary("gvrvideo");
  }

  /**
   * Called when there is an exception caught by the player. Exceptions that are raised here are
   * logged by default, but no other action is taken. This means the state of the player may be
   * inconsistent and it is the responsibility of the caller to determine the best course of action.
   *
   * @param e - the exception encountered.
   */
  @Override
  public void onError(VideoPlayer player, Exception e) {
    String msg = e.getMessage();
    if (msg == null) {
      msg = e.toString();
    }
    onError(player, e.getClass().getName(), msg);
  }

  protected native void onError(VideoPlayer player, String type, String msg);

  /** Called when the video player is ready to show the first frame. */
  @Override
  public native void onVideoEvent(VideoPlayer player, int eventId);
}

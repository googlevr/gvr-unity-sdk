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

import android.content.Context;

/**
 * Interface defining the factory for video players. The implementation of this interface is
 * discovered using @see DefaultVideoSupport or equivalent class.
 */
public interface VideoPlayerFactory {
  /**
   * Creates a new instance of the video player.
   *
   * @param context - the current context that the player will be used with.
   * @return the uninitialized video player object.
   */
  VideoPlayer createPlayer(Context context);

  /**
   * Destroys the instance of the video player. This is called when the player is no longer needed
   * by the application. DestroyPlayer is called on an instance of the same type of factory.
   *
   * @param player - the video player to destroy.
   */
  void destroyPlayer(VideoPlayer player);

  /**
   * Creates a builder for creating the renderers for the given parameters. This is used when
   * initializing the player as well as when switching content within an existing player.
   *
   * @param context - the current context that the player will be used with.
   * @param type - the type of video, see the constants in VideoPlayerFactory or a custom type
   *     value.
   * @param videoURL - the URL of the video.
   * @param contentId - the content id of the video.
   * @param providerId - the provider id of the video.
   * @param requireSecurePlayback - true for secure rendering in order to support DRM.
   * @return AsyncRendererBuilder that will create the proper renderers for the player.
   */
  AsyncRendererBuilder createRendererBuilder(
      Context context,
      int type,
      String videoURL,
      String contentId,
      String providerId,
      boolean requireSecurePlayback);

  /**
   * Constant used when locating a factory type. DashType indicates the video stream is encoded
   * using DASH. The value of the constant matches the constants in the Util class of the Exoplayer
   * library.
   */
  int DashType = 0;
  /**
   * Constant used when locating a factory type. DashType indicates the video stream is encoded
   * using HLS. The value of the constant matches the constants in the Util class of the Exoplayer
   * library.
   */
  int HLSType = 2;
  /**
   * Constant used when locating a factory type. DashType indicates the video stream is encoded
   * using some 'other' type. How to handle other is up to the support class that is registered. The
   * value of the constant matches the constants in the Util class of the Exoplayer library.
   */
  int OtherType = 3;
}

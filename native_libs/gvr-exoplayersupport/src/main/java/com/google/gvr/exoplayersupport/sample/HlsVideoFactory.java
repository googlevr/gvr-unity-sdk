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
package com.google.gvr.exoplayersupport.sample;

import android.content.Context;

import com.google.android.exoplayer.util.Util;
import com.google.gvr.exoplayersupport.AsyncRendererBuilder;
import com.google.gvr.exoplayersupport.VideoPlayer;
import com.google.gvr.exoplayersupport.VideoPlayerFactory;

/** Video factory implementation for HLS videos. */
public class HlsVideoFactory implements VideoPlayerFactory {
  @Override
  public VideoPlayer createPlayer(Context context) {
    return new VideoExoPlayer(context);
  }

  @Override
  public void destroyPlayer(VideoPlayer player) {
    if (player instanceof VideoExoPlayer) {
      ((VideoExoPlayer) player).stop();
    }
  }

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
  @Override
  public AsyncRendererBuilder createRendererBuilder(
      Context context,
      int type,
      String videoURL,
      String contentId,
      String providerId,
      boolean requireSecurePlayback) {
    String userAgent = Util.getUserAgent(context, "VRSampleVideo");
    return new HLSAsyncRendererBuilder(context, userAgent, videoURL);
  }
}

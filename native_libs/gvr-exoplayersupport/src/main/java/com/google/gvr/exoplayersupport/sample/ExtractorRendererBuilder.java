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
import android.media.AudioManager;
import android.media.MediaCodec;
import android.net.Uri;
import android.os.Handler;
import com.google.android.exoplayer.MediaCodecAudioTrackRenderer;
import com.google.android.exoplayer.MediaCodecSelector;
import com.google.android.exoplayer.MediaCodecVideoTrackRenderer;
import com.google.android.exoplayer.TrackRenderer;
import com.google.android.exoplayer.audio.AudioCapabilities;
import com.google.android.exoplayer.extractor.ExtractorSampleSource;
import com.google.android.exoplayer.text.TextTrackRenderer;
import com.google.android.exoplayer.upstream.Allocator;
import com.google.android.exoplayer.upstream.DataSource;
import com.google.android.exoplayer.upstream.DefaultAllocator;
import com.google.android.exoplayer.upstream.DefaultBandwidthMeter;
import com.google.android.exoplayer.upstream.DefaultUriDataSource;
import com.google.gvr.exoplayersupport.AsyncRendererBuilder;
import com.google.gvr.exoplayersupport.VideoPlayer;

/** Builder class for rendering video from a local source, such as the assets directory. */
public class ExtractorRendererBuilder implements AsyncRendererBuilder {

  private static final int BUFFER_SEGMENT_SIZE = 64 * 1024;
  private static final int BUFFER_SEGMENT_COUNT = 256;

  private final Context context;
  private final String userAgent;
  private final String videoUrl;

  public ExtractorRendererBuilder(Context context, String userAgent, String videoUrl) {
    this.context = context;
    this.userAgent = userAgent;
    this.videoUrl = videoUrl;
  }

  /**
   * Initialize the pipeline of renderers.
   *
   * @param videoPlayer - the video player object.
   * @param targetResolution - the initial target resolution for video playback. This parameter is
   *     ignored for local content since it is not adaptive.
   */
  @Override
  public void init(VideoPlayer videoPlayer, int targetResolution) {
    VideoExoPlayer player = (VideoExoPlayer) videoPlayer;
    Allocator allocator = new DefaultAllocator(BUFFER_SEGMENT_SIZE);
    Handler mainHandler = player.getMainHandler();

    Uri uri = Uri.parse(videoUrl);
    // Build the video and audio renderers.
    DefaultBandwidthMeter bandwidthMeter = new DefaultBandwidthMeter(mainHandler, null);
    DataSource dataSource;
    if (uri.getScheme().startsWith("jar")) {
      dataSource = new ObbDataSource(bandwidthMeter);
    } else {
      dataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
    }
    ExtractorSampleSource sampleSource =
        new ExtractorSampleSource(
            uri,
            dataSource,
            allocator,
            BUFFER_SEGMENT_COUNT * BUFFER_SEGMENT_SIZE,
            mainHandler,
            player,
            0);
    MediaCodecVideoTrackRenderer videoRenderer =
        new MediaCodecVideoTrackRenderer(
            context,
            sampleSource,
            MediaCodecSelector.DEFAULT,
            MediaCodec.VIDEO_SCALING_MODE_SCALE_TO_FIT,
            5000,
            mainHandler,
            player,
            50);
    MediaCodecAudioTrackRenderer audioRenderer =
        new MediaCodecAudioTrackRenderer(
            sampleSource,
            MediaCodecSelector.DEFAULT,
            null,
            true,
            mainHandler,
            player,
            AudioCapabilities.getCapabilities(context),
            AudioManager.STREAM_MUSIC);
    TrackRenderer textRenderer =
        new TextTrackRenderer(sampleSource, player, mainHandler.getLooper());

    // Invoke the callback.
    TrackRenderer[] renderers = new TrackRenderer[VideoExoPlayer.RENDERER_COUNT];
    renderers[VideoExoPlayer.TYPE_VIDEO] = videoRenderer;
    renderers[VideoExoPlayer.TYPE_AUDIO] = audioRenderer;
    renderers[VideoExoPlayer.TYPE_TEXT] = textRenderer;
    player.onRenderers(renderers);
  }

  /** Cancel building the pipeline. */
  @Override
  public void cancel() {
    // does nothing since this is actually an inline builder.
  }
}

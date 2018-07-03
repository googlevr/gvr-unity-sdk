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
import android.os.Handler;
import com.google.android.exoplayer.DefaultLoadControl;
import com.google.android.exoplayer.LoadControl;
import com.google.android.exoplayer.MediaCodecAudioTrackRenderer;
import com.google.android.exoplayer.MediaCodecSelector;
import com.google.android.exoplayer.MediaCodecVideoTrackRenderer;
import com.google.android.exoplayer.SampleSource;
import com.google.android.exoplayer.TrackRenderer;
import com.google.android.exoplayer.audio.AudioCapabilities;
import com.google.android.exoplayer.hls.DefaultHlsTrackSelector;
import com.google.android.exoplayer.hls.HlsChunkSource;
import com.google.android.exoplayer.hls.HlsMasterPlaylist;
import com.google.android.exoplayer.hls.HlsPlaylist;
import com.google.android.exoplayer.hls.HlsPlaylistParser;
import com.google.android.exoplayer.hls.HlsSampleSource;
import com.google.android.exoplayer.hls.PtsTimestampAdjusterProvider;
import com.google.android.exoplayer.metadata.MetadataTrackRenderer;
import com.google.android.exoplayer.metadata.id3.Id3Frame;
import com.google.android.exoplayer.metadata.id3.Id3Parser;
import com.google.android.exoplayer.text.TextTrackRenderer;
import com.google.android.exoplayer.text.eia608.Eia608TrackRenderer;
import com.google.android.exoplayer.upstream.DataSource;
import com.google.android.exoplayer.upstream.DefaultAllocator;
import com.google.android.exoplayer.upstream.DefaultBandwidthMeter;
import com.google.android.exoplayer.upstream.DefaultUriDataSource;
import com.google.android.exoplayer.util.ManifestFetcher;
import com.google.gvr.exoplayersupport.AsyncRendererBuilder;
import com.google.gvr.exoplayersupport.VideoPlayer;
import java.io.IOException;
import java.util.List;

/** Builds the renderers for playing HLS videos. */
public class HLSAsyncRendererBuilder
    implements AsyncRendererBuilder, ManifestFetcher.ManifestCallback<HlsPlaylist> {

  private static final int BUFFER_SEGMENT_SIZE = 64 * 1024;
  private static final int MAIN_BUFFER_SEGMENTS = 254;
  private static final int AUDIO_BUFFER_SEGMENTS = 54;
  private static final int TEXT_BUFFER_SEGMENTS = 2;

  private final Context context;
  private final String userAgent;
  private final String videoUrl;
  private VideoExoPlayer player;
  private int targetResolution;

  private boolean canceled;

  /**
   * Construct the renderer builder. This is for DASH video, both secure and clear.
   *
   * @param context - the context of the application
   * @param userAgent - the user agent name to use
   */
  public HLSAsyncRendererBuilder(Context context, String userAgent, String videoUrl) {
    this.context = context;
    this.userAgent = userAgent;
    this.videoUrl = videoUrl;
  }

  /**
   * Initialize the pipeline of renderers.
   *
   * @param videoPlayer - the video player object.
   */
  @Override
  public void init(VideoPlayer videoPlayer, int targetResolution) {
    this.player = (VideoExoPlayer) videoPlayer;
    this.targetResolution = targetResolution;
    ManifestFetcher<HlsPlaylist> playlistFetcher =
        new ManifestFetcher<>(
            videoUrl, new DefaultUriDataSource(context, userAgent), new HlsPlaylistParser());
    playlistFetcher.singleLoad(player.getMainHandler().getLooper(), this);
  }

  public void cancel() {
    canceled = true;
  }

  @Override
  public void onSingleManifestError(IOException e) {
    if (canceled) {
      return;
    }

    player.onRenderersError(e);
  }

  @Override
  public void onSingleManifest(HlsPlaylist manifest) {
    if (canceled) {
      return;
    }

    Handler mainHandler = player.getMainHandler();
    LoadControl loadControl = new DefaultLoadControl(new DefaultAllocator(BUFFER_SEGMENT_SIZE));
    DefaultBandwidthMeter bandwidthMeter = new DefaultBandwidthMeter();
    PtsTimestampAdjusterProvider timestampAdjusterProvider = new PtsTimestampAdjusterProvider();

    boolean haveSubtitles = false;
    boolean haveAudios = false;
    if (manifest instanceof HlsMasterPlaylist) {
      HlsMasterPlaylist masterPlaylist = (HlsMasterPlaylist) manifest;
      haveSubtitles = !masterPlaylist.subtitles.isEmpty();
      haveAudios = !masterPlaylist.audios.isEmpty();
    }

    // Build the video/id3 renderers.
    DataSource dataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
    HlsChunkSource chunkSource =
        new GVRHlsChunkSource(
            true /* isMaster */,
            dataSource,
            manifest,
            DefaultHlsTrackSelector.newDefaultInstance(context),
            bandwidthMeter,
            timestampAdjusterProvider,
            targetResolution);
    HlsSampleSource sampleSource =
        new HlsSampleSource(
            chunkSource,
            loadControl,
            MAIN_BUFFER_SEGMENTS * BUFFER_SEGMENT_SIZE,
            mainHandler,
            player,
            VideoExoPlayer.TYPE_VIDEO);
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
    MetadataTrackRenderer<List<Id3Frame>> id3Renderer =
        new MetadataTrackRenderer<>(sampleSource, new Id3Parser(), player, mainHandler.getLooper());

    // Build the audio renderer.
    MediaCodecAudioTrackRenderer audioRenderer;
    if (haveAudios) {
      DataSource audioDataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
      HlsChunkSource audioChunkSource =
          new HlsChunkSource(
              false /* isMaster */,
              audioDataSource,
              manifest,
              DefaultHlsTrackSelector.newAudioInstance(),
              bandwidthMeter,
              timestampAdjusterProvider);
      HlsSampleSource audioSampleSource =
          new HlsSampleSource(
              audioChunkSource,
              loadControl,
              AUDIO_BUFFER_SEGMENTS * BUFFER_SEGMENT_SIZE,
              mainHandler,
              player,
              VideoExoPlayer.TYPE_AUDIO);
      audioRenderer =
          new MediaCodecAudioTrackRenderer(
              new SampleSource[] {sampleSource, audioSampleSource},
              MediaCodecSelector.DEFAULT,
              null,
              true,
              player.getMainHandler(),
              player,
              AudioCapabilities.getCapabilities(context),
              AudioManager.STREAM_MUSIC);
    } else {
      audioRenderer =
          new MediaCodecAudioTrackRenderer(
              sampleSource,
              MediaCodecSelector.DEFAULT,
              null,
              true,
              player.getMainHandler(),
              player,
              AudioCapabilities.getCapabilities(context),
              AudioManager.STREAM_MUSIC);
    }

    // Build the text renderer.
    TrackRenderer textRenderer;
    if (haveSubtitles) {
      DataSource textDataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
      HlsChunkSource textChunkSource =
          new HlsChunkSource(
              false /* isMaster */,
              textDataSource,
              manifest,
              DefaultHlsTrackSelector.newSubtitleInstance(),
              bandwidthMeter,
              timestampAdjusterProvider);
      HlsSampleSource textSampleSource =
          new HlsSampleSource(
              textChunkSource,
              loadControl,
              TEXT_BUFFER_SEGMENTS * BUFFER_SEGMENT_SIZE,
              mainHandler,
              player,
              VideoExoPlayer.TYPE_TEXT);
      textRenderer = new TextTrackRenderer(textSampleSource, player, mainHandler.getLooper());
    } else {
      textRenderer = new Eia608TrackRenderer(sampleSource, player, mainHandler.getLooper());
    }

    TrackRenderer[] renderers = new TrackRenderer[VideoExoPlayer.RENDERER_COUNT];
    renderers[VideoExoPlayer.TYPE_VIDEO] = videoRenderer;
    renderers[VideoExoPlayer.TYPE_AUDIO] = audioRenderer;
    renderers[VideoExoPlayer.TYPE_METADATA] = id3Renderer;
    renderers[VideoExoPlayer.TYPE_TEXT] = textRenderer;
    player.onRenderers(renderers);
  }
}

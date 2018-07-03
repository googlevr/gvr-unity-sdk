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
import android.media.MediaCodec;
import android.os.Handler;
import android.util.Log;
import com.google.android.exoplayer.DefaultLoadControl;
import com.google.android.exoplayer.LoadControl;
import com.google.android.exoplayer.MediaCodecAudioTrackRenderer;
import com.google.android.exoplayer.MediaCodecSelector;
import com.google.android.exoplayer.MediaCodecVideoTrackRenderer;
import com.google.android.exoplayer.TrackRenderer;
import com.google.android.exoplayer.chunk.ChunkSampleSource;
import com.google.android.exoplayer.chunk.ChunkSource;
import com.google.android.exoplayer.chunk.FormatEvaluator;
import com.google.android.exoplayer.dash.DashChunkSource;
import com.google.android.exoplayer.dash.DefaultDashTrackSelector;
import com.google.android.exoplayer.dash.mpd.AdaptationSet;
import com.google.android.exoplayer.dash.mpd.MediaPresentationDescription;
import com.google.android.exoplayer.dash.mpd.MediaPresentationDescriptionParser;
import com.google.android.exoplayer.dash.mpd.Period;
import com.google.android.exoplayer.dash.mpd.Representation;
import com.google.android.exoplayer.dash.mpd.UtcTimingElement;
import com.google.android.exoplayer.dash.mpd.UtcTimingElementResolver;
import com.google.android.exoplayer.drm.StreamingDrmSessionManager;
import com.google.android.exoplayer.drm.UnsupportedDrmException;
import com.google.android.exoplayer.upstream.DataSource;
import com.google.android.exoplayer.upstream.DefaultAllocator;
import com.google.android.exoplayer.upstream.DefaultBandwidthMeter;
import com.google.android.exoplayer.upstream.DefaultUriDataSource;
import com.google.android.exoplayer.upstream.UriDataSource;
import com.google.android.exoplayer.util.ManifestFetcher;
import com.google.android.exoplayer.util.Util;
import com.google.gvr.exoplayersupport.AsyncRendererBuilder;
import com.google.gvr.exoplayersupport.VideoPlayer;
import java.io.IOException;

/**
 * Builds the renderers for playing DASH videos. This also includes using the YouTube locator URL
 * when playing back videos when the provider is YouTube.
 */
public final class DASHAsyncRendererBuilder
    implements AsyncRendererBuilder,
        ManifestFetcher.ManifestCallback<MediaPresentationDescription>,
        UtcTimingElementResolver.UtcTimingCallback {
  private static final String TAG = "AsyncRendererBuilder";

  private static final int BUFFER_SEGMENT_SIZE = 64 * 1024;
  private static final int VIDEO_BUFFER_SEGMENTS = 200;
  private static final int LIVE_EDGE_LATENCY_MS = 30000;
  private static final int AUDIO_BUFFER_SEGMENTS = 54;

  private static final int SECURITY_LEVEL_UNKNOWN = -1;
  private static final int SECURITY_LEVEL_1 = 1;
  private static final int SECURITY_LEVEL_3 = 3;

  private final Context context;
  private String userAgent;
  private String videoUrl;
  private String contentId;
  private String providerId;
  private boolean requiresSecurePlayback;
  private VideoExoPlayer player;
  private ManifestFetcher<MediaPresentationDescription> manifestFetcher;
  private UriDataSource manifestDataSource;
  private WidevineTestMediaDrmCallback drmCallback;
  private boolean canceled;
  private MediaPresentationDescription manifest;
  private long elapsedRealtimeOffset;
  private int targetResolution;

  /**
   * Construct the renderer builder. This is for DASH video, both secure and clear. It also handles
   * using the YouTube locator URL if the provider is 'YouTube'
   *
   * @param context - the context of the application
   * @param userAgent - the user agent use use with network requests.
   */
  public DASHAsyncRendererBuilder(
      Context context,
      String userAgent,
      String videoURL,
      String contentId,
      String providerId,
      boolean requiresSecurePlayback) {
    this.context = context;
    this.userAgent = userAgent;
    this.videoUrl = videoURL;
    this.contentId = contentId;
    this.providerId = providerId;
    this.requiresSecurePlayback = requiresSecurePlayback;
  }

  public void init(VideoPlayer videoPlayer, int targetResolution) {
    this.player = (VideoExoPlayer) videoPlayer;
    this.targetResolution = targetResolution;
    final MediaPresentationDescriptionParser parser = new MediaPresentationDescriptionParser();
    manifestDataSource = new DefaultUriDataSource(context, userAgent);

    // For DRM, use the widevine test server.  If you are using actual widevine DRM, this
    // class should be replaced with one that is correctly configured for your CDN environment.
    drmCallback = new WidevineTestMediaDrmCallback(contentId, providerId);

    // If the provider is YouTube, then use the locator URL to find the actual DASH URL.
    // otherwise, use what was passed in directly.
    if (providerId.equalsIgnoreCase("YouTube")) {
      final VideoExoPlayer thePlayer = player;
      YouTubeDashInfo info =
          new YouTubeDashInfo(contentId) {
            @Override
            protected void onPostExecute() {
              if (!isCanceled()) {
                manifestFetcher = new ManifestFetcher<>(getUrl(), manifestDataSource, parser);
                manifestFetcher.singleLoad(
                    thePlayer.getMainHandler().getLooper(), DASHAsyncRendererBuilder.this);
              }
            }
          };
      info.execute();
    } else {
      manifestFetcher = new ManifestFetcher<>(videoUrl, manifestDataSource, parser);
      Log.d(TAG, "starting manifest fetcher");
      manifestFetcher.singleLoad(player.getMainHandler().getLooper(), this);
    }
  }

  public void cancel() {
    canceled = true;
  }

  @Override
  public void onSingleManifest(MediaPresentationDescription manifest) {
    if (canceled) {
      return;
    }

    this.manifest = manifest;
    if (manifest.dynamic && manifest.utcTiming != null) {
      UtcTimingElementResolver.resolveTimingElement(
          manifestDataSource,
          manifest.utcTiming,
          manifestFetcher.getManifestLoadCompleteTimestamp(),
          this);
    } else {
      buildRenderers();
    }
  }

  @Override
  public void onSingleManifestError(IOException e) {
    if (canceled) {
      return;
    }

    player.onRenderersError(e);
  }

  @Override
  public void onTimestampResolved(UtcTimingElement utcTiming, long elapsedRealtimeOffset) {
    if (canceled) {
      return;
    }

    this.elapsedRealtimeOffset = elapsedRealtimeOffset;
    buildRenderers();
  }

  @Override
  public void onTimestampError(UtcTimingElement utcTiming, IOException e) {
    if (canceled) {
      return;
    }

    Log.e(TAG, "Failed to resolve UtcTiming element [" + utcTiming + "]", e);
    // Be optimistic and continue in the hope that the device clock is correct.
    buildRenderers();
  }

  private void buildRenderers() {
    Period period = manifest.getPeriod(0);
    Handler mainHandler = player.getMainHandler();
    LoadControl loadControl = new DefaultLoadControl(new DefaultAllocator(BUFFER_SEGMENT_SIZE));
    DefaultBandwidthMeter bandwidthMeter = new DefaultBandwidthMeter(mainHandler, null);
    int maxInitialBitrate = 0;
    int minInitialBitrate = Integer.MAX_VALUE;

    boolean hasContentProtection = false;
    for (int i = 0; i < period.adaptationSets.size(); i++) {
      AdaptationSet adaptationSet = period.adaptationSets.get(i);
      if (adaptationSet.type != AdaptationSet.TYPE_UNKNOWN) {
        hasContentProtection |= adaptationSet.hasContentProtection();
      }

      // Determine the bitrate to target based on the target resolution.  This is used to
      // initialize the FormatEvaluator.
      if (targetResolution > 0 && adaptationSet.type == AdaptationSet.TYPE_VIDEO) {
        for (Representation rep : adaptationSet.representations) {
          if (rep.format.height <= targetResolution && rep.format.bitrate > maxInitialBitrate) {
            maxInitialBitrate = rep.format.bitrate;
          } else if (rep.format.bitrate < minInitialBitrate) {
            minInitialBitrate = rep.format.bitrate;
          }
        }
      }
    }

    maxInitialBitrate /= FormatEvaluator.AdaptiveEvaluator.DEFAULT_BANDWIDTH_FRACTION;
    if (maxInitialBitrate == 0) {
      maxInitialBitrate =
          Math.min(
              minInitialBitrate, FormatEvaluator.AdaptiveEvaluator.DEFAULT_MAX_INITIAL_BITRATE);
    }

    // Check drm support if necessary.
    boolean filterHdContent = false;
    StreamingDrmSessionManager drmSessionManager = null;
    if (hasContentProtection) {
      if (Util.SDK_INT < 18) {
        player.onRenderersError(
            new UnsupportedDrmException(UnsupportedDrmException.REASON_UNSUPPORTED_SCHEME));
        return;
      }
      try {
        drmSessionManager =
            StreamingDrmSessionManager.newWidevineInstance(
                player.getPlaybackLooper(), drmCallback, null, player.getMainHandler(), null);

        if (!requiresSecurePlayback) {
          // Force to L3 to be able to direct to SurfaceTexture
          drmSessionManager.setPropertyString("securityLevel", "L3");
        }

        filterHdContent = getWidevineSecurityLevel(drmSessionManager) != SECURITY_LEVEL_1;
      } catch (UnsupportedDrmException e) {
        player.onRenderersError(e);
        return;
      }
    }

    FormatEvaluator.AdaptiveEvaluator evaluator =
        new FormatEvaluator.AdaptiveEvaluator(
            bandwidthMeter,
            maxInitialBitrate,
            FormatEvaluator.AdaptiveEvaluator.DEFAULT_MIN_DURATION_FOR_QUALITY_INCREASE_MS,
            FormatEvaluator.AdaptiveEvaluator.DEFAULT_MAX_DURATION_FOR_QUALITY_DECREASE_MS,
            FormatEvaluator.AdaptiveEvaluator.DEFAULT_MIN_DURATION_TO_RETAIN_AFTER_DISCARD_MS,
            FormatEvaluator.AdaptiveEvaluator.DEFAULT_BANDWIDTH_FRACTION);

    // Build the video renderer.
    DataSource videoDataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
    ChunkSource videoChunkSource =
        new DashChunkSource(
            manifestFetcher,
            DefaultDashTrackSelector.newVideoInstance(context, true, filterHdContent),
            videoDataSource,
            evaluator,
            LIVE_EDGE_LATENCY_MS,
            elapsedRealtimeOffset,
            mainHandler,
            player,
            0);
    ChunkSampleSource videoSampleSource =
        new ChunkSampleSource(
            videoChunkSource,
            loadControl,
            VIDEO_BUFFER_SEGMENTS * BUFFER_SEGMENT_SIZE,
            mainHandler,
            player,
            VideoExoPlayer.TYPE_VIDEO);
    TrackRenderer videoRenderer =
        new MediaCodecVideoTrackRenderer(
            context,
            videoSampleSource,
            MediaCodecSelector.DEFAULT,
            MediaCodec.VIDEO_SCALING_MODE_SCALE_TO_FIT,
            5000,
            drmSessionManager,
            true,
            mainHandler,
            player,
            50);

    // Build the audio renderer.
    DataSource audioDataSource = new DefaultUriDataSource(context, bandwidthMeter, userAgent);
    ChunkSource audioChunkSource =
        new DashChunkSource(
            manifestFetcher,
            DefaultDashTrackSelector.newAudioInstance(),
            audioDataSource,
            null,
            LIVE_EDGE_LATENCY_MS,
            elapsedRealtimeOffset,
            mainHandler,
            player,
            0);
    ChunkSampleSource audioSampleSource =
        new ChunkSampleSource(
            audioChunkSource,
            loadControl,
            AUDIO_BUFFER_SEGMENTS * BUFFER_SEGMENT_SIZE,
            mainHandler,
            player,
            VideoExoPlayer.TYPE_AUDIO);
    TrackRenderer audioRenderer =
        new MediaCodecAudioTrackRenderer(
            audioSampleSource,
            MediaCodecSelector.DEFAULT,
            drmSessionManager,
            true,
            mainHandler,
            player);

    // Invoke the callback.
    TrackRenderer[] renderers = new TrackRenderer[VideoExoPlayer.RENDERER_COUNT];
    renderers[VideoExoPlayer.TYPE_VIDEO] = videoRenderer;
    renderers[VideoExoPlayer.TYPE_AUDIO] = audioRenderer;
    player.onRenderers(renderers);
  }

  private static int getWidevineSecurityLevel(StreamingDrmSessionManager sessionManager) {
    String securityLevelProperty = sessionManager.getPropertyString("securityLevel");
    Log.d(TAG, "WV security: " + securityLevelProperty);
    return securityLevelProperty.equals("L1")
        ? SECURITY_LEVEL_1
        : securityLevelProperty.equals("L3") ? SECURITY_LEVEL_3 : SECURITY_LEVEL_UNKNOWN;
  }
}

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
import android.graphics.SurfaceTexture;
import android.media.AudioManager;
import android.media.MediaCodec;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.Surface;
import com.google.android.exoplayer.DummyTrackRenderer;
import com.google.android.exoplayer.ExoPlaybackException;
import com.google.android.exoplayer.ExoPlayer;
import com.google.android.exoplayer.MediaCodecAudioTrackRenderer;
import com.google.android.exoplayer.MediaCodecTrackRenderer;
import com.google.android.exoplayer.MediaCodecVideoTrackRenderer;
import com.google.android.exoplayer.MediaFormat;
import com.google.android.exoplayer.TimeRange;
import com.google.android.exoplayer.TrackRenderer;
import com.google.android.exoplayer.audio.AudioTrack;
import com.google.android.exoplayer.chunk.ChunkSampleSource;
import com.google.android.exoplayer.chunk.Format;
import com.google.android.exoplayer.dash.DashChunkSource;
import com.google.android.exoplayer.drm.StreamingDrmSessionManager;
import com.google.android.exoplayer.extractor.ExtractorSampleSource;
import com.google.android.exoplayer.hls.HlsSampleSource;
import com.google.android.exoplayer.metadata.MetadataTrackRenderer;
import com.google.android.exoplayer.metadata.id3.Id3Frame;
import com.google.android.exoplayer.text.Cue;
import com.google.android.exoplayer.text.TextRenderer;
import com.google.gvr.exoplayersupport.AsyncRendererBuilder;
import com.google.gvr.exoplayersupport.VideoPlayer;
import java.io.IOException;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.CountDownLatch;

/** Video player based on the ExoPlayer library. This player handles DASH and HLS videos. */
public class VideoExoPlayer
    implements VideoPlayer,
        DashChunkSource.EventListener,
        ChunkSampleSource.EventListener,
        MediaCodecVideoTrackRenderer.EventListener,
        MediaCodecAudioTrackRenderer.EventListener,
        StreamingDrmSessionManager.EventListener,
        HlsSampleSource.EventListener,
        TextRenderer,
        MetadataTrackRenderer.MetadataRenderer<List<Id3Frame>>,
        ExtractorSampleSource.EventListener {

  private static final String TAG = "VideoExoPlayer";
  private ExoPlayer player;
  private final Handler mainHandler;
  private final CopyOnWriteArrayList<VideoPlayer.Listener> listeners;
  private AsyncRendererBuilder currentAsyncBuilder;
  private AudioManager audioManager;
  private int mediaAudioVolume;

  private SurfaceTexture surfaceTexture;

  private boolean videoReadyFlag;
  private boolean paused;
  private TrackRenderer audioRenderer;
  private TrackRenderer videoRenderer;
  private int videoWidth;
  private int videoHeight;

  private Format audioFormat;
  private Format videoFormat;

  private static VideoLooperThread videoThread = null;

  /**
   * Creates a VideoExoPlayer.
   *
   * @param context The Application context.
   */
  public VideoExoPlayer(Context context) {
    audioManager = (AudioManager) context.getSystemService(Context.AUDIO_SERVICE);
    player = ExoPlayer.Factory.newInstance(RENDERER_COUNT, 1000, 5000);

    if (videoThread == null) {
      videoThread = new VideoLooperThread("VideoLooperThread");
      videoThread.start();
    }

    // need to wait for the thread to start before continuing.
    try {
      videoThread.latch.await();
    } catch (InterruptedException e) {
      Log.d(TAG, "VideoLooper thread ctor Interrupted!");
    }
    mainHandler = new Handler(videoThread.theLooper);
    listeners = new CopyOnWriteArrayList<>();

    // set the size to 1x1 to avoid div by zero
    videoHeight = 1;
    videoWidth = 0;

    mediaAudioVolume = 100;
  }

  /**
   * Adds a listener for crtical playback events.
   *
   * @param listener The Listener to add.
   */
  public void addListener(VideoPlayer.Listener listener) {
    listeners.add(listener);
  }

  /**
   * Removes a listener.
   *
   * @param listener The Listener to remove.
   */
  public void removeListener(VideoPlayer.Listener listener) {
    listeners.remove(listener);
  }

  @Override
  public boolean initialize(AsyncRendererBuilder rendererBuilder, final int targetResolution) {

    currentAsyncBuilder = rendererBuilder;

    Log.d(TAG, "initializing player rendererBuilder: " + rendererBuilder);
    mainHandler.post(
        new Runnable() {
          @Override
          public void run() {
            initPlayer(targetResolution);
          }
        });
    return true;
  }

  @Override
  public int getTrackCount(int rendererIndex) {
    int ret = player.getTrackCount(rendererIndex);

    if (ret == 0) {
      for (int i = 0; i < RENDERER_COUNT; i++) {
        Log.d(TAG, "gettrackcount: " + i + " == " + player.getTrackCount(i));
      }

      if (rendererIndex == TYPE_AUDIO && audioFormat != null) {
        ret = 1;
      }

      if (rendererIndex == TYPE_VIDEO && videoFormat != null) {
        ret = 1;
      }
    }
    return ret;
  }

  @Override
  public int getChannelCount(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.audioChannels;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.audioChannels;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.channelCount;
    }
    return 0;
  }

  @Override
  public int getSampleRate(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.audioSamplingRate;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.audioSamplingRate;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.sampleRate;
    }
    return 0;
  }

  @Override
  public int getBitRate(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.bitrate;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.bitrate;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.bitrate;
    }
    return 0;
  }

  @Override
  public float getFrameRate(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.frameRate;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.frameRate;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.sampleRate;
    }
    return 0;
  }

  @Override
  public int getTrackWidth(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.width;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.width;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.width;
    }
    return 0;
  }

  @Override
  public int getTrackHeight(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.height;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.height;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.height;
    }
    return 0;
  }

  @Override
  public int getStereoMode() {
    int trackCount = player.getTrackCount(TYPE_VIDEO);
    if (trackCount == 0) {
      return -1;
    }

    int selectedTrack = selectedTrack = player.getSelectedTrack(TYPE_VIDEO);
    MediaFormat format = player.getTrackFormat(TYPE_VIDEO, selectedTrack);
    return format.stereoMode;
  }

  @Override
  public byte[] getProjectionData() {
    int trackCount = player.getTrackCount(TYPE_VIDEO);
    if (trackCount == 0) {
      return null;
    }

    int selectedTrack = selectedTrack = player.getSelectedTrack(TYPE_VIDEO);
    MediaFormat format = player.getTrackFormat(TYPE_VIDEO, selectedTrack);
    return format.projectionData;
  }

  @Override
  public String getDisplayName(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.id;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.id;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.trackId;
    }
    return null;
  }

  @Override
  public String getLanguage(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.language;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.language;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.language;
    }
    return null;
  }

  @Override
  public String getMimeType(int rendererIndex, int track) {

    if (rendererIndex == TYPE_AUDIO && track == 0 && audioFormat != null) {
      return audioFormat.mimeType;
    } else if (rendererIndex == TYPE_VIDEO && track == 0 && videoFormat != null) {
      return videoFormat.mimeType;
    }
    if (player.getTrackCount(rendererIndex) > 0) {
      MediaFormat fmt = player.getTrackFormat(rendererIndex, track);
      return fmt.mimeType;
    }
    return null;
  }

  @Override
  public String getName(int rendererIndex, int track) {

    return getDisplayName(rendererIndex, track);
  }

  private void initPlayer(int targetResolution) {
    if (player != null && currentAsyncBuilder != null) {
      currentAsyncBuilder.init(this, targetResolution);
    } else {
      Log.d(TAG, "internal player or builder is null, skipping initPlayer");
    }
  }

  /** Releases the player. Call init() to initialize it again. */
  public void release() {
    if (videoThread != null) {
      videoThread.quit();
      videoThread = null;
    }
    releasePlayer();
  }

  /**
   * Sets the SurfaceTexture that will contain video frames. This also starts playback of the
   * player.
   *
   * @param surfaceTexture The SurfaceTexture that should receive video.
   */
  public void setSurfaceTexture(SurfaceTexture surfaceTexture) {
    this.surfaceTexture = surfaceTexture;
    sendVideoEvent(VideoPlayer.VIDEO_EVENT_SURFACE_SET);
    if (videoRenderer != null) {
      Log.d(TAG, "Surface texture set to " + surfaceTexture + "  posting videoReady!");
      mainHandler.post(
          new Runnable() {
            @Override
            public void run() {
              beginPlayback(true);
            }
          });
    } else {
      Log.d(TAG, "videoRenderer is null, so not starting playback");
    }
  }

  /** Pauses or restarts the player. */
  public void togglePause() {
    Log.d(TAG, "togglePause()");

    if (player != null) {
      if (paused) {
        player.setPlayWhenReady(true);
      } else {
        player.setPlayWhenReady(false);
      }
      paused = !paused;
    }
  }

  public boolean isPaused() {
    return paused;
  }

  public int playVideo() {
    if (player != null && paused) {
      togglePause();
    }
    return 0;
  }

  public int getPlaybackState() {
    return player.getPlaybackState();
  }

  public long getDuration() {
    return player.getDuration();
  }

  public long getBufferedPosition() {
    return player.getBufferedPosition();
  }

  public long getCurrentPosition() {
    return player.getCurrentPosition();
  }

  public void setCurrentPosition(long pos) {
    seek(pos);
  }

  public void seek(long pos) {
    player.seekTo(pos);
  }

  public int getBufferedPercentage() {
    return player.getBufferedPercentage();
  }

  /** Pauses playback. */
  public int pauseVideo() {
    if (player != null && !paused) {
      togglePause();
    }
    return 0;
  }

  private void beginPlayback(boolean paused) {

    player.sendMessage(
        videoRenderer, MediaCodecVideoTrackRenderer.MSG_SET_SURFACE, new Surface(surfaceTexture));
    player.seekTo(0);
    player.setPlayWhenReady(!paused);
    this.paused = paused;
    videoReadyFlag = true;
    sendVideoEvent(VideoPlayer.VIDEO_EVENT_READY);
  }

  public boolean isVideoReady() {
    return videoReadyFlag;
  }

  private void sendVideoEvent(int eventId) {
    Log.i(TAG, "VideoEvent " + eventId + " sent!");
    for (VideoPlayer.Listener listener : listeners) {
      listener.onVideoEvent(this, eventId);
    }
  }

  private void raiseException(Exception e) {
    Log.e(TAG, "raising exception to listeners", e);
    for (VideoPlayer.Listener listener : listeners) {
      listener.onError(this, e);
    }
  }

  /** Stops the player and releases it. */
  public void stop() {
    // Release the player instead of stopping so that an async prepare gets stopped.
    releasePlayer();
    audioManager.abandonAudioFocus(null);
  }

  @Override
  public int getCurrentVolume() {
    return mediaAudioVolume;
  }

  @Override
  public int getMaxVolume() {
    return 100;
  }

  @Override
  public void setCurrentVolume(int val) {
    mediaAudioVolume = val;
    if (this.audioRenderer != null) {
      player.sendMessage(this.audioRenderer, MediaCodecAudioTrackRenderer.MSG_SET_VOLUME, val/100.0f);
    }
  }

  private void releasePlayer() {
    if (currentAsyncBuilder != null) {
      currentAsyncBuilder.cancel();
      currentAsyncBuilder = null;
    }
    surfaceTexture = null;
    if (player != null) {
      paused = false;
      player.release();
      player = null;
    }
  }

  /**
   * Invoked with the results from a RendererBuilder.
   *
   * @param renderers Renderers indexed by {@link VideoExoPlayer} TYPE_* constants. An individual
   *     element may be null if there do not exist tracks of the corresponding type.
   */
  void onRenderers(TrackRenderer[] renderers) {
    Log.d(TAG, "renderers set!");
    for (int i = 0; i < RENDERER_COUNT; i++) {
      if (renderers[i] == null) {
        // Convert a null renderer to a dummy renderer.
        renderers[i] = new DummyTrackRenderer();
      }
    }
    // Complete preparation.
    this.videoRenderer =
        renderers[TYPE_VIDEO] != null ? renderers[TYPE_VIDEO] : new DummyTrackRenderer();
    this.audioRenderer =
        renderers[TYPE_AUDIO] != null ? renderers[TYPE_AUDIO] : new DummyTrackRenderer();

    if (player == null) {
      Log.w(TAG,"player is null in onRenderers - stopping initialization");
      return;
    }
    player.addListener(new VideoLooperListener());
    player.prepare(videoRenderer, audioRenderer);

    // Set current media volume on new audio renderer.
    setCurrentVolume(mediaAudioVolume);

    if (surfaceTexture != null) {
      beginPlayback(true);
    } else {
      Log.d(TAG, "Surface Texture not set yet, so not beginning playback");
    }
  }

  void onRenderersError(Exception e) {
    Log.e(TAG, "Renderer init error: ", e);
    raiseException(e);
  }

  Looper getPlaybackLooper() {
    return player.getPlaybackLooper();
  }

  Handler getMainHandler() {
    return mainHandler;
  }

  public int getWidth() {
    return videoWidth;
  }

  public int getHeight() {
    return videoHeight;
  }

  /** Invoked each time keys are loaded. */
  @Override
  public void onDrmKeysLoaded() {
    Log.d(TAG, "DRM keys loaded");
  }

  /**
   * Invoked when a drm error occurs.
   *
   * @param e The corresponding exception.
   */
  @Override
  public void onDrmSessionManagerError(Exception e) {

    Log.e(TAG, "DrmSessionManager error", e);
    raiseException(e);
  }

  /**
   * Invoked when the available seek range of the stream has changed.
   *
   * @param sourceId The id of the reporting {@link DashChunkSource}.
   * @param availableRange The range which specifies available content that can be seeked to.
   */
  @Override
  public void onAvailableRangeChanged(int sourceId, TimeRange availableRange) {
    Log.d(TAG, "onAvailableRangeChanged: " + sourceId);
  }

  /**
   * Invoked when an upstream load is started.
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param length The length of the data being loaded in bytes, or LENGTH_UNBOUNDED if the length
   *     of the data is not known in advance.
   * @param type The type of the data being loaded.
   * @param trigger The reason for the data being loaded.
   * @param format The particular format to which this data corresponds, or null if the data being
   *     loaded does not correspond to a format.
   * @param mediaStartTimeMs The media time of the start of the data being loaded, or -1 if this
   *     load is for initialization data.
   * @param mediaEndTimeMs The media time of the end of the data being loaded, or -1 if this
   */
  @Override
  public void onLoadStarted(
      int sourceId,
      long length,
      int type,
      int trigger,
      Format format,
      long mediaStartTimeMs,
      long mediaEndTimeMs) {

    Log.d(TAG, "onloadStarted");
  }

  /**
   * Invoked when the current load operation completes.
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param bytesLoaded The number of bytes that were loaded.
   * @param type The type of the loaded data.
   * @param trigger The reason for the data being loaded.
   * @param format The particular format to which this data corresponds, or null if the loaded data
   *     does not correspond to a format.
   * @param mediaStartTimeMs The media time of the start of the loaded data, or -1 if this load was
   *     for initialization data.
   * @param mediaEndTimeMs The media time of the end of the loaded data, or -1 if this load was for
   *     initialization data.
   * @param elapsedRealtimeMs {@code elapsedRealtime} timestamp of when the load finished.
   * @param loadDurationMs Amount of time taken to load the data.
   */
  @Override
  public void onLoadCompleted(
      int sourceId,
      long bytesLoaded,
      int type,
      int trigger,
      Format format,
      long mediaStartTimeMs,
      long mediaEndTimeMs,
      long elapsedRealtimeMs,
      long loadDurationMs) {
    Log.d(TAG, "onLoadCompleted");
  }

  /**
   * Invoked when the current upstream load operation is canceled.
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param bytesLoaded The number of bytes that were loaded prior to the cancellation.
   */
  @Override
  public void onLoadCanceled(int sourceId, long bytesLoaded) {
    Log.d(TAG, "onLoadCanceled");
  }

  /**
   * Invoked when an error occurs loading media data.
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param e The cause of the failure.
   */
  @Override
  public void onLoadError(int sourceId, IOException e) {
    Log.d(TAG, "onLoadError");
    raiseException(e);
  }

  /**
   * Invoked when data is removed from the back of the buffer, typically so that it can be
   * re-buffered using a different representation.
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param mediaStartTimeMs The media time of the start of the discarded data.
   * @param mediaEndTimeMs The media time of the end of the discarded data.
   */
  @Override
  public void onUpstreamDiscarded(int sourceId, long mediaStartTimeMs, long mediaEndTimeMs) {
    Log.d(TAG, "onUpstreamDiscarded");
  }

  /**
   * Invoked when the downstream format changes (i.e. when the format being supplied to the caller
   * of SampleSourceReader#readData changes).
   *
   * @param sourceId The id of the reporting SampleSource.
   * @param format The format.
   * @param trigger The trigger specified in the corresponding upstream load, as specified by the
   *     ChunkSource.
   * @param mediaTimeMs The media time at which the change occurred.
   */
  @Override
  public void onDownstreamFormatChanged(
      int sourceId, Format format, int trigger, long mediaTimeMs) {
    Log.d(
        TAG,
        "onDownstreamFormatChanged : "
            + sourceId
            + " trigger: "
            + trigger
            + " time: "
            + mediaTimeMs);
    if (sourceId == TYPE_AUDIO) {
      audioFormat = format;
    } else if (sourceId == TYPE_VIDEO) {
      videoFormat = format;
    }

    if (audioFormat != null && videoFormat != null) {
      sendVideoEvent(VideoPlayer.VIDEO_EVENT_FORMAT_CHANGED);
    }
  }

  /**
   * Invoked to report the number of frames dropped by the renderer. Dropped frames are reported
   * whenever the renderer is stopped having dropped frames, and optionally, whenever the count
   * reaches a specified threshold whilst the renderer is started.
   *
   * @param count The number of dropped frames.
   * @param elapsed The duration in milliseconds over which the frames were dropped. This duration
   *     is timed from when the renderer was started or from when dropped frames were last reported
   *     (whichever was more recent), and not from when the first of the reported
   */
  @Override
  public void onDroppedFrames(int count, long elapsed) {

    Log.d(TAG, "onDroppedFrames");
  }

  /**
   * Invoked each time there's a change in the size of the video being rendered.
   *
   * @param width The video width in pixels.
   * @param height The video height in pixels.
   * @param unappliedRotationDegrees For videos that require a rotation, this is the clockwise
   *     rotation in degrees that the application should apply for the video for it to be rendered
   *     in the correct orientation. This value will always be zero on API levels 21 and above,
   *     since the renderer will apply all necessary rotations internally. On earlier API levels
   *     this is not possible. Applications that use TextureView can apply the rotation by calling
   *     TextureView#setTransform. Applications that do not expect to encounter rotated videos can
   *     safely ignore this parameter.
   * @param pixelWidthHeightRatio The width to height ratio of each pixel. For the normal case of
   *     square pixels this will be equal to 1.0. Different values are indicative of anamorphic
   */
  @Override
  public void onVideoSizeChanged(
      int width, int height, int unappliedRotationDegrees, float pixelWidthHeightRatio) {
    Log.d(TAG, "onVideoSizeChanged");
    videoWidth = width;
    videoHeight = height;
    sendVideoEvent(VideoPlayer.VIDEO_EVENT_SIZE_CHANGED);
  }

  /**
   * Invoked when a frame is rendered to a surface for the first time following that surface having
   * been set as the target for the renderer.
   *
   * @param surface The surface to which a first frame has been rendered.
   */
  @Override
  public void onDrawnToSurface(Surface surface) {
    Log.d(TAG, "onDrawnToSurface");
  }

  /**
   * Invoked when a decoder fails to initialize.
   *
   * @param e The corresponding exception.
   */
  @Override
  public void onDecoderInitializationError(
      MediaCodecTrackRenderer.DecoderInitializationException e) {
    Log.d(TAG, "onDecoderInitializationError");
    raiseException(e);
  }

  /**
   * Invoked when a decoder operation raises a CryptoException.
   *
   * @param e The corresponding exception.
   */
  @Override
  public void onCryptoError(MediaCodec.CryptoException e) {
    Log.d(TAG, "onCryptoError");
    raiseException(e);
  }

  /**
   * Invoked when a decoder is successfully created.
   *
   * @param decoderName The decoder that was configured and created.
   * @param elapsedRealtimeMs {@code elapsedRealtime} timestamp of when the initialization finished.
   * @param initializationDurationMs Amount of time taken to initialize the decoder.
   */
  @Override
  public void onDecoderInitialized(
      String decoderName, long elapsedRealtimeMs, long initializationDurationMs) {
    Log.d(TAG, "onDecoderInitialized");
  }

  /**
   * Invoked when an {@link AudioTrack} fails to initialize.
   *
   * @param e The corresponding exception.
   */
  @Override
  public void onAudioTrackInitializationError(AudioTrack.InitializationException e) {
    Log.d(TAG, "onAudioTrackInitializationError");
    raiseException(e);
  }

  /**
   * Invoked when an {@link AudioTrack} write fails.
   *
   * @param e The corresponding exception.
   */
  @Override
  public void onAudioTrackWriteError(AudioTrack.WriteException e) {
    Log.d(TAG, "onAudioTrackWriteError");
    raiseException(e);
  }

  /**
   * Invoked when an {@link AudioTrack} underrun occurs.
   *
   * @param bufferSize The size of the {@link AudioTrack}'s buffer, in bytes.
   * @param bufferSizeMs The size of the {@link AudioTrack}'s buffer, in milliseconds, if it is
   *     configured for PCM output. -1 if it is configured for passthrough output, as the buffered
   *     media can have a variable bitrate so the duration may be unknown.
   * @param elapsedSinceLastFeedMs The time since the {@link AudioTrack} was last fed data.
   */
  @Override
  public void onAudioTrackUnderrun(int bufferSize, long bufferSizeMs, long elapsedSinceLastFeedMs) {
    Log.d(TAG, "onAudioTrackUnderrun");
  }

  /**
   * Invoked each time there is a metadata associated with current playback time.
   *
   * @param metadata The metadata to process.
   */
  @Override
  public void onMetadata(List<Id3Frame> metadata) {
    Log.d(TAG, "onMetadata: " + metadata);
  }

  /**
   * Invoked each time there is a change in the {@link Cue}s to be rendered.
   *
   * @param cues The {@link Cue}s to be rendered, or an empty list if no cues are to be rendered.
   */
  @Override
  public void onCues(List<Cue> cues) {
    Log.d(TAG, "onCues: " + cues);
  }

  /** Listens for player state changes. */
  private final class VideoLooperListener implements ExoPlayer.Listener {

    @Override
    public void onPlayerStateChanged(boolean playWhenReady, int playbackState) {
      Log.i(TAG, "ExoPlayer state changed " + playWhenReady + " : " + playbackState);
    }

    @Override
    public void onPlayWhenReadyCommitted() {
      Log.d(TAG, "playWhenReadyCommitted");
    }

    @Override
    public void onPlayerError(ExoPlaybackException error) {
      Log.e(TAG, "ExoPlayer error", error);
      raiseException(error);
    }
  }

  /**
   * Private looper thread if need when playing back video. Depending on the playback application
   * the "main" thread may not be a looper thread.
   */
  private class VideoLooperThread extends Thread {
    private Looper theLooper;

    private CountDownLatch latch;

    VideoLooperThread(String name) {
      super(name);
      latch = new CountDownLatch(1);
    }

    /**
     * Calls the <code>run()</code> method of the Runnable object the receiver holds. If no Runnable
     * is set, does nothing.
     *
     * @see Thread#start
     */
    @Override
    public void run() {
      Looper.prepare();
      theLooper = Looper.myLooper();
      latch.countDown();
      Looper.loop();
    }

    /** Quits the looper. */
    void quit() {
      this.interrupt();
      theLooper.quit();
    }
  }
}

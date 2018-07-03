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

import android.graphics.SurfaceTexture;

/**
 * Interface defining a video player for use by GVR. This interface is used to loosely couple the
 * GVR SDK and plugins with Android video players, such as ExoPlayer. The intent is to allow
 * customization and extension of the video player without being restricted by the GVR SDK
 * implementation.
 */
public interface VideoPlayer {

  public static final int TYPE_VIDEO = 0;
  public static final int TYPE_AUDIO = 1;
  public static final int TYPE_TEXT = 2;
  public static final int TYPE_METADATA = 3;

  public static final int RENDERER_COUNT = 4;

  public static final int VIDEO_EVENT_READY = 1;
  public static final int VIDEO_EVENT_START_PLAYBACK = 2;
  public static final int VIDEO_EVENT_FORMAT_CHANGED = 3;
  public static final int VIDEO_EVENT_SURFACE_SET = 4;
  public static final int VIDEO_EVENT_SIZE_CHANGED = 5;

  /**
   * Initializes the video player for playback. This is called when playback should start. The
   * target resolution is the height of the video that should be attempted to be played first. The
   * resolution ultimately selected will be the highest available resolution that is equal or less
   * than the target resolution
   *
   * @param rendererBuilder - builder to use to create the renderer components for the player.
   * @param targetResolution - the target height dimension (e.g. 720, 1080)
   * @return - true if the player was initialized successfully.
   */
  boolean initialize(AsyncRendererBuilder rendererBuilder, int targetResolution);

  /**
   * Sets the surface texture the player should use to render the video. This texture is used by the
   * GVR SDK to display the video correctly in VR.
   *
   * @param videoSurface - the surface instance.
   */
  void setSurfaceTexture(SurfaceTexture videoSurface);

  /**
   * Starts playing the video. If already playing, there should be no change to the state.
   *
   * @return non-negative value for success, negative return value indicate error.
   */
  int playVideo();

  /**
   * Pauses the video. If already paused, there should be no change to the state.
   *
   * @return non-negative value for success, negative return value indicate error.
   */
  int pauseVideo();

  /** Return true if the video playback is paused. */
  boolean isPaused();

  /**
   * Return true if the video is ready to play, meaning the first frame of the video is received.
   */
  boolean isVideoReady();

  /**
   * Returns the state of the player. This state should be the same as what is defined in the
   * Exoplayer library:
   *
   * <ol>
   *   <li>Idle = 1,
   *   <li>Preparing = 2,
   *   <li>Buffering = 3,
   *   <li>Ready = 4,
   *   <li>Ended = 5
   * </ol>
   */
  int getPlaybackState();

  /**
   * Returns the length of the video in milliseconds.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  long getDuration();

  /**
   * Returns the position in the video stream that buffered in milliseconds.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  long getBufferedPosition();

  /**
   * Returns the current playback position in the video stream in milliseconds.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  long getCurrentPosition();

  /**
   * Sets the current playback position in the video stream.
   *
   * @param pos - the position in milliseconds in the video stream.
   */
  void setCurrentPosition(long pos);

  /**
   * Returns the percentage of the video stream that is currently buffered locally.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  int getBufferedPercentage();

  /**
   * Returns the encoded width of the video.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  int getWidth();

  /**
   * Returns the encoded height of the video.
   *
   * @return less then 0 if there is no video loaded or of there is an error.
   */
  int getHeight();

  /**
   * Returns the maximum volume level that can be set
   *
   * @return less then 0 if there is an error.
   */
  int getMaxVolume();

  /**
   * Returns the current volume level. This value is between 0 and getMaxVolume()
   *
   * @return less then 0 if there is an error.
   */
  int getCurrentVolume();

  /**
   * Sets the current volume to the provided value. This value should be between 0 and
   * getMaxVolume()
   *
   * @param value - the new volume level.
   */
  void setCurrentVolume(int value);

  /**
   * Adds a listener to the player for receiving events from the player.
   *
   * @param listener - the instance of the listener to add.
   */
  void addListener(VideoPlayer.Listener listener);

  /**
   * Removes a listener from the player.
   *
   * @param listener - the instance of the listener to remove.
   */
  void removeListener(VideoPlayer.Listener listener);

  int getTrackCount(int rendererIndex);

  int getChannelCount(int rendererIndex, int track);

  int getSampleRate(int rendererIndex, int track);

  String getDisplayName(int rendererIndex, int track);

  String getLanguage(int rendererIndex, int track);

  String getMimeType(int rendererIndex, int track);

  String getName(int rendererIndex, int track);

  int getBitRate(int rendererIndex, int track);

  float getFrameRate(int rendererIndex, int track);

  int getTrackWidth(int rendererIndex, int track);

  int getTrackHeight(int rendererIndex, int track);

  int getStereoMode();

  byte[] getProjectionData();

  /**
   * The listener interface for the video player. Implementations of this interface can be added to
   * the videoPlayer instance by calling #addListener()
   */
  interface Listener {
    /**
     * Called when there is an exception caught by the player. Exceptions that are raised here are
     * logged by default, but no other action is taken. This means the state of the player may be
     * inconsistent and it is the responsibility of the caller to determine the best course of
     * action.
     *
     * @param e - the exception encountered.
     */
    void onError(VideoPlayer player, Exception e);

    /** Called when the video player is ready to show the first frame. */
    void onVideoEvent(VideoPlayer player, int eventId);
  }
}

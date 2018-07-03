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

import android.graphics.SurfaceTexture;
import android.opengl.GLES20;
import android.os.Build.VERSION;
import android.os.Build.VERSION_CODES;
import android.os.Handler;
import android.util.Log;

/** GL texture that holds a video frame. */
public class VideoTexture implements SurfaceTexture.OnFrameAvailableListener {

  private static final String TAG = "VideoTexture";

  private SurfaceTexture surfaceTexture;
  private boolean surfaceNeedsUpdate = false;
  private boolean haveFirstFrame = false;

  private final float[] videoSTMatrix = new float[16];
  private int[] textureIds;
  private int videoTextureId;
  private long videoTimestampNs = -1;

  /** Creates a VideoTexture. Must be called from the opengl thread. */
  public VideoTexture(VideoExoPlayer player) {
    // Create the external texture used for video playback.
    textureIds = new int[1];
    GLES20.glGenTextures(1, textureIds, 0);

    createSurfaceTexture(textureIds[0], player.getMainHandler());
  }

  private void createSurfaceTexture(int videoTextureId, Handler handler) {
    this.videoTextureId = videoTextureId;
    GLES20.glBindTexture(TextureHandle.GL_TEXTURE_EXTERNAL_OES, videoTextureId);
    GLUtil.checkGlError(TAG, "glBindTexture videoTextureId");

    GLES20.glTexParameterf(
        TextureHandle.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
    GLES20.glTexParameterf(
        TextureHandle.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
    GLES20.glTexParameterf(
        TextureHandle.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
    GLES20.glTexParameterf(
        TextureHandle.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);

    surfaceTexture = new SurfaceTexture(videoTextureId);
    if (VERSION.SDK_INT >= VERSION_CODES.LOLLIPOP && handler != null) {
      surfaceTexture.setOnFrameAvailableListener(this, handler);
    } else {
      surfaceTexture.setOnFrameAvailableListener(this);
    }

    Log.i(TAG, "Video Texture created! " + videoTextureId);
  }

  /**
   * Gets the SurfaceTexture that receives video frames.
   *
   * @return The SurfaceTexture that receives video frames.
   */
  public SurfaceTexture getSurfaceTexture() {
    return surfaceTexture;
  }

  /** Releases the video texture, deleting it's GL ID. */
  public void release() {
    if (surfaceTexture != null) {
      surfaceTexture.release();
    }
    if (textureIds != null) {
      GLES20.glDeleteTextures(textureIds.length, textureIds, 0);
      textureIds[0] = 0;
    }
  }

  /**
   * Gets whether playback has started.
   *
   * @return Whether playback has started.
   */
  public synchronized boolean isPlaybackStarted() {
    return haveFirstFrame;
  }

  /** Resets playback state. */
  public synchronized void prepareForNewMovie() {
    haveFirstFrame = false;
  }

  /**
   * Retrieves the latest video frame and stores it in a SurfaceTexture.
   *
   * @return whether the SurfaceTexture was updated.
   */
  public synchronized boolean updateTexture() {
    if (surfaceNeedsUpdate) {
      surfaceTexture.updateTexImage();
      surfaceTexture.getTransformMatrix(videoSTMatrix);
      videoTimestampNs = surfaceTexture.getTimestamp();
      surfaceNeedsUpdate = false;
      return true;
    }
    return false;
  }

  /** Returns the texture ID for the video texture. */
  public int getVideoTextureId() {
    return videoTextureId;
  }

  public int getVideoTextureWidth() { return 0; }

  /** Returns the transformation matrix for the video. */
  public float[] getVideoMatrix() {
    return videoSTMatrix;
  }

  public long getVideoTimestampNs() {
    return videoTimestampNs;
  }

  /**
   * Gets a TextureHandle wrapping the video frame.
   *
   * @return A TextureHandle wrapping the video frame with the current transform matrix.
   */
  public synchronized TextureHandle getTextureHandle() {
    return new TextureHandle(
        videoTextureId, TextureHandle.TextureType.TEXTURE_EXTERNAL, videoSTMatrix);
  }

  /**
   * Gets a TextureHandle wrapping the video frame.
   *
   * @return A TextureHandle wrapping the video frame with the no transformation.
   */
  public synchronized TextureHandle getTextureHandleWithIdentity() {
    return new TextureHandle(videoTextureId, TextureHandle.TextureType.TEXTURE_EXTERNAL);
  }

  @Override
  public synchronized void onFrameAvailable(SurfaceTexture surface) {
    surfaceNeedsUpdate = true;
    haveFirstFrame = true;
  }
}

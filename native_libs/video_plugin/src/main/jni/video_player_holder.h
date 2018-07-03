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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_HOLDER_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_HOLDER_H_

#include <jni.h>

#include "video_externs.h"

namespace gvrvideo {

// Class used to wrap a Java based video player.
class VideoPlayerHolder {
 public:
  // Constructs a new VideoPlayerHolder using the java object instance of the
  // player and the player type, which is used to get the factory when needed.
  VideoPlayerHolder(jobject playerObj, int type);

  virtual ~VideoPlayerHolder();

  // Initializes the class loading the JNI method Ids.
  static void Initialize();

  // Returns the underlying java object.  This is intended to allow the caller
  // to access other methods not defined in the VideoPlayer interface.
  jobject GetRawObject() const;

  // Sets the surface texture object that is used by the video player to render
  // the video.
  void SetSurfaceTexture(jobject texture_obj);

  bool Initialize(jobject renderer_builder_obj, int target_resolution);

  // Add and remove the listener for events and exceptions.
  void AddListener(jobject jListener);
  void RemoveListener(jobject jListener);

  int PlayVideo() const;

  int PauseVideo() const;

  bool IsVideoReady() const;

  bool IsVideoPaused() const;

  int GetPlaybackState() const;

  long long GetDuration() const;

  long long GetBufferedPosition() const;

  long long GetCurrentPosition() const;

  void SetCurrentPosition(long long pos) const;

  int GetBufferedPercentage() const;

  int GetWidth() const;

  int GetHeight() const;

  int GetMaxVolume() const;

  int GetCurrentVolume() const;

  void SetCurrentVolume(int value) const;

  int GetType();

  int GetTrackCount(int rendererIndex) const;

  ExoTrackInfo *GetTrackInfo(int rendererIndex) const;

  int GetStereoMode() const;

  bool HasProjectionData() const;

  void ReleaseTrackInfo(ExoTrackInfo *info, int ct) const;

 private:
  jobject playerObj;
  int type;

  static jmethodID addListenerMethodID;
  static jmethodID removeListenerMethodID;
  static jmethodID isVideoReadyMethodID;
  static jmethodID isPausedMethodID;
  static jmethodID initializeMethodID;
  static jmethodID getPlaybackStateMethodID;
  static jmethodID getDurationMethodID;
  static jmethodID getBufferedPositionMethodID;
  static jmethodID getCurrentPositionMethodID;
  static jmethodID setCurrentPositionMethodID;
  static jmethodID getBufferedPercentageMethodID;
  static jmethodID playVideoMethodID;
  static jmethodID pauseVideoMethodID;
  static jmethodID getWidthMethodID;
  static jmethodID getHeightMethodID;
  static jmethodID setSurfaceTextureMethodID;
  static jmethodID getMaxVolumeMethodID;
  static jmethodID getCurrentVolumeMethodID;
  static jmethodID setCurrentVolumeMethodID;
  static jmethodID getTrackCountMethodID;
  static jmethodID getChannelCountMethodID;
  static jmethodID getSampleRateMethodID;
  static jmethodID getDisplayNameMethodID;
  static jmethodID getLanguageMethodID;
  static jmethodID getMimeTypeMethodID;
  static jmethodID getNameMethodID;
  static jmethodID getBitRateMethodID;
  static jmethodID getFrameRateMethodID;
  static jmethodID getTrackWidthMethodID;
  static jmethodID getTrackHeightMethodID;
  static jmethodID getStereoModeMethodID;
  static jmethodID getProjectionDataMethodID;
};
}  // namespace gvrvideo
#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_HOLDER_H_

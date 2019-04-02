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

#include "video_player_holder.h"
#include "jni_helper.h"
#include "logger.h"
#include "vecmath.h"

namespace gvrvideo {

// Static member storage for the JNI method Ids.
jmethodID VideoPlayerHolder::addListenerMethodID;
jmethodID VideoPlayerHolder::removeListenerMethodID;
jmethodID VideoPlayerHolder::isVideoReadyMethodID;
jmethodID VideoPlayerHolder::isPausedMethodID;
jmethodID VideoPlayerHolder::initializeMethodID;
jmethodID VideoPlayerHolder::getPlaybackStateMethodID;
jmethodID VideoPlayerHolder::getDurationMethodID;
jmethodID VideoPlayerHolder::getBufferedPositionMethodID;
jmethodID VideoPlayerHolder::getCurrentPositionMethodID;
jmethodID VideoPlayerHolder::setCurrentPositionMethodID;
jmethodID VideoPlayerHolder::getBufferedPercentageMethodID;
jmethodID VideoPlayerHolder::playVideoMethodID;
jmethodID VideoPlayerHolder::pauseVideoMethodID;
jmethodID VideoPlayerHolder::getWidthMethodID;
jmethodID VideoPlayerHolder::getHeightMethodID;
jmethodID VideoPlayerHolder::setSurfaceTextureMethodID;
jmethodID VideoPlayerHolder::getMaxVolumeMethodID;
jmethodID VideoPlayerHolder::getCurrentVolumeMethodID;
jmethodID VideoPlayerHolder::setCurrentVolumeMethodID;
jmethodID VideoPlayerHolder::getTrackCountMethodID;
jmethodID VideoPlayerHolder::getChannelCountMethodID;
jmethodID VideoPlayerHolder::getSampleRateMethodID;
jmethodID VideoPlayerHolder::getDisplayNameMethodID;
jmethodID VideoPlayerHolder::getLanguageMethodID;
jmethodID VideoPlayerHolder::getMimeTypeMethodID;
jmethodID VideoPlayerHolder::getNameMethodID;
jmethodID VideoPlayerHolder::getBitRateMethodID;
jmethodID VideoPlayerHolder::getFrameRateMethodID;
jmethodID VideoPlayerHolder::getTrackWidthMethodID;
jmethodID VideoPlayerHolder::getTrackHeightMethodID;
jmethodID VideoPlayerHolder::getStereoModeMethodID;
jmethodID VideoPlayerHolder::getProjectionDataMethodID;

// Initialize the JNI values.
void VideoPlayerHolder::Initialize() {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  jclass clz =
      JNIHelper::Get().FindClass("com/google/gvr/exoplayersupport/VideoPlayer");
  assert(clz);

  isVideoReadyMethodID = jni_env->GetMethodID(clz, "isVideoReady", "()Z");
  isPausedMethodID = jni_env->GetMethodID(clz, "isPaused", "()Z");

  initializeMethodID = jni_env->GetMethodID(
      clz, "initialize",
      "("
      "Lcom/google/gvr/exoplayersupport/AsyncRendererBuilder;I"
      ")Z");

  addListenerMethodID = jni_env->GetMethodID(
      clz, "addListener",
      "("
      "Lcom/google/gvr/exoplayersupport/VideoPlayer$Listener;"
      ")V");

  removeListenerMethodID = jni_env->GetMethodID(
      clz, "removeListener",
      "("
      "Lcom/google/gvr/exoplayersupport/VideoPlayer$Listener;"
      ")V");
  getPlaybackStateMethodID =
      jni_env->GetMethodID(clz, "getPlaybackState", "()I");
  getDurationMethodID = jni_env->GetMethodID(clz, "getDuration", "()J");
  getBufferedPositionMethodID =
      jni_env->GetMethodID(clz, "getBufferedPosition", "()J");
  getCurrentPositionMethodID =
      jni_env->GetMethodID(clz, "getCurrentPosition", "()J");
  setCurrentPositionMethodID =
      jni_env->GetMethodID(clz, "setCurrentPosition", "(J)V");
  getBufferedPercentageMethodID =
      jni_env->GetMethodID(clz, "getBufferedPercentage", "()I");
  playVideoMethodID = jni_env->GetMethodID(clz, "playVideo", "()I");
  pauseVideoMethodID = jni_env->GetMethodID(clz, "pauseVideo", "()I");
  getWidthMethodID = jni_env->GetMethodID(clz, "getWidth", "()I");
  getHeightMethodID = jni_env->GetMethodID(clz, "getHeight", "()I");
  setSurfaceTextureMethodID = jni_env->GetMethodID(
      clz, "setSurfaceTexture", "(Landroid/graphics/SurfaceTexture;)V");
  getMaxVolumeMethodID = jni_env->GetMethodID(clz, "getMaxVolume", "()I");
  getCurrentVolumeMethodID =
      jni_env->GetMethodID(clz, "getCurrentVolume", "()I");
  setCurrentVolumeMethodID =
      jni_env->GetMethodID(clz, "setCurrentVolume", "(I)V");

  getTrackCountMethodID = jni_env->GetMethodID(clz, "getTrackCount", "(I)I");
  getChannelCountMethodID =
      jni_env->GetMethodID(clz, "getChannelCount", "(II)I");
  getSampleRateMethodID = jni_env->GetMethodID(clz, "getSampleRate", "(II)I");
  getDisplayNameMethodID =
      jni_env->GetMethodID(clz, "getDisplayName", "(II)Ljava/lang/String;");
  getLanguageMethodID =
      jni_env->GetMethodID(clz, "getLanguage", "(II)Ljava/lang/String;");
  getMimeTypeMethodID =
      jni_env->GetMethodID(clz, "getMimeType", "(II)Ljava/lang/String;");
  getNameMethodID =
      jni_env->GetMethodID(clz, "getName", "(II)Ljava/lang/String;");

  getBitRateMethodID = jni_env->GetMethodID(clz, "getBitRate", "(II)I");
  getFrameRateMethodID = jni_env->GetMethodID(clz, "getFrameRate", "(II)F");
  getTrackWidthMethodID = jni_env->GetMethodID(clz, "getTrackWidth", "(II)I");
  getTrackHeightMethodID = jni_env->GetMethodID(clz, "getTrackHeight", "(II)I");
  getStereoModeMethodID = jni_env->GetMethodID(clz, "getStereoMode", "()I");
  getProjectionDataMethodID = jni_env->GetMethodID(clz, "getProjectionData", "()[B");

  assert(addListenerMethodID);
  assert(removeListenerMethodID);
  assert(isVideoReadyMethodID);
  assert(isPausedMethodID);
  assert(initializeMethodID);
  assert(getPlaybackStateMethodID);
  assert(getDurationMethodID);
  assert(getBufferedPositionMethodID);
  assert(getCurrentPositionMethodID);
  assert(setCurrentPositionMethodID);
  assert(getBufferedPercentageMethodID);
  assert(playVideoMethodID);
  assert(pauseVideoMethodID);
  assert(getWidthMethodID);
  assert(getHeightMethodID);
  assert(setSurfaceTextureMethodID);
  assert(getMaxVolumeMethodID);
  assert(getCurrentVolumeMethodID);
  assert(setCurrentVolumeMethodID);
  assert(getTrackCountMethodID);
  assert(getChannelCountMethodID);
  assert(getSampleRateMethodID);
  assert(getDisplayNameMethodID);
  assert(getLanguageMethodID);
  assert(getMimeTypeMethodID);
  assert(getNameMethodID);
  assert(getBitRateMethodID);
  assert(getFrameRateMethodID);
  assert(getTrackWidthMethodID);
  assert(getTrackHeightMethodID);
  assert(getStereoModeMethodID);
  assert(getProjectionDataMethodID);

  // done with the local refs to the  class and instance
  jni_env->DeleteLocalRef(clz);
}

// constructor, always have an underlying Java object for the player.
VideoPlayerHolder::VideoPlayerHolder(jobject playerObj, int type) {
  this->playerObj = playerObj;
  this->type = type;
}

VideoPlayerHolder::~VideoPlayerHolder() {
  JNIEnv *jni_env = JNIHelper::Get().Env();
  if (playerObj) {
    jni_env->DeleteGlobalRef(playerObj);
  }
}

jobject VideoPlayerHolder::GetRawObject() const { return playerObj; }

void VideoPlayerHolder::AddListener(jobject jListener) {
  JNIHelper::Get().CallVoidMethod(playerObj, addListenerMethodID, jListener);
}

void VideoPlayerHolder::RemoveListener(jobject jListener) {
  JNIHelper::Get().CallVoidMethod(playerObj, removeListenerMethodID, jListener);
}

bool VideoPlayerHolder::Initialize(jobject renderer_builder_obj,
                                   int target_resolution) {
  return JNIHelper::Get().CallBooleanMethod(
      playerObj, initializeMethodID, renderer_builder_obj, target_resolution);
}

void VideoPlayerHolder::SetSurfaceTexture(jobject texture_obj) {
  JNIHelper::Get().CallVoidMethod(playerObj, setSurfaceTextureMethodID,
                                  texture_obj);
}

int VideoPlayerHolder::PlayVideo() const {
  return JNIHelper::Get().CallIntMethod(playerObj, playVideoMethodID);
}

int VideoPlayerHolder::PauseVideo() const {
  return JNIHelper::Get().CallIntMethod(playerObj, pauseVideoMethodID);
}

bool VideoPlayerHolder::IsVideoReady() const {
  if (!playerObj) {
    LOGI("videoplayerholder:", "Not Ready vm or player is null!");
    return false;
  }
  return JNIHelper::Get().CallBooleanMethod(playerObj, isVideoReadyMethodID);
}

bool VideoPlayerHolder::IsVideoPaused() const {
  if (!playerObj) {
    LOGI("videoplayerholder:", "Not Ready vm or player is null!");
    return false;
  }
  return JNIHelper::Get().CallBooleanMethod(playerObj, isPausedMethodID);
}

int VideoPlayerHolder::GetPlaybackState() const {
  return JNIHelper::Get().CallIntMethod(playerObj, getPlaybackStateMethodID);
}

long long VideoPlayerHolder::GetDuration() const {
  return JNIHelper::Get().CallLongMethod(playerObj, getDurationMethodID);
}

long long VideoPlayerHolder::GetBufferedPosition() const {
  return JNIHelper::Get().CallLongMethod(playerObj,
                                         getBufferedPositionMethodID);
}

long long VideoPlayerHolder::GetCurrentPosition() const {
  return JNIHelper::Get().CallLongMethod(playerObj, getCurrentPositionMethodID);
}

void VideoPlayerHolder::SetCurrentPosition(long long pos) const {
  LOGD("videoplayerholder:", "Setting Current position to %lld", pos);
  JNIHelper::Get().CallVoidMethod(playerObj, setCurrentPositionMethodID, pos);
}

int VideoPlayerHolder::GetBufferedPercentage() const {
  return JNIHelper::Get().CallIntMethod(playerObj,
                                        getBufferedPercentageMethodID);
}

int VideoPlayerHolder::GetWidth() const {
  if (playerObj) {
    return JNIHelper::Get().CallIntMethod(playerObj, getWidthMethodID);
  }
  return -1;
}

int VideoPlayerHolder::GetHeight() const {
  if (playerObj) {
    return JNIHelper::Get().CallIntMethod(playerObj, getHeightMethodID);
  }
  return -1;
}

int VideoPlayerHolder::GetMaxVolume() const {
  if (playerObj) {
    return JNIHelper::Get().CallIntMethod(playerObj, getMaxVolumeMethodID);
  }
  return -1;
}

int VideoPlayerHolder::GetCurrentVolume() const {
  if (playerObj) {
    return JNIHelper::Get().CallIntMethod(playerObj, getCurrentVolumeMethodID);
  }
  return -1;
}

void VideoPlayerHolder::SetCurrentVolume(int value) const {
  if (playerObj) {
    JNIHelper::Get().CallVoidMethod(playerObj, setCurrentVolumeMethodID, value);
  }
}

int VideoPlayerHolder::GetType() { return type; }

int VideoPlayerHolder::GetTrackCount(int rendererIndex) const {
  if (playerObj) {
    int ret = JNIHelper::Get().CallIntMethod(playerObj, getTrackCountMethodID,
                                             rendererIndex);
    LOGD("videoplayerholder:", "GetTrackCount %d returned %d", rendererIndex,
         ret);
    return ret;
  } else {
    LOGW("videoplayerholder:", "PlayerObject is null!!");
  }
  return 0;
}

ExoTrackInfo *VideoPlayerHolder::GetTrackInfo(int rendererIndex) const {
  if (playerObj) {
    int ct = GetTrackCount(rendererIndex);
    if (ct > 0) {
      ExoTrackInfo *info = new ExoTrackInfo[ct];
      for (int i = 0; i < ct; i++) {
        info[i].Channels = JNIHelper::Get().CallIntMethod(
            playerObj, getChannelCountMethodID, rendererIndex, i);
        info[i].Index = i;
        info[i].SampleRate = JNIHelper::Get().CallIntMethod(
            playerObj, getSampleRateMethodID, rendererIndex, i);
        info[i].DisplayName = JNIHelper::Get().CallStringMethod(
            playerObj, getDisplayNameMethodID, rendererIndex, i);
        info[i].Language = JNIHelper::Get().CallStringMethod(
            playerObj, getLanguageMethodID, rendererIndex, i);
        info[i].MimeType = JNIHelper::Get().CallStringMethod(
            playerObj, getMimeTypeMethodID, rendererIndex, i);
        info[i].Name = JNIHelper::Get().CallStringMethod(
            playerObj, getNameMethodID, rendererIndex, i);
        info[i].BitRate = JNIHelper::Get().CallIntMethod(
            playerObj, getBitRateMethodID, rendererIndex, i);
        info[i].FrameRate = JNIHelper::Get().CallFloatMethod(
            playerObj, getFrameRateMethodID, rendererIndex, i);
        info[i].Width = JNIHelper::Get().CallIntMethod(
            playerObj, getTrackWidthMethodID, rendererIndex, i);
        info[i].Height = JNIHelper::Get().CallIntMethod(
            playerObj, getTrackHeightMethodID, rendererIndex, i);
      }
      return info;
    }
  }
  return nullptr;
}

int VideoPlayerHolder::GetStereoMode() const {
  if (playerObj) {
    return JNIHelper::Get().CallIntMethod(playerObj, getStereoModeMethodID);
  }
  return -1;
}

bool VideoPlayerHolder::HasProjectionData() const {
  jbyte* projectionData = nullptr;
  int size = 0;
  if (playerObj) {
    projectionData = JNIHelper::Get().CallByteArrayMethod(playerObj, getProjectionDataMethodID, &size);
  }

  // TODO: Parse the projection data to support all valid projection types
  // Instead of just returning true and assuming it is spherical if there is any projection data.
  if (projectionData != nullptr) {
    delete[] projectionData;
    projectionData = nullptr;
  }

  if (size > 0) {
    return true;
  }

  return false;
}

void VideoPlayerHolder::ReleaseTrackInfo(ExoTrackInfo *info, int ct) const {
  if (info && ct) {
    for (int i = 0; i < ct; i++) {
      JNIHelper::Get().ReleaseString(info->Name);
      JNIHelper::Get().ReleaseString(info->Language);
      JNIHelper::Get().ReleaseString(info->DisplayName);
      JNIHelper::Get().ReleaseString(info->MimeType);
    }

    delete info;
  }
}
}  // namespace gvrvideo

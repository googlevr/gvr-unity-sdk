/* Copyright 2016 Google Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#include <string.h>
#include <map>

#include "jni_helper.h"
#include "logger.h"
#include "vecmath.h"
#include "video_player_impl.h"

namespace gvrvideo {

#define DEFAULT_SUPPORT_CLASSNAME \
  "com/google/gvr/exoplayersupport/DefaultVideoSupport"

// Map of instances used to correlate the event id offset to the player.
static std::map<int, VideoPlayerImpl *> g_instances;
static int g_instanceNumber = 1;

// Static class members used when calling JNI.
jobject VideoPlayerImpl::unity_player_activity;

// VideoTexture methodIds.
jmethodID VideoPlayerImpl::getSurfaceTextureMethodID;
jmethodID VideoPlayerImpl::getVideoTextureIdMethodID;
jmethodID VideoPlayerImpl::updateTextureMethodID;
jmethodID VideoPlayerImpl::getVideoMatrixMethodID;
jmethodID VideoPlayerImpl::getVideoTimestampNsMethodID;
jmethodID VideoPlayerImpl::releaseMethodID;
jclass VideoPlayerImpl::video_texture_class;
jclass VideoPlayerImpl::native_listener_class;

bool VideoPlayerImpl::SetSupportClassname(const char *clzname) {
  if (clzname) {
    LOGD("videoplayerimpl::", "Creating factory initializer from %s", clzname);
    pVideoFactoryHolder = VideoSupportImpl::Create(clzname);
    if (pVideoFactoryHolder) {
      InitPlayerActivity();
      pVideoFactoryHolder->Initialize(unity_player_activity);
    }
    return pVideoFactoryHolder != NULL;
  }
  return false;
}

const VideoSupportImpl *VideoPlayerImpl::GetVideoSupportImpl() {
  if (pVideoFactoryHolder) {
    return pVideoFactoryHolder;
  } else {
    SetSupportClassname(DEFAULT_SUPPORT_CLASSNAME);
    assert(pVideoFactoryHolder);
    return pVideoFactoryHolder;
  }
}

void VideoPlayerImpl::InitPlayerActivity() {
  // Look for Unity first, then Unreal.
  auto jni_env = JNIHelper::Get().Env();
  jclass uclz = JNIHelper::Get().FindClass("com/unity3d/player/UnityPlayer");
  if (uclz) {
    jfieldID unity_activity_id = jni_env->GetStaticFieldID(
        uclz, "currentActivity", "Landroid/app/Activity;");
    assert(unity_activity_id);
    jobject unity_activity =
        jni_env->GetStaticObjectField(uclz, unity_activity_id);
    assert(unity_activity);

    unity_player_activity = jni_env->NewGlobalRef(unity_activity);

    jni_env->DeleteLocalRef(uclz);
    jni_env->DeleteLocalRef(unity_activity);
  } else {
    if (jni_env->ExceptionCheck()) {
      jni_env->ExceptionClear();
    }
    uclz = JNIHelper::Get().FindClass("com/epicgames/ue4/GameActivity");
    assert("UE4class" && uclz);
    if (uclz) {
      jmethodID getter = jni_env->GetStaticMethodID(
          uclz, "Get", "()Lcom/epicgames/ue4/GameActivity;");
      assert(getter);
      jobject act = JNIHelper::Get().CallStaticObjectMethod(uclz, getter);
      if (act) {
        unity_player_activity = jni_env->NewGlobalRef(act);
      }
      jni_env->DeleteLocalRef(uclz);
      jni_env->DeleteLocalRef(act);
    }
  }
}

// Initialize the JNI environment for accessing java code.
void VideoPlayerImpl::Initialize() {
  auto jni_env = JNIHelper::Get().Env();

  jclass uclz = JNIHelper::Get().FindClass(
      "com/google/gvr/exoplayersupport/sample/VideoTexture");
  assert(uclz);
  {
    getSurfaceTextureMethodID = jni_env->GetMethodID(
        uclz, "getSurfaceTexture", "()Landroid/graphics/SurfaceTexture;");
    getVideoTextureIdMethodID =
        jni_env->GetMethodID(uclz, "getVideoTextureId", "()I");
    updateTextureMethodID = jni_env->GetMethodID(uclz, "updateTexture", "()Z");
    getVideoMatrixMethodID =
        jni_env->GetMethodID(uclz, "getVideoMatrix", "()[F");
    getVideoTimestampNsMethodID =
        jni_env->GetMethodID(uclz, "getVideoTimestampNs", "()J");
    releaseMethodID = jni_env->GetMethodID(uclz, "release", "()V");
    video_texture_class = (jclass)jni_env->NewGlobalRef(uclz);
    jni_env->DeleteLocalRef(uclz);
  }

  uclz = JNIHelper::Get().FindClass(
      "com/google/gvr/exoplayersupport/impl/NativeVideoCallbacks");
  assert(uclz);
  native_listener_class = (jclass)jni_env->NewGlobalRef(uclz);
  jni_env->DeleteLocalRef(uclz);
}

VideoPlayerImpl::VideoPlayerImpl() {
  video_player_obj = 0;
  video_texture_obj = 0;
  listener_obj = 0;
  pVideoFactoryHolder = 0;
  onevent_callback = NULL;
  onexception_callback = NULL;
  initial_resolution = 0;

  // Start out on both being 0.
  renderableTexture = 0;
  drawableTexture = 0;
  newFrameAvail = false;
  num_textures = 0;
  externalTexture = NULL;

  // event base is 100 * the instances.
  g_instanceNumber++;
  eventBase = g_instanceNumber * 100;
  g_instances[g_instanceNumber] = this;

  memcpy(videoMatrix, ndk_helper::Mat4::Identity().Ptr(), sizeof(float) * 16);

  LOGD("videoplayerimpl::", "Creating VideoPlayerImpl number %d", eventBase);
}

// Clean up the java objects referenced and the instance map.
VideoPlayerImpl::~VideoPlayerImpl() {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  const VideoSupportImpl *fac = GetVideoSupportImpl();

  if (externalTexture) {
    delete[] externalTexture;
  }

  if (video_player_obj) {
    if (fac) {
      fac->DestroyPlayer(video_player_obj);
    }
    delete video_player_obj;
    video_player_obj = NULL;
  }

  if (listener_obj) {
    jni_env->DeleteGlobalRef(listener_obj);
    listener_obj = 0;
  }

  if (video_texture_obj) {
    LOGD("videoplayerimpl::", "Deleting video texture");
    JNIHelper::Get().CallVoidMethod(video_texture_obj, releaseMethodID);
    jni_env->DeleteGlobalRef(video_texture_obj);
    video_texture_obj = 0;
  }

  if (pVideoFactoryHolder) {
    LOGD("videoplayerimpl::", "Deleting pVideoFactoryHolder");
    delete pVideoFactoryHolder;
    pVideoFactoryHolder = NULL;
  }
  g_instances.erase(eventBase / 100);
}

// Initialize the video player and create the playback stream.
void *VideoPlayerImpl::CreateVideoPlayer(int videoType, const char *videoURL,
                                         const char *contentId,
                                         const char *provider,
                                         bool useSecurePath, bool useExisting) {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  const VideoSupportImpl *fac = GetVideoSupportImpl();

  if (!fac) {
    LOGE("videoplayerimpl::", "Cannot find factory for player type %d",
         videoType);
    return this;
  }

  if (video_player_obj && !useExisting) {
    LOGW("videoplayerimpl::", "Destroying existing video player object: %p",
         video_player_obj);
    fac->DestroyPlayer(video_player_obj);
    delete video_player_obj;
    video_player_obj = NULL;
  }

  if (!video_player_obj) {
    LOGD("videoplayerimpl::", "Creating video player of type %d", videoType);
    video_player_obj = fac->CreateVideoPlayer(videoType);

    assert(video_player_obj);

    AddNativeListener();

    SetVideoTexture();
  }

  jobject rendererbuilder = fac->CreateRendererBuilder(
      videoType, videoURL, contentId, provider, useSecurePath);
  video_player_obj->Initialize(rendererbuilder, initial_resolution);

  jni_env->DeleteGlobalRef(rendererbuilder);

  return this;
}

void VideoPlayerImpl::SetInitialResolution(int initialResolution) {
  this->initial_resolution = initialResolution;
}

void VideoPlayerImpl::AddNativeListener() {
  JNIEnv *jni_env = JNIHelper::Get().Env();
  if (!listener_obj) {
    jmethodID ctor = jni_env->GetMethodID(native_listener_class, "<init>",
                                          "()V");
    jobject obj = jni_env->NewObject(native_listener_class, ctor);
    listener_obj = jni_env->NewGlobalRef(obj);
  }

  LOGD("videoplayerimpl::", "Adding native listener");
  video_player_obj->AddListener(listener_obj);
}

void VideoPlayerImpl::SetVideoTexture() {
  JNIEnv *jni_env = JNIHelper::Get().Env();
  jobject s = 0;

  if (!video_player_obj) {
    LOGD("videoplayerimpl::",
         "Videoplayer not created yet, skipping setting the surface");
    return;
  }

  if (!video_texture_obj) {
    LOGE("videoplayerimpl::", "video_texture_obj is null!");
  } else {
    s = JNIHelper::Get().CallObjectMethod(video_texture_obj,
                                          getSurfaceTextureMethodID);
  }

  if (s) {
    jobject surface = jni_env->NewGlobalRef(s);
    video_player_obj->SetSurfaceTexture(surface);
    jni_env->DeleteGlobalRef(surface);
    jni_env->DeleteLocalRef(s);
  } else {
    LOGE("videoplayerimpl::", "Surface texture is null!");
  }
}

GLuint VideoPlayerImpl::GetVideoTextureId() {
  return (GLuint)JNIHelper::Get().CallIntMethod(video_texture_obj,
                                                getVideoTextureIdMethodID);
}

float *VideoPlayerImpl::GetVideoMatrix() {
  return videoMatrix;
}

long long VideoPlayerImpl::GetVideoTimestampNs() {
  return videoTimestampNs;
}

bool VideoPlayerImpl::UpdateVideo() {
  JNIEnv *jni_env = JNIHelper::Get().Env();
  if (!video_texture_obj) {
    LOGI("videoplayerimpl::", "no texture");
    return false;
  }

  if (JNIHelper::Get().CallBooleanMethod(video_texture_obj,
                                         updateTextureMethodID)) {
    videoTimestampNs = JNIHelper::Get().CallLongMethod(video_texture_obj,
                                                       getVideoTimestampNsMethodID);
    jobject jmat = JNIHelper::Get().CallObjectMethod(video_texture_obj,
                                                     getVideoMatrixMethodID);
    if (jmat) {
      jboolean isCopy;
      jfloat *arr = jni_env->GetFloatArrayElements((jfloatArray)jmat, &isCopy);
      if (arr) {
        memcpy(videoMatrix, arr, sizeof(float) * 16);
      } else {
        memcpy(videoMatrix, ndk_helper::Mat4::Identity().Ptr(),
               sizeof(float) * 16);
      }
      jni_env->ReleaseFloatArrayElements((jfloatArray)jmat, arr, JNI_ABORT);
      jni_env->DeleteLocalRef(jmat);
    }
    return true;
  }
  return false;
}

void VideoPlayerImpl::CreateVideoTexture() {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  if (!video_texture_class) {
    LOGE("videoplayerimpl::", "VideoTextureClass not found");
    return;
  }
  if (!video_player_obj) {
    LOGE("videoplayerimpl::", "VideoPlayer not created yet");
    return;
  }
  if (!video_player_obj->GetRawObject()) {
    LOGE("videoplayerimpl::", "VideoPlayer doesn't have java object");
    return;
  }

  jmethodID ctor = jni_env->GetMethodID(video_texture_class, "<init>",
      "(Lcom/google/gvr/exoplayersupport/sample/VideoExoPlayer;)V");

  jobject obj = jni_env->NewObject(video_texture_class, ctor,
                                   video_player_obj->GetRawObject());
  video_texture_obj = jni_env->NewGlobalRef(obj);

  SetVideoTexture();

  LOGD("videoplayerimpl::", "video Texture created!");
  jni_env->DeleteLocalRef(obj);
}

VideoPlayerImpl *VideoPlayerImpl::GetInstance(int id) {
  int index = id / 100;
  VideoPlayerImpl *impl = g_instances[index];
  if (!impl) {
    LOGE("videoplayerimpl::",
         "Cannot find impl %d.  There are currently %d instances", index,
         g_instances.size());
  }
  return impl;
}

int VideoPlayerImpl::GetEventBase() { return eventBase; }

int VideoPlayerImpl::EventOperation(int eventId) { return eventId % 100; }

// Render the video in a quad.  This is usually called when using a framebuffer
// to copy the video texture to an external texture.
void VideoPlayerImpl::DrawVideo(float *mvpMatrix, int view) {
  videoScreen.Draw(mvpMatrix, GetVideoTextureId(), videoMatrix, view);
}

// The external texture that should be attached to the framebuffer when
// rendering this instance of the videoplayer.
void VideoPlayerImpl::SetExternalTextures(const int* texture_ids, int size,
                                          int w,
                                          int h) {
  num_textures = size;
  externalTexture = new ExternalTexture[size];
  for(int i=0;i<size;i++) {
    externalTexture[i].SetTexture((GLuint) texture_ids[i]);
    externalTexture[i].SetWidth(w);
    externalTexture[i].SetHeight(h);
  }
}

const ExternalTexture &VideoPlayerImpl::GetDrawableExternalTexture() {
  return externalTexture[drawableTexture];
}

const ExternalTexture &VideoPlayerImpl::GetRenderableExternalTexture() {
  return externalTexture[renderableTexture];
}

void VideoPlayerImpl::SwapExternalTexture() {
  if (num_textures > 0) {
    renderableTexture = drawableTexture;
    drawableTexture = (drawableTexture + 1) % num_textures;
    newFrameAvail = true;
  }
}

void VideoPlayerImpl::FrameDrawn() {
  newFrameAvail = false;
}

bool VideoPlayerImpl::IsNewFrameAvailable() {
  return newFrameAvail;
}

const VideoPlayerHolder *VideoPlayerImpl::GetVideoPlayer() const {
  return video_player_obj;
}

VideoPlayerImpl *VideoPlayerImpl::FromJavaObject(jobject player_obj) {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  for (auto it = g_instances.begin(); it != g_instances.end(); ++it) {
    if (it->second && it->second->GetVideoPlayer() &&
        jni_env->IsSameObject(it->second->GetVideoPlayer()->GetRawObject(),
                              player_obj)) {
      return it->second;
    }
  }
  LOGW("videoplayerimpl::", "DID NOT FIND THE PLAYER for %p", player_obj);
  return NULL;
}

void VideoPlayerImpl::OnVideoEvent(int eventId) {
  if (onevent_callback) {
    onevent_callback(callback_data, eventId);
  }
}

void VideoPlayerImpl::OnException(jstring type, jstring msg) {
  if (onexception_callback) {
    JNIEnv *jni_env = JNIHelper::Get().Env();

    jboolean copy;
    const char *str_type = jni_env->GetStringUTFChars(type, &copy);
    const char *str_msg = jni_env->GetStringUTFChars(msg, &copy);

    onexception_callback(str_type, str_msg, exceptioncallback_data);

    jni_env->ReleaseStringUTFChars(type, str_type);
    jni_env->ReleaseStringUTFChars(msg, str_msg);
  }
}

void VideoPlayerImpl::SetOnEventCallback(void (*callback)(void *, int),
                                         void *cb_data) {
  this->onevent_callback = callback;
  this->callback_data = cb_data;
}

void VideoPlayerImpl::SetOnExceptionCallback(
    void (*callback)(const char *, const char *, void *), void *cb_data) {
  this->onexception_callback = callback;
  this->exceptioncallback_data = cb_data;
}
}  // namespace gvrvideo

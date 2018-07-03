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
#include "video_support_impl.h"
#include <android/log.h>
#include <assert.h>
#include "jni_helper.h"
#include "logger.h"

namespace gvrvideo {

VideoSupportImpl *VideoSupportImpl::Create(const char *clzname) {
  VideoPlayerHolder::Initialize();

  JNIEnv *jni_env = JNIHelper::Get().Env();

  VideoSupportImpl *ret = nullptr;
  jclass clz = JNIHelper::Get().FindClass(clzname);
  if (clz) {
    jmethodID initMethodID = jni_env->GetStaticMethodID(
        clz, "initializePlayerFactory", "(Landroid/app/Activity;)V");
    jmethodID getFactoryMethodID = jni_env->GetStaticMethodID(
        clz, "getPlayerFactory",
        "(I)Lcom/google/gvr/exoplayersupport/VideoPlayerFactory;");
    assert(initMethodID);
    assert(getFactoryMethodID);

    if (initMethodID && getFactoryMethodID) {
      ret = new VideoSupportImpl((jclass)jni_env->NewGlobalRef(clz),
                                 initMethodID, getFactoryMethodID);
    }
    jni_env->DeleteLocalRef(clz);
  }

  assert(ret);
  return ret;
}

VideoSupportImpl::VideoSupportImpl(jclass supportclazz, jmethodID initMethodID,
                                   jmethodID getFactoryMethodID) {
  this->support_clazz = supportclazz;
  this->initMethodID = initMethodID;
  this->getFactoryMethodID = getFactoryMethodID;
  initialized = false;
  activityObj = 0;
  createPlayerMethodID = 0;
  destroyPlayerMethodID = 0;
  createRendererBuilderMethodID = 0;
}

VideoSupportImpl::~VideoSupportImpl() {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  if (support_clazz) {
    jni_env->DeleteGlobalRef(support_clazz);
    support_clazz = 0;
  }
}

void VideoSupportImpl::Initialize(jobject activityObj) {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  if (!initialized) {
    JNIHelper::Get().CallStaticVoidMethod(support_clazz, initMethodID,
                                          activityObj);
  }
  assert(activityObj);

  this->activityObj = activityObj;
  jclass pclz = JNIHelper::Get().FindClass(
      "com/google/gvr/exoplayersupport/VideoPlayerFactory");
  if (pclz) {
    createPlayerMethodID =
        jni_env->GetMethodID(pclz, "createPlayer",
                             "(Landroid/content/Context;)Lcom/google/gvr/"
                             "exoplayersupport/VideoPlayer;");
    destroyPlayerMethodID = jni_env->GetMethodID(
        pclz, "destroyPlayer",
        "(Lcom/google/gvr/exoplayersupport/VideoPlayer;)V");
    createRendererBuilderMethodID = jni_env->GetMethodID(
        pclz, "createRendererBuilder",
        "(Landroid/content/Context;"
        "I"
        "Ljava/lang/String;"
        "Ljava/lang/String;"
        "Ljava/lang/String;"
        "Z)"
        "Lcom/google/gvr/exoplayersupport/AsyncRendererBuilder;");

    jni_env->DeleteLocalRef(pclz);
  }

  assert(createPlayerMethodID);
  assert(destroyPlayerMethodID);
  assert(createRendererBuilderMethodID);

  initialized = true;
}

VideoPlayerHolder *VideoSupportImpl::CreateVideoPlayer(int type) const {
  LOGD("videosupportimpl::", "player holder being created of type %d", type);

  JNIEnv *jni_env = JNIHelper::Get().Env();
  VideoPlayerHolder *ret = 0;

  assert(initialized);

  jobject fac = JNIHelper::Get().CallStaticObjectMethod(
      support_clazz, getFactoryMethodID, type);
  if (fac) {
    jobject obj = JNIHelper::Get().CallObjectMethod(fac, createPlayerMethodID,
                                                    activityObj);
    if (obj) {
      ret = new VideoPlayerHolder(jni_env->NewGlobalRef(obj), type);
      jni_env->DeleteLocalRef(obj);
    } else {
      if (jni_env->ExceptionCheck()) {
        jni_env->ExceptionDescribe();
      }
      LOGE("videosupportimpl::", "createPlayer returned null!");
    }
  } else {
    LOGE("videosupportimpl::", "Cannot get factory for player type %d", type);
  }
  return ret;
}

void VideoSupportImpl::DestroyPlayer(VideoPlayerHolder *playerObj) const {
  JNIEnv *jni_env = JNIHelper::Get().Env();

  assert(initialized);

  jobject fac = JNIHelper::Get().CallStaticObjectMethod(
      support_clazz, getFactoryMethodID, playerObj->GetType());
  if (fac && playerObj->GetRawObject()) {
    JNIHelper::Get().CallVoidMethod(fac, destroyPlayerMethodID,
                                    playerObj->GetRawObject());
  } else {
    LOGE("videosupportimpl::", "Cannot get factory for type %d",
         playerObj->GetType());
  }
}

jobject VideoSupportImpl::CreateRendererBuilder(int type, const char *videoURL,
                                                const char *contentId,
                                                const char *providerId,
                                                bool useSecure) const {
  JNIEnv *jni_env = JNIHelper::Get().Env();
  jobject ret = 0;

  assert(initialized);

  jobject fac = JNIHelper::Get().CallStaticObjectMethod(
      support_clazz, getFactoryMethodID, type);
  if (fac) {
    jstring j_videoURL = jni_env->NewStringUTF(videoURL);
    jstring j_contentid = jni_env->NewStringUTF(contentId);
    jstring j_providerid = jni_env->NewStringUTF(providerId);

    jobject obj = JNIHelper::Get().CallObjectMethod(
        fac, createRendererBuilderMethodID, activityObj, type, j_videoURL,
        j_contentid, j_providerid, useSecure);

    jni_env->DeleteLocalRef(j_videoURL);
    jni_env->DeleteLocalRef(j_contentid);
    jni_env->DeleteLocalRef(j_providerid);

    if (obj) {
      ret = jni_env->NewGlobalRef(obj);
    } else {
      LOGE("videosupportimpl::",
           "Cannot get rendererbuilder for type %d: %s %s %s %d", type,
           videoURL, contentId, providerId, useSecure);
    }
  } else {
    LOGE("videosupportimpl::", "Cannot get factory for player type %d", type);
  }
  return ret;
}
}  // namespace gvrvideo

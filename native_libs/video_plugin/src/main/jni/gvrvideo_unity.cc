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
#include <stdio.h>
#include <cstring>

#include "Unity/IUnityInterface.h"
#include "Unity/UnityGraphics.h"
#include "frame_buffer.h"
#include "glutils.h"
#include "jni_helper.h"
#include "logger.h"
#include "video_externs.h"
#include "video_player_impl.h"

gvrvideo::FrameBuffer g_framebuffer;

static IUnityInterfaces *s_UnityInterfaces = NULL;
static IUnityGraphics *s_Graphics = NULL;

static void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static void OnRenderEvent(int eventID);

// Called by Unity when the plugin is loaded.
extern "C" void UnityPluginLoad(IUnityInterfaces *unityInterfaces) {
  s_UnityInterfaces = unityInterfaces;
  s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
  s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

  // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
  OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Called by Unity when the plugin is unloaded
extern "C" void UnityPluginUnload() {
  s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// Called by the Java Runtime when the plugin is loaded.  This gives access to
// the JNI environment.
extern "C" jint JNI_OnLoad(JavaVM *vm, void *reserved) {
  gvrvideo::JNIHelper::Initialize(
      vm, "com/google/gvr/exoplayersupport/VideoPlayer");

  gvrvideo::VideoPlayerImpl::Initialize();

  return JNI_VERSION_1_6;
}

// GetRenderEventFunc, an example function we export which is used to get a
// rendering event callback function.
UnityRenderingEvent GetRenderEventFunc() { return OnRenderEvent; }

bool SetVideoPlayerSupportClassname(void *ptr, const char *clzname) {
  LOGD("gvrvideo:", "SetVideoPlayerSupportClassname: %s", clzname);
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  return pObj->SetSupportClassname(clzname);
}

void *GetRawPlayer(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  return (void *)pObj->GetVideoPlayer()->GetRawObject();
}

void SetOnVideoEventCallback(void *ptr, OnVideoEventCallback callback,
                             void *cb_data) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }
  pObj->SetOnEventCallback(callback, cb_data);
}

void SetOnExceptionCallback(void *ptr, OnExceptionCallback callback,
                            void *cb_data) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }
  pObj->SetOnExceptionCallback(callback, cb_data);
}

void SetExternalTextures(void *ptr, const int* texture_ids, int size, int w,
                         int h) {
  gvrvideo::VideoPlayerImpl *pObj =
          reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }
  pObj->SetExternalTextures(texture_ids, size, w, h);
}

void* GetRenderableTextureId(void* ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
          reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return 0;
  }

  return reinterpret_cast<void*>(
      pObj->GetRenderableExternalTexture().GetTexture());
}

int GetExternalSurfaceTextureId(void* ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return 0;
  }

  return pObj->GetVideoTextureId();
}

void GetVideoMatrix(void* ptr, float* vMat) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }

  memcpy(vMat, pObj->GetVideoMatrix(), 16 * sizeof(float));
}

long long GetVideoTimestampNs(void* ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -1;
  }

  return pObj->GetVideoTimestampNs();
}

void *CreateVideoPlayer() {
  LOGD("gvrvideo:", "CreateVideoPlayer");
  return new gvrvideo::VideoPlayerImpl();
}

void DestroyVideoPlayer(void *ptr) {
  LOGD("gvrvideo:", "DestroyVideoPlayer");
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    return;
  }
  delete pObj;
}

// Called by Unity code to create the exoplayer object.  The return value
// should be cleaned up by the caller by calling DestroyPlayer().
void *InitVideoPlayer(void *ptr, int videoType, const char *videoURL,
                      const char *contentId, const char *provider,
                      bool useSecurePath, bool useExisting) {
  LOGD("gvrvideo:", "InitVideoPlayer");

  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return pObj;
  }

  return pObj->CreateVideoPlayer(videoType, videoURL, contentId, provider,
                                 useSecurePath, useExisting);
}

void SetInitialResolution(void *ptr, int initialResolution) {
  LOGD("gvrvideo:", "SetInitialResoluition: %d", initialResolution);

  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }

  return pObj->SetInitialResolution(initialResolution);
}

int GetVideoPlayerEventBase(void *ptr) {
  LOGD("gvrvideo:", "GetVideoPlayerEventBase");
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return 0;
  }
  return pObj->GetEventBase();
}

bool IsVideoReady(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return false;
  }
  return pObj->GetVideoPlayer() && pObj->GetVideoPlayer()->IsVideoReady();
}

bool IsVideoPaused(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return false;
  }
  return pObj->GetVideoPlayer() && pObj->GetVideoPlayer()->IsVideoPaused();
}

int GetPlayerState(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -1;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetPlaybackState()
                                : -1;
}

long long GetDuration(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetDuration() : -1;
}

long long GetBufferedPosition(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetBufferedPosition()
                                : -1;
}

long long GetCurrentPosition(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetCurrentPosition()
                                : -1;
}

void SetCurrentPosition(void *ptr, long long pos) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return;
  }
  if (pObj->GetVideoPlayer()) {
    pObj->GetVideoPlayer()->SetCurrentPosition(pos);
  }
}

int GetBufferedPercentage(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer()
             ? pObj->GetVideoPlayer()->GetBufferedPercentage()
             : -1;
}

int PlayVideo(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }

  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->PlayVideo() : -1;
}

int PauseVideo(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->PauseVideo() : -1;
}

int GetWidth(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj = (gvrvideo::VideoPlayerImpl *)ptr;
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetWidth() : -1;
}

int GetHeight(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetHeight() : -1;
}

// gets the max volume value that is settable.
int GetMaxVolume(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetMaxVolume() : -1;
}

int GetCurrentVolume(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
    return -2;
  }
  return pObj->GetVideoPlayer() ? pObj->GetVideoPlayer()->GetCurrentVolume()
                                : -1;
}

void SetCurrentVolume(void *ptr, int value) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    pObj->GetVideoPlayer()->SetCurrentVolume(value);
  }
}

int GetTrackCount(void *ptr, int rendererIndex) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    return pObj->GetVideoPlayer()->GetTrackCount(rendererIndex);
  }
  return 0;
}

ExoTrackInfo *GetTrackInfo(void *ptr, int rendererIndex) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    return pObj->GetVideoPlayer()->GetTrackInfo(rendererIndex);
  }
  return nullptr;
}

int GetStereoMode(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    return pObj->GetVideoPlayer()->GetStereoMode();
  }
  return -1;
}

bool HasProjectionData(void *ptr) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    return pObj->GetVideoPlayer()->HasProjectionData();
  }

  return false;
}


void ReleaseTrackInfo(void *ptr, ExoTrackInfo *info, int ct) {
  gvrvideo::VideoPlayerImpl *pObj =
      reinterpret_cast<gvrvideo::VideoPlayerImpl *>(ptr);
  if (!pObj) {
    LOGE("gvrvideo:", "Calling with null player object!");
  }
  if (pObj->GetVideoPlayer()) {
    return pObj->GetVideoPlayer()->ReleaseTrackInfo(info, ct);
  }
}

extern "C" void
Java_com_google_gvr_exoplayersupport_impl_NativeVideoCallbacks_onError(
    JNIEnv *env, jobject instance, jobject player, jstring type, jstring msg) {
  gvrvideo::VideoPlayerImpl *pObj =
      gvrvideo::VideoPlayerImpl::FromJavaObject(player);
  if (!pObj) {
    LOGE("gvrvideo:",
         "Calling onException with null player object from java %p!", player);
    return;
  }
  pObj->OnException(type, msg);
}

extern "C" void
Java_com_google_gvr_exoplayersupport_impl_NativeVideoCallbacks_onVideoEvent(
    JNIEnv *env, jobject instance, jobject player, jint eventId) {
  gvrvideo::VideoPlayerImpl *pObj =
      gvrvideo::VideoPlayerImpl::FromJavaObject(player);
  if (!pObj) {
    LOGE("gvrvideo:",
         "Calling onVideoEvent with null player object from java %p!", player);
    return;
  }
  pObj->OnVideoEvent(eventId);
}

// Called from Unity to set the device type and allow for initialization.
static void UNITY_INTERFACE_API
OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType) {
  switch (eventType) {
    case kUnityGfxDeviceEventInitialize: {
      LOGD("gvrvideo:", "OnGraphicsDeviceEvent(Initialize).\n");
      LOGD("gvrvideo:", "device string: %s",
           reinterpret_cast<const char *>(glGetString(GL_VERSION)));

      gvrvideo::VideoQuadScreen::InitGL();

      break;
    }

    case kUnityGfxDeviceEventShutdown: {
      LOGD("gvrvideo:", "OnGraphicsDeviceEvent(Shutdown).\n");
      break;
    }

    case kUnityGfxDeviceEventBeforeReset: {
      LOGD("gvrvideo:", "OnGraphicsDeviceEvent(BeforeReset).\n");
      break;
    }

    case kUnityGfxDeviceEventAfterReset: {
      LOGD("gvrvideo:", "OnGraphicsDeviceEvent(AfterReset).\n");
      break;
    }
  };
}

void DoVideoUpdate(gvrvideo::VideoPlayerImpl *pObj) {
  if (pObj && pObj->UpdateVideo()) {
    pObj->SwapExternalTexture();
  }
}

bool StartFramebuffer(gvrvideo::VideoPlayerImpl *pObj) {
  gvrvideo::CheckGLError("Start of framebuffer");
  bool ret = true;
  const gvrvideo::ExternalTexture& texture = pObj->GetDrawableExternalTexture();
  if (g_framebuffer.GetExternalTexture() != texture) {
    ret = g_framebuffer.ReInitialize(texture);
  }

  if (ret) {
    ret = g_framebuffer.Bind();
  }

  return ret;
}

struct GraphicsState {
  GLboolean Cullface;
  GLboolean Blend;
  GLenum DepthFunc;
  GLboolean DepthTest;
  GLboolean DepthMask;
  GLint Viewport[4] = {};
};

GraphicsState g_GraphicsState;

static void SetGraphicsState(const GraphicsState &newState,
                             GraphicsState *oldState) {
  if (oldState) {
    glGetBooleanv(GL_CULL_FACE, &oldState->Cullface);
    glGetBooleanv(GL_BLEND, &oldState->Blend);
    glGetIntegerv(GL_DEPTH_FUNC, (GLint *)&oldState->DepthFunc);
    glGetBooleanv(GL_DEPTH_TEST, &oldState->DepthTest);
    glGetBooleanv(GL_DEPTH_WRITEMASK, &oldState->DepthMask);
    glGetIntegerv(GL_VIEWPORT, oldState->Viewport);
  }

  if (newState.Cullface) {
    glEnable(GL_CULL_FACE);
  } else {
    glDisable(GL_CULL_FACE);
  }

  if (newState.Blend) {
    glEnable(GL_BLEND);
  } else {
    glDisable(GL_BLEND);
  }

  if (newState.DepthTest) {
    glEnable(GL_DEPTH_TEST);
  } else {
    glDisable(GL_DEPTH_TEST);
  }

  glDepthMask(newState.DepthMask);
  glDepthFunc(newState.DepthFunc);

  glViewport(newState.Viewport[0], newState.Viewport[1], newState.Viewport[2],
             newState.Viewport[3]);

  gvrvideo::CheckGLError("Set DefaultGraphics State");
}

void GetTextureData(gvrvideo::VideoPlayerImpl *pObj, int view) {
  GLint gltex = pObj->GetVideoTextureId();
  if (gltex <= 0) {
    LOGW("gvrvideo:", "gltex is <= 0 for VideoTextureId");
    return;
  }
  if (!pObj->GetVideoPlayer() || !pObj->GetVideoPlayer()->IsVideoReady()) {
    LOGW("gvrvideo:", "videoplayer is null or not ready!");
    return;
  }

  if (!pObj->GetDrawableExternalTexture().GetTexture()) {
    LOGW("gvrvideo:", "External Texture not set!");
    return;
  }

  if (!glIsTexture(pObj->GetDrawableExternalTexture().GetTexture())) {
    LOGW("gvrvideo:", "Texture is not a valid texture.");
    return;
  }

  if (!pObj->IsNewFrameAvailable()) {
    return;
  }

  g_GraphicsState.Cullface = GL_FALSE;
  g_GraphicsState.Blend = GL_FALSE;
  g_GraphicsState.DepthFunc = GL_LEQUAL;
  g_GraphicsState.DepthTest = GL_TRUE;
  g_GraphicsState.DepthMask = GL_FALSE;
  g_GraphicsState.Viewport[0] = 0;
  g_GraphicsState.Viewport[1] = 0;
  g_GraphicsState.Viewport[2] = pObj->GetDrawableExternalTexture().GetWidth();
  g_GraphicsState.Viewport[3] = pObj->GetDrawableExternalTexture().GetHeight();

  // Actual functions defined below
  GraphicsState oldState;

  SetGraphicsState(g_GraphicsState, &oldState);

  if (StartFramebuffer(pObj)) {

    glClear(GL_COLOR_BUFFER_BIT);
    gvrvideo::CheckGLError("Clear Draw");

    // the model is the "Unity unit size, from -.5 to +.5, so scale it by 2.0
    ndk_helper::Mat4 wm;
    pObj->DrawVideo(wm.Scale(2.0f, 2.0f, 2.0f).Ptr(), view);
    gvrvideo::CheckGLError("Video Draw");

    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    gvrvideo::CheckGLError("StopFrameBuffer");

    pObj->FrameDrawn();
  } else {
    LOGE("gvrvideo:", "FRAMEBUFFER COULD NOT BE INITIALIZED");
  }
  SetGraphicsState(oldState, nullptr);

}

// Passed to Unity as the entry point to the native plugin.  Calls to this
// method have the GL context set on the thread so openGL calls will work.
static void OnRenderEvent(int eventID) {
  // Unreal is initialized slightly differently from Unity.  Unreal passes
  // this event vs. Unity calls another method.
  if (eventID == EVENT_UE4INITIALIZE) {
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
    return;
  }

  int gl_error = glGetError();
  if (gl_error != GL_NO_ERROR) {
    LOGW("gvrvideo:", "Clearing gl_error 0x%x at begin OnRenderEvent",
         gl_error);
  }

  gvrvideo::VideoPlayerImpl *pObj = gvrvideo::VideoPlayerImpl::GetInstance(eventID);

  int operation = gvrvideo::VideoPlayerImpl::EventOperation(eventID);

  if (!pObj) {
    LOGE("gvrvideo:", "Invalid event ID: %d", eventID);
    return;
  }

  switch (operation) {
    case EVENT_INITIALIZE:  // initialize
      LOGD("gvrvideo:", "--------- I N I T --------------------");
      pObj->CreateVideoTexture();
      break;
    case EVENT_UPDATE:  // update Video
      DoVideoUpdate(pObj);
      break;
    case EVENT_SHUTDOWN:
      break;
    case EVENT_RENDER_MONO:  // render texture
      GetTextureData(pObj, gvrvideo::VideoQuadScreen::MONO_VIEW);
      break;
    case EVENT_RENDER_LEFT:  // left eye
      GetTextureData(pObj, gvrvideo::VideoQuadScreen::LEFT_EYE_VIEW);
      break;
    case EVENT_RENDER_RIGHT:
      GetTextureData(pObj, gvrvideo::VideoQuadScreen::RIGHT_EYE_VIEW);
      break;
    case EVENT_RENDER_INVERTED_MONO:
      GetTextureData(pObj, gvrvideo::VideoQuadScreen::INVERTED_MONO_VIEW);
      break;
    default:
      LOGE("gvrvideo:", "Unknown Render eventid: %d", eventID);
  }
  gvrvideo::CheckGLError("End Render event");
}

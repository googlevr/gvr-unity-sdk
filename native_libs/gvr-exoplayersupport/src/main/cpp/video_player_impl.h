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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_IMP_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_IMP_H_

#include <GLES2/gl2.h>
#include <android/log.h>
#include <jni.h>

#include "external_texture.h"
#include "video_quad_screen.h"
#include "video_support_impl.h"

namespace gvrvideo {

// Encapsulates the video player used to decode the video stream (which is
// accessed via JNI) and
// the external texture that that should be attached to a framebuffer to copy
// the video texture
// to an external texture.
class VideoPlayerImpl {
 public:
  // Initialize the JNI environment for accessing java code.
  static void Initialize();

  VideoPlayerImpl();

  virtual ~VideoPlayerImpl();

  // Sets the name of the java class to use to access the player factory.
  // Returns true if the class is found.
  bool SetSupportClassname(const char *clzname);

  // Initialize the video player and create the playback stream.
  void *CreateVideoPlayer(int videoType, const char *videoURL,
                          const char *contentId, const char *provider,
                          bool useSecurePath, bool useExisting);

  // Sets the initial resolution to attempt to load when starting the video.
  void SetInitialResolution(int initialResolution);

  // Returns the texture ID used by the video player to draw the video.
  GLuint GetVideoTextureId();

  // Returns the video texture transformation matrix from the SurfaceTexture.
  float *GetVideoMatrix();

  // Returns the most recent video frame's timestamp in nanoseconds.
  long long GetVideoTimestampNs();

  // Called to process a new video frame.
  bool UpdateVideo();

  // Creates a surface texture for the video player.
  void CreateVideoTexture();

  // Returns the player object with the matching id.  This is used to
  // handle multiple instances in the same app in Unity.  Each event has the
  // id of a player encoded in the eventId.  This way the correct internal
  // player object is used to process the event.
  static VideoPlayerImpl *GetInstance(int id);

  // Returns the event base ID for this player instance.  This is used by Unity
  // to offset eventIds for multiple players.
  int GetEventBase();

  // Parses the encoded eventId and returns the operation Id.
  static int EventOperation(int eventId);

  // Draw the video frame using the given matrix and view index.
  void DrawVideo(float *mvpMatrix, int view);

  // Sets the external texture that the video texture should be copied to.
  // This setting includes the width and height of the texture.
  void SetExternalTextures(const int* texture_ids, int size, int w, int h);

  // Gets the texture that should be used when copying the video image.
  const ExternalTexture &GetDrawableExternalTexture();

  // Gets the texture that should be rendered.
  const ExternalTexture &GetRenderableExternalTexture();

  // Swaps the renderable and drawable textures.
  void SwapExternalTexture();

  // Indicates the drawable texture has been updated.
  void FrameDrawn();

  // Indicates a new frame is available and should be rendered.
  bool IsNewFrameAvailable();

  // Sets the callback to invoke when there is a video event.  The
  // parameters of the callback are the void* data and the event id.
  void SetOnEventCallback(void (*callback)(void *, int), void *pVoid);

  // Sets the callback to invoke when there is an error or exception.  The
  // parameters of the callback are the void* data and the event id.
  void SetOnExceptionCallback(void (*callback)(const char *, const char *,
                                               void *), void *cb_data);

  const VideoPlayerHolder *GetVideoPlayer() const;

  // Returns the implementation instance based on the java object.  This is
  // called primarily when receiving a callback from Java.
  static VideoPlayerImpl *FromJavaObject(jobject player_obj);

  // Fires the video event given.
  void OnVideoEvent(int eventId);

  // Fires the exception.
  void OnException(jstring type, jstring msg);

 protected:
  const VideoSupportImpl *GetVideoSupportImpl();

  void InitPlayerActivity();

  void AddNativeListener();

  void SetVideoTexture();

 private:
  // This are per instance objects.
  VideoSupportImpl *pVideoFactoryHolder;
  VideoPlayerHolder *video_player_obj;

  // Callback for video events.
  void (*onevent_callback)(void *, int);

  // The callback data to use when calling back.
  void *callback_data;

  // The exception callback & data.
  void (*onexception_callback)(const char *type, const char *msg,
                               void *cb_data);
  void *exceptioncallback_data;

  jobject video_texture_obj;
  jobject listener_obj;

  float videoMatrix[16] = {};
  long long videoTimestampNs;
  VideoQuadScreen videoScreen;

  int renderableTexture;
  int drawableTexture;
  bool newFrameAvail = false;
  int num_textures;
  ExternalTexture *externalTexture;

  int eventBase;

  int initial_resolution;

  static jobject unity_player_activity;

  // From VideoTexture.
  static jmethodID getSurfaceTextureMethodID;
  static jmethodID getVideoTextureIdMethodID;
  static jmethodID updateTextureMethodID;
  static jmethodID getVideoMatrixMethodID;
  static jmethodID getVideoTimestampNsMethodID;
  static jmethodID releaseMethodID;
  static jclass video_texture_class;

  static jclass native_listener_class;
};
}  // namespace gvrvideo
#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_PLAYER_IMP_H_

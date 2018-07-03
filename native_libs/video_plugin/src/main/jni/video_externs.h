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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_EXTERNS_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_EXTERNS_H_

extern "C" {

// collected constant values across the API surface.  This are defined here so
// consumers of the
// plugin can use them.
#define EVENT_NONE -1
#define EVENT_INITIALIZE 0
#define EVENT_UPDATE 1
#define EVENT_RENDER_MONO 2
#define EVENT_RENDER_LEFT 3
#define EVENT_RENDER_RIGHT 4
#define EVENT_SHUTDOWN 5
#define EVENT_UE4INITIALIZE 6
#define EVENT_RENDER_INVERTED_MONO 7

#define TYPE_DASH 0
#define TYPE_HLS 2
#define TYPE_OTHER 3

#define TYPE_VIDEO 0
#define TYPE_AUDIO 1
#define TYPE_TEXT 2
#define TYPE_METADATA 3

#define VIDEO_EVENT_READY 1
#define VIDEO_EVENT_STARTED_PLAYBACK 2
#define VIDEO_EVENT_FORMAT_CHANGED 3
#define VIDEO_EVENT_SURFACE_SET 4
#define VIDEO_EVENT_SIZE_CHANGED 5

#define RES_LOWEST 0
#define RES_720 720
#define RES_1080 1080
#define RES_2048 2048
#define RES_HIGHEST 4096

// typedef of Unity plugin callback.
typedef void (*UnityRenderingEvent)(int eventId);

// callback defined by this module to communicate back to the game engine app
// when something
// interesting happens.
typedef void (*OnVideoEventCallback)(void *ptr, int eventId);

// callback defined by this module to communicate back to the game engine app
// when an exception
// or error is encountered.
typedef void (*OnExceptionCallback)(const char *type, const char *msg,
                                    void *cb_data);

// structure defining the track info from Exoplayer.  This is mostly the same
// and is intended
// to provide a certain amount of decoupling from Exoplayer to avoid version
// incompatibilities.
struct ExoTrackInfo {
  int Index;
  int BitRate;
  float FrameRate;
  int Width;
  int Height;
  const char *MimeType;
  const char *DisplayName;
  const char *Language;
  const char *Name;
  int Channels;
  int SampleRate;
};

// GetRenderEventFunc, an example function we export which is used to get a
// rendering event callback function.
UnityRenderingEvent GetRenderEventFunc();

// Create an instance of the video player.  The return value is used on all the
// subsequent calls to identify the instance of the player.  When the player
// is no longer needed, it should be destroyed by calling DestroyVideoPlayer.
void *CreateVideoPlayer();

// Destroys the instance of the video player.
void DestroyVideoPlayer(void *ptr);

// Returns the event base offset for the player.  Events are sent to the
// renderer thread from scripts in Unity.  This offset is used to correlate
// which player is firing the event.
int GetVideoPlayerEventBase(void *obj);

// Sets the array of textures to use as a circular buffer of textures.
// Use when rendering the video texture, and should be large enough to
// allow some overlapping of drawning and rendering, e.g. 2 <= size <= 10.
void SetExternalTextures(void *obj, const int* texture_ids, int size, int w,
                         int h);

// Initializes the player and starts playing the specified stream.
void *InitVideoPlayer(void *obj, int videoType, const char *videoURL,
                      const char *contentId, const char *provider,
                      bool useSecurePath, bool useExisting);

void* GetRenderableTextureId(void *ptr);

int GetExternalSurfaceTextureId(void* ptr);

void GetVideoMatrix(void* ptr, float* vMat);

long long GetVideoTimestampNs(void* ptr);

// Sets the initial resolution to attempt when starting the video player
void SetInitialResolution(void *ptr, int initialResolution);

// returns true if the video stream is ready
bool IsVideoReady(void *ptr);

// returns true if the video stream is paused
bool IsVideoPaused(void *ptr);

// returns the player state -- see exoplayer documentation for details
int GetPlayerState(void *ptr);

// returns the duration of the video in milliseconds
long long GetDuration(void *ptr);

// returns the buffered position of the video.
long long GetBufferedPosition(void *ptr);

// returns the currently playing position of the video.
long long GetCurrentPosition(void *ptr);

// sets the current position (seek).
void SetCurrentPosition(void *ptr, long long pos);

// gets the percentate 0-100 of the video that is buffered.
int GetBufferedPercentage(void *ptr);

// resumes playback of the video.
int PlayVideo(void *ptr);

// pauses the playback of the video.
int PauseVideo(void *ptr);

// returns the width of the video image.
int GetWidth(void *ptr);

// returns the height of the video image.
int GetHeight(void *ptr);

// gets the max volume value that is settable.
int GetMaxVolume(void *ptr);

// gets the current volume level (0 - GetMaxVolume())
int GetCurrentVolume(void *ptr);

// sets the current volume level.
void SetCurrentVolume(void *videoPlayerPtr, int value);

// Sets the name of the video player support class.  This class needs to have 2
// static methods.<p/>
// initializePlayerFactory() is called the first time before getting
// the player factory.  This allows the implementation to perform startup
// initialization before creating any factories.
// <code>
// public static void initializePlayerFactory(Activity unityPlayerActivity);
// </code>
// <p/> getFactory() is called each time the factory is needed.  The return
// value, is not cached by the caller.
// <code>
// public static VideoPlayerFactory getFactory();
// </code>
//<p/>
// If this method is not called, the default video player factory is used.
// The return value is true if the class can be found.
bool SetVideoPlayerSupportClassname(void *ptr, const char *clzname);

// Returns the underlying java object to allow for custom manipulation and
// inspection.
void *GetRawPlayer(void *ptr);

// Sets the callback handler for video events.
void SetOnVideoEventCallback(void *ptr, OnVideoEventCallback callback,
                             void *cb_data);

// Sets the exception callback.
void SetOnExceptionCallback(void *ptr, OnExceptionCallback callback,
                            void *cb_data);

// Gets the track count for the given renderer.
int GetTrackCount(void *ptr, int rendererIndex);

// Gets the track info array for the given renderer.  The buffer is allocated
// internally and should be released when done by calling ReleaseTrackInfo.
ExoTrackInfo *GetTrackInfo(void *ptr, int rendererIndex);

// Gets the stereo mode for the video.
// Returns -1 for no value, 0 for Mono, 1 for TopBottom, and 2 for LeftRight.
int GetStereoMode(void *ptr);

// Returns true if the video contains projection data for 360/VR video.
bool HasProjectionData(void *ptr);

// Releases the track info buffer returned by GetTrackInfo.  The ct parameter
// should be the value returned by GetTrackCount for the renderer index that
// matches this buffer.
void ReleaseTrackInfo(void *ptr, ExoTrackInfo info[], int ct);
};
#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_EXTERNS_H_

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

#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_SUPPORT_IMPL_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_SUPPORT_IMPL_H_

#include <jni.h>
#include "video_player_holder.h"

namespace gvrvideo {

// Class for wrapping using the Java based video player factory locator.
class VideoSupportImpl {
 public:
  // use a factory pattern to create these so we can validate the parameters.
  // specifically, does the class pass in exist.
  static VideoSupportImpl *Create(const char *clzname);

  virtual ~VideoSupportImpl();

  void Initialize(jobject playerObj);

  VideoPlayerHolder *CreateVideoPlayer(int type) const;

  void DestroyPlayer(VideoPlayerHolder *playerObj) const;

  jobject CreateRendererBuilder(int type, const char *videoURL,
                                const char *contentId, const char *providerId,
                                bool useSecure) const;

 private:
  VideoSupportImpl(jclass supportclazz, jmethodID initMethodID,
                   jmethodID getFactoryMethodID);

  bool initialized;
  jclass support_clazz;
  jobject activityObj;

  // these are static methods on the locator class.
  jmethodID initMethodID;
  jmethodID getFactoryMethodID;

  // these are methods from the VideoPlayerFactory interface.
  jmethodID createPlayerMethodID;
  jmethodID destroyPlayerMethodID;
  jmethodID createRendererBuilderMethodID;
};
}  // namespace gvrvideo

#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_SUPPORT_IMPL_H_

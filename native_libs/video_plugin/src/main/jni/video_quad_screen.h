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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_QUAD_SCREEN_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_QUAD_SCREEN_H_

#include <GLES2/gl2.h>
#include <android/log.h>

namespace gvrvideo {

// Class that contains a quad geomerty and the shader and parameters to draw the
// video in order to copy it via the frame buffer.
class VideoQuadScreen {
 public:
  static const int MONO_VIEW = 0;
  static const int RIGHT_EYE_VIEW = 1;
  static const int LEFT_EYE_VIEW = 2;
  static const int INVERTED_MONO_VIEW = 3;
  ~VideoQuadScreen();

  static void InitGL();

  void Draw(float *mvp, GLuint videoTextureId,
            const float *videoTransformMatrix, int view);

 private:
  static GLuint video_program_;
  static GLuint video_modelview_projection_param_;
  static GLuint video_st_param_;
  static GLuint video_texcoord_param_;
  static GLuint video_position_param_;
  static GLuint array_buffers_[4];
};
}  // namespace gvrvideo

#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_VIDEO_VIDEO_QUAD_SCREEN_H_

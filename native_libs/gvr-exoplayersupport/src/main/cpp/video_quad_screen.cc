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
#include "video_quad_screen.h"
#include "glutils.h"
#include "logger.h"

namespace gvrvideo {

GLuint VideoQuadScreen::video_program_;
GLuint VideoQuadScreen::video_modelview_projection_param_;
GLuint VideoQuadScreen::video_st_param_;
GLuint VideoQuadScreen::video_texcoord_param_;
GLuint VideoQuadScreen::video_position_param_;
GLuint VideoQuadScreen::array_buffers_[4] = {};

// Vertices have a stride of 5 floats.
static const int kVertexStrideBytes = 5 * sizeof(float);
// Texture coords come after the x,y,z coords.
static const int kTexCoordOffsetBytes = 3 * sizeof(float);

// This is a 1 unit wide quad which matches the scale of 1.0 and is
// inverted UV on the X.  This is to draw nicely in Unreal.
static const float inverted_screen_vertices[] = {
    // X,   Y,   Z,    U, V
    -0.5f,  0.5f, 0.0f, 0, 0,
    -0.5f, -0.5f, 0.0f, 0, 1,
     0.5f, -0.5f, 0.0f, 1, 1,
     0.5f,  0.5f, 0.0f, 1, 0,
};

// This is a 1 unit wide quad which matches the scale of 1.0 in Unity.
static const float screen_vertices[] = {
    // X,   Y,   Z,    U, V
    -0.5f,  0.5f, 0.0f, 0, 1,
    -0.5f, -0.5f, 0.0f, 0, 0,
     0.5f, -0.5f, 0.0f, 1, 0,
     0.5f,  0.5f, 0.0f, 1, 1,
};
// This is a 1 unit wide quad which matches the scale of 1.0 in Unity,
// for the right eye, which is the top half of the video.
static const float right_vertices[] = {
    // X,   Y,   Z,    U, V
    -0.5f,  0.5f, 0.0f, 1, 1,
    -0.5f, -0.5f, 0.0f, 1, 0.5f,
     0.5f, -0.5f, 0.0f, 0, 0.5f,
     0.5f,  0.5f, 0.0f, 0, 1,
};
// This is a 1 unit wide quad which matches the scale of 1.0 in Unity.
static const float left_vertices[] = {
    // X,   Y,   Z,    U, V
    -0.5f,  0.5f, 0.0f, 1, 0.5f,
    -0.5f, -0.5f, 0.0f, 1, 0,
     0.5f, -0.5f, 0.0f, 0, 0,
     0.5f,  0.5f, 0.0f, 0, 0.5f,
};
static const int kNumScreenVertices = sizeof(screen_vertices) / kVertexStrideBytes;

static const char *kVideoVertexShader = {
    "uniform mat4 uMVPMatrix;\n"
    "uniform mat4 uSTMatrix;\n"
    "attribute vec4 a_TexCoord;\n"
    "attribute vec4 aPosition;\n"
    "varying vec2 vTextureCoord;\n"
    "void main() {\n"
    "  gl_Position = uMVPMatrix * aPosition;\n"
    "  vTextureCoord = (uSTMatrix * a_TexCoord).xy;\n"
    "}\n"};
// Fragment shader using the samplerExternalOES so that it can use video as a
// texture.
static const char *kVideoFragmentShader =
    "#extension GL_OES_EGL_image_external : require\n"
    "precision mediump float;\n"
    "varying vec2 vTextureCoord;\n"
    "uniform samplerExternalOES sTexture;\n"
    "void main() {\n"
    "  gl_FragColor =  texture2D(sTexture, vTextureCoord);\n"
    "}\n";

VideoQuadScreen::~VideoQuadScreen() { }

void VideoQuadScreen::InitGL() {
  GLuint video_vertex_shader =
      CreateShader(GL_VERTEX_SHADER, kVideoVertexShader);
  CheckGLError("video_vertex_shader");
  assert(video_vertex_shader > 0);

  GLuint video_texture_shader =
      CreateShader(GL_FRAGMENT_SHADER, kVideoFragmentShader);
  CheckGLError("video_texture_shader");
  assert(video_texture_shader > 0);

  video_program_ = glCreateProgram();
  glAttachShader(video_program_, video_vertex_shader);
  glAttachShader(video_program_, video_texture_shader);
  glLinkProgram(video_program_);
  glUseProgram(video_program_);

  video_modelview_projection_param_ =
      (GLuint)glGetUniformLocation(video_program_, "uMVPMatrix");
  assert(video_modelview_projection_param_ >= 0);

  video_st_param_ = (GLuint)glGetUniformLocation(video_program_, "uSTMatrix");
  assert(video_st_param_ >= 0);

  video_texcoord_param_ = (GLuint)glGetAttribLocation(video_program_,
                                                      "a_TexCoord");
  assert(video_texcoord_param_ >= 0);

  video_position_param_ = (GLuint)glGetAttribLocation(video_program_,
                                                      "aPosition");
  assert(video_position_param_ >= 0);

  CheckGLError("video program params");

  glGenBuffers(4, array_buffers_);
  glBindBuffer(GL_ARRAY_BUFFER, array_buffers_[MONO_VIEW]);
  glBufferData(GL_ARRAY_BUFFER, sizeof(screen_vertices), screen_vertices,
               GL_STATIC_DRAW);
  CheckGLError("buffer data mono");

  glBindBuffer(GL_ARRAY_BUFFER, array_buffers_[RIGHT_EYE_VIEW]);
  glBufferData(GL_ARRAY_BUFFER, sizeof(right_vertices), right_vertices,
               GL_STATIC_DRAW);
  CheckGLError("right buffer data");

  glBindBuffer(GL_ARRAY_BUFFER, array_buffers_[LEFT_EYE_VIEW]);
  glBufferData(GL_ARRAY_BUFFER, sizeof(left_vertices), left_vertices,
               GL_STATIC_DRAW);
  CheckGLError("left buffer data");

  glBindBuffer(GL_ARRAY_BUFFER, array_buffers_[INVERTED_MONO_VIEW]);
  glBufferData(GL_ARRAY_BUFFER, sizeof(inverted_screen_vertices),
               inverted_screen_vertices, GL_STATIC_DRAW);
  CheckGLError("inverted buffer mono");
}

void VideoQuadScreen::Draw(float *mvp, GLuint videoTextureId,
                           const float *videoTransformMatrix, int view) {
  if (videoTextureId <= 0) {
    LOGE("videoquadscreen:", "No texture id!");
    return;
  }

  glUseProgram(video_program_);
  glActiveTexture(GL_TEXTURE0);
  glBindTexture(GL_TEXTURE_EXTERNAL_OES, videoTextureId);
  CheckGLError("bind video texture");

  glUniformMatrix4fv(video_st_param_, 1, GL_FALSE, videoTransformMatrix);
  CheckGLError("screen video mat");

  assert(view >= 0 && view < 4);
  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
  glBindBuffer(GL_ARRAY_BUFFER, array_buffers_[view]);

  glEnableVertexAttribArray(video_position_param_);
  glVertexAttribPointer(video_position_param_, 3, GL_FLOAT, GL_FALSE,
                        kVertexStrideBytes, 0);
  CheckGLError("vertex attrib");

  glEnableVertexAttribArray(video_texcoord_param_);
  glVertexAttribPointer(video_texcoord_param_, 2, GL_FLOAT, GL_FALSE,
                        kVertexStrideBytes,
                        (const GLvoid *)kTexCoordOffsetBytes);
  CheckGLError("texcoord attrib");

  // Set the ModelViewProjection matrix in the shader.
  glUniformMatrix4fv(video_modelview_projection_param_, 1, GL_FALSE, mvp);

  glDrawArrays(GL_TRIANGLE_FAN, 0, kNumScreenVertices);
  glBindTexture(GL_TEXTURE_EXTERNAL_OES, 0);
  CheckGLError("Drawing screen");
}
}  // namespace gvrvideo

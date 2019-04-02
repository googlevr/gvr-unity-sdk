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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_GLUTILS_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_GLUTILS_H_

#include <GLES2/gl2.h>

#include "ndk_helper/vecmath.h"

#define GL_TEXTURE_EXTERNAL_OES 0x8D65

namespace gvrvideo {

// Helper function to compile a shader.
GLuint CreateShader(GLenum type, const char *text);

// Helper function to check for errors and log them.
int CheckGLError(const char *label);
}  // namespace gvrvideo

#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_GLUTILS_H_

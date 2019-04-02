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
#include "glutils.h"
#include "logger.h"

namespace gvrvideo {

GLuint CreateShader(GLenum type, const char *text) {
  GLuint ret = glCreateShader(type);
  glShaderSource(ret, 1, &text, NULL);
  glCompileShader(ret);

  // Get the compilation status.
  int compileStatus = GL_TRUE;
  glGetShaderiv(ret, GL_COMPILE_STATUS, &compileStatus);

  if (compileStatus == GL_FALSE) {
    GLint maxLength = 0;
    glGetShaderiv(ret, GL_INFO_LOG_LENGTH, &maxLength);

    LOGE("glutils::", "status is %d", compileStatus);
    // The maxLength includes the NULL character
    GLchar *msg = new char[maxLength];
    glGetShaderInfoLog(ret, maxLength, &maxLength, msg);

    LOGE("glutils::", "Error compiling shader type: %d: %s", type, msg);

    delete[] msg;

    assert(compileStatus != GL_FALSE);
  }
  return ret;
}

int CheckGLError(const char *label) {
  int gl_error = glGetError();
  if (gl_error != GL_NO_ERROR) {
    LOGE("glutils::", "GL error @ %s: 0x%x", label, gl_error);
    assert(glGetError() != GL_NO_ERROR);
  }
  assert(glGetError() == GL_NO_ERROR);
  return gl_error;
}
}  // namespace gvrvideo

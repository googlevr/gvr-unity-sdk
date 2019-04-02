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
#include <assert.h>
#include <EGL/egl.h>

#include "frame_buffer.h"
#include "glutils.h"
#include "logger.h"

namespace gvrvideo {

// Declare the egl3es functions we use when using the FrameBuffer.
static GL_APICALL void (*GL_APIENTRY glDrawBuffers)(GLsizei n,
                                                    const GLenum *bufs);

static GL_APICALL void (*GL_APIENTRY glReadBuffer)(GLenum mode);

FrameBuffer::~FrameBuffer() {
  if (framebufferID) {
    glDeleteFramebuffers(1, &framebufferID);
  }
}

// Re-inits the framebuffer with the given external texture.
bool FrameBuffer::ReInitialize(const ExternalTexture &texture) {
  externalTexture = texture;

  // clean up existing buffer if allocated.
  if (this->framebufferID) {
    glDeleteFramebuffers(1, &framebufferID);
    framebufferID = 0;
  }

 return Initialize();
}

bool FrameBuffer::Initialize() {
  // Get the gl3 function pointers.
  if (!glDrawBuffers) {
    glDrawBuffers = (void (*)(GLsizei n, const GLenum *bufs))eglGetProcAddress(
        "glDrawBuffers");
    assert(glDrawBuffers);
  }
  if (!glReadBuffer) {
    glReadBuffer = (void (*)(GLenum mode))eglGetProcAddress("glReadBuffer");
    assert(glReadBuffer);
  }

  bool ret = framebufferID > 0;
  if (!framebufferID) {
    glGenFramebuffers(1, &framebufferID);
    CheckGLError("glRenderbufferStorage");

    glBindFramebuffer(GL_FRAMEBUFFER, framebufferID);
    CheckGLError("bind framebuffer");

    // Bind the external texture to the framebuffer.
    glBindTexture(GL_TEXTURE_2D, externalTexture.GetTexture());
    CheckGLError("glbintexutre external framebuffer");

    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D,
                           externalTexture.GetTexture(), 0);
    CheckGLError("glFramebufferRenderbuffer framebuffer");

    GLenum DrawBuffers[1] = {GL_COLOR_ATTACHMENT0};
    glDrawBuffers(1, DrawBuffers);
    CheckGLError("color attachment");

    GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    assert(status == GL_FRAMEBUFFER_COMPLETE);
    if(status != GL_FRAMEBUFFER_COMPLETE) {
      LOGE("FrameBuffer::", "Frame buffer is not complete! Code: 0x%04x",
           status);
    }

    ret = status == GL_FRAMEBUFFER_COMPLETE &&
        CheckGLError("End of initialize") == GL_NO_ERROR;
  }
    return ret;

}

// Binds this framebuffer to the context.
bool FrameBuffer::Bind() {
  bool ret = Initialize();

  if(ret) {
    glBindFramebuffer(GL_FRAMEBUFFER, framebufferID);
    ret = CheckGLError("glBindFramebuffer") == GL_NO_ERROR;
  } else {
    glDeleteFramebuffers(1, &framebufferID);
    framebufferID = 0;
  }
  return ret;
}
}  // namespace gvrvideo

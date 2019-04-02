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

#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_EXTERNAL_TEXTURE_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_EXTERNAL_TEXTURE_H_

namespace gvrvideo {

// Container for an openGL texture created externally.  This includes the
// texture id and the dimensions of the texture.  Equality and assignment
// operations are defined.
class ExternalTexture {
 public:
  ExternalTexture() : texture(0), texWidth(0), texHeight(0) {}

  ExternalTexture(const ExternalTexture &dup) {
    texture = dup.texture;
    texWidth = dup.texWidth;
    texHeight = dup.texHeight;
  }

  ExternalTexture &operator=(const ExternalTexture &other) {
    if (this == &other) {
      return *this;
    }
    texture = other.texture;
    texWidth = other.texWidth;
    texHeight = other.texHeight;
    return *this;
  }

  void SetTexture(GLuint texture) { this->texture = texture; }

  void SetWidth(int w) { this->texWidth = w; }

  void SetHeight(int h) { this->texHeight = h; }

  GLuint GetTexture() const { return texture; }

  int GetWidth() const { return texWidth; }

  int GetHeight() const { return texHeight; }

 private:
  GLuint texture;
  int texWidth;
  int texHeight;
};

inline bool operator==(const ExternalTexture &left,
                       const ExternalTexture &right) {
  return left.GetTexture() == right.GetTexture() &&
         left.GetWidth() == right.GetWidth() &&
         left.GetHeight() == right.GetHeight();
}

inline bool operator!=(const ExternalTexture &left,
                       const ExternalTexture &right) {
  return !(left == right);
}
}  // namespace gvrvideo
#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_EXTERNAL_TEXTURE_H_

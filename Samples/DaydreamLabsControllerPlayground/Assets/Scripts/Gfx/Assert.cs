// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace GVR.Gfx {
  public static class Assert {
    /// <summary>
    /// Does the provided MonoBehaviour have a Camera Component attached?
    /// </summary>
    public static bool AttachedCamera(MonoBehaviour behaviour) {
      if (behaviour.GetComponent<Camera>() == null) {
        behaviour.enabled = false;
        Debug.LogError(
            string.Format("Component '{0}' requires a Camera Component to be attached.", behaviour.name),
            behaviour);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Is the specified object not null?
    /// </summary>
    public static bool NotNull<T>(MonoBehaviour behaviour, T obj, string fieldIdentifier) {
      if (obj.Equals(null)) {
        behaviour.enabled = false;
        Debug.LogError(
            string.Format("Component '{0}' requires that the field or property '{1}' of type '{2}' not be null.",
                          behaviour.name, fieldIdentifier, typeof(T).ToString()),
            behaviour);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Does the current platform support RenderTextures?
    /// </summary>
    public static bool SupportsRenderTextures(MonoBehaviour behaviour) {
      if (SystemInfo.supportsRenderTextures == false) {
        behaviour.enabled = false;
        Debug.LogError("Platform does not support RenderTextures.", behaviour);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Does the current platform support any of the provided RenderTexture Formats?
    /// </summary>
    public static bool SupportsRenderTextureFormats(MonoBehaviour behaviour, params RenderTextureFormat[] formats) {
      for (int i = 0; i < formats.Length; i++) {
        if (SystemInfo.SupportsRenderTextureFormat(formats[i]) == false) {
          behaviour.enabled = false;
          Debug.LogError(
              string.Format("Platform does not support RenderTexture Format '{0}'.", formats[i]),
              behaviour);
          return false;
        }
      }
      return true;
    }

  }
}

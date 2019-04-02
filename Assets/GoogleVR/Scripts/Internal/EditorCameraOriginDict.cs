//-----------------------------------------------------------------------
// <copyright file="EditorCameraOriginDict.cs" company="Google LLC.">
// Copyright 2019 Google LLC. All rights reserved.
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
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Gvr.Internal
{
    /// <summary>A class module for providing Camera origin position and rotation.</summary>
    /// <remarks>
    /// A VR camera's position and rotation at start-time should be the center of its experience.
    /// To accomodate this, this module saves their initial localPosition and localRotation as
    /// localOriginPosition and localOriginRotation, allowing those to be applied every Update.
    /// </remarks>
    public class EditorCameraOriginDict : MonoBehaviour
    {
#if UNITY_EDITOR
        public struct LocalOrigin
        {
            public Vector3 position;
            public Quaternion rotation;

            public LocalOrigin(Vector3 p, Quaternion r)
            {
                position = p;
                rotation = r;
            }
        }

        /// <summary>A lazy getter for the Camera's origin.</summary>
        public static LocalOrigin Get(Camera camera)
        {
            if (!cameraOrigins.ContainsKey(camera)) {
                Set(camera);
            }
            return cameraOrigins[camera];
        }

        private static Dictionary<Camera, LocalOrigin> cameraOrigins
                = new Dictionary<Camera, LocalOrigin>();

        private void Awake()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                Set(cam);
            }
        }

        private static void Set(Camera camera)
        {
            cameraOrigins[camera] = new LocalOrigin(camera.transform.localPosition,
                                                    camera.transform.localRotation);
        }
#endif
    }
}

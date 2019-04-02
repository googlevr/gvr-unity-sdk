//-----------------------------------------------------------------------
// <copyright file="GvrBetaHeadsetProvider.cs" company="Google LLC">
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

/// @cond
namespace Gvr.Internal
{
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using GoogleVR.Beta;

    /// <summary>Daydream headset beta provider.</summary>
    public class GvrBetaHeadsetProvider
    {
        private static IntPtr seeThroughConfig;

        private static void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr gvrContext = GvrSettings.GetValidGvrNativePtrOrLogError();
            seeThroughConfig = gvr_beta_see_through_config_create(gvrContext);
#endif
        }

        public static void SetSeeThroughConfig(GvrBetaSeeThroughCameraMode cameraMode,
                                                GvrBetaSeeThroughSceneType sceneType)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (seeThroughConfig == IntPtr.Zero)
            {
                Initialize();
            }

            gvr_beta_see_through_config_set_camera_mode(seeThroughConfig, (int)cameraMode);
            gvr_beta_see_through_config_set_scene_type(seeThroughConfig, (int)sceneType);

            IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
            gvr_beta_set_see_through_config(gvrContextPtr, seeThroughConfig);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern IntPtr gvr_beta_see_through_config_create(IntPtr gvr_context);

        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern void gvr_beta_see_through_config_set_camera_mode(
            IntPtr config,
            int camera_mode);

        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern void gvr_beta_see_through_config_set_scene_type(
            IntPtr config,
            int scene_type);

        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern void gvr_beta_set_see_through_config(
            IntPtr context,
            IntPtr config);

#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }
}

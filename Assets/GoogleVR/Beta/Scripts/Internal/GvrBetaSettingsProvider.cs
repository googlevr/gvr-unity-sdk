//-----------------------------------------------------------------------
// <copyright file="GvrBetaSettingsProvider.cs" company="Google Inc.">
// Copyright 2018 Google Inc. All rights reserved.
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
    public class GvrBetaSettingsProvider
    {
        public static bool IsFeatureSupported(GvrBetaFeature feature)
        {
            bool supported = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
            supported = AndroidNativeHeadsetProvider.gvr_is_feature_supported(gvrContextPtr, (int)feature);
#endif
            return supported;
        }

        public static bool IsFeatureEnabled(GvrBetaFeature feature)
        {
            bool enabled = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
            IntPtr gvrUserPrefsPtr = GvrSettings.gvr_get_user_prefs(gvrContextPtr);
            enabled = gvr_user_prefs_is_feature_enabled(gvrUserPrefsPtr, (int)feature);
#endif
            return enabled;
        }

        public static void RequestFeatures(GvrBetaFeature[] requiredFeatures,
                                           GvrBetaFeature[] optionalFeatures)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr gvrContextPtr = GvrSettings.GetValidGvrNativePtrOrLogError();
            int reqLen = requiredFeatures != null ? requiredFeatures.Length : 0;
            int optLen = optionalFeatures != null ? optionalFeatures.Length : 0;

            gvr_request_features(gvrContextPtr,
                                 FeaturesToIds(requiredFeatures), reqLen,
                                 FeaturesToIds(optionalFeatures), optLen,
                                 IntPtr.Zero);
#endif
        }

        private static int[] FeaturesToIds(GvrBetaFeature[] features)
        {
            if (features == null)
            {
                return null;
            }

            int[] ids = Array.ConvertAll<GvrBetaFeature, int>(
                features,
                delegate(GvrBetaFeature value)
                {
                    return (int)value;
                });
            return ids;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern bool gvr_user_prefs_is_feature_enabled(IntPtr user_prefs,
                                                                     int runtime_feature);

        [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
        private static extern void gvr_request_features(IntPtr gvr_context,
                                                        int[] required_features,
                                                        int required_count,
                                                        int[] optional_features,
                                                        int optional_count,
                                                        IntPtr on_complete_activity);

#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }
}

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

/// Daydream Beta API.  This API surface is for experimental purposes and may
/// change or be removed in any future release without forewarning.
namespace GoogleVR.Beta {
  using UnityEngine;
  using System;
  using System.Runtime.InteropServices;

  /// Daydream controller Beta API.
  public class GvrBetaControllerInput {

    /// Daydream Controller configurations.
    // enum gvr_beta_controller_configuration_type
    public enum Configuration {
      /// Used when controller configuration is unknown.
      Unknown = 0,
      /// Daydream controller.
      Is3DoF = 1,
      /// Daydream 6DoF controller.
      Is6DoF = 2,
    }

    /// Tracking status flags for Daydream 6DoF controllers. Although enum values are
    /// in practice currently mutually exclusive, returned values should be tested
    /// using bitwise tests.
    // enum gvr_beta_controller_tracking_status_flags
    public enum TrackingStatusFlags {
      /// The controller's tracking status is unknown.
      Unknown = (1 << 0),
      /// The controller is tracking in 6DoF mode.
      Nominal = (1 << 1),
      /// The controller is occluded. Controller reports 3DoF pose
      /// and last known position in this case.
      Occluded = (1 << 2),
      /// The controller is out of field of view. Controller
      /// reports 3DoF pose and last known position in this case.
      OutOfFov = (1 << 3),
    }

    // Gets the current controller configuration.  Controller configuration will only
    // change while the app is paused.
    internal static Configuration GetConfigurationType(int device) {
#if UNITY_ANDROID && !UNITY_EDITOR
      return (Configuration)GvrShimUnity_betaControllerGetConfigurationType(device);
#else
      return Configuration.Is3DoF;
#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }

    // Gets the tracking status flags for the given controller.
    internal static TrackingStatusFlags GetTrackingStatusFlags(int device) {
#if UNITY_ANDROID && !UNITY_EDITOR
      return (TrackingStatusFlags)GvrShimUnity_betaControllerStateGetTrackingStatus(device);
#else
      return TrackingStatusFlags.Nominal;
#endif  // UNITY_ANDROID && !UNITY_EDITOR
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string shimDllName = GvrActivityHelper.GVR_SHIM_DLL_NAME;

    [DllImport(shimDllName)]
    private static extern int GvrShimUnity_betaControllerGetConfigurationType(int device);

    [DllImport(shimDllName)]
    private static extern int GvrShimUnity_betaControllerStateGetTrackingStatus(int device);
#endif  // UNITY_ANDROID && !UNITY_EDITOR
  }

  /// Class extension for GvrControllerInputDevice to add beta tracking status getter.
  public static class GvrControllerInputDeviceExtension {

    /// Gets a controller's configuration type.  Controller configuration will only
    /// change while the app is paused.
    public static GvrBetaControllerInput.Configuration GetConfigurationType(this GvrControllerInputDevice device) {
      return GvrBetaControllerInput.GetConfigurationType(device.IsDominantHand ? 0 : 1);
    }

    /// Gets a controller's tracking status. Although TrackingStatusFlags values are
    /// in practice currently mutually exclusive, returned values should be tested
    /// using bitwise tests.
    public static GvrBetaControllerInput.TrackingStatusFlags GetTrackingStatusFlags(this GvrControllerInputDevice device) {
      return GvrBetaControllerInput.GetTrackingStatusFlags(device.IsDominantHand ? 0 : 1);
    }
  }
}

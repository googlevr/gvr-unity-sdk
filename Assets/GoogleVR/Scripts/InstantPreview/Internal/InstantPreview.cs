//-----------------------------------------------------------------------
// <copyright file="InstantPreview.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
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
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace Gvr.Internal
{
    /// <summary>A class module for handling Instant Preview.</summary>
    /// <remarks><para>
    /// Handles connecting to the Instant Preview Unity plugin.
    /// </para><para>
    /// Serves as an interface for retrieving many headset-oriented fields.
    /// </para><para>
    /// Streams video data to the Instant Preview Unity plugin.
    /// </para></remarks>
    [HelpURL("https://developers.google.com/vr/unity/reference/class/InstantPreview")]
    public class InstantPreview : MonoBehaviour
    {
        /// <summary>
        /// Gets whether Instant Preview is currently connected to and running on a remote device.
        /// </summary>
        public static bool IsActive
        {
            get
            {
#if UNITY_EDITOR
                return Gvr.Internal.InstantPreview.Instance != null
                       && Gvr.Internal.InstantPreview.Instance.enabled
                       && Gvr.Internal.InstantPreview.Instance.IsCurrentlyConnected;
#else
                return false;
#endif // UNITY_EDITOR
            }
        }

        private const string NoDevicesFoundAdbResult = "error: no devices/emulators found";

        /// <summary>Gets or sets this singleton's instance.</summary>
        internal static InstantPreview Instance { get; set; }

        /// <summary>The .dll filename of the Instant Preview Unity plugin.</summary>
        internal const string dllName = "instant_preview_unity_plugin";

        /// <summary>Video resolutions for streaming to the connected device.</summary>
        public enum Resolutions : int
        {
            /// A high-resolution image.
            Big,

            /// A regular-resolution image.
            Regular,

            /// A window-sized image.
            WindowSized,
        }

        struct ResolutionSize
        {
            public int width;
            public int height;
        }

        /// <summary>Resolution of video stream.</summary>
        /// <remarks>Higher = more expensive / better visual quality.</remarks>
        [Tooltip("Resolution of video stream. Higher = more expensive / better visual quality.")]
        public Resolutions OutputResolution = Resolutions.Big;

        /// <summary>Options for anti-aliasing sampling.</summary>
        public enum MultisampleCounts
        {
            /// <summary>Take one sample per frame.</summary>
            One,

            /// <summary>Take two samples per frame.</summary>
            Two,

            /// <summary>Take four samples per frame.</summary>
            Four,

            /// <summary>Take eight samples per frame.</summary>
            Eight,
        }

        /// <summary>Anti-aliasing for video preview.</summary>
        /// <remarks>Higher = more expensive / better visual quality.</remarks>
        [Tooltip("Anti-aliasing for video preview. Higher = more expensive / better visual quality.")]
        public MultisampleCounts MultisampleCount = MultisampleCounts.One;

        /// <summary>Bit rates for video codec streaming.</summary>
        public enum BitRates
        {
            /// <summary>A bit rate of 2000kb/s.  The lowest available bit rate.</summary>
            _2000,

            /// <summary>A bit rate of 4000kb/s.</summary>
            _4000,

            /// <summary>A bit rate of 8000kb/s.</summary>
            _8000,

            /// <summary>A bit rate of 16000kb/s.</summary>
            _16000,

            /// <summary>A bit rate of 24000kb/s.</summary>
            _24000,

            /// <summary>A bit rate of 32000kb/s.  The highest available bit rate</summary>
            _32000,
        }

        /// <summary>Video codec streaming bit rate.</summary>
        /// <remarks>Higher = more expensive / better visual quality.</remarks>
        [Tooltip("Video codec streaming bit rate. Higher = more expensive / better visual quality.")]
        public BitRates BitRate = BitRates._16000;

        /// <summary>
        /// If true, installs the Instant Preview app if it isn't found on the connected device.
        /// </summary>
        [Tooltip("Installs the Instant Preview app if it isn't found on the connected device.")]
        public bool InstallApkOnRun = true;

        /// <summary>An .apk file containing the Instant Preview app.</summary>
        /// <remarks>
        /// Will be installed on connected devices which don't already have it, or which have an
        /// out-of-date version.
        /// </remarks>
        public UnityEngine.Object InstantPreviewApk;

        struct UnityRect
        {
            public float right;
            public float left;
            public float top;
            public float bottom;
        }

        struct UnityEyeViews
        {
            public Matrix4x4 leftEyePose;
            public Matrix4x4 rightEyePose;
            public UnityRect leftEyeViewSize;
            public UnityRect rightEyeViewSize;
        }

        /// <summary>A Unity C#-compliant wrapper for a boolean.</summary>
        /// <remarks><para>
        /// This is also defined on the Instant Preview plugin in native C++.
        /// </para><para>
        /// If `isValid` is `false`, `value` should be ignored.
        /// </para></remarks>
        public struct UnityBoolAtom
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool value;
            [MarshalAs(UnmanagedType.I1)]
            public bool isValid;
        }

        /// <summary>A Unity C#-compliant wrapper for a float.</summary>
        /// <remarks><para>
        /// This is also defined on the Instant Preview plugin in native C++.
        /// </para><para>
        /// If `isValid` is `false`, `value` should be ignored.
        /// </para></remarks>
        public struct UnityFloatAtom
        {
            public float value;
            [MarshalAs(UnmanagedType.I1)]
            public bool isValid;
        }

        /// <summary>A Unity C#-compliant wrapper for an integer.</summary>
        /// <remarks><para>
        /// This is also defined on the Instant Preview plugin in native C++.
        /// </para><para>
        /// If `isValid` is `false`, `value` should be ignored.
        /// </para></remarks>
        public struct UnityIntAtom
        {
            public int value;
            [MarshalAs(UnmanagedType.I1)]
            public bool isValid;
        }

        /// <summary>A Unity C#-compliant wrapper for a 4x4 matrix.</summary>
        /// <remarks><para>
        /// This is also defined on the Instant Preview plugin in native C++.
        /// </para><para>
        /// If `isValid` is `false`, `value` should be ignored.
        /// </para></remarks>
        public struct UnityGvrMat4fAtom
        {
            public Matrix4x4 value;
            [MarshalAs(UnmanagedType.I1)]
            public bool isValid;
        }

        struct UnityGlobalGvrProperties
        {
            internal UnityBoolAtom supportsPositionalHeadTracking;
            internal UnityBoolAtom supportsSeeThrough;
            internal UnityFloatAtom floorHeight;
            internal UnityGvrMat4fAtom recenterTransform;
            internal UnityIntAtom safetyRegionType;
            internal UnityFloatAtom safetyCylinderEnterRadius;
            internal UnityFloatAtom safetyCylinderExitRadius;
        }

        /// <summary>GVR Event Types. Associated with ephemeral (one-frame-long) events.</summary>
        public enum GvrEventType
        {
            /// <summary>A default value. If this is seen, something has gone wrong.</summary>
            GVR_EVENT_NONE,

            /// <summary>Indicates a recenter event.</summary>
            /// <remarks>
            /// This should always be accompanied by a`GvrRecenterEventType` providing additional
            /// details.
            /// </remarks>
            GVR_EVENT_RECENTER,

            /// <summary>Indicates that the safety region has been exited.</summary>
            GVR_EVENT_SAFETY_REGION_EXIT,

            /// <summary>Indicates that the safety region has been entered.</summary>
            GVR_EVENT_SAFETY_REGION_ENTER,

            /// <summary>Indicates that head tracking has resumed.</summary>
            GVR_EVENT_HEAD_TRACKING_RESUMED,

            /// <summary>Indicates that head tracking has paused.</summary>
            GVR_EVENT_HEAD_TRACKING_PAUSED,
        }

        /// <summary>
        /// GVR Recenter Event Types. Provides details for `GvrEventType.GVR_EVENT_RECENTER`.
        /// </summary>
        public enum GvrRecenterEventType
        {
            /// <summary>A default value. If this is seen, something has gone wrong.</summary>
            GVR_RECENTER_EVENT_NONE,

            /// <summary>Indicates that the recenter event occurred because of a restart.</summary>
            GVR_RECENTER_EVENT_RESTART,

            /// <summary>Indicates that the recenter event occurred because of a realign.</summary>
            GVR_RECENTER_EVENT_ALIGNED,

            /// <summary>
            /// Indicates that the recenter event occurred because the headset was donned.
            /// </summary>
            GVR_RECENTER_EVENT_DON,
        }

        /// <summary>
        /// Accompanies a `GvrEventType.GVR_EVENT_RECENTER`.  Provides additional details about
        /// the recenter event.
        /// </summary>
        internal struct UnityGvrRecenterEventData
        {
            /// <summary>The type of recenter event.</summary>
            internal GvrRecenterEventType recenter_type;

            /// <summary>Recenter event flags.</summary>
            internal uint recenter_event_flags;

            /// <summary>The offset from the initial start-space.</summary>
            /// <remarks>
            /// Allows the app to continue to stream from the same point of reference, while keeping
            /// the post-recenter orientation consistent.
            /// </summary>
            internal Matrix4x4 start_space_from_tracking_space_transform;
        }

        /// <summary>GVR event details, received any time an event occurs.</summary>
        /// <remarks>This is also defined on the Instant Preview plugin in native C++.</remarks>
        internal struct UnityGvrEvent
        {
            /// <summary>Timestamp in nanoseconds.</summary>
            internal long timestamp;

            /// <summary>The event type.</summary>
            internal GvrEventType type;

            /// <summary>The event's flags.</summary>
            internal uint flags;

            /// <summary>Additional details on recenter events.</summary>
            /// <remarks>Not null if and only if event type is `GVR_EVENT_RECENTER`.</remarks>
            internal UnityGvrRecenterEventData gvr_recenter_event_data;
        }

        /// <summary>GVR User Preferences.</summary>
        /// <remarks>
        /// Associated with options set by the user in the Daydream app.</remarks>
        [System.Serializable]
        public struct UnityGvrUserPreferences
        {
            /// <summary>The user's handedness preference.</summary>
            public GvrSettings.UserPrefsHandedness handedness;
        }

        /// <summary>If `true`, overrides user preferences received from remote device with those
        /// set in the InstantPreview editor inspector.</summary>
        [Tooltip("Override user preferences from remote device with Editor preferences.")]
        public bool overrideDeviceUserPrefs = false;

        [HideInInspector]
        /// <summary>The User Preferences to use if `overrideDeviceUserPrefs` is `true`.</summary>
        public UnityGvrUserPreferences editorUserPrefs;

        /// <summary>The User Preferences to use if `overrideDeviceUserPrefs` is `false`.</summary>
        public UnityGvrUserPreferences deviceUserPrefs { get; private set; }

#if UNITY_EDITOR
        private readonly string[] RequiredAndroidFeatures = {
            "feature:android.software.vr.mode",
            "feature:android.hardware.vr.high_performance",
        };

        static ResolutionSize[] resolutionSizes = new ResolutionSize[]
        {
            new ResolutionSize()
            {
                // ResolutionSize.Big
                width = 2560, height = 1440,
            },
            new ResolutionSize()
            {
                // ResolutionSize.Regular
                width = 1920, height = 1080,
            },

            // ResolutionSize.WindowSized
            new ResolutionSize(),
        };

        private static readonly int[] multisampleCounts = new int[]
        {
            1,    // MultisampleCounts.One
            2,    // MultisampleCounts.Two
            4,    // MultisampleCounts.Four
            8,    // MultisampleCounts.Eight
        };

        private static readonly int[] bitRates = new int[]
        {
            2000,     // BitRates._2000
            4000,     // BitRates._4000
            8000,     // BitRates._8000
            16000,    // BitRates._16000
            24000,    // BitRates._24000
            32000,    // BitRates._32000
        };

        [DllImport(dllName)]
        private static extern bool IsConnected();

        [DllImport(dllName)]
        private static extern bool GetHeadPose(out Matrix4x4 pose, out double timestamp);

        [DllImport(dllName)]
        private static extern bool GetEyeViews(out UnityEyeViews outputEyeViews);

        [DllImport(dllName)]
        private static extern bool GetGlobalGvrProperties(ref UnityGlobalGvrProperties outputProperties);

        [DllImport(dllName)]
        private static extern bool GetGvrEvent(ref UnityGvrEvent outputEvent);

        [DllImport(dllName)]
        private static extern bool GetGvrUserPreferences(ref UnityGvrUserPreferences outputUserPrefs);

        [DllImport(dllName)]
        private static extern IntPtr GetRenderEventFunc();

        [DllImport(dllName)]
        private static extern void SendFrame(IntPtr renderTexture, ref Matrix4x4 pose, double timestamp, int bitRate);

        [DllImport(dllName)]
        private static extern void GetVersionString(StringBuilder dest, uint n);

        /// <summary>
        /// Gets whether this module is currently connected to a running Instant Preview app.
        /// </summary>
        /// <value>
        /// Value `true` if this module is currently connected to a running Instant Preview app,
        /// `false` otherwise.
        /// </value>
        public bool IsCurrentlyConnected
        {
            get { return connected; }
        }

        private IntPtr renderEventFunc;
        private RenderTexture renderTexture;
        private Matrix4x4 headPose = Matrix4x4.identity;
        private double timestamp;

        private class EyeCamera
        {
            public Camera leftEyeCamera = null;
            public Camera rightEyeCamera = null;
        }

        Dictionary<Camera, EyeCamera> eyeCameras = new Dictionary<Camera, EyeCamera>();
        List<Camera> camerasLastFrame = new List<Camera>();
        private bool connected;

        /// <summary>Gets whether the connected device supports positional tracking.</summary>
        public UnityBoolAtom supportsPositionalHeadTracking { get; private set; }

        /// <summary>Gets whether the connected device supports see-through mode.</summary>
        public UnityBoolAtom supportsSeeThrough { get; private set; }

        /// <summary>Gets the current height the headset is off its perceived floor.</summary>
        /// <remarks>The `value` is valid only if `floorHeight.isValid == true`.</remarks>
        public UnityFloatAtom floorHeight { get; private set; }

        /// <summary>Gets the last recenter's offset transform.</summary>
        /// <remarks>The `value` is valid only if `recenterTransform.isValid == true`.</remarks>
        public UnityGvrMat4fAtom recenterTransform { get; private set; }

        /// <summary>Gets the type of the safety region.</summary>
        /// <remarks>The `value` is valid only if `safetyRegionType.isValid == true`.</remarks>
        public UnityIntAtom safetyRegionType { get; private set; }

        /// <summary>Gets the reentry radius of a cylindrical safety region.</summary>
        /// <remarks><para>
        /// Entering the safety cylinder means stepping close enough to its center to suppress an
        /// active warning.
        /// </para><para>
        /// The `value` is valid only if `safetyCylinderEnterRadius.isValid == true`.
        /// </para></remarks>
        public UnityFloatAtom safetyCylinderEnterRadius { get; private set; }

        /// <summary>Gets the exit radius of a cylindrical safety region.</summary>
        /// <remarks><para>
        /// Exiting the safety cylinder means stepping far enough from its center to prompt a
        /// warning.
        /// </para><para>
        /// The `value` is valid only if `safetyCylinderExitRadius.isValid == true`.
        /// </para></remarks>
        public UnityFloatAtom safetyCylinderExitRadius { get; private set; }

        /// <summary>Gets the user's handedness preference.</summary>
        public GvrSettings.UserPrefsHandedness handedness
        {
            get
            {
                if (overrideDeviceUserPrefs)
                {
                    return editorUserPrefs.handedness;
                }
                else
                {
                    return deviceUserPrefs.handedness;
                }
            }
        }

        /// <summary>A queue for active GVR events.</summary>
        /// <remarks>
        /// This is used because events only trigger for one frame, and in some high-CPU edge cases
        /// the Instant Preview stream may run at a different speed than the Unity player.
        /// Because they are stored on a queue, the Unity player can guarantee it will eventually
        /// trigger them all.</remarks>
        internal Queue<UnityGvrEvent> events = new Queue<UnityGvrEvent>();

        void Awake()
        {
            renderEventFunc = GetRenderEventFunc();

            if (Instance != null)
            {
                Destroy(gameObject);
                gameObject.SetActive(false);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Gets local version name and prints it out.
            var sb = new StringBuilder(256);
            GetVersionString(sb, (uint)sb.Capacity);
            var pluginIPVersionName = sb.ToString();

            // Tries to install Instant Preview apk if set to do so.
            if (InstallApkOnRun)
            {
                // Early outs if set to install but the apk can't be found.
                if (InstantPreviewApk == null)
                {
                    Debug.LogError("Trying to install Instant Preview apk but reference to InstantPreview.apk is broken.");
                    return;
                }

                // Gets the apk path and installs it on a separate thread.
                var apkPath = Path.GetFullPath(UnityEditor.AssetDatabase.GetAssetPath(InstantPreviewApk));
                if (File.Exists(apkPath))
                {
                    new Thread(
                    () =>
                    {
                        string output;
                        string errors;
                        string deviceIPVersionName = null;
                        string unityAPKVersionName = null;

                        // Gets version of apk installed on device (to remove, if dated).
                        RunCommand(InstantPreviewHelper.adbPath,
                                "shell dumpsys package com.google.instantpreview | grep versionName",
                                out output, out errors);

                        // Early outs if no device is connected.
                        if (string.Compare(errors, NoDevicesFoundAdbResult) == 0)
                        {
                            return;
                        }

                        if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(errors))
                        {
                            deviceIPVersionName = output.Substring(output.IndexOf('=') + 1);
                        }

                        // Ensures connected device is Daydream-compatible before continuing.
                        RunCommand(InstantPreviewHelper.adbPath,
                                "shell pm list features", out output, out errors);
                        foreach (string feature in RequiredAndroidFeatures)
                        {
                            if (output.IndexOf(feature) == -1)
                            {
                                Debug.Log(
                                    "Instant Preview disabled; device is not Daydream-compatible.");
                                return;
                            }
                        }

                        // Prints errors and exits on failure.
                        if (!string.IsNullOrEmpty(errors))
                        {
                            Debug.LogError(errors);
                            return;
                        }

                        Debug.Log("Instant Preview Version: " + pluginIPVersionName);

                        // Gets version of Unity's local .apk version (to install, if needed).
                        RunCommand(InstantPreviewHelper.aaptPath,
                                string.Format("dump badging {0}", apkPath),
                                out output, out errors);
                        if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(errors))
                        {
                            string unityAPKVersionInfoDump = output;

                            // Finds (versionName='), captures any alphaNumerics separated by periods, and selects them until (').
                            System.Text.RegularExpressions.Match unityAPKVersionNameRegex = Regex.Match(
                                unityAPKVersionInfoDump, "versionName=\'([^']*)\'");
                            if (unityAPKVersionNameRegex.Groups.Count > 1)
                            {
                                unityAPKVersionName = unityAPKVersionNameRegex.Groups[1].Value;
                            }
                            else
                            {
                                Debug.Log(string.Format("Failed to extract version from: {0}", unityAPKVersionInfoDump));
                            }
                        }
                        else
                        {
                            Debug.Log(string.Format("Failed to run: {0} dump badging {1}", InstantPreviewHelper.aaptPath, apkPath));
                        }

                        // Determines if Unity plugin and Unity's local .apk IP file are the same version, and exits if not.
                        if (pluginIPVersionName != unityAPKVersionName)
                        {
                            Debug.LogWarning(string.Format(
                                "Unity Instant Preview plugin version ({0}) does not match Unity Instant Preview .apk version ({1})."
                                + "  This may cause unpredictable behavior.",
                                pluginIPVersionName, unityAPKVersionName));
                        }

                        // Determines if app is installed, and installs it if not.
                        if (deviceIPVersionName != unityAPKVersionName)
                        {
                            if (deviceIPVersionName == null)
                            {
                                Debug.Log(string.Format(
                                "Instant Preview: app not found on device, attempting to install it from {0}.",
                                apkPath));
                            }
                            else
                            {
                                Debug.Log(string.Format(
                                "Instant Preview: installed version \"{0}\" does not match local version \"{1}\", attempting upgrade.",
                                deviceIPVersionName, unityAPKVersionName));
                            }

                            RunCommand(InstantPreviewHelper.adbPath,
                                        string.Format("uninstall com.google.instantpreview", apkPath),
                                        out output, out errors);

                            RunCommand(InstantPreviewHelper.adbPath,
                                        string.Format("install \"{0}\"", apkPath),
                                        out output, out errors);

                            // Prints any output from trying to install.
                            if (!string.IsNullOrEmpty(output))
                            {
                                Debug.Log(output);
                            }

                            if (!string.IsNullOrEmpty(errors))
                            {
                                if (string.Equals(errors.Trim(), "Success"))
                                {
                                    Debug.Log("Successfully installed Instant Preview app.");
                                }
                                else
                                {
                                    Debug.LogError(errors);
                                }
                            }
                        }

                        StartInstantPreviewActivity(InstantPreviewHelper.adbPath);
                    }).Start();
                }
            }
            else
            {
                Debug.Log("Instant Preview Version: " + pluginIPVersionName);
                new Thread(() =>
                {
                    StartInstantPreviewActivity(InstantPreviewHelper.adbPath);
                }).Start();
            }
        }

        void UpdateCamera(Camera camera)
        {
            EyeCamera eyeCamera;

            if (!eyeCameras.TryGetValue(camera, out eyeCamera))
            {
                return;
            }

            if (connected)
            {
                if (GetHeadPose(out headPose, out timestamp))
                {
                    SetEditorEmulatorsEnabled(false);
                    camera.transform.localRotation =
                        Quaternion.LookRotation(headPose.GetColumn(2), headPose.GetColumn(1)) *
                        EditorCameraOriginDict.Get(camera).rotation;
                    camera.transform.localPosition =
                        camera.transform.localRotation * headPose.GetRow(3) * -1 +
                        EditorCameraOriginDict.Get(camera).position;
                }
                else
                {
                    SetEditorEmulatorsEnabled(true);
                }

                var eyeViews = new UnityEyeViews();
                if (GetEyeViews(out eyeViews))
                {
                    SetTransformFromMatrix(eyeCamera.leftEyeCamera.gameObject.transform, eyeViews.leftEyePose);
                    SetTransformFromMatrix(eyeCamera.rightEyeCamera.gameObject.transform, eyeViews.rightEyePose);

                    var near = Camera.main.nearClipPlane;
                    var far = Camera.main.farClipPlane;
                    eyeCamera.leftEyeCamera.projectionMatrix =
                        PerspectiveMatrixFromUnityRect(eyeViews.leftEyeViewSize, near, far);
                    eyeCamera.rightEyeCamera.projectionMatrix =
                        PerspectiveMatrixFromUnityRect(eyeViews.rightEyeViewSize, near, far);

                    bool multisampleChanged = multisampleCounts[(int)MultisampleCount] != renderTexture.antiAliasing;

                    // Adjusts render texture size.
                    if (OutputResolution != Resolutions.WindowSized)
                    {
                        var selectedResolutionSize = resolutionSizes[(int)OutputResolution];
                        if (selectedResolutionSize.width != renderTexture.width ||
                            selectedResolutionSize.height != renderTexture.height ||
                            multisampleChanged)
                        {
                            ResizeRenderTexture(selectedResolutionSize.width, selectedResolutionSize.height);
                        }
                    }
                    else
                    {
                        // OutputResolution == Resolutions.WindowSized
                        var screenAspectRatio = (float)Screen.width / Screen.height;

                        var eyeViewsWidth =
                            -eyeViews.leftEyeViewSize.left +
                            eyeViews.leftEyeViewSize.right +
                            -eyeViews.rightEyeViewSize.left +
                            eyeViews.rightEyeViewSize.right;
                        var eyeViewsHeight =
                            eyeViews.leftEyeViewSize.top +
                            -eyeViews.leftEyeViewSize.bottom;
                        if (eyeViewsHeight > 0f)
                        {
                            int renderTextureHeight;
                            int renderTextureWidth;
                            var eyeViewsAspectRatio = eyeViewsWidth / eyeViewsHeight;
                            if (screenAspectRatio > eyeViewsAspectRatio)
                            {
                                renderTextureHeight = Screen.height;
                                renderTextureWidth = (int)(Screen.height * eyeViewsAspectRatio);
                            }
                            else
                            {
                                renderTextureWidth = Screen.width;
                                renderTextureHeight = (int)(Screen.width / eyeViewsAspectRatio);
                            }

                            renderTextureWidth = renderTextureWidth & ~0x3;
                            renderTextureHeight = renderTextureHeight & ~0x3;

                            if (multisampleChanged ||
                                renderTexture.width != renderTextureWidth ||
                                renderTexture.height != renderTextureHeight)
                            {
                                ResizeRenderTexture(renderTextureWidth, renderTextureHeight);
                            }
                        }
                    }
                }
            }
            else
            {
                // !connected
                SetEditorEmulatorsEnabled(true);

                if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
                {
                    ResizeRenderTexture(Screen.width, Screen.height);
                }
            }
        }

        void UpdateProperties()
        {
            UnityGlobalGvrProperties unityGlobalGvrProperties = new UnityGlobalGvrProperties();
            if (GetGlobalGvrProperties(ref unityGlobalGvrProperties))
            {
                supportsPositionalHeadTracking
                    = unityGlobalGvrProperties.supportsPositionalHeadTracking;

                supportsSeeThrough = unityGlobalGvrProperties.supportsSeeThrough;
                floorHeight = unityGlobalGvrProperties.floorHeight;
                recenterTransform = unityGlobalGvrProperties.recenterTransform;
                safetyRegionType = unityGlobalGvrProperties.safetyRegionType;
                safetyCylinderEnterRadius = unityGlobalGvrProperties.safetyCylinderEnterRadius;
                safetyCylinderExitRadius = unityGlobalGvrProperties.safetyCylinderExitRadius;
            }
        }

        void UpdateEvents()
        {
            UnityGvrEvent unityGvrEvent = new UnityGvrEvent();
            while (GetGvrEvent(ref unityGvrEvent))
            {
                events.Enqueue(unityGvrEvent);
            }
        }

        void UpdateUserPreferences()
        {
            UnityGvrUserPreferences unityGvrUserPreferences = new UnityGvrUserPreferences();
            if (GetGvrUserPreferences(ref unityGvrUserPreferences))
            {
                deviceUserPrefs = unityGvrUserPreferences;
            }
        }

        void Update()
        {
            if (!EnsureCameras())
            {
                return;
            }

            var newConnectionState = IsConnected();
            if (connected && !newConnectionState)
            {
                Debug.Log("Disconnected from Instant Preview.");
            }
            else if (!connected && newConnectionState)
            {
                Debug.Log("Connected to Instant Preview.");
            }

            connected = newConnectionState;

            foreach (KeyValuePair<Camera, EyeCamera> eyeCamera in eyeCameras)
            {
                UpdateCamera(eyeCamera.Key);
            }

            UpdateProperties();
            UpdateEvents();
            UpdateUserPreferences();
        }

        void OnPostRender()
        {
            if (connected && renderTexture != null)
            {
                var nativeTexturePtr = renderTexture.GetNativeTexturePtr();
                SendFrame(nativeTexturePtr, ref headPose, timestamp, bitRates[(int)BitRate]);
                GL.IssuePluginEvent(renderEventFunc, 69);
            }
        }

        void EnsureCamera(Camera camera)
        {
            // renderTexture might still be null so this creates and assigns it.
            if (renderTexture == null)
            {
                if (OutputResolution != Resolutions.WindowSized)
                {
                    var selectedResolutionSize = resolutionSizes[(int)OutputResolution];
                    ResizeRenderTexture(selectedResolutionSize.width, selectedResolutionSize.height);
                }
                else
                {
                    ResizeRenderTexture(Screen.width, Screen.height);
                }
            }

            EyeCamera eyeCamera;

            if (!eyeCameras.TryGetValue(camera, out eyeCamera))
            {
                eyeCamera = new EyeCamera();
                eyeCameras.Add(camera, eyeCamera);
            }

            EnsureEyeCamera(camera, ":Instant Preview Left", new Rect(0.0f, 0.0f, 0.5f, 1.0f), ref eyeCamera.leftEyeCamera);
            EnsureEyeCamera(camera, ":Instant Preview Right", new Rect(0.5f, 0.0f, 0.5f, 1.0f), ref eyeCamera.rightEyeCamera);
        }

        private void CheckRemoveCameras(List<Camera> cameras)
        {
            // Any cameras that were here last frame and not here this frame need removing from eyeCameras.
            foreach (Camera oldCamera in camerasLastFrame)
            {
                if (!cameras.Contains(oldCamera))
                {
                    // Destroys the eye cameras.
                    EyeCamera curEyeCamera;
                    if (eyeCameras.TryGetValue(oldCamera, out curEyeCamera))
                    {
                        if (curEyeCamera.leftEyeCamera != null)
                        {
                            Destroy(curEyeCamera.leftEyeCamera.gameObject);
                        }

                        if (curEyeCamera.rightEyeCamera != null)
                        {
                            Destroy(curEyeCamera.rightEyeCamera.gameObject);
                        }
                    }

                    // Removes eye camera entry from dictionary.
                    eyeCameras.Remove(oldCamera);
                }
            }

            camerasLastFrame = cameras;
        }

        bool EnsureCameras()
        {
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                // If the main camera doesn't exist, destroys a remaining render texture and exits.
                if (renderTexture != null)
                {
                    Destroy(renderTexture);
                    renderTexture = null;
                }

                return false;
            }

            // Find all the cameras and make sure any non-Instant Preview cameras have left/right eyes attached.
            var cameras = new List<Camera>(ValidCameras());
            CheckRemoveCameras(cameras);

            // Now go and make sure that all cameras that are to be driven by Instant Preview have the correct setup.
            foreach (Camera camera in cameras)
            {
                // Skips the Instant Preview camera, which is used for a
                // convenience preview.
                if (camera.gameObject == gameObject)
                {
                    continue;
                }

                EnsureCamera(camera);
            }

            return true;
        }

        void EnsureEyeCamera(Camera mainCamera, String eyeCameraName, Rect rect, ref Camera eyeCamera)
        {
            // Creates eye camera object if it doesn't exist.
            if (eyeCamera == null)
            {
                var eyeCameraObject = new GameObject(mainCamera.gameObject.name + eyeCameraName);
                eyeCamera = eyeCameraObject.AddComponent<Camera>();
                eyeCameraObject.transform.SetParent(mainCamera.gameObject.transform, false);
            }

            eyeCamera.CopyFrom(mainCamera);
            eyeCamera.rect = rect;
            eyeCamera.targetTexture = renderTexture;

            // Match child camera's skyboxes to main camera.
            Skybox monoCameraSkybox = mainCamera.gameObject.GetComponent<Skybox>();
            Skybox customSkybox = eyeCamera.GetComponent<Skybox>();
            if (monoCameraSkybox != null)
            {
                if (customSkybox == null)
                {
                    customSkybox = eyeCamera.gameObject.AddComponent<Skybox>();
                }

                customSkybox.material = monoCameraSkybox.material;
            }
            else if (customSkybox != null)
            {
                Destroy(customSkybox);
            }
        }

        void ResizeRenderTexture(int width, int height)
        {
            var newRenderTexture = new RenderTexture(width, height, 16);
            newRenderTexture.antiAliasing = multisampleCounts[(int)MultisampleCount];
            if (renderTexture != null)
            {
                foreach (KeyValuePair<Camera, EyeCamera> camera in eyeCameras)
                {
                    if (camera.Value.leftEyeCamera != null)
                    {
                        camera.Value.leftEyeCamera.targetTexture = null;
                    }

                    if (camera.Value.rightEyeCamera != null)
                    {
                        camera.Value.rightEyeCamera.targetTexture = null;
                    }
                }

                Destroy(renderTexture);
            }

            renderTexture = newRenderTexture;
        }

        private static void SetEditorEmulatorsEnabled(bool enabled)
        {
            foreach (var editorEmulator in FindObjectsOfType<GvrEditorEmulator>())
            {
                editorEmulator.enabled = enabled;
            }
        }

        private static Matrix4x4 PerspectiveMatrixFromUnityRect(UnityRect rect, float near, float far)
        {
            if (rect.left == rect.right || rect.bottom == rect.top || near == far ||
                near <= 0f || far <= 0f)
            {
                return Matrix4x4.identity;
            }

            rect.left *= near;
            rect.right *= near;
            rect.top *= near;
            rect.bottom *= near;
            var X = (2 * near) / (rect.right - rect.left);
            var Y = (2 * near) / (rect.top - rect.bottom);
            var A = (rect.right + rect.left) / (rect.right - rect.left);
            var B = (rect.top + rect.bottom) / (rect.top - rect.bottom);
            var C = (near + far) / (near - far);
            var D = (2 * near * far) / (near - far);

            var perspectiveMatrix = new Matrix4x4();
            perspectiveMatrix[0, 0] = X;
            perspectiveMatrix[0, 2] = A;
            perspectiveMatrix[1, 1] = Y;
            perspectiveMatrix[1, 2] = B;
            perspectiveMatrix[2, 2] = C;
            perspectiveMatrix[2, 3] = D;
            perspectiveMatrix[3, 2] = -1f;
            return perspectiveMatrix;
        }

        private static void SetTransformFromMatrix(Transform transform, Matrix4x4 matrix)
        {
            var position = matrix.GetRow(3);
            position.x *= -1;
            transform.localPosition = position;
            transform.localRotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        private static void StartInstantPreviewActivity(string adbPath)
        {
            string output;
            string errors;
            RunCommand(adbPath,
                       "shell am start -n com.google.instantpreview/.InstantPreviewActivity",
                       out output,
                       out errors);

            // Early outs if no device is connected.
            if (string.Compare(errors, NoDevicesFoundAdbResult) == 0)
            {
                return;
            }
        }

        private static void RunCommand(string fileName, string arguments, out string output, out string errors)
        {
            using (var process = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(fileName, arguments);
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                process.OutputDataReceived += (o, ef) => outputBuilder.AppendLine(ef.Data);
                process.ErrorDataReceived += (o, ef) => errorBuilder.AppendLine(ef.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();

                // Trims the output strings to make comparison easier.
                output = outputBuilder.ToString().Trim();
                errors = errorBuilder.ToString().Trim();
            }
        }

        // Gets active, stereo, non-eye cameras in the scene.
        private IEnumerable<Camera> ValidCameras()
        {
            foreach (var camera in Camera.allCameras)
            {
                if (!camera.enabled || camera.stereoTargetEye == StereoTargetEyeMask.None)
                {
                    continue;
                }

                // Skips camera if it is determined to be an eye camera.
                var parent = camera.transform.parent;
                if (parent != null)
                {
                    var parentCamera = parent.GetComponent<Camera>();
                    if (parentCamera != null)
                    {
                        EyeCamera parentEyeCamera;
                        if (eyeCameras.TryGetValue(parentCamera, out parentEyeCamera))
                        {
                            if (camera == parentEyeCamera.leftEyeCamera || camera == parentEyeCamera.rightEyeCamera)
                            {
                                continue;
                            }
                        }
                    }
                }

                yield return camera;
            }
        }
#else
        /// <summary>
        /// Gets whether this module is currently connected to a running Instant Preview app.
        /// </summary>
        /// <value>
        /// Value `true` if this module is currently connected to a running Instant Preview app,
        /// `false` otherwise.
        /// </value>
        public bool IsCurrentlyConnected
        {
            get { return false; }
        }
#endif
    }
}

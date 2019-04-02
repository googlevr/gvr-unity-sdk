//-----------------------------------------------------------------------
// <copyright file="GvrVideoPlayerTexture.cs" company="Google Inc.">
// Copyright 2016 Google Inc. All Rights Reserved.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Plays video using Exoplayer rendering it on the main texture.</summary>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrVideoPlayerTexture")]
public class GvrVideoPlayerTexture : MonoBehaviour
{
    /// <summary>Attach a text component to get some debug status info.</summary>
    public Text statusText;

    /// <summary>The type of the video.</summary>
    public VideoType videoType;

    /// <summary>The video URL.</summary>
    public string videoURL;

    /// <summary>The video content ID.</summary>
    public string videoContentID;

    /// <summary>The video provider ID.</summary>
    public string videoProviderId;

    /// <summary>The video resolution used when streaming begins.</summary>
    /// <remarks><para>
    /// For multi-rate streams like Dash and HLS, the stream used at the beginning of playback is
    /// selected such that its vertical resolution is greater than or equal to this value.
    /// </para><para>
    /// After streaming begins, the player auto-selects the highest rate stream the network
    /// connection is capable of delivering.
    /// </para></remarks>
    public VideoResolution initialResolution = VideoResolution.Highest;

    /// <summary>
    /// Value `true` indicates that the aspect ratio of the renderer needs adjusting.
    /// </summary>
    public bool adjustAspectRatio;

    /// <summary>Whether to use secure path for DRM protected video.</summary>
    public bool useSecurePath;

    private const string DLL_NAME = "gvrvideo";

#if !UNITY_ANDROID || UNITY_EDITOR
    private const string NOT_IMPLEMENTED_MSG = "Not implemented on this platform";
#endif // !UNITY_ANDROID || UNITY_EDITOR

    private static Queue<Action> executeOnMainThread = new Queue<Action>();

    /// <summary>The video player pointer used to uniquely identify the player instance.</summary>
    private IntPtr videoPlayerPtr;

    /// <summary>The video player event base.</summary>
    /// <remarks>This is added to the event id when issues events to the plugin.</remarks>
    private int videoPlayerEventBase;

    private Texture initialTexture;
    private Texture surfaceTexture;
    private float[] videoMatrixRaw;
    private Matrix4x4 videoMatrix;
    private int videoMatrixPropertyId;
    private long lastVideoTimestamp;

    private bool initialized;
    private int texWidth = 1024;
    private int texHeight = 1024;
    private long lastBufferedPosition;
    private float framecount = 0;

    private Renderer screen;

    /// <summary>The render event function.</summary>
    private IntPtr renderEventFunction;
    private bool playOnResume;

    /// <summary>List of callbacks to invoke when the video is ready.</summary>
    private List<Action<int>> onEventCallbacks;

    /// <summary>List of callbacks to invoke on exception.</summary>
    /// <remarks>The first parameter is the type of exception, the second is the message.</remarks>
    private List<Action<string, string>> onExceptionCallbacks;

    /// <summary>A delegate to be triggered by event callbacks.</summary>
    /// <param name="cbdata">An integer pointer to the Video Player.</param>
    /// <param name="eventId">The ID of the triggering event.</param>
    internal delegate void OnVideoEventCallback(IntPtr cbdata, int eventId);

    /// <summary>A delegate to be triggered by exception callbacks.</summary>
    /// <param name="type">The type of exception.</param>
    /// <param name="msg">The message associated with the exception.</param>
    /// <param name="cbdata">An integer pointer to the Video Player.</param>
    internal delegate void OnExceptionCallback(string type, string msg, IntPtr cbdata);

    /// <summary>Video type.</summary>
    public enum VideoType
    {
        /// <summary>Dynamic Adaptive Streaming over HTTP.</summary>
        Dash = 0,

        /// <summary>HTTP Live Streaming.</summary>
        HLS = 2,

        /// <summary>Another video type.</summary>
        Other = 3,
    }

    /// <summary>
    /// Video resolutions which can be selected as the initial resolution when streaming begins.
    /// See `initialResolution` for more information.
    /// </summary>
    public enum VideoResolution
    {
        /// <summary>The lowest available resolution.</summary>
        Lowest = 1,

        /// <summary>720p resolution.</summary>
        _720 = 720,

        /// <summary>1080p resolution.</summary>
        _1080 = 1080,

        /// <summary>2K resolution.</summary>
        _2048 = 2048,

        /// <summary>4K resolution.</summary>
        Highest = 4096,
    }

    /// <summary>Video player state.</summary>
    public enum VideoPlayerState
    {
        /// <summary>An idle state.</summary>
        Idle = 1,

        /// <summary>Preparing for video.</summary>
        Preparing = 2,

        /// <summary>Buffering video.</summary>
        Buffering = 3,

        /// <summary>Ready for video.</summary>
        Ready = 4,

        /// <summary>Done with video.</summary>
        Ended = 5,
    }

    /// <summary>Video events.</summary>
    public enum VideoEvents
    {
        /// <summary>Indicates that video is ready.</summary>
        VideoReady = 1,

        /// <summary>Indicates that the video playback should begin.</summary>
        VideoStartPlayback = 2,

        /// <summary>Indicates that the video format has changed.</summary>
        VideoFormatChanged = 3,

        /// <summary>Indicates that the video surface has been set.</summary>
        VideoSurfaceSet = 4,

        /// <summary>Indicates that the video size has changed.</summary>
        VideoSizeChanged = 5,
    }

    /// <summary>Stereo mode formats.</summary>
    public enum StereoMode
    {
        /// <summary>An error-state indicating that no value has been set.</summary>
        NoValue = -1,

        /// <summary>Mono sound.</summary>
        Mono = 0,

        /// <summary>Top-and-bottom stereo sound.</summary>
        TopBottom = 1,

        /// <summary>Left-and-right stereo sound.</summary>
        LeftRight = 2,
    }

    /// <summary>Plugin render commands.</summary>
    /// <remarks>
    /// These are added to the eventbase for the specific player object and issued to the plugin.
    /// </remarks>
    private enum RenderCommand
    {
        None = -1,
        InitializePlayer = 0,
        UpdateVideo = 1,
        RenderMono = 2,
        RenderLeftEye = 3,
        RenderRightEye = 4,
        Shutdown = 5,
    }

    /// <summary>Gets a value indicating whether the video is ready to be played.</summary>
    /// <value>Value `true` if the video is ready to be played.</value>
    public bool VideoReady
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero && IsVideoReady(videoPlayerPtr);
        }
    }

    /// <summary>Gets or sets the current position in seconds in the video stream.</summary>
    /// <value>The current position in seconds in the video stream.</value>
    public long CurrentPosition
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero ? GetCurrentPosition(videoPlayerPtr) : 0;
        }

        set
        {
            // If the position is being set to 0, reset the framecount as well.
            // This allows the texture swapping to work correctly at the beginning
            // of the stream.
            if (value == 0)
            {
                framecount = 0;
            }

            SetCurrentPosition(videoPlayerPtr, value);
        }
    }

    /// <summary>Gets the duration in seconds of the video stream.</summary>
    /// <value>The duration in seconds of the video stream.</value>
    public long VideoDuration
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetDuration(videoPlayerPtr) : 0; }
    }

    /// <summary>Gets the buffered position in seconds of the video stream.</summary>
    /// <value>The buffered position in seconds of the video stream.</value>
    public long BufferedPosition
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetBufferedPosition(videoPlayerPtr) : 0; }
    }

    /// <summary>Gets the buffered percentage of the video stream.</summary>
    /// <value>The buffered percentage of the video stream.</value>
    public int BufferedPercentage
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetBufferedPercentage(videoPlayerPtr) : 0; }
    }

    /// <summary>Gets a value indicating whether the video is paused.</summary>
    /// <value>Value `true` if the video is paused, `false` otherwise.</value>
    public bool IsPaused
    {
        get
        {
            return !initialized || videoPlayerPtr == IntPtr.Zero || IsVideoPaused(videoPlayerPtr);
        }
    }

    /// <summary>Gets the player state.</summary>
    /// <value>The player state.</value>
    public VideoPlayerState PlayerState
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero ?
                (VideoPlayerState)GetPlayerState(videoPlayerPtr) : VideoPlayerState.Idle;
        }
    }

    /// <summary>Gets the maximum volume value which can be set.</summary>
    /// <value>The maximum volume value which can be set.</value>
    public int MaxVolume
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetMaxVolume(videoPlayerPtr) : 0; }
    }

    /// <summary>Gets or sets the current volume setting.</summary>
    /// <value>The current volume setting.</value>
    public int CurrentVolume
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetCurrentVolume(videoPlayerPtr) : 0; }
        set { SetCurrentVolume(value); }
    }

    /// <summary>Gets the current stereo mode.</summary>
    /// <value>The current stereo mode.</value>
    public StereoMode CurrentStereoMode
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero ?
                (StereoMode)GetStereoMode(videoPlayerPtr) : StereoMode.NoValue;
        }
    }

    /// <summary>Gets a value indicating whether the video has a projection.</summary>
    /// <value>Value `true` if the video has a projection, `false` otherwise.</value>
    public bool HasProjection
    {
        get { return videoPlayerPtr != IntPtr.Zero ? HasProjectionData(videoPlayerPtr) : false; }
    }

    /// <summary>Gets or sets the renderer for the video texture.</summary>
    /// <value>The renderer for the video texture.</value>
    public Renderer Screen
    {
        get
        {
            return screen;
        }

        set
        {
            if (screen == value)
            {
                return;
            }

            if (screen != null && initialTexture != null)
            {
                screen.sharedMaterial.mainTexture = initialTexture;
            }

            screen = value;

            if (screen != null)
            {
                initialTexture = screen.sharedMaterial.mainTexture;
            }
        }
    }

    /// <summary>Gets the current frame texture.</summary>
    /// <value>The current frame texture.</value>
    public Texture CurrentFrameTexture
    {
        get { return surfaceTexture; }
    }

    /// <summary>Gets the width of the texture.</summary>
    /// <value>The width of the texture.</value>
    public int Width
    {
        get { return texWidth; }
    }

    /// <summary>Gets the height of the texture.</summary>
    /// <value>The height of the texture.</value>
    public int Height
    {
        get { return texHeight; }
    }

    /// <summary>Gets the aspect ratio of the texture.</summary>
    /// <value>The aspect ratio of the texture.</value>
    public float AspectRatio
    {
        get
        {
            if (texHeight == 0)
            {
                return 0.0f;
            }

            return (float)texWidth / (float)texHeight;
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Keep public so we can check for the dll being present at runtime.

    /// <summary>Creates the video player.</summary>
    /// <returns>A pointer to the newly-created video player.</returns>
    [DllImport(DLL_NAME)]
    public static extern IntPtr CreateVideoPlayer();

    // Keep public so we can check for the dll being present at runtime.

    /// <summary>Destroys the video player.</summary>
    /// <param name="videoPlayerPtr">A pointer to the video player.</param>
    [DllImport(DLL_NAME)]
    public static extern void DestroyVideoPlayer(IntPtr videoPlayerPtr);
#else
    /// @cond
    /// <summary>Creates the Video Player.</summary>
    /// <remarks>Make this public so we can test the loading of the DLL.</remarks>
    /// <returns>An integer pointer to the Video Player.</returns>
    public static IntPtr CreateVideoPlayer()
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    /// @endcond
    /// @cond
    /// <summary>Destroys the Video Player.</summary>
    /// <remarks>Make this public so we can test the loading of the DLL.</remarks>
    /// <param name="videoPlayerPtr">A pointer to the video player.</param>
    public static void DestroyVideoPlayer(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    /// @endcond
#endif  // UNITY_ANDROID && !UNITY_EDITOR

    /// <summary>Sets the display texture.</summary>
    /// <param name="texture">
    /// Texture to display.  If `null`, the initial texture of the renderer is used.
    /// </param>
    public void SetDisplayTexture(Texture texture)
    {
        if (texture == null)
        {
            texture = initialTexture;
        }

        if (texture == null)
        {
            return;
        }

        if (screen != null)
        {
            screen.sharedMaterial.mainTexture = texture;
        }
    }

    /// <summary>Cleans up the current video player and texture.</summary>
    public void CleanupVideo()
    {
        Debug.Log("Cleaning Up video!");
        if (videoPlayerPtr != IntPtr.Zero)
        {
            DestroyVideoPlayer(videoPlayerPtr);
            videoPlayerPtr = IntPtr.Zero;
        }

        if (surfaceTexture != null)
        {
            Destroy(surfaceTexture);
            surfaceTexture = null;
        }

        if (screen != null)
        {
            screen.sharedMaterial.mainTexture = initialTexture;
        }
    }

    /// <summary>
    /// Reinitializes the current video player or creates one if there is no player.
    /// </summary>
    public void ReInitializeVideo()
    {
        if (screen != null)
        {
            screen.sharedMaterial.mainTexture = initialTexture;
        }

        if (videoPlayerPtr == IntPtr.Zero)
        {
            CreatePlayer();
        }

        Init();
    }

    /// <summary>Resets the video player.</summary>
    public void RestartVideo()
    {
        SetOnVideoEventCallback(OnRestartVideoEvent);

        string theUrl = ProcessURL();

        InitVideoPlayer(videoPlayerPtr, (int)videoType, theUrl,
            videoContentID,
            videoProviderId,
            useSecurePath,
            true);
        framecount = 0;
        lastVideoTimestamp = -1;
    }

    /// <summary>Set the volume level.</summary>
    /// <param name="val">The new volume level.</param>
    public void SetCurrentVolume(int val)
    {
        SetCurrentVolume(videoPlayerPtr, val);
    }

    /// <summary>Initialize the video player.</summary>
    /// <returns>Returns `true` if successful.</returns>
    public bool Init()
    {
        if (initialized)
        {
            Debug.Log("Skipping initialization: video player already loaded");
            return true;
        }

        if (videoURL == null || videoURL.Length == 0)
        {
            Debug.LogError("Cannot initialize with null videoURL");
            return false;
        }

        videoURL = videoURL == null ? "" : videoURL.Trim();
        videoContentID = videoContentID == null ? "" : videoContentID.Trim();
        videoProviderId = videoProviderId == null ? "" : videoProviderId.Trim();

        SetInitialResolution(videoPlayerPtr, (int)initialResolution);

        string theUrl = ProcessURL();
        Debug.Log("Playing " + videoType + " " + theUrl);
        Debug.Log("videoContentID = " + videoContentID);
        Debug.Log("videoProviderId = " + videoProviderId);
        videoPlayerPtr = InitVideoPlayer(videoPlayerPtr, (int)videoType, theUrl,
            videoContentID, videoProviderId,
            useSecurePath, false);
        IssuePlayerEvent(RenderCommand.InitializePlayer);
        initialized = true;
        framecount = 0;
        lastVideoTimestamp = -1;
        return videoPlayerPtr != IntPtr.Zero;
    }

    /// <summary>Play the video.</summary>
    /// <returns>Returns `true` if the video plays successfully, `false` otherwise.</returns>
    public bool Play()
    {
        if (!initialized)
        {
            Init();
        }

        if (videoPlayerPtr != IntPtr.Zero && IsVideoReady(videoPlayerPtr))
        {
            return PlayVideo(videoPlayerPtr) == 0;
        }
        else
        {
            Debug.LogError("Video player not ready to Play!");
            return false;
        }
    }

    /// <summary>Pauses video playback.</summary>
    /// <returns>Returns `true` if the operation is successful, `false` otherwise.</returns>
    public bool Pause()
    {
        if (!initialized)
        {
            Init();
        }

        if (VideoReady)
        {
            return PauseVideo(videoPlayerPtr) == 0;
        }
        else
        {
            Debug.LogError("Video player not ready to Pause!");
            return false;
        }
    }

    /// <summary>Removes the callback for exceptions.</summary>
    /// <param name="callback">The callback to remove.</param>
    public void RemoveOnVideoEventCallback(Action<int> callback)
    {
        if (onEventCallbacks != null)
        {
            onEventCallbacks.Remove(callback);
        }
    }

    /// <summary>Sets the callback for video events.</summary>
    /// <param name="callback">The callback to set for video events.</param>
    public void SetOnVideoEventCallback(Action<int> callback)
    {
        if (onEventCallbacks == null)
        {
            onEventCallbacks = new List<Action<int>>();
        }

        onEventCallbacks.Add(callback);
        SetOnVideoEventCallback(videoPlayerPtr, InternalOnVideoEventCallback,
            ToIntPtr(this));
    }

    /// <summary>Sets the callback for exceptions.</summary>
    /// <param name="callback">The callback to set.</param>
    public void SetOnExceptionCallback(Action<string, string> callback)
    {
        if (onExceptionCallbacks == null)
        {
            onExceptionCallbacks = new List<Action<string, string>>();
            SetOnExceptionCallback(videoPlayerPtr, InternalOnExceptionCallback,
                ToIntPtr(this));
        }

        onExceptionCallbacks.Add(callback);
    }

    /// <summary>Generates an integer pointer for a given object.</summary>
    /// <param name="obj">The object to generate an integer pointer for.</param>
    /// <returns>An integer pointer.</returns>
    internal static IntPtr ToIntPtr(System.Object obj)
    {
        GCHandle handle = GCHandle.Alloc(obj);
        return GCHandle.ToIntPtr(handle);
    }

    /// <summary>Fires off a video event.</summary>
    /// <param name="eventId">The ID of the event to fire.</param>
    internal void FireVideoEvent(int eventId)
    {
        if (onEventCallbacks == null)
        {
            return;
        }

        // Copy the collection so the callbacks can remove themselves from the list.
        Action<int>[] cblist = onEventCallbacks.ToArray();
        foreach (Action<int> cb in cblist)
        {
            try
            {
                cb(eventId);
            }
            catch (Exception e)
            {
                Debug.LogError("exception calling callback: " + e);
            }
        }
    }

    /// <summary>Fires all callbacks registered to `onExceptionCallbacks`.</summary>
    /// <param name="type">The `type` parameter for the callback.</param>
    /// <param name="msg">The `msg` parameter for the callback.</param>
    internal void FireOnException(string type, string msg)
    {
        if (onExceptionCallbacks == null)
        {
            return;
        }

        foreach (Action<string, string> cb in onExceptionCallbacks)
        {
            try
            {
                cb(type, msg);
            }
            catch (Exception e)
            {
                Debug.LogError("exception calling callback: " + e);
            }
        }
    }

    /// <summary>Processes the URL.</summary>
    /// <returns>The processed URL.</returns>
    internal string ProcessURL()
    {
        return videoURL.Replace("${Application.dataPath}", Application.dataPath);
    }

#if UNITY_ANDROID && !UNITY_EDITOR

    [DllImport(DLL_NAME)]
    private static extern IntPtr GetRenderEventFunc();

    [DllImport(DLL_NAME)]
    private static extern void SetExternalTextures(IntPtr videoPlayerPtr,
                                                   int[] texIds,
                                                   int size,
                                                   int w,
                                                   int h);

    [DllImport(DLL_NAME)]
    private static extern IntPtr GetRenderableTextureId(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetExternalSurfaceTextureId(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern void GetVideoMatrix(IntPtr videoPlayerPtr,
                                              float[] videoMatrix);

    [DllImport(DLL_NAME)]
    private static extern long GetVideoTimestampNs(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetVideoPlayerEventBase(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern IntPtr InitVideoPlayer(IntPtr videoPlayerPtr,
                                                 int videoType,
                                                 string videoURL,
                                                 string contentID,
                                                 string providerId,
                                                 bool useSecurePath,
                                                 bool useExisting);

    [DllImport(DLL_NAME)]
    private static extern void SetInitialResolution(IntPtr videoPlayerPtr,
                                                    int initialResolution);

    [DllImport(DLL_NAME)]
    private static extern int GetPlayerState(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetWidth(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetHeight(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int PlayVideo(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int PauseVideo(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern bool IsVideoReady(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern bool IsVideoPaused(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern long GetDuration(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern long GetBufferedPosition(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern long GetCurrentPosition(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern void SetCurrentPosition(IntPtr videoPlayerPtr,
                                                  long pos);

    [DllImport(DLL_NAME)]
    private static extern int GetBufferedPercentage(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetMaxVolume(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern int GetCurrentVolume(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern void SetCurrentVolume(IntPtr videoPlayerPtr,
                                                int value);

    [DllImport(DLL_NAME)]
    private static extern int GetStereoMode(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern bool HasProjectionData(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern bool SetVideoPlayerSupportClassname(
        IntPtr videoPlayerPtr,
        string classname);

    [DllImport(DLL_NAME)]
    private static extern IntPtr GetRawPlayer(IntPtr videoPlayerPtr);

    [DllImport(DLL_NAME)]
    private static extern void SetOnVideoEventCallback(IntPtr videoPlayerPtr,
                                                       OnVideoEventCallback callback,
                                                       IntPtr callback_arg);

    [DllImport(DLL_NAME)]
    private static extern void SetOnExceptionCallback(IntPtr videoPlayerPtr,
                                                      OnExceptionCallback callback,
                                                      IntPtr callback_arg);
#else

    private static IntPtr GetRenderEventFunc()
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    private static void SetExternalTextures(IntPtr videoPlayerPtr,
                                            int[] texIds,
                                            int size,
                                            int w,
                                            int h)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static IntPtr GetRenderableTextureId(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    private static int GetExternalSurfaceTextureId(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static void GetVideoMatrix(IntPtr videoPlayerPtr,
                                       float[] videoMatrix)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static long GetVideoTimestampNs(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    /// <summary>A pure-virtual method for getting a video player event base.</summary>
    /// <param name="videoPlayerPtr">A pointer to the video player.</param>
    /// <returns>An integer representing the success status.</returns>
    private static int GetVideoPlayerEventBase(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static IntPtr InitVideoPlayer(IntPtr videoPlayerPtr, int videoType,
                                          string videoURL,
                                          string contentID,
                                          string providerId,
                                          bool useSecurePath,
                                          bool useExisting)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    private static void SetInitialResolution(IntPtr videoPlayerPtr,
                                             int initialResolution)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static int GetPlayerState(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static int GetWidth(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static int GetHeight(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static int PlayVideo(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static int PauseVideo(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static bool IsVideoReady(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return false;
    }

    private static bool IsVideoPaused(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return true;
    }

    private static long GetDuration(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static long GetBufferedPosition(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static long GetCurrentPosition(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static void SetCurrentPosition(IntPtr videoPlayerPtr, long pos)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static int GetBufferedPercentage(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static int GetMaxVolume(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static int GetCurrentVolume(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return 0;
    }

    private static void SetCurrentVolume(IntPtr videoPlayerPtr, int value)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static int GetStereoMode(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return -1;
    }

    private static bool HasProjectionData(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return false;
    }

    private static bool SetVideoPlayerSupportClassname(IntPtr videoPlayerPtr,
                                                       string classname)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return false;
    }

    private static IntPtr GetRawPlayer(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    private static void SetOnVideoEventCallback(IntPtr videoPlayerPtr,
                                                OnVideoEventCallback callback,
                                                IntPtr callback_arg)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    private static void SetOnExceptionCallback(IntPtr videoPlayerPtr,
                                               OnExceptionCallback callback,
                                               IntPtr callback_arg)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }
#endif  // UNITY_ANDROID && !UNITY_EDITOR

    [AOT.MonoPInvokeCallback(typeof(OnVideoEventCallback))]
    private static void InternalOnVideoEventCallback(IntPtr cbdata, int eventId)
    {
        if (cbdata == IntPtr.Zero)
        {
            return;
        }

        GvrVideoPlayerTexture player;
        var gcHandle = GCHandle.FromIntPtr(cbdata);
        try
        {
            player = (GvrVideoPlayerTexture)gcHandle.Target;
        }
        catch (InvalidCastException e)
        {
            Debug.LogError("GC Handle pointed to unexpected type: " +
            gcHandle.Target + ". Expected " +
            typeof(GvrVideoPlayerTexture));
            throw e;
        }

        if (player != null)
        {
            executeOnMainThread.Enqueue(() => player.FireVideoEvent(eventId));
        }
    }

    [AOT.MonoPInvokeCallback(typeof(OnExceptionCallback))]
    private static void InternalOnExceptionCallback(string type, string msg,
                                            IntPtr cbdata)
    {
        if (cbdata == IntPtr.Zero)
        {
            return;
        }

        GvrVideoPlayerTexture player;
        var gcHandle = GCHandle.FromIntPtr(cbdata);
        try
        {
            player = (GvrVideoPlayerTexture)gcHandle.Target;
        }
        catch (InvalidCastException e)
        {
            Debug.LogError("GC Handle pointed to unexpected type: " +
            gcHandle.Target + ". Expected " +
            typeof(GvrVideoPlayerTexture));
            throw e;
        }

        if (player != null)
        {
            executeOnMainThread.Enqueue(() => player.FireOnException(type, msg));
        }
    }

    // Create the video player instance and the event base id.
    private void Awake()
    {
        videoMatrixRaw = new float[16];
        videoMatrixPropertyId = Shader.PropertyToID("video_matrix");

        // Defaults the Screen to the Renderer component on the same object as this script.
        // The Screen can also be set explicitly.
        Screen = GetComponent<Renderer>();

        CreatePlayer();
    }

    private void CreatePlayer()
    {
        videoPlayerPtr = CreateVideoPlayer();
        videoPlayerEventBase = GetVideoPlayerEventBase(videoPlayerPtr);
        Debug.Log(" -- " + gameObject.name + " created with base " +
        videoPlayerEventBase);

        SetOnVideoEventCallback((eventId) =>
        {
            Debug.Log("------------- E V E N T " + eventId + " -----------------");
            UpdateStatusText();
        });

        SetOnExceptionCallback((type, msg) =>
        {
            Debug.LogError("Exception: " + type + ": " + msg);
        });

        initialized = false;
    }

    private void OnDisable()
    {
        if (videoPlayerPtr != IntPtr.Zero)
        {
            if (GetPlayerState(videoPlayerPtr) == (int)VideoPlayerState.Ready)
            {
                PauseVideo(videoPlayerPtr);
            }
        }
    }

    private void OnDestroy()
    {
        CleanupVideo();
    }

    private void OnApplicationPause(bool bPause)
    {
        if (videoPlayerPtr != IntPtr.Zero)
        {
            if (bPause)
            {
                playOnResume = !IsPaused;
                PauseVideo(videoPlayerPtr);
            }
            else
            {
                if (playOnResume)
                {
                    PlayVideo(videoPlayerPtr);
                }
            }
        }
    }

    private void UpdateMaterial()
    {
        // Don't render if not initialized.
        if (videoPlayerPtr == IntPtr.Zero)
        {
            return;
        }

        texWidth = GetWidth(videoPlayerPtr);
        texHeight = GetHeight(videoPlayerPtr);

        int externalTextureId = GetExternalSurfaceTextureId(videoPlayerPtr);
        if (surfaceTexture != null
            && surfaceTexture.GetNativeTexturePtr().ToInt32() != externalTextureId)
        {
            Destroy(surfaceTexture);
            surfaceTexture = null;
        }

        if (surfaceTexture == null && externalTextureId != 0)
        {
            Debug.Log("Creating external texture with surface texture id " + externalTextureId);

            // Size of this texture doesn't really matter and can change on the fly anyway.
            surfaceTexture = Texture2D.CreateExternalTexture(4, 4, TextureFormat.RGBA32,
                false, false, new System.IntPtr(externalTextureId));
        }

        if (surfaceTexture == null)
        {
            return;
        }

        // Don't swap the textures if the video ended.
        if (PlayerState == VideoPlayerState.Ended)
        {
            return;
        }

        if (screen == null)
        {
            Debug.LogError("GvrVideoPlayerTexture: No screen to display the video is set.");
            return;
        }

        if (screen != null)
        {
            // Unity may build new a new material instance when assigning
            // material.x which can lead to duplicating materials each frame
            // whereas using the shared material will modify the original material.
            // Update the material's texture if it is different.
            if (screen.sharedMaterial.mainTexture == null ||
                screen.sharedMaterial.mainTexture.GetNativeTexturePtr() !=
                    surfaceTexture.GetNativeTexturePtr())
            {
                screen.sharedMaterial.mainTexture = surfaceTexture;
            }

            screen.sharedMaterial.SetMatrix(videoMatrixPropertyId, videoMatrix);
        }
    }

    private void OnRestartVideoEvent(int eventId)
    {
        if (eventId == (int)VideoEvents.VideoReady)
        {
            Debug.Log("Restarting video complete.");
            RemoveOnVideoEventCallback(OnRestartVideoEvent);
        }
    }

    /// <summary>Adjusts the aspect ratio.</summary>
    /// <remarks>
    /// This adjusts the transform scale to match the aspect ratio of the texture.
    /// </remarks>
    private void AdjustAspectRatio()
    {
        float aspectRatio = AspectRatio;
        if (aspectRatio == 0.0f)
        {
            return;
        }

        // set the y scale based on the x value
        Vector3 newscale = transform.localScale;
        newscale.y = Mathf.Min(newscale.y, newscale.x / aspectRatio);

        transform.localScale = newscale;
    }

    private void UpdateStatusText()
    {
        float fps = CurrentPosition > 0 ?
            (framecount / (CurrentPosition / 1000f)) : CurrentPosition;

        string status = texWidth + " x " + texHeight + " buffer: " +
                        (BufferedPosition / 1000) + " " + PlayerState + " fps: " + fps;
        if (statusText != null)
        {
            if (statusText.text != status)
            {
                statusText.text = status;
            }
        }
    }

    /// <summary>Issues the player event.</summary>
    /// <param name="evt">The event to send to the video player instance.</param>
    private void IssuePlayerEvent(RenderCommand evt)
    {
        if (renderEventFunction == IntPtr.Zero)
        {
            renderEventFunction = GetRenderEventFunc();
        }

        if (renderEventFunction == IntPtr.Zero || evt == RenderCommand.None)
        {
            Debug.LogError("Attempt to IssuePlayerEvent before renderEventFunction ready.");
            return;
        }

        GL.IssuePluginEvent(renderEventFunction, videoPlayerEventBase + (int)evt);
    }

    private void Update()
    {
        while (executeOnMainThread.Count > 0)
        {
            executeOnMainThread.Dequeue().Invoke();
        }

        if (VideoReady)
        {
            IssuePlayerEvent(RenderCommand.UpdateVideo);
            GetVideoMatrix(videoPlayerPtr, videoMatrixRaw);
            videoMatrix = GvrMathHelpers.ConvertFloatArrayToMatrix(videoMatrixRaw);
            long vidTimestamp = GetVideoTimestampNs(videoPlayerPtr);
            if (vidTimestamp != lastVideoTimestamp)
            {
                framecount++;
            }

            lastVideoTimestamp = vidTimestamp;

            UpdateMaterial();

            if (adjustAspectRatio)
            {
                AdjustAspectRatio();
            }

            if ((int)framecount % 30 == 0)
            {
                UpdateStatusText();
            }

            long bp = BufferedPosition;
            if (bp != lastBufferedPosition)
            {
                lastBufferedPosition = bp;
                UpdateStatusText();
            }
        }
    }
}

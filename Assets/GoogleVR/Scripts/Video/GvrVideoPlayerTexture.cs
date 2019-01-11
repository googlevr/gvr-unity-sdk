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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

/// <summary>
/// Plays video using Exoplayer rendering it on the main texture.
/// </summary>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrVideoPlayerTexture")]
public class GvrVideoPlayerTexture : MonoBehaviour
{
    /// <summary>
    /// The video player pointer used to uniquely identify the player instance.
    /// </summary>
    private IntPtr videoPlayerPtr;

    /// <summary>
    /// The video player event base.
    /// </summary>
    /// <remarks>This is added to the event id when issues events to
    ///     the plugin.
    /// </remarks>
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

    /// <summary>
    /// The render event function.
    /// </summary>
    private IntPtr renderEventFunction;
    private bool playOnResume;

    /// <summary>List of callbacks to invoke when the video is ready.</summary>
    private List<Action<int>> onEventCallbacks;

    /// <summary>List of callbacks to invoke on exception.</summary>
    /// <remarks>The first parameter is the type of exception,
    ///     the second is the message.
    /// </remarks>
    private List<Action<string, string>> onExceptionCallbacks;

    private readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    /// <summary>Attach a text component to get some debug status info.</summary>
    public Text statusText;

    /// <summary>
    /// Video type.
    /// </summary>
    public enum VideoType
    {
        Dash = 0,
        HLS = 2,
        Other = 3,
    }

    /// <summary>
    /// Video resolutions which can be selected as the initial resolution when streaming begins.
    /// See `initialResolution` for more information.
    /// </summary>
    public enum VideoResolution
    {
        Lowest = 1,
        _720 = 720,
        _1080 = 1080,
        _2048 = 2048,
        Highest = 4096,
    }

    /// <summary>
    /// Video player state.
    /// </summary>
    public enum VideoPlayerState
    {
        Idle = 1,
        Preparing = 2,
        Buffering = 3,
        Ready = 4,
        Ended = 5,
    }

    /// <summary>Video events</summary>
    public enum VideoEvents
    {
        VideoReady = 1,
        VideoStartPlayback = 2,
        VideoFormatChanged = 3,
        VideoSurfaceSet = 4,
        VideoSizeChanged = 5,
    }

    /// <summary>Stereo mode formats.</summary>
    public enum StereoMode
    {
        NoValue = -1,
        Mono = 0,
        TopBottom = 1,
        LeftRight = 2,
    }

    /// <summary>
    /// Plugin render commands.
    /// </summary>
    /// <remarks>
    /// These are added to the eventbase for the specific player object and
    ///   issued to the plugin.
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

    /// <summary>
    /// The type of the video.
    /// </summary>
    public VideoType videoType;

    /// <summary>
    /// The video URL.
    /// </summary>
    public string videoURL;

    /// <summary>
    /// The video content ID.
    /// </summary>
    public string videoContentID;

    /// <summary>
    /// The video provider ID.
    /// </summary>
    public string videoProviderId;

    /// <summary>The video resolution used when streaming begins.</summary>
    /// <remarks>For multi-rate streams like Dash and HLS, the stream used at
    ///  the beginning of playback is selected such that its vertical
    ///  resolution is greater than or equal to this value.  After streaming
    ///  begins, the player auto-selects the highest rate stream the network
    ///  connection is capable of delivering.
    /// </remarks>
    public VideoResolution initialResolution = VideoResolution.Highest;

    /// <summary>
    /// True for adjusting the aspect ratio of the renderer.
    /// </summary>
    public bool adjustAspectRatio;

    /// <summary>
    /// The use secure path for DRM protected video.
    /// </summary>
    public bool useSecurePath;

    /// <summary>
    /// Returns true when the video is ready to be played.
    /// </summary>
    public bool VideoReady
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero && IsVideoReady(videoPlayerPtr);
        }
    }

    /// <summary>
    /// The current position in seconds in the video stream.
    /// </summary>
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

    /// <summary>
    /// The duration in seconds of the video stream.
    /// </summary>
    public long VideoDuration
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetDuration(videoPlayerPtr) : 0; }
    }

    /// <summary>
    /// The buffered position in seconds of the video stream.
    /// </summary>
    public long BufferedPosition
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetBufferedPosition(videoPlayerPtr) : 0; }
    }

    /// <summary>
    /// The buffered percentage of the video stream.
    /// </summary>
    public int BufferedPercentage
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetBufferedPercentage(videoPlayerPtr) : 0; }
    }

    /// <summary>
    /// True if the video is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return !initialized || videoPlayerPtr == IntPtr.Zero || IsVideoPaused(videoPlayerPtr); }
    }

    /// <summary>
    /// Returns the player state.
    /// </summary>
    public VideoPlayerState PlayerState
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero ? (VideoPlayerState)GetPlayerState(videoPlayerPtr) : VideoPlayerState.Idle;
        }
    }

    /// <summary>
    /// Returns the maximum volume value which can be set.
    /// </summary>
    public int MaxVolume
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetMaxVolume(videoPlayerPtr) : 0; }
    }

    /// <summary>
    /// Returns the current volume setting.
    /// </summary>
    public int CurrentVolume
    {
        get { return videoPlayerPtr != IntPtr.Zero ? GetCurrentVolume(videoPlayerPtr) : 0; }
        set { SetCurrentVolume(value); }
    }

    /// <summary>
    /// Returns the current stereo mode.
    /// </summary>
    public StereoMode CurrentStereoMode
    {
        get
        {
            return videoPlayerPtr != IntPtr.Zero ? (StereoMode)GetStereoMode(videoPlayerPtr) : StereoMode.NoValue;
        }
    }

    /// <summary>
    /// Returns true if the video has a projection.
    /// </summary>
    public bool HasProjection
    {
        get { return videoPlayerPtr != IntPtr.Zero ? HasProjectionData(videoPlayerPtr) : false; }
    }

    /// <summary>
    /// The renderer for the video texture.
    /// </summary>
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

    /// <summary>
    /// Returns the current frame texture.
    /// </summary>
    public Texture CurrentFrameTexture
    {
        get { return surfaceTexture; }
    }

    /// <summary>
    /// Returns the width of the texture.
    /// </summary>
    public int Width
    {
        get { return texWidth; }
    }

    /// <summary>
    /// Returns the height of the texture.
    /// </summary>
    public int Height
    {
        get { return texHeight; }
    }

    /// <summary>
    /// Returns the aspect ratio of the texture.
    /// </summary>
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

    /// Create the video player instance and the event base id.
    void Awake()
    {
        videoMatrixRaw = new float[16];
        videoMatrixPropertyId = Shader.PropertyToID("video_matrix");

        // Defaults the Screen to the Renderer component on the same object as this script.
        // The Screen can also be set explicitly.
        Screen = GetComponent<Renderer>();

        CreatePlayer();
    }

    void CreatePlayer()
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

    void OnDisable()
    {
        if (videoPlayerPtr != IntPtr.Zero)
        {
            if (GetPlayerState(videoPlayerPtr) == (int)VideoPlayerState.Ready)
            {
                PauseVideo(videoPlayerPtr);
            }
        }
    }

    /// <summary>
    /// Sets the display texture.
    /// </summary>
    /// <param name="texture">Texture to display.
    /// If null, the initial texture of the renderer is used.</param>
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

    /// <summary>
    /// Cleans up the current video player and texture.
    /// </summary>
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

    void OnDestroy()
    {
        CleanupVideo();
    }

    void OnApplicationPause(bool bPause)
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

    void UpdateMaterial()
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
                screen.sharedMaterial.mainTexture.GetNativeTexturePtr() != surfaceTexture.GetNativeTexturePtr())
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

    /// <summary>
    /// Resets the video player.
    /// </summary>
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

    /// <summary>Set the volume level</summary>
    public void SetCurrentVolume(int val)
    {
        SetCurrentVolume(videoPlayerPtr, val);
    }

    /// <summary>
    /// Initialize the video player.
    /// </summary>
    /// <returns>true if successful.</returns>
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

    /// <summary>
    /// Adjusts the aspect ratio.
    /// </summary>
    /// <remarks>
    /// This adjusts the transform scale to match the aspect
    ///     ratio of the texture.
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
        float fps = CurrentPosition > 0 ? (framecount / (CurrentPosition / 1000f)) : CurrentPosition;
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

    /// <summary>
    /// Issues the player event.
    /// </summary>
    /// <param name="evt">The event to send to the video player
    ///     instance.
    /// </param>
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

    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
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

    /// <summary>Removes the callback for exceptions.</summary>
    public void RemoveOnVideoEventCallback(Action<int> callback)
    {
        if (onEventCallbacks != null)
        {
            onEventCallbacks.Remove(callback);
        }
    }

    /// <summary>Sets the callback for video events.</summary>
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

    [AOT.MonoPInvokeCallback(typeof(OnVideoEventCallback))]
    static void InternalOnVideoEventCallback(IntPtr cbdata, int eventId)
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
            ExecuteOnMainThread.Enqueue(() => player.FireVideoEvent(eventId));
        }
    }

    /// <summary>Sets the callback for exceptions.</summary>
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

    [AOT.MonoPInvokeCallback(typeof(OnExceptionCallback))]
    static void InternalOnExceptionCallback(string type, string msg,
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
            ExecuteOnMainThread.Enqueue(() => player.FireOnException(type, msg));
        }
    }

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

    internal static IntPtr ToIntPtr(System.Object obj)
    {
        GCHandle handle = GCHandle.Alloc(obj);
        return GCHandle.ToIntPtr(handle);
    }

    internal string ProcessURL()
    {
        return videoURL.Replace("${Application.dataPath}", Application.dataPath);
    }

    internal delegate void OnVideoEventCallback(IntPtr cbdata, int eventId);

    internal delegate void OnExceptionCallback(string type, string msg,
                         IntPtr cbdata);

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string dllName = "gvrvideo";

    [DllImport(dllName)]
    private static extern IntPtr GetRenderEventFunc();

    [DllImport(dllName)]
    private static extern void SetExternalTextures(IntPtr videoPlayerPtr,
                                                   int[] texIds,
                                                   int size,
                                                   int w,
                                                   int h);

    [DllImport(dllName)]
    private static extern IntPtr GetRenderableTextureId(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetExternalSurfaceTextureId(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern void GetVideoMatrix(IntPtr videoPlayerPtr,
                                              float[] videoMatrix);

    [DllImport(dllName)]
    private static extern long GetVideoTimestampNs(IntPtr videoPlayerPtr);

    // Keep public so we can check for the dll being present at runtime.
    [DllImport(dllName)]
    public static extern IntPtr CreateVideoPlayer();

    // Keep public so we can check for the dll being present at runtime.
    [DllImport(dllName)]
    public static extern void DestroyVideoPlayer(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetVideoPlayerEventBase(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern IntPtr InitVideoPlayer(IntPtr videoPlayerPtr,
                                                 int videoType,
                                                 string videoURL,
                                                 string contentID,
                                                 string providerId,
                                                 bool useSecurePath,
                                                 bool useExisting);

    [DllImport(dllName)]
    private static extern void SetInitialResolution(IntPtr videoPlayerPtr,
                                                    int initialResolution);

    [DllImport(dllName)]
    private static extern int GetPlayerState(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetWidth(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetHeight(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int PlayVideo(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int PauseVideo(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern bool IsVideoReady(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern bool IsVideoPaused(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern long GetDuration(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern long GetBufferedPosition(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern long GetCurrentPosition(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern void SetCurrentPosition(IntPtr videoPlayerPtr,
                                                  long pos);

    [DllImport(dllName)]
    private static extern int GetBufferedPercentage(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetMaxVolume(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern int GetCurrentVolume(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern void SetCurrentVolume(IntPtr videoPlayerPtr,
                                                int value);

    [DllImport(dllName)]
    private static extern int GetStereoMode(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern bool HasProjectionData(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern bool SetVideoPlayerSupportClassname(
        IntPtr videoPlayerPtr,
        string classname);

    [DllImport(dllName)]
    private static extern IntPtr GetRawPlayer(IntPtr videoPlayerPtr);

    [DllImport(dllName)]
    private static extern void SetOnVideoEventCallback(IntPtr videoPlayerPtr,
                                                       OnVideoEventCallback callback,
                                                       IntPtr callback_arg);

    [DllImport(dllName)]
    private static extern void SetOnExceptionCallback(IntPtr videoPlayerPtr,
                                                      OnExceptionCallback callback,
                                                      IntPtr callback_arg);
#else
    private const string NOT_IMPLEMENTED_MSG =
        "Not implemented on this platform";

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

    /// @cond
    /// <summary>Make this public so we can test the loading of the DLL.</summary>
    public static IntPtr CreateVideoPlayer()
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
        return IntPtr.Zero;
    }

    /// @endcond

    /// @cond
    /// <summary>Make this public so we can test the loading of the DLL.</summary>
    public static void DestroyVideoPlayer(IntPtr videoPlayerPtr)
    {
        Debug.Log(NOT_IMPLEMENTED_MSG);
    }

    /// @endcond

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
}

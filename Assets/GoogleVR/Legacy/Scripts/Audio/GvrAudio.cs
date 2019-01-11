//-----------------------------------------------------------------------
// <copyright file="GvrAudio.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/// This is the main GVR audio class that communicates with the native code implementation of
/// the audio system. Native functions of the system can only be called through this class to
/// preserve the internal system functionality. Public function calls are *not* thread-safe.
#if UNITY_2017_1_OR_NEWER
[System.Obsolete("GvrAudio is deprecated. Please upgrade to Resonance Audio (https://developers.google.com/resonance-audio/migrate).")]
#endif  // UNITY_2017_1_OR_NEWER
public static class GvrAudio
{
    /// Audio system rendering quality.
    public enum Quality
    {
        /// Stereo-only rendering
        Stereo = 0,

        /// Low quality binaural rendering (first-order HRTF)
        Low = 1,

        /// High quality binaural rendering (third-order HRTF)
        High = 2,
    }

    /// Native audio spatializer effect data.
    public enum SpatializerData
    {
        /// ID.
        Id = 0,

        /// Spatializer type.
        Type = 1,

        /// Number of input channels.
        NumChannels = 2,

        /// Soundfield channel set.
        ChannelSet = 3,

        /// Gain.
        Gain = 4,

        /// Computed distance attenuation.
        DistanceAttenuation = 5,

        /// Minimum distance for distance-based attenuation.
        MinDistance = 6,

        /// Should zero out the output buffer?
        ZeroOutput = 7,
    }

    /// Native audio spatializer type.
    public enum SpatializerType
    {
        /// 3D sound object.
        Source = 0,

        /// First-order ambisonic soundfield.
        Soundfield = 1
    }

    /// System sampling rate.
    public static int SampleRate
    {
        get { return sampleRate; }
    }

    private static int sampleRate = -1;

    /// System number of output channels.
    public static int NumChannels
    {
        get { return numChannels; }
    }

    private static int numChannels = -1;

    /// System number of frames per buffer.
    public static int FramesPerBuffer
    {
        get { return framesPerBuffer; }
    }

    private static int framesPerBuffer = -1;

    /// Initializes the audio system with the current audio configuration.
    /// @note This should only be called from the main Unity thread.
    public static void Initialize(GvrAudioListener listener, Quality quality)
    {
        if (!initialized)
        {
            // Initialize the audio system.
            AudioConfiguration config = AudioSettings.GetConfiguration();
            sampleRate = config.sampleRate;
            numChannels = (int)config.speakerMode;
            framesPerBuffer = config.dspBufferSize;
            if (numChannels != (int)AudioSpeakerMode.Stereo)
            {
                Debug.LogError("Only 'Stereo' speaker mode is supported by GVR Audio.");
                return;
            }

            Initialize((int)quality, sampleRate, numChannels, framesPerBuffer);
            listenerTransform = listener.transform;

            initialized = true;
        }
        else if (listener.transform != listenerTransform)
        {
            Debug.LogError("Only one GvrAudioListener component is allowed in the scene.");
            GvrAudioListener.Destroy(listener);
        }
    }

    /// Shuts down the audio system.
    /// @note This should only be called from the main Unity thread.
    public static void Shutdown(GvrAudioListener listener)
    {
        if (initialized && listener.transform == listenerTransform)
        {
            initialized = false;

            Shutdown();
            sampleRate = -1;
            numChannels = -1;
            framesPerBuffer = -1;
            listenerTransform = null;
        }
    }

    /// Updates the audio listener.
    /// @note This should only be called from the main Unity thread.
    public static void UpdateAudioListener(float globalGainDb, LayerMask occlusionMask)
    {
        if (initialized)
        {
            occlusionMaskValue = occlusionMask.value;
            SetListenerGain(ConvertAmplitudeFromDb(globalGainDb));
        }
    }

    /// Creates a new first-order ambisonic soundfield with a unique id.
    /// @note This should only be called from the main Unity thread.
    public static int CreateAudioSoundfield()
    {
        int soundfieldId = -1;
        if (initialized)
        {
            soundfieldId = CreateSoundfield(numFoaChannels);
        }

        return soundfieldId;
    }

    /// Updates the |soundfield| with given |id| and its properties.
    /// @note This should only be called from the main Unity thread.
    public static void UpdateAudioSoundfield(int id, GvrAudioSoundfield soundfield)
    {
        if (initialized)
        {
            SetSourceBypassRoomEffects(id, soundfield.bypassRoomEffects);
        }
    }

    /// Creates a new audio source with a unique id.
    /// @note This should only be called from the main Unity thread.
    public static int CreateAudioSource(bool hrtfEnabled)
    {
        int sourceId = -1;
        if (initialized)
        {
            sourceId = CreateSoundObject(hrtfEnabled);
        }

        return sourceId;
    }

    /// Destroys the audio source with given |id|.
    /// @note This should only be called from the main Unity thread.
    public static void DestroyAudioSource(int id)
    {
        if (initialized)
        {
            DestroySource(id);
        }
    }

    /// Updates the audio |source| with given |id| and its properties.
    /// @note This should only be called from the main Unity thread.
    public static void UpdateAudioSource(int id, GvrAudioSource source, float currentOcclusion)
    {
        if (initialized)
        {
            SetSourceBypassRoomEffects(id, source.bypassRoomEffects);
            SetSourceDirectivity(id, source.directivityAlpha, source.directivitySharpness);
            SetSourceListenerDirectivity(id, source.listenerDirectivityAlpha,
                source.listenerDirectivitySharpness);
            SetSourceOcclusionIntensity(id, currentOcclusion);
        }
    }

    /// Updates the room effects of the environment with given |room| properties.
    /// @note This should only be called from the main Unity thread.
    public static void UpdateAudioRoom(GvrAudioRoom room, bool roomEnabled)
    {
        // Update the enabled rooms list.
        if (roomEnabled)
        {
            if (!enabledRooms.Contains(room))
            {
                enabledRooms.Add(room);
            }
        }
        else
        {
            enabledRooms.Remove(room);
        }

        // Update the current room effects to be applied.
        if (initialized)
        {
            if (enabledRooms.Count > 0)
            {
                GvrAudioRoom currentRoom = enabledRooms[enabledRooms.Count - 1];
                RoomProperties roomProperties = GetRoomProperties(currentRoom);

                // Pass the room properties into a pointer.
                IntPtr roomPropertiesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(roomProperties));
                Marshal.StructureToPtr(roomProperties, roomPropertiesPtr, false);
                SetRoomProperties(roomPropertiesPtr);
                Marshal.FreeHGlobal(roomPropertiesPtr);
            }
            else
            {
                // Set the room properties to null, which will effectively disable the room effects.
                SetRoomProperties(IntPtr.Zero);
            }
        }
    }

    /// Computes the occlusion intensity of a given |source| using point source detection.
    /// @note This should only be called from the main Unity thread.
    public static float ComputeOcclusion(Transform sourceTransform)
    {
        float occlusion = 0.0f;
        if (initialized)
        {
            Vector3 listenerPosition = listenerTransform.position;
            Vector3 sourceFromListener = sourceTransform.position - listenerPosition;
            int numHits = Physics.RaycastNonAlloc(listenerPosition, sourceFromListener, occlusionHits,
                                 sourceFromListener.magnitude, occlusionMaskValue);
            for (int i = 0; i < numHits; ++i)
            {
                if (occlusionHits[i].transform != listenerTransform &&
                        occlusionHits[i].transform != sourceTransform)
                {
                    occlusion += 1.0f;
                }
            }
        }

        return occlusion;
    }

    /// Converts given |db| value to its amplitude equivalent where 'dB = 20 * log10(amplitude)'.
    public static float ConvertAmplitudeFromDb(float db)
    {
        return Mathf.Pow(10.0f, 0.05f * db);
    }

    /// Generates a set of points to draw a 2D polar pattern.
    public static Vector2[] Generate2dPolarPattern(float alpha, float order, int resolution)
    {
        Vector2[] points = new Vector2[resolution];
        float interval = 2.0f * Mathf.PI / resolution;
        for (int i = 0; i < resolution; ++i)
        {
            float theta = i * interval;

            // Magnitude |r| for |theta| in radians.
            float r = Mathf.Pow(Mathf.Abs((1 - alpha) + alpha * Mathf.Cos(theta)), order);
            points[i] = new Vector2(r * Mathf.Sin(theta), r * Mathf.Cos(theta));
        }

        return points;
    }

    /// Returns whether the listener is currently inside the given |room| boundaries.
    public static bool IsListenerInsideRoom(GvrAudioRoom room)
    {
        bool isInside = false;
        if (initialized)
        {
            Vector3 relativePosition = listenerTransform.position - room.transform.position;
            Quaternion rotationInverse = Quaternion.Inverse(room.transform.rotation);

            bounds.size = Vector3.Scale(room.transform.lossyScale, room.size);
            isInside = bounds.Contains(rotationInverse * relativePosition);
        }

        return isInside;
    }

    /// Listener directivity GUI color.
    public static readonly Color listenerDirectivityColor = 0.65f * Color.magenta;

    /// Source directivity GUI color.
    public static readonly Color sourceDirectivityColor = 0.65f * Color.blue;

    /// Minimum distance threshold between |minDistance| and |maxDistance|.
    public const float distanceEpsilon = 0.01f;

    /// Max distance limit that can be set for volume rolloff.
    public const float maxDistanceLimit = 1000000.0f;

    /// Min distance limit that can be set for volume rolloff.
    public const float minDistanceLimit = 990099.0f;

    /// Maximum allowed gain value in decibels.
    public const float maxGainDb = 24.0f;

    /// Minimum allowed gain value in decibels.
    public const float minGainDb = -24.0f;

    /// Maximum allowed reverb brightness modifier value.
    public const float maxReverbBrightness = 1.0f;

    /// Minimum allowed reverb brightness modifier value.
    public const float minReverbBrightness = -1.0f;

    /// Maximum allowed reverb time modifier value.
    public const float maxReverbTime = 3.0f;

    /// Maximum allowed reflectivity multiplier of a room surface material.
    public const float maxReflectivity = 2.0f;

    /// Maximum allowed number of raycast hits for occlusion computation per source.
    public const int maxNumOcclusionHits = 12;

    /// Source occlusion detection rate in seconds.
    public const float occlusionDetectionInterval = 0.2f;

    /// Number of first-order ambisonic input channels.
    public const int numFoaChannels = 4;

    [StructLayout(LayoutKind.Sequential)]
    private struct RoomProperties
    {
        // Center position of the room in world space.
        public float positionX;
        public float positionY;
        public float positionZ;

        // Rotation (quaternion) of the room in world space.
        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public float rotationW;

        // Size of the shoebox room in world space.
        public float dimensionsX;
        public float dimensionsY;
        public float dimensionsZ;

        // Material name of each surface of the shoebox room.
        public GvrAudioRoom.SurfaceMaterial materialLeft;
        public GvrAudioRoom.SurfaceMaterial materialRight;
        public GvrAudioRoom.SurfaceMaterial materialBottom;
        public GvrAudioRoom.SurfaceMaterial materialTop;
        public GvrAudioRoom.SurfaceMaterial materialFront;
        public GvrAudioRoom.SurfaceMaterial materialBack;

        // User defined uniform scaling factor for reflectivity. This parameter has no effect when set
        // to 1.0f.
        public float reflectionScalar;

        // User defined reverb tail gain multiplier. This parameter has no effect when set to 0.0f.
        public float reverbGain;

        // Adjusts the reverberation time across all frequency bands. RT60 values are multiplied by this
        // factor. Has no effect when set to 1.0f.
        public float reverbTime;

        // Controls the slope of a line from the lowest to the highest RT60 values (increases high
        // frequency RT60s when positive, decreases when negative). Has no effect when set to 0.0f.
        public float reverbBrightness;
    }

    // Converts given |position| and |rotation| from Unity space to audio space.
    private static void ConvertAudioTransformFromUnity(ref Vector3 position,
                                                        ref Quaternion rotation)
    {
        transformMatrix = Pose3D.FlipHandedness(Matrix4x4.TRS(position, rotation, Vector3.one));
        position = transformMatrix.GetColumn(3);
        rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
    }

    // Returns room properties of the given |room|.
    private static RoomProperties GetRoomProperties(GvrAudioRoom room)
    {
        RoomProperties roomProperties;
        Vector3 position = room.transform.position;
        Quaternion rotation = room.transform.rotation;
        Vector3 scale = Vector3.Scale(room.transform.lossyScale, room.size);
        ConvertAudioTransformFromUnity(ref position, ref rotation);
        roomProperties.positionX = position.x;
        roomProperties.positionY = position.y;
        roomProperties.positionZ = position.z;
        roomProperties.rotationX = rotation.x;
        roomProperties.rotationY = rotation.y;
        roomProperties.rotationZ = rotation.z;
        roomProperties.rotationW = rotation.w;
        roomProperties.dimensionsX = scale.x;
        roomProperties.dimensionsY = scale.y;
        roomProperties.dimensionsZ = scale.z;
        roomProperties.materialLeft = room.leftWall;
        roomProperties.materialRight = room.rightWall;
        roomProperties.materialBottom = room.floor;
        roomProperties.materialTop = room.ceiling;
        roomProperties.materialFront = room.frontWall;
        roomProperties.materialBack = room.backWall;
        roomProperties.reverbGain = ConvertAmplitudeFromDb(room.reverbGainDb);
        roomProperties.reverbTime = room.reverbTime;
        roomProperties.reverbBrightness = room.reverbBrightness;
        roomProperties.reflectionScalar = room.reflectivity;
        return roomProperties;
    }

    // Boundaries instance to be used in room detection logic.
    private static Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

    // Container to store the currently active rooms in the scene.
    private static List<GvrAudioRoom> enabledRooms = new List<GvrAudioRoom>();

    // Denotes whether the system is initialized properly.
    private static bool initialized = false;

    // Listener transform.
    private static Transform listenerTransform = null;

    // Pre-allocated raycast hit list for occlusion computation.
    private static RaycastHit[] occlusionHits = new RaycastHit[maxNumOcclusionHits];

    // Occlusion layer mask.
    private static int occlusionMaskValue = -1;

    // 4x4 transformation matrix to be used in transform space conversion.
    private static Matrix4x4 transformMatrix = Matrix4x4.identity;

#if !UNITY_EDITOR && UNITY_IOS
    private const string pluginName = "__Internal";

#else
    private const string pluginName = "audioplugingvrunity";
#endif  // !UNITY_EDITOR && UNITY_IOS

    // Listener handlers.
    [DllImport(pluginName)]
    private static extern void SetListenerGain(float gain);

    // Soundfield handlers.
    [DllImport(pluginName)]
    private static extern int CreateSoundfield(int numChannels);

    // Source handlers.
    [DllImport(pluginName)]
    private static extern int CreateSoundObject(bool enableHrtf);

    [DllImport(pluginName)]
    private static extern void DestroySource(int sourceId);

    [DllImport(pluginName)]
    private static extern void SetSourceBypassRoomEffects(int sourceId, bool bypassRoomEffects);

    [DllImport(pluginName)]
    private static extern void SetSourceDirectivity(int sourceId, float alpha, float order);

    [DllImport(pluginName)]
    private static extern void SetSourceListenerDirectivity(int sourceId, float alpha, float order);

    [DllImport(pluginName)]
    private static extern void SetSourceOcclusionIntensity(int sourceId, float intensity);

    // Room handlers.
    [DllImport(pluginName)]
    private static extern void SetRoomProperties(IntPtr roomProperties);

    // System handlers.
    [DllImport(pluginName)]
    private static extern void Initialize(int quality, int sampleRate, int numChannels,
                                          int framesPerBuffer);

    [DllImport(pluginName)]
    private static extern void Shutdown();
}

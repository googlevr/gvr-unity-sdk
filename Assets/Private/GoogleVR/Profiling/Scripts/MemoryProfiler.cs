//-----------------------------------------------------------------------
// <copyright file="MemoryProfiler.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.InternalTools.Profiling
{
#if UNITY_5_3_OR_NEWER
    using UnityEngine;

    /// <summary>
    /// Wrapper interface for the Unity Profiler static class (for testing).
    /// </summary>
    internal interface IUnityApiWrapper
    {
        /// <summary>
        /// Returns the used size from mono.
        /// </summary>
        /// <returns>The used memory size from mono.</returns>
        uint GetMonoUsedSize();

        /// <summary>
        /// Returns the unity frame count.
        /// </summary>
        /// <returns>The unity frame count.</returns>
        int GetFrameCount();

        /// <summary>
        /// Prvents a unity object from being destroyed on scene changes.
        /// </summary>
        /// <param name="theObject">The object to not be destroyed.</param>
        void DontDestroyOnLoad(UnityEngine.Object theObject);
    }

    /// <summary>
    /// Wrapper instance class for the Unity Profiler static class.
    /// </summary>
    internal class UnityApiWrapper : IUnityApiWrapper
    {
        /// <summary>
        /// Returns the used size from mono.
        /// </summary>
        /// <returns>The used memory size from mono.</returns>
        public uint GetMonoUsedSize()
        {
#if UNITY_5_5_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetMonoUsedSize();
#else
            return Profiler.GetMonoUsedSize();
#endif
        }

        /// <summary>
        /// Returns the unity frame count.
        /// </summary>
        /// <returns>The unity frame count.</returns>
        public int GetFrameCount()
        {
            return Time.frameCount;
        }

        /// <summary>
        /// Prvents a unity object from being destroyed on scene changes.
        /// </summary>
        /// <param name="theObject">The object to not be destroyed.</param>
        public void DontDestroyOnLoad(UnityEngine.Object theObject)
        {
            Object.DontDestroyOnLoad(theObject);
        }
    }

    /// <summary>
    /// A GameObject that logs memory profiling information across scenes.
    /// </summary>
    internal class MemoryProfiler : MonoBehaviour
    {
        /// <summary>
        /// The number array segments that will be pre-allocated for profiling.
        /// </summary>
        private static readonly int ARRAY_SEGMENT_COUNT = 80;

        /// <summary>
        /// The length of each pre-allocated array for profiling.
        /// </summary>
        private static readonly int ARRAY_SEGMENT_LENGTH = 3600;

        /// <summary>
        /// The maximum number of scenes the profiler will allocate storage to profile.
        /// </summary>
        private static readonly int MAX_NUMBER_OF_SCENES_PROFILED = 100;

        /// <summary>
        /// A pool of pre-allocated array segments for the profiler.
        /// </summary>
        private MemoryPoolOfArraySegments<uint> m_memoryPool;

        /// <summary>
        /// A collection of profiler runs by scene.
        /// </summary>
        private MemoryProfilingRun[] m_memoryProfilingRuns;

        /// <summary>
        /// The index of the curerent profiling run.
        /// </summary>
        private int m_profilingRunIndex = -1;

        /// <summary>
        /// An instance of the profiler api.
        /// </summary>
        private IUnityApiWrapper m_unityApiWrapper;

        /// <summary>
        /// Determines if the profiler is running.
        /// </summary>
        private bool m_isRunning;

        /// <summary>
        /// Gets a value that represents the active memory profiling run.
        /// </summary>
        public MemoryProfilingRun ActiveProfilingRun
        {
            get
            {
                if (m_isRunning && m_profilingRunIndex >= 0 && m_profilingRunIndex < MAX_NUMBER_OF_SCENES_PROFILED)
                {
                    return m_memoryProfilingRuns[m_profilingRunIndex];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the profiler is enabled for the current build.
        /// </summary>
        public static bool IsProfilerEnabledForBuild
        {
            get { return Debug.isDebugBuild; }
        }

        /// <summary>
        /// Gets or instantiates an instance of a memory profiler in the current scene.
        /// </summary>
        /// <returns>An instance of the MemoryProfiler or null.</returns>
        public static MemoryProfiler Get()
        {
            if (!IsProfilerEnabledForBuild)
            {
                return null;
            }

            MemoryProfiler instance = FindObjectOfType<MemoryProfiler>();
            if (instance == null)
            {
                GameObject profilerObject = new GameObject("Memory Profiler");
                instance = profilerObject.AddComponent<MemoryProfiler>();
                instance.Initialize(new UnityApiWrapper());
            }

            return instance;
        }

        /// <summary>
        /// Unity Update routine.
        /// </summary>
        public void Update()
        {
            if (m_isRunning && ActiveProfilingRun != null)
            {
                ActiveProfilingRun.LogFrameMemoryUsage();
            }
        }

        /// <summary>
        /// Initializes this instance of the garbage profiler GameObject.
        /// </summary>
        /// <param name="unityApiWrapper">An instance of the unityApiWrapper to access memory usage.</param>
        public void Initialize(IUnityApiWrapper unityApiWrapper)
        {
            m_unityApiWrapper = unityApiWrapper;
            m_memoryPool = new MemoryPoolOfArraySegments<uint>(ARRAY_SEGMENT_COUNT, ARRAY_SEGMENT_LENGTH);
            m_memoryProfilingRuns = new MemoryProfilingRun[MAX_NUMBER_OF_SCENES_PROFILED];

            for (int i = 0; i < MAX_NUMBER_OF_SCENES_PROFILED; i++)
            {
                m_memoryProfilingRuns[i] = new MemoryProfilingRun();
            }

            unityApiWrapper.DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Starts the profiler for a new run.
        /// </summary>
        /// <param name="runName">An identifier for the newly started run.</param>
        public void StartProfilingRun(string runName)
        {
            if (m_profilingRunIndex >= (MAX_NUMBER_OF_SCENES_PROFILED - 1))
            {
                Debug.LogError("Exceeded pre-allocated scene profiling maximum.");
                return;
            }

            m_isRunning = true;
            m_profilingRunIndex++;
            ActiveProfilingRun.Initialize(m_unityApiWrapper, m_memoryPool, runName);
        }

        /// <summary>
        /// Ends the current profiling run.
        /// </summary>
        public void EndProfilingRun()
        {
            m_isRunning = false;
        }

        /// <summary>
        /// Returns a json string containing profiling information.
        /// </summary>
        /// <returns>A json string containing profiling information.</returns>
        public string GetProfilingJson()
        {
            SerializeableMemoryProfilingRunArray runsContainer = new SerializeableMemoryProfilingRunArray();

            runsContainer.profilingRuns = new SerializeableMemoryProfilingRunData[m_profilingRunIndex + 1];

            for (int i = 0; i <= m_profilingRunIndex; i++)
            {
                runsContainer.profilingRuns[i] = new SerializeableMemoryProfilingRunData(m_memoryProfilingRuns[i]);
            }

            return JsonUtility.ToJson(runsContainer);
        }
    }

    /// <summary>
    /// Data stored by a profiling run of an active scene.
    /// </summary>
    internal class MemoryProfilingRun
    {
        /// <summary>
        /// The profiling API to get memory current usage information.
        /// </summary>
        private IUnityApiWrapper m_unityApiWrapper;

        /// <summary>
        /// The amount of memory used by mono last frame.
        /// </summary>
        private uint m_memoryUsedLastFrame;

        /// <summary>
        /// The memory used for in each frame of a profile run.
        /// </summary>
        private ContainerUsingMemoryPool<uint> m_memoryUsedPerFrame = new ContainerUsingMemoryPool<uint>();

        /// <summary>
        /// Gets the name of the profiling run.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the start frame of the profiling run.
        /// </summary>
        public int StartFrame { get; private set; }

        /// <summary>
        /// Gets the number of frames in the profiling run.
        /// </summary>
        public uint FrameCount { get; private set; }

        /// <summary>
        /// Gets the garbage generated during the current frame (always 0 for GC frames).
        /// </summary>
        public uint CurrentFrameGarbageGeneratedBytes { get; private set; }

        /// <summary>
        /// Gets the total garbage generated during the profiling run.
        /// </summary>
        public uint TotalGarbageGeneratedBytes { get; private set; }

        /// <summary>
        /// Gets the number of times garbage collection was performed during the profile run.
        /// </summary>
        public uint GarbageCollectionCount { get; private set; }

        /// <summary>
        /// Initializes an instance of a memory profiling run.
        /// </summary>
        /// <param name="unityApiWrapper">An instance of unityApiWrapper to make unity calls.</param>
        /// <param name="arrayMemoryPool">A pool of memory to allocate arrays from.</param>
        /// <param name="name">The name of the run.</param>
        public void Initialize(IUnityApiWrapper unityApiWrapper, MemoryPoolOfArraySegments<uint> arrayMemoryPool,
            string name)
        {
            m_unityApiWrapper = unityApiWrapper;
            Name = name;
            StartFrame = m_unityApiWrapper.GetFrameCount();
            FrameCount = 0;
            TotalGarbageGeneratedBytes = 0;
            GarbageCollectionCount = 0;
            m_memoryUsedPerFrame.SetMemoryPool(arrayMemoryPool);
            m_memoryUsedLastFrame = 0;
        }

        /// <summary>
        /// Adds the current frame's memory usage to this profiling run.
        /// </summary>
        public void LogFrameMemoryUsage()
        {
            uint memoryUsedThisFrame = m_unityApiWrapper.GetMonoUsedSize();
            m_memoryUsedPerFrame.TryAdd(memoryUsedThisFrame);

            // This is currently the most compatible way to access gc collection data across
            // environments.  However, it is (slightly) a hack and we should look into injecting custom
            // .so libraries to execute mono_profiler_install_allocation/mono_profiler_install_gc
            // from the player's process in the future.
            if (FrameCount > 0 && memoryUsedThisFrame < m_memoryUsedLastFrame)
            {
                GarbageCollectionCount++;
            }
            else if (FrameCount > 0 && memoryUsedThisFrame > m_memoryUsedLastFrame)
            {
                TotalGarbageGeneratedBytes += memoryUsedThisFrame - m_memoryUsedLastFrame;
            }

            m_memoryUsedLastFrame = memoryUsedThisFrame;
            FrameCount++;
        }

        /// <summary>
        /// Returns an array representing per-frame memory usage for this profiling run.
        /// </summary>
        /// <returns>An array representing per-frame memory usage for this profiling run.</returns>
        public uint[] GetPerFrameMemoryUsage()
        {
            uint[] usedMemoryArray = new uint[m_memoryUsedPerFrame.Count];
            int i = 0;

            foreach (uint usedMemory in m_memoryUsedPerFrame)
            {
                usedMemoryArray[i++] = usedMemory;
            }

            return usedMemoryArray;
        }
    }

    /// <summary>
    /// A serializable container for an array of memory profiling runs.
    /// </summary>
    [System.Serializable]
    internal class SerializeableMemoryProfilingRunArray
    {
        /// <summary>
        /// An array of memory profiling runs.
        /// </summary>
        public SerializeableMemoryProfilingRunData[] profilingRuns;
    }

    /// <summary>
    /// A serializable container for an individual memory profiling run.
    /// </summary>
    [System.Serializable]
    internal class SerializeableMemoryProfilingRunData
    {
        /// <summary>
        /// The name of the profiling run.
        /// </summary>
        public string name;

        /// <summary>
        /// The starting frame of the profiling run.
        /// </summary>
        public int startFrame;

        /// <summary>
        /// The total frame count for the profiling run.
        /// </summary>
        public uint frameCount;

        /// <summary>
        /// The total garbage generated for the profiling run.
        /// </summary>
        public uint garbageGeneratedBytes;

        /// <summary>
        /// The number of garbage collection events in the profiling run.
        /// </summary>
        public uint gcCollectCount;

        /// <summary>
        /// Per-frame memory allocation data for the profiling run.
        /// </summary>
        public uint[] memoryUsedPerFrameBytes;

        /// <summary>
        /// The constructor for the serializable profiling run.
        /// </summary>
        /// <param name="run">A non-serializable profiling run to copy.</param>
        public SerializeableMemoryProfilingRunData(MemoryProfilingRun run)
        {
            name = run.Name;
            startFrame = run.StartFrame;
            frameCount = run.FrameCount;
            garbageGeneratedBytes = run.TotalGarbageGeneratedBytes;
            gcCollectCount = run.GarbageCollectionCount;
            memoryUsedPerFrameBytes = run.GetPerFrameMemoryUsage();
        }
    }
#endif // UNITY_5_3_OR_NEWER
}

#if UNITY_5_3_OR_NEWER
namespace MemoryProfilerTest
{
    using System;
    using NSubstitute;
    using NUnit.Framework;
    using Google.InternalTools.Profiling;
    using UnityEngine;

    internal class MemoryProfilerTestMocks
    {
         public MemoryProfilerTestMocks()
         {
             UnityApiWrapper = Substitute.For<IUnityApiWrapper>();
             MemoryProfilingRun = Substitute.For<MemoryProfilingRun>();
         }

        public IUnityApiWrapper UnityApiWrapper { get; private set; }
        public MemoryProfilingRun MemoryProfilingRun { get; private set; }
    }

    internal class MemoryProfilerTestBase<T>
    {
        protected readonly MemoryProfilerTestMocks MOCKS = new MemoryProfilerTestMocks();
        protected T m_instance;
    }

    [TestFixture]
    internal class MemoryProfilerTest : MemoryProfilerTestBase<MemoryProfiler>
    {
        uint m_monoUsedSize;
        int m_frameCount;

        [SetUp]
        public void Setup()
        {
            MOCKS.UnityApiWrapper
                .GetMonoUsedSize()
                .Returns(x => { return m_monoUsedSize; });

            MOCKS.UnityApiWrapper
                .GetFrameCount()
                .Returns(x => { return m_frameCount; });

            var profilerObject = new GameObject("Memory Profiler");
            m_instance = profilerObject.AddComponent<MemoryProfiler>();
            m_instance.Initialize(MOCKS.UnityApiWrapper);
        }

        [Test]
        public void MemoryProfiler_SingleRun()
        {
            const string RUN_NAME = "TestScene";
            const uint INITIAL_MEMORY_USED = 1234;
            const uint SECOND_MEMORY_USED = 2345;

            m_monoUsedSize = INITIAL_MEMORY_USED;
            m_frameCount = 1;
            m_instance.StartProfilingRun(RUN_NAME);
            m_instance.Update();

            m_monoUsedSize = SECOND_MEMORY_USED;
            m_frameCount = 2;
            m_instance.Update();

            m_monoUsedSize = INITIAL_MEMORY_USED;
            m_frameCount = 3;
            m_instance.Update();

            var profile = JsonUtility.FromJson<SerializeableMemoryProfilingRunArray>(m_instance.GetProfilingJson());;
            Assert.AreEqual(profile.profilingRuns.Length, 1);
            Assert.AreEqual(profile.profilingRuns[0].name, RUN_NAME);
            Assert.AreEqual(profile.profilingRuns[0].startFrame, 1);
            Assert.AreEqual(profile.profilingRuns[0].frameCount, 3);
            Assert.AreEqual(profile.profilingRuns[0].memoryUsedPerFrameBytes[0], INITIAL_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[0].memoryUsedPerFrameBytes[1], SECOND_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[0].memoryUsedPerFrameBytes[2], INITIAL_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[0].gcCollectCount, 1);
            Assert.AreEqual(profile.profilingRuns[0].garbageGeneratedBytes, SECOND_MEMORY_USED - INITIAL_MEMORY_USED);
        }

        [Test]
        public void MemoryProfiler_MultipleRuns()
        {
            const string RUN1_NAME = "TestScene1";
            const string RUN2_NAME = "TestScene2";
            const uint INITIAL_MEMORY_USED = 1234;
            const uint SECOND_MEMORY_USED = 2345;
            const uint LAST_MEMORY_USED = 1233;


            m_monoUsedSize = INITIAL_MEMORY_USED;
            m_frameCount = 1;
            m_instance.StartProfilingRun(RUN1_NAME);
            m_instance.Update();

            m_monoUsedSize = SECOND_MEMORY_USED;
            m_frameCount = 2;
            m_instance.Update();
            m_instance.EndProfilingRun();

            m_monoUsedSize = LAST_MEMORY_USED;
            m_frameCount = 33;
            m_instance.StartProfilingRun(RUN2_NAME);
            m_instance.Update();

            m_monoUsedSize = LAST_MEMORY_USED;
            m_frameCount = 34;
            m_instance.Update();

            m_monoUsedSize = LAST_MEMORY_USED;
            m_frameCount = 35;
            m_instance.Update();
            m_instance.EndProfilingRun();

            var profile = JsonUtility.FromJson<SerializeableMemoryProfilingRunArray>(m_instance.GetProfilingJson());;
            Assert.AreEqual(profile.profilingRuns.Length, 2);
            Assert.AreEqual(profile.profilingRuns[0].name, RUN1_NAME);
            Assert.AreEqual(profile.profilingRuns[0].startFrame, 1);
            Assert.AreEqual(profile.profilingRuns[0].frameCount, 2);
            Assert.AreEqual(profile.profilingRuns[0].memoryUsedPerFrameBytes[0], INITIAL_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[0].memoryUsedPerFrameBytes[1], SECOND_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[0].gcCollectCount, 0);
            Assert.AreEqual(profile.profilingRuns[0].garbageGeneratedBytes, SECOND_MEMORY_USED - INITIAL_MEMORY_USED);

            Assert.AreEqual(profile.profilingRuns[1].name, RUN2_NAME);
            Assert.AreEqual(profile.profilingRuns[1].startFrame, 33);
            Assert.AreEqual(profile.profilingRuns[1].frameCount, 3);
            Assert.AreEqual(profile.profilingRuns[1].memoryUsedPerFrameBytes[0], LAST_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[1].memoryUsedPerFrameBytes[1], LAST_MEMORY_USED);
            Assert.AreEqual(profile.profilingRuns[1].gcCollectCount, 0);
            Assert.AreEqual(profile.profilingRuns[1].garbageGeneratedBytes, 0);
        }
    }
}
#endif // UNITY_5_3_OR_NEWER

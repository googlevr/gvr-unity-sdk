#if UNITY_5_3_OR_NEWER
namespace ContainerUsingMemoryPoolTest
{
    using NSubstitute;
    using NUnit.Framework;
    using Google.InternalTools.Profiling;

    [TestFixture]
    internal class ContainerUsingMemoryPoolTest
    {
        ContainerUsingMemoryPool<int> m_instance;

        [Test]
        public void ContainerUsingMemoryPool()
        {
            const int ALLOCATION_COUNT = 2;
            const int ARRAY_LENGTH = 3;
            const int TOTAL_ALLOCATION = ALLOCATION_COUNT * ARRAY_LENGTH;

            var memoryPool = new MemoryPoolOfArraySegments<int>(2, 3);
            m_instance = new ContainerUsingMemoryPool<int>();
            m_instance.SetMemoryPool(memoryPool);

            for (int i = 0; i < TOTAL_ALLOCATION + 4; i++)
            {
                 bool result = m_instance.TryAdd(i);
                 Assert.AreEqual(result, i < TOTAL_ALLOCATION);
            }

            Assert.AreEqual(m_instance.Count, TOTAL_ALLOCATION);

            int count = 0;
            foreach (int val in m_instance)
            {
                Assert.AreEqual(val, count++);
            }
        }
    }
}
#endif // UNITY_5_3_OR_NEWER

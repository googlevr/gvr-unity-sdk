//-----------------------------------------------------------------------
// <copyright file="ContainerUsingMemoryPool.cs" company="Google">
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

#if UNITY_5_3_OR_NEWER
namespace Google.InternalTools.Profiling
{
    using System.Collections.Generic;

    /// <summary>
    /// Implements a generic ordered container that allocates space from a
    /// provided pre-allocated pool of arrays that can be shared accross objects to
    /// mitigate memory fragmentation.
    /// </summary>
    /// <typeparam name="T">The type contained by the container.</typeparam>
    internal class ContainerUsingMemoryPool<T>
    {
        /// <summary>
        /// The memory pool used for allocation.
        /// </summary>
        private MemoryPoolOfArraySegments<T> m_arrayMemoryPool;

        /// <summary>
        /// A collection of allocated array segments.
        /// </summary>
        private List<T[]> m_arraySegments = new List<T[]>();

        /// <summary>
        /// The current segment's element count.
        /// </summary>
        private int m_currentSegmentElementCount = 0;

        /// <summary>
        /// Constructor for ContainerUsingMemoryPool.
        /// </summary>
        public ContainerUsingMemoryPool()
        {
            Count = 0;
        }

        /// <summary>
        /// Gets a value representing the number of elements in the container.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the current array segment.
        /// </summary>
        private T[] CurrentSegment
        {
            get
            {
                if (m_arraySegments.Count > 0)
                {
                    return m_arraySegments[m_arraySegments.Count - 1];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current segment's length.
        /// </summary>
        private int CurrentSegmentLength
        {
            get
            {
                var currentSegment = CurrentSegment;
                if (currentSegment != null)
                {
                    return currentSegment.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Sets the memory pool used to allocate elements for the container.
        /// </summary>
        /// <param name="memoryPool">The memory pool to set.</param>
        public void SetMemoryPool(MemoryPoolOfArraySegments<T> memoryPool)
        {
            m_arrayMemoryPool = memoryPool;
        }

        /// <summary>
        /// Attempts to add an element to the end of the container.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>true if element was added, false otherwise.</returns>
        public bool TryAdd(T element)
        {
            if (CurrentSegment == null || m_currentSegmentElementCount >= CurrentSegmentLength)
            {
                // Allocate a new segment.
                T[] allocation;
                if (m_arrayMemoryPool != null && m_arrayMemoryPool.TryAlloc(out allocation))
                {
                    m_arraySegments.Add(allocation);
                    m_currentSegmentElementCount = 0;
                }
                else
                {
                    return false;
                }
            }

            // Add the element to the current segment.
            CurrentSegment[m_currentSegmentElementCount++] = element;
            Count++;
            return true;
        }

        /// <summary>
        /// Gets an enumerator for this container.
        /// </summary>
        /// <returns>An enumerator for this container.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            // Yield all elements except current.
            for (int i = 0; i < m_arraySegments.Count - 1; i++)
            {
                for (int j = 0; j < m_arraySegments[i].Length; j++)
                {
                    yield return m_arraySegments[i][j];
                }
            }

            // Yield the current segment.
            for (int i = 0; i < m_currentSegmentElementCount; i++)
            {
                yield return CurrentSegment[i];
            }
        }
    }

    /// <summary>
    /// A memory pool of array segments.
    /// </summary>
    /// <typeparam name="T">The type contained within the memory pool array segments.</typeparam>
    internal class ArrayMemoryPool<T>
    {
        /// <summary>
        /// A collection of array segment allocations that comprise the memory pool.
        /// </summary>
        private T[][] m_allocations;

        /// <summary>
        /// The index of the next allocation give out from the pool.
        /// </summary>
        private int m_allocationIndex;

        /// <summary>
        /// Constructs a new ArrayMemoryPool object.
        /// </summary>
        /// <param name="allocationCount">The number of array allocations to make when generating the pool.</param>
        /// <param name="arrayLength">The length of each array alloction in the pool.</param>
        public ArrayMemoryPool(int allocationCount, int arrayLength)
        {
            m_allocations = new T[allocationCount][];
            for (int i = 0; i < m_allocations.Length; i++)
            {
                m_allocations[i] = new T[arrayLength];
            }
        }

        /// <summary>
        /// Attempts to allocate an array segment from the memory pool.
        /// </summary>
        /// <param name="allocation">Reference set to the allocated array segment if successful, null otherwise.</param>
        /// <returns>True upon success, false otherwise.</returns>
        public bool TryAlloc(out T[] allocation)
        {
            if (m_allocations == null || m_allocationIndex >= m_allocations.Length)
            {
                allocation = null;
                return false;
            }

            allocation = m_allocations[m_allocationIndex++];
            return true;
        }
    }
}
#endif // UNITY_5_3_OR_NEWER

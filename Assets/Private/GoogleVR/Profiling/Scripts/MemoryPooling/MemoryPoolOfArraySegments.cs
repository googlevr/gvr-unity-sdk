//-----------------------------------------------------------------------
// <copyright file="MemoryPoolOfArraySegments.cs" company="Google">
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
    /// <summary>
    /// A memory pool of array segments.
    /// </summary>
    /// <typeparam name="T">The type contained within the memory pool array segments.</typeparam>
    internal class MemoryPoolOfArraySegments<T>
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
        public MemoryPoolOfArraySegments(int allocationCount, int arrayLength)
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

//-----------------------------------------------------------------------
// <copyright file="MemoryProfilingRunConfiguration.cs" company="Google">
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
/// <summary>
/// A configuration for automatically profiling a scene.
/// </summary>
public class MemoryProfilingRunConfiguration
{
    /// <summary>
    /// The name of the scene.
    /// </summary>
    public string m_sceneName;

    /// <summary>
    /// The duration (s) that the scene should be profiled.
    /// </summary>
    public float m_profilingDuration;

    /// <summary>
    /// The maximum memory used (bytes) before a violation is considered to have occured.
    /// </summary>
    public uint m_garbageGenerationConstraint;

    /// <summary>
    /// The maximum number of times garbage collection can run before a violation is
    /// considered to have occured.
    /// </summary>
    public uint m_garbageCollectionConstraint;
}
#endif // UNITY_5_3_OR_NEWER

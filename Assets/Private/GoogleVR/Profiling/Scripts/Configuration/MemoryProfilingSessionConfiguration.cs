//-----------------------------------------------------------------------
// <copyright file="MemoryProfilingSessionConfiguration.cs" company="Google">
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
/// The configutation for a profling session.
/// </summary>
public class MemoryProfilingSessionConfiguration
{
    /// <summary>
    /// The scene to start in for manual profiling mode.
    /// </summary>
    public string m_manualStartSceneName = "GVRDemo";

    /// <summary>
    /// A collection of scenes profiling configurations that compise the profiling session.
    /// </summary>
    public MemoryProfilingRunConfiguration[] m_automaticProfilingScenes =
    {
        new MemoryProfilingRunConfiguration() { m_sceneName = "GVRDemo", m_profilingDuration = 5.0f,
            m_garbageCollectionConstraint = 0, m_garbageGenerationConstraint = 0 },
    };
}
#endif // UNITY_5_3_OR_NEWER

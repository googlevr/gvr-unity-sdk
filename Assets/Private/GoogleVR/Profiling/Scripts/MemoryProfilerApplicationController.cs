//-----------------------------------------------------------------------
// <copyright file="MemoryProfilerApplicationController.cs" company="Google">
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

using System.Collections.Generic;
using System.IO;
using Google.InternalTools.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// A controller for our memory profiling application.
/// </summary>
internal class MemoryProfilerApplicationController : MonoBehaviour
{
    /// <summary>
    /// The root GameObject for the application home screen.
    /// </summary>
    public GameObject m_homeScreenOverlay = null;

    /// <summary>
    /// The root GameObject for the active profiling screen.
    /// </summary>
    public GameObject m_profilingScreenOverlay = null;

    /// <summary>
    /// The root GameObject for the application summary screen.
    /// </summary>
    public GameObject m_summaryScreenOverlay = null;

    /// <summary>
    /// The root GameObject for the debug build error screen.
    /// </summary>
    public GameObject m_debugErrorScreenOverlay = null;

    /// <summary>
    /// The UI component that displays the profiling summary.
    /// </summary>
    public ProfilingSessionSummaryUI m_sessionSummaryUI = null;

    /// <summary>
    /// The event system GameObject for the profiling UI.
    /// </summary>
    public GameObject m_profilerEventSystem = null;

#if UNITY_5_3_OR_NEWER
    /// <summary>
    /// The period of time (s) spent on the homescreen before profiling will automatically begin.
    /// </summary>
    private static readonly float HOMESCREEN_WAIT_DURATION = 5.0f;

    /// <summary>
    /// The period of time (s) spent on the summary screen before application will quit.
    /// </summary>
    private static readonly float SUMMARY_SCREEN_DURATION = 20.0f;

    /// <summary>
    /// The period of time (s) to wait after scene loading to start the profiling run.
    /// </summary>
    private static readonly float DELAY_PROFILER_START_DURATION = .1f;

    /// <summary>
    /// The name of the output json file for the profiling session.
    /// </summary>
    private static readonly string JSON_OUTPUT_FILE_NAME = "MemoryProfiling.json";

    /// <summary>
    /// The name of the home scene for the profiler.
    /// </summary>
    private static readonly string MEMORY_PROFILER_HOME_SCENE_NAME = "MemoryProfilerHomeScene";

    /// <summary>
    /// The current UI screen state.
    /// </summary>
    private UIScreenStateEnum m_currentScreenState = UIScreenStateEnum.None;

    /// <summary>
    /// The configuration used for a session of the memory profiling application.
    /// </summary>
    private MemoryProfilingSessionConfiguration m_profilerConfiguration = new MemoryProfilingSessionConfiguration();

    /// <summary>
    /// The instance of the memory profiler running in the scene.
    /// </summary>
    private MemoryProfiler m_memoryProfiler;

    /// <summary>
    /// The index of the current automatic profiling scene.
    /// </summary>
    private int m_currentProfilingRunIndex;

    /// <summary>
    /// The start time of the current scene.
    /// </summary>
    private float m_sceneStartTime;

    /// <summary>
    /// A collection of summaries for scenes that have been profiled.
    /// </summary>
    private List<MemoryProfilingRunSummary> m_sceneProfilingSummaryList = new List<MemoryProfilingRunSummary>();

    /// <summary>
    /// An enumeration of UI states.
    /// </summary>
    private enum UIScreenStateEnum
    {
        None = 0,
        HomeScreen = 1,
        ProfilingSceneScreen = 2,
        SummaryScreen = 3,
        DubugBuildErrorScreen = 4,
    }

    /// <summary>
    /// Gets the profiling run configuration for the current run.
    /// </summary>
    private MemoryProfilingRunConfiguration CurrentProfilingRunConfiguration
    {
        get
        {
            if (m_currentProfilingRunIndex >= 0 && m_currentProfilingRunIndex <
                m_profilerConfiguration.m_automaticProfilingScenes.Length)
            {
                return m_profilerConfiguration.m_automaticProfilingScenes[m_currentProfilingRunIndex];
            }

            return null;
        }
    }

    /// <summary>
    /// The Unity Start method.
    /// </summary>`
    public void Start()
    {
        // Ensure only one scene UI.
        if (FindObjectsOfType<MemoryProfilerApplicationController>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
        }

        // Initialize the UI to the home screen.
        _SetUIState(Debug.isDebugBuild ? UIScreenStateEnum.HomeScreen : UIScreenStateEnum.DubugBuildErrorScreen);

        // Initialize the memory profiler, set to inactive until profiling scene load.
        m_memoryProfiler = MemoryProfiler.Get();
    }

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    public void Update()
    {
        if (m_currentScreenState == UIScreenStateEnum.HomeScreen)
        {
            float timeUntilAutorun = HOMESCREEN_WAIT_DURATION - (Time.time - m_sceneStartTime);
            if (timeUntilAutorun <= 0.0f)
            {
                _StartProfilingSession();
            }

            return;
        }
        else if (m_currentScreenState == UIScreenStateEnum.SummaryScreen)
        {
            // Calculate the remaining automatic profiling time in this scene.
            float remainingTimeInScene = SUMMARY_SCREEN_DURATION - (Time.time - m_sceneStartTime);

            // Update state automatic profiling time in scene expires.
            if (remainingTimeInScene <= 0.0f)
            {
                Application.Quit();
            }

            return;
        }
        else if (m_currentScreenState == UIScreenStateEnum.ProfilingSceneScreen &&
            CurrentProfilingRunConfiguration != null)
        {
            // Calculate the remaining automatic profiling time in this scene.
            float remainingTimeInScene = CurrentProfilingRunConfiguration.m_profilingDuration -
                (Time.time - m_sceneStartTime);

            // Update state in automatic profiling time in scene expires.
            if (remainingTimeInScene <= 0.0f)
            {
                _SetupSceneLoad();
                if (CurrentProfilingRunConfiguration != null)
                {
                    SceneManager.LoadScene(CurrentProfilingRunConfiguration.m_sceneName);
                }
                else
                {
                    _EndProfilingSession();
                }
            }
        }
    }

    /// <summary>
    /// Handles a Unity level load.
    /// </summary>
    public void OnLevelWasLoaded()
    {
        m_sceneStartTime = Time.time;
        m_profilerEventSystem.SetActive(FindObjectsOfType<EventSystem>().Length == 0);

        // Delays the start of the profiler slightly to assure scene construction garbage is not
        // still being generated.
        if (m_currentScreenState == UIScreenStateEnum.ProfilingSceneScreen)
        {
            Invoke("_StartProfilingRun", DELAY_PROFILER_START_DURATION);
        }
    }

    /// <summary>
    /// Starts the profiling session.
    /// </summary>
    private void _StartProfilingSession()
    {
        m_currentProfilingRunIndex = -1;
        _SetUIState(UIScreenStateEnum.ProfilingSceneScreen);
        _SetupSceneLoad();
        if (CurrentProfilingRunConfiguration != null)
        {
            SceneManager.LoadScene(CurrentProfilingRunConfiguration.m_sceneName);
        }
        else
        {
            _EndProfilingSession();
        }
    }

    /// <summary>
    /// Ends the profiling session.
    /// </summary>
    private void _EndProfilingSession()
    {
        _SetUIState(UIScreenStateEnum.SummaryScreen);
        m_sessionSummaryUI.DisplaySummaries(m_sceneProfilingSummaryList);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, JSON_OUTPUT_FILE_NAME),
            m_memoryProfiler.GetProfilingJson());
        SceneManager.LoadScene(MEMORY_PROFILER_HOME_SCENE_NAME);
    }

    /// <summary>
    /// Starts a new profiling run.
    /// </summary>
    private void _StartProfilingRun()
    {
        m_memoryProfiler.StartProfilingRun(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Ends the profiling run.
    /// </summary>
    private void _EndProfilingRun()
    {
        // No profiling run to end
        if (m_memoryProfiler.ActiveProfilingRun == null)
        {
            return;
        }

        // Record summary for profiling run
        m_sceneProfilingSummaryList.Add(new MemoryProfilingRunSummary(
            m_memoryProfiler.ActiveProfilingRun,
            CurrentProfilingRunConfiguration.m_garbageGenerationConstraint,
            CurrentProfilingRunConfiguration.m_garbageCollectionConstraint));

        m_memoryProfiler.EndProfilingRun();
    }

    /// <summary>
    /// Sets the state of the memory profiling application's UI.
    /// </summary>
    /// <param name="targetState">The new desired state of the UI.</param>
    private void _SetUIState(UIScreenStateEnum targetState)
    {
        m_homeScreenOverlay.SetActive(targetState == UIScreenStateEnum.HomeScreen);
        m_summaryScreenOverlay.SetActive(targetState == UIScreenStateEnum.SummaryScreen);
        m_profilingScreenOverlay.SetActive(targetState == UIScreenStateEnum.ProfilingSceneScreen);
        m_debugErrorScreenOverlay.SetActive(targetState == UIScreenStateEnum.DubugBuildErrorScreen);
        m_currentScreenState = targetState;
    }

    /// <summary>
    /// Gets ready for when the scene is about to change.
    /// </summary>
    private void _SetupSceneLoad()
    {
        // If a profling run is active then it should end.
        _EndProfilingRun();

        // Disable EventSystem in case one exists in the new scene.
        m_profilerEventSystem.SetActive(false);

        // Queue up the next profiling scene.
        m_currentProfilingRunIndex++;
    }

    /// <summary>
    /// A wrapper for a memory profiling run that helps summarize the run in relation to set constraints.
    /// </summary>
    public struct MemoryProfilingRunSummary
    {
        /// <summary>
        /// The total garbage generation constraint for the profiling run.
        /// </summary>
        private uint m_totalGarbageGenerationBytesConstraint;

        /// <summary>
        /// The garbage collection count constraint for the profiling run.
        /// </summary>
        private uint m_garbageCollectionCountConstraint;

        /// <summary>
        /// Indicates which constraints are active.  An active constraint that is not met by the
        /// profiling run will result in a violation.
        /// </summary>
        private ConstraintEnum m_activeConstraints;

        /// <summary>
        /// Constructs a new MemoryProfilingRunSummary.
        /// </summary>
        /// <param name="profilingRun">The profiling run being summarized.</param>
        public MemoryProfilingRunSummary(MemoryProfilingRun profilingRun)
        {
            MemoryProfilingRun = profilingRun;
            m_totalGarbageGenerationBytesConstraint = m_garbageCollectionCountConstraint = 0;
            m_activeConstraints = 0;
        }

        /// <summary>
        /// Constructs a new MemoryProfilingRunSummary.
        /// </summary>
        /// <param name="profilingRun">The profiling run being summarized.</param>
        /// <param name="totalGarbageGenerationBytesConstraint">The constraint on garbage generation.</param>
        /// <param name="garbageCollectionCountConstraint">The constraint on garbage collection count.</param>
        public MemoryProfilingRunSummary(MemoryProfilingRun profilingRun,
            uint totalGarbageGenerationBytesConstraint, uint garbageCollectionCountConstraint)
        {
            MemoryProfilingRun = profilingRun;
            m_totalGarbageGenerationBytesConstraint = totalGarbageGenerationBytesConstraint;
            m_garbageCollectionCountConstraint = garbageCollectionCountConstraint;
            m_activeConstraints = ConstraintEnum.GARBAGE_GENERATION_CONSTRAINT | ConstraintEnum.GARBAGE_COLLECTION_CONSTRAINT;
        }

        /// <summary>
        /// An enumeration of constraints that can be applied to the profiling run.
        /// </summary>
        [System.FlagsAttribute]
        private enum ConstraintEnum
        {
            GARBAGE_GENERATION_CONSTRAINT = 1,
            GARBAGE_COLLECTION_CONSTRAINT = 2,
        }

        /// <summary>
        /// Gets the memory profiling run being summarized.
        /// </summary>
        public MemoryProfilingRun MemoryProfilingRun { get; private set; }

        /// <summary>
        /// Gets any violations of constraints in the profiling run.
        /// </summary>
        /// <param name="garbageGenerationViolation">Set to true by this method if a garbage generation violation
        /// occured.</param>
        /// <param name="garbageCollectionViolation">Set to true by this method if a garbage collection violation
        /// occured.</param>
        public void GetViolations(out bool garbageGenerationViolation, out bool garbageCollectionViolation)
        {
            garbageGenerationViolation = (m_activeConstraints & ConstraintEnum.GARBAGE_GENERATION_CONSTRAINT) ==
                ConstraintEnum.GARBAGE_GENERATION_CONSTRAINT ?
                MemoryProfilingRun.TotalGarbageGeneratedBytes > m_totalGarbageGenerationBytesConstraint : false;
            garbageCollectionViolation = (m_activeConstraints & ConstraintEnum.GARBAGE_COLLECTION_CONSTRAINT) ==
                ConstraintEnum.GARBAGE_COLLECTION_CONSTRAINT ?
                MemoryProfilingRun.GarbageCollectionCount > m_garbageCollectionCountConstraint : false;
        }

        /// <summary>
        /// Returns a string representation of the contraint violations for the profiling run.
        /// </summary>
        /// <returns>A string representation of the contraint violations for the profiling run.</returns>
        public string GetViolationString()
        {
            bool garbageGeneration;
            bool garbageCountViolation;
            GetViolations(out garbageGeneration, out garbageCountViolation);
            System.Text.StringBuilder resultBuilder = new System.Text.StringBuilder();

            if (garbageGeneration)
            {
                resultBuilder.Append(string.Format(
                    "\tGarbage generation exceeds constraint, allowed: {0}B actual: {1}B\n",
                    m_totalGarbageGenerationBytesConstraint,
                    MemoryProfilingRun.TotalGarbageGeneratedBytes));
            }

            if (garbageCountViolation)
            {
                resultBuilder.Append(string.Format(
                    "\tGarbage collection count exceeds constraint, allowed: {0}\tactual: {1}\n",
                    m_garbageCollectionCountConstraint,
                    MemoryProfilingRun.GarbageCollectionCount));
            }

            if (garbageGeneration || garbageCountViolation)
            {
                resultBuilder.Append("\tNo Violations\n");
            }

            return resultBuilder.ToString();
        }
    }
#endif // UNITY_5_3_OR_NEWER
}

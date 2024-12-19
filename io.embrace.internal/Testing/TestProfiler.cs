using System;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestRunner;
using Debug = UnityEngine.Debug;


// Registers this type for callbacks from the Unity test runner
[assembly: TestRunCallback(typeof(EmbraceSDK.Tests.TestProfiler))]

namespace EmbraceSDK.Tests
{
    /// Used to profile tests and print their durations to the CI console
    internal class TestProfiler : ITestRunCallback
    {
        private const char PASS_GLYPH = '\u2705';
        private const char FAIL_GLYPH = '\u274C';
        private const char OTHER_GLYPH = '\u2753';

        private const string LOG_PREFIX = "[Test Profiler] ";

        // Any state in this type would be volatile since several of our tests recompile all scripts. Therefore,
        // we need to store what would normally be internal state externally in EditorPrefs
        private const string TIMED_TEST_ID_KEY = "TestProfiler.timedTestId";
        private const string START_TIME_KEY = "TestProfiler.testStartTime";
        private const string NUM_PASSED_KEY = "TestProfiler.numPassed";
        private const string NUM_FAILED_KEY = "TestProfiler.numFailed";

        public void RunStarted(ITest testsToRun)
        {
            EditorPrefs.SetInt(NUM_FAILED_KEY, 0);
            EditorPrefs.SetInt(NUM_PASSED_KEY, 0);
        }

        public void RunFinished(ITestResult testResults)
        {
            const string format = LOG_PREFIX + "Tests Results: {0} Passed: {1}, {2} Failed: {3}";

            int numPassed = EditorPrefs.GetInt(NUM_PASSED_KEY, 0);
            int numFailed = EditorPrefs.GetInt(NUM_FAILED_KEY, 0);

            Debug.LogFormat((numFailed > 0 ? LogType.Error : LogType.Log), LogOption.NoStacktrace, null,
                format, PASS_GLYPH, numPassed, FAIL_GLYPH, numFailed);
        }

        public void TestStarted(ITest test)
        {
            if (!test.HasChildren)
            {
                if (Application.isBatchMode)
                {
                    const string format = LOG_PREFIX + "Starting test {0}";
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, format, test.FullName);
                }

                EditorPrefs.SetString(TIMED_TEST_ID_KEY, test.Id);
                EditorPrefs.SetString(START_TIME_KEY, DateTime.Now.ToString());
            }
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.HasChildren && EditorPrefs.GetString(TIMED_TEST_ID_KEY, "") == result.Test.Id)
            {
                DateTime now = DateTime.Now;

                if (!DateTime.TryParse(EditorPrefs.GetString(START_TIME_KEY, ""), out DateTime startTime))
                {
                    return;
                }

                TimeSpan duration = now - startTime;

                EditorPrefs.SetString(TIMED_TEST_ID_KEY, null);
                EditorPrefs.SetString(START_TIME_KEY, null);

                char statusGlyph = OTHER_GLYPH;
                switch (result.ResultState.Status)
                {
                    case TestStatus.Failed:
                        statusGlyph = FAIL_GLYPH;
                        int numFailed = EditorPrefs.GetInt(NUM_FAILED_KEY, 0) + 1;
                        EditorPrefs.SetInt(NUM_FAILED_KEY, numFailed);
                        break;

                    case TestStatus.Passed:
                        statusGlyph = PASS_GLYPH;
                        int numPassed = EditorPrefs.GetInt(NUM_PASSED_KEY, 0) + 1;
                        EditorPrefs.SetInt(NUM_PASSED_KEY, numPassed);
                        break;
                }

                if (Application.isBatchMode)
                {
                    const string format = LOG_PREFIX + "{0}! {1} {2} {3} in {4}ms";
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, format, result.ResultState.Status, statusGlyph, result.FullName, result.ResultState.Status, duration.TotalMilliseconds);
                }
            }
        }
    }
}
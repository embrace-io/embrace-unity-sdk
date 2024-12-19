using System.Collections;
using System.Runtime.InteropServices;
using EmbraceSDK;
using EmbraceSDK.Editor;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

/*
 * Notably, these have not been used or expanded upon in awhile because of the state of Copeland's test harness.
 * We will likely be deprecating this in the future.
 */
namespace Embrace.Internal.SmokeTests
{
    /// <summary>
    /// Contains smoke test methods related to crashes
    /// </summary>
    [ExcludeFromCoverage]
    public class CrashSmokeTests
    {
        // The iOS SDK has historically lost some Unity exception logs if the process crashed/terminated before the exception log could be
        // sent or written to disk. This is because the log API contained several async operations.
        // This test logs an exception and crashes immediately afterward to confirm that we receive both the exception log and the crash payload.
        [Preserve]
        [SmokeTest]
        public void LogExceptionThenSdkCrash()
        {
            EmbraceSDK.Embrace.Instance.StartSDK();
            EmbraceSDK.Embrace.Instance.LogUnhandledUnityException("TestException", "Test exception message.", "__test_stack_trace__");
        }

        // The iOS SDK has historically lost some Unity exception logs if the process crashed/terminated before the exception log could be
        // sent or written to disk. This is because the log API contained several async operations.
        // This test logs an exception and crashes immediately afterward to confirm that we receive both the exception log and the crash payload.
        [Preserve]
        [SmokeTest]
        public void LogExceptionThenAbort()
        {
            EmbraceSDK.Embrace.Instance.StartSDK();
            EmbraceSDK.Embrace.Instance.LogUnhandledUnityException("TestException", "Test exception message.", "__test_stack_trace__");
            Utils.ForceCrash(ForcedCrashCategory.Abort);
        }

        // The iOS SDK has historically lost some Unity exception logs if the process crashed/terminated before the exception log could be
        // sent or written to disk. This is because the log API contained several async operations.
        // This test logs an exception and crashes immediately afterward to confirm that we receive both the exception log and the crash payload.
        [Preserve]
        [SmokeTest]
        public void LogExceptionThenCallPureVirtualFunc()
        {
            EmbraceSDK.Embrace.Instance.StartSDK();
            EmbraceSDK.Embrace.Instance.LogUnhandledUnityException("TestException", "Test exception message.", "__test_stack_trace__");
            Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);
        }

        // End a session before starting it to confirm that it does not crash.
        [Preserve]
        [SmokeTest]
        public void EndSessionBeforeStart()
        {
            EmbraceSDK.Embrace.Instance.EndSession();
            EmbraceSDK.Embrace.Instance.StartSDK();
        }
        
        // Destroy an Embrace instance before starting it again to confirm that it does not crash.
        [Preserve]
        [SmokeTest]
        public void DestroyEmbraceInstanceBeforeStart()
        {
            EmbraceSDK.Embrace.Instance.StartView("TestView");
            Object.DestroyImmediate(EmbraceSDK.Embrace.Instance);
            EmbraceSDK.Embrace.Instance.StartSDK();
        }

        // Intentionally inject a bad provider to confirm that the SDK does not crash.
        // This specific behavior is definitely not how the SDK is intended to be used.
        [Preserve]
        [SmokeTest]
        public IEnumerator InjectBadProvider()
        {
            EmbraceSDK.Embrace.Instance.StartSDK();
            EmbraceSDK.Embrace.Instance.provider = new Embrace_Stub();
            Object.DestroyImmediate(EmbraceSDK.Embrace.Instance);
            var embraceRoot = new GameObject();
            var instance = embraceRoot.AddComponent<EmbraceSDK.Embrace>();
            Object.DestroyImmediate(embraceRoot);
            
            yield return null;
            instance.StartSDK();
        }

        // Call every function in the public API to validate that none of them lead to a crash.
        [Preserve]
        [SmokeTest]
        public void ExercisePublicApi()
        {
            EmbraceSDK.Embrace.Instance.StartSDK();

            // This won't actually create a new session because we're still within the minimum session length,
            // so this test should still expect 2 session payloads.
            EmbraceSDK.Embrace.Instance.EndSession();

            EmbraceSDK.Embrace.Instance.AddSessionProperty("tempTestKey", "tempTestValue", false);
            EmbraceSDK.Embrace.Instance.AddSessionProperty("permTestKey", "permTestValue", true);
            
            EmbraceSDK.Embrace.Instance.StartView("testView");

            EmbraceSDK.Embrace.Instance.SetUsername("testUserName");
            EmbraceSDK.Embrace.Instance.SetUserIdentifier("0123456789");
            EmbraceSDK.Embrace.Instance.SetUserEmail("testuser@embrace.io");
            EmbraceSDK.Embrace.Instance.SetUserAsPayer();

            EmbraceSDK.Embrace.Instance.GetLastRunEndState();
            EmbraceSDK.Embrace.Instance.GetDeviceId();
            EmbraceSDK.Embrace.Instance.GetSessionProperties();
            
            EmbraceSDK.Embrace.Instance.LogMessage("info message", EMBSeverity.Info);
            EmbraceSDK.Embrace.Instance.LogMessage("warning message", EMBSeverity.Warning);
            EmbraceSDK.Embrace.Instance.LogMessage("error message", EMBSeverity.Error);

            EmbraceSDK.Embrace.Instance.LogUnhandledUnityException("Exception", "Exception message.", "__test stack trace__");
            EmbraceSDK.Embrace.Instance.LogHandledUnityException("Exception", "Exception message.", "__test stack trace__");

            long time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

            EmbraceSDK.Embrace.Instance.ClearUserPersona("testPersona");
            EmbraceSDK.Embrace.Instance.ClearUserAsPayer();
            EmbraceSDK.Embrace.Instance.ClearUserEmail();
            EmbraceSDK.Embrace.Instance.ClearUserIdentifier();
            EmbraceSDK.Embrace.Instance.ClearUsername();
            EmbraceSDK.Embrace.Instance.ClearAllUserPersonas();

            EmbraceSDK.Embrace.Instance.EndView("testView");

            EmbraceSDK.Embrace.Instance.RemoveSessionProperty("permTestKey");
            EmbraceSDK.Embrace.Instance.RemoveSessionProperty("tempTestKey");
        }
    }
}
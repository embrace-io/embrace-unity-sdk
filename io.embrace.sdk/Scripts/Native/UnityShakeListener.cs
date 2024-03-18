using UnityEngine;

namespace EmbraceSDK.Bugshake
{
    #if UNITY_2020_2_OR_NEWER && UNITY_ANDROID
    public class UnityShakeListener : AndroidJavaProxy
    {
        public UnityShakeListener() : base("io.embrace.android.embracesdk.bugshake.ShakeListener")
        {
            // We may want to mark this as debug only in the future.
            EmbraceLogger.Log("Embrace UnityShakeListener created. IMPORTANT: The Embrace Bug Shake Beta has ended and the feature has been sunset. " +
                              "Any and all bug-shake functionality is no longer supported. " +
                              "Please contact Embrace Customer Success if you have any questions.");
        }
        
        public void onShake()
        {
            #if EMBRACE_ENABLE_BUGSHAKE_FORM
            // We need to record the current timestamp of receipt here so that we can both filter out duplicate shakes as well as capture
            // timeouts if responding to a shake takes too long.
            EmbraceLogger.Log("Received java callback for EmbraceShakeCallback.onShake()");
            BugshakeService.Instance.TakeBugshakeScreenshot();
            BugshakeService.Instance.ShowBugReportForm();
            #endif
        }
    }
    #endif
}

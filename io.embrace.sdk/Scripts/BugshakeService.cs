using System.Collections;
using System.Threading;
using EmbraceSDK.Internal;
using EmbraceSDK.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Bugshake
{
#if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
    [ExcludeFromCoverage]
    internal class BugshakeService
    {
        internal Coroutine _bugReportFormSwapRoutine
        {
            get;
            set;
        }

        private static BugshakeService _instance;
        private bool _isBugReportFormSwapSafe
        {
            get;
            set;
        } = true; // TODO: Set to true by default for now. We need to make this stronger.
        private readonly WaitUntil _waitForSwapSafety = new WaitUntil(() => Instance._isBugReportFormSwapSafe);
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private readonly float _bugReportFormSwapSafetyTimeout = 5.0f; // Set the bug report form swap safety timeout to 5 seconds by default for now.
        
        internal static BugshakeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BugshakeService();
                }
                return _instance;
            }
        }
        
        public void ShowBugReportForm()
        {
            if (Instance._bugReportFormSwapRoutine == null) // This will implicitly debounce the coroutine requests to only allow one at a time.
                Instance._bugReportFormSwapRoutine = CoroutineRunner.Instance.RunCoroutine(Instance.BugReportFormSwapRoutine());
        }
        
        /// <summary>
        /// Embrace users should call this method when they are ready to show the bug report form.
        /// Do not call this method if you are in the middle of a scene transition or other operation that may cause a swap to
        /// the bug report form to occur at an inopportune time.
        /// Calling this after MarkBugReportFormSwapUnsafe has been called is required,
        /// otherwise the swap to the bug report form will be rendered impossible.
        /// </summary>
        internal void MarkBugReportFormSwapSafe()
        {
            _isBugReportFormSwapSafe = true;
        }
        
        /// <summary>
        /// Embrace users should call this method when they are not ready to show the bug report form.
        /// This will prevent the bug report form from being shown until MarkBugReportFormSwapSafe is called.
        /// Call this if you are in the middle of a scene transition or other operation that may cause the bug report form
        /// to be shown at an inopportune time. NOT calling this can result in ANRs or other issues.
        /// </summary>
        public void MarkBugReportFormSwapUnsafe()
        {
            _isBugReportFormSwapSafe = false;
        }
        
        internal IEnumerator BugReportFormSwapRoutine()
        {
            var requestTimestamp = Time.realtimeSinceStartup;
            
            yield return _waitForSwapSafety;
            yield return _waitForEndOfFrame;
            
            if (Time.realtimeSinceStartup - requestTimestamp > _bugReportFormSwapSafetyTimeout)
            {
                EmbraceLogger.LogWarning("Bug report form swap request timeout exceeded. Skipping bug report form swap.");
            }
            else
            {
                Embrace.Instance.provider.ShowBugReportForm();
            }
            _bugReportFormSwapRoutine = null;
        }
        
        internal void RegisterShakeListener()
        {
            CoroutineRunner.Instance.RunCoroutine(ShakeListenerRegistrator());
        }
        
        // It is possible to break this by disabling the Embrace GameObject in the scene before we complete registration.
        // We have no good way of fixing this at the moment because of the design of the Embrace MonoBehaviour.
        internal IEnumerator ShakeListenerRegistrator()
        {
            var androidProvider = Embrace.Instance.provider as Embrace_Android;
            if (androidProvider == null)
            {
                // This implies that we are in the Unity Editor
                yield break;
            }
            var waitFor = new WaitUntil(() => androidProvider.IsReady);
            yield return waitFor;

            Embrace.Instance.provider.setShakeListener(new UnityShakeListener());
        }
        
        internal void TakeBugshakeScreenshot()
        {
            CoroutineRunner.Instance.RunCoroutine(TakeScreenshot());
        }

        internal IEnumerator TakeScreenshot()
        {
            yield return _waitForEndOfFrame;
            Embrace.Instance.provider.saveShakeScreenshot(ScreenshotUtil.TakeScreenshot());
        }
    }
#endif
}
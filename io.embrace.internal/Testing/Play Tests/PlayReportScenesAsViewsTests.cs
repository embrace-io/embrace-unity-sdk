using EmbraceSDK.Demo;
using NSubstitute;
using System.Collections;
using EmbraceSDK.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayReportScenesAsViewsTests : PlayTestBase
    {
        /// <summary>
        /// Tests that the StartViewFromScene function does result in the 
        /// provider receiving the appropriate StartView call.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ReportsStartViewFromScene()
        {
            ProviderSetup();

            var reporter = new EmbraceScenesToViewReporter();
            reporter.StartViewFromScene(SceneManager.GetActiveScene());

            yield return new WaitForSeconds(0.25f);

            Embrace.Instance.provider.Received()
                .StartView(SceneManager.GetActiveScene().name);

            Cleanup();
        }

        /// <summary>
        /// Tests that the EndViewFromScene function does result in the
        /// provider receiving the appropriate EndView call.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ReportsEndViewFromScene()
        {
            ProviderSetup();

            var reporter = new EmbraceScenesToViewReporter();
            reporter.EndViewFromScene(SceneManager.GetActiveScene());

            yield return new WaitForSeconds(0.25f);

            Embrace.Instance.provider.Received()
                .EndView(SceneManager.GetActiveScene().name);

            Cleanup();
        }

        /// <summary>
        /// Tests that the reporter does catch scene changes and reports
        /// them to the provider appropriately.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ReportsScenesAsChanged()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, 0.25f);

            yield return LoadScene(DemoConstants.SCENE_NAME_INTEGRATE, 0.25f);

            Embrace.Instance.provider.Received().EndView(DemoConstants.SCENE_NAME_DEMO_HOME);
            Embrace.Instance.provider.Received().StartView(DemoConstants.SCENE_NAME_INTEGRATE);

            Cleanup();
        }
    }
}
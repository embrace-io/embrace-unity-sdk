using System.Collections;
using EmbraceSDK.Demo;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayBreadcrumbTests : PlayTestBase
    {
        /// <summary>
        /// Tests the button click and AddBreadcrumb() invocation of the Breadcrumb scene.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestBreadcrumb()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_BREADCRUMB, waitSeconds: .25f);

            BreadcrumbDemo demo = GameObject.FindObjectOfType<BreadcrumbDemo>();
            demo.breadcrumbInputField.text = DemoConstants.TEST_MESSAGE;
            demo.breadcrumbSendButton.onClick.Invoke();

            Embrace.Instance.provider.Received().AddBreadcrumb(DemoConstants.TEST_MESSAGE);

            Cleanup();
        }
    }
}
using System.Collections;
using EmbraceSDK.Demo;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayUserTests : PlayTestBase
    {
        /// <summary>
        /// Test the SetUserName(), SetUserEmail(), SetUserIdentifier, and AddUserPersona() invocations of the Users scene.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestUserName()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_USERS, waitSeconds: .25f);

            UserDemo demo = GameObject.FindObjectOfType<UserDemo>();
            demo.userName.text = DemoConstants.TEST_NAME;
            demo.userEmail.text = DemoConstants.TEST_EMAIL;
            demo.userIdentifier.text = DemoConstants.TEST_ID;
            demo.userPersona.text = DemoConstants.TEST_PERSONA;

            yield return PressButton(DemoConstants.BUTTON_NAME_SUBMIT);

            Embrace embrace = Embrace.Instance;
            embrace.provider.Received().SetUsername(DemoConstants.TEST_NAME);
            embrace.provider.Received().SetUserEmail(DemoConstants.TEST_EMAIL);
            embrace.provider.Received().SetUserIdentifier(DemoConstants.TEST_ID);
            embrace.provider.Received().AddUserPersona(DemoConstants.TEST_PERSONA);

            Cleanup();
        }

        /// <summary>
        /// Tests the SetUserAsPayer() and ClearUserAsPayer() invocations of the Users scene.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestSetAsPayer()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_USERS, waitSeconds: .25f);

            // Emulates pressing the setAsPayer button, disables it, and enables the clearAsPayer button
            yield return PressButton(DemoConstants.BUTTON_NAME_SET_PAYER);

            Embrace embrace = Embrace.Instance;
            embrace.provider.Received().SetUserAsPayer();

            // Emulates pressing the clearAsPayer button, disables it, and enables the setAsPayer button
            yield return PressButton(DemoConstants.BUTTON_NAME_CLEAR_PAYER);

            embrace.provider.Received().ClearUserAsPayer();

            Cleanup();
        }
    }
}
using System.Collections;
using EmbraceSDK.Demo;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayLogsTests : PlayTestBase
    {
        /// <summary>
        /// Tests the LogMessage invocation with custom properties in the Logs scene.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestLogExample()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_LOGS, waitSeconds: .25f);

            int propertyCount = 3;

            // Add property item views.
            // Subtracting one since the property view already contains one by default.
            for (int i = 0; i < propertyCount - 1; i++)
            {
                yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_ADD);
            }

            PropertiesItemView[] itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();
            for (int i = 0; i < itemViews.Length; i++)
            {
                PropertiesItemView itemView = itemViews[i];
                itemView.keyInput.text = $"{DemoConstants.TEST_KEY}{i}";
                itemView.valueInput.text = $"{DemoConstants.TEST_MESSAGE}{i}";
                itemView.saveButton.onClick.Invoke();
                yield return null;
            }

            string message = DemoConstants.TEST_MESSAGE;
            int dropDownValue = (int)EMBSeverity.Error;
            EMBSeverity severity = EMBSeverity.Error;
            bool allowScreenshot = true;

            LogsDemo demo = GameObject.FindObjectOfType<LogsDemo>();
            demo.logInputField.text = message;
            demo.EMBSeverityDropdown.value = dropDownValue;

            PropertiesController controller = GameObject.FindObjectOfType<PropertiesController>();

            yield return PressButton(DemoConstants.BUTTON_NAME_SEND);

            Embrace.Instance.provider.Received().LogMessage(
                message,
                severity,
                controller.properties
            );

            Cleanup();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates how to use the Breadcrumb API. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/breadcrumbs/
    /// </summary>
    public class BreadcrumbDemo : DemoBase
    {
        [Header("Breadcrumb Example")]
        public Button breadcrumbSendButton;
        public InputField breadcrumbInputField;

        private void Start()
        {
            Embrace.Instance.StartSDK();
            breadcrumbSendButton.onClick.AddListener(HandleBreadcrumbSendClick);
        }

        /// <summary>
        /// Example of using breadcrumbs.
        /// </summary>
        /// <param name="message"></param>
        private void BreadcrumbExample(string message)
        {
            // Use breadcrumbs to track the journey of the user through your application.
            Embrace.Instance.AddBreadcrumb(message);
        }

        private void HandleBreadcrumbSendClick()
        {
            BreadcrumbExample(breadcrumbInputField.text);
        }
    }
}

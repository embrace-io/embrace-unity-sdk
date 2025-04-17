using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates how to use the log message API. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/log-message-api/
    /// </summary>
    public class LogsDemo : DemoBase
    {
        [Header("Logs Example")]
        public Button LogSendButton;
        public InputField logInputField;
        public Dropdown EMBSeverityDropdown;
        public PropertiesController propertiesController;
        
        private void Start()
        {
            LogSendButton.onClick.AddListener(HandleLogSendClick);
        }

        /// <summary>
        /// Example of using the Log Message API.
        /// </summary>
        /// <param name="message">the name of the message, which is how it will show up on the dashboard</param>
        /// <param name="severity">will flag the message as one of info, warning, or error for filtering on the dashboard</param>
        /// <param name="properties">an optional dictionary of up to 10 key/value pairs</param>
        private void LogExample(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            Embrace.Instance.LogMessage(message, severity, properties);

            //The maximum length for a log message is 128 characters.Messages are truncated if they exceed the limit.
            //Properties are limited to 10 per log.
            //Property keys have a limit of 128 characters.
            //Property values have a limit of 256 characters.
        }

        #region HelperFunctions
        private void HandleLogSendClick()
        {
            EMBSeverity eMBSeverity = (EMBSeverity)Enum.Parse(typeof(EMBSeverity), EMBSeverityDropdown.options[EMBSeverityDropdown.value].text);
            LogExample(logInputField.text, eMBSeverity, propertiesController.properties);
        }
        #endregion
    }
}

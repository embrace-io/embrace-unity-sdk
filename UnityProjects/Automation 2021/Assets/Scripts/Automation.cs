using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Automation
{
    public class Automation : MonoBehaviour
    {
        [SerializeField] private Button AddBreadcrumbButton;
        [SerializeField] private Button LogInfoButton;
        [SerializeField] private Button LogInfoWithPropertiesButton;
        [SerializeField] private Button LogMessageWithAttachmentUrlButton;
        [SerializeField] private Button LogMessageWithAttachmentButton;
        [SerializeField] private Button LogWarningButton;
        [SerializeField] private Button LogErrorButton;
        
        private void Awake()
        {
            // TODO: Pull app id from the environment variables
            EmbraceStartupArgs args = new EmbraceStartupArgs("");
            Embrace.Instance.StartSDK(args);

            Embrace.Instance.SetUsername(AutomationConstants.AUTOMATION_USERNAME);
            Embrace.Instance.SetUserEmail(AutomationConstants.AUTOMATION_USERNAME);
            Embrace.Instance.SetUserIdentifier(AutomationConstants.AUTOMATION_USERNAME);
            Embrace.Instance.AddUserPersona(AutomationConstants.AUTOMATION_USERNAME);
            Embrace.Instance.SetUserAsPayer();
        }

        private void Start()
        {
            AddBreadcrumbButton.onClick.AddListener(AddBreadcrumb);
            LogInfoButton.onClick.AddListener(LogInfo);
            LogInfoWithPropertiesButton.onClick.AddListener(LogInfoWithProperties);
            LogMessageWithAttachmentUrlButton.onClick.AddListener(LogMessageWithAttachmentUrl);
            LogMessageWithAttachmentButton.onClick.AddListener(LogMessageWithAttachment);
            LogWarningButton.onClick.AddListener(LogWarning);
            LogErrorButton.onClick.AddListener(LogError);
        }
        
        private void AddBreadcrumb()
        {
            Embrace.Instance.AddBreadcrumb(AutomationConstants.AUTOMATION_BREADCRUMB);
        }

        private void LogInfo()
        {
            Embrace.Instance.LogMessage(AutomationConstants.AUTOMATION_LOG_INFO, EMBSeverity.Info);
        }
        
        private void LogInfoWithProperties()
        {
            Embrace.Instance.LogMessage(AutomationConstants.AUTOMATION_LOG_INFO, EMBSeverity.Info, new Dictionary<string, string>
            {
                {AutomationConstants.AUTOMATION_LOG_KEY, AutomationConstants.AUTOMATION_LOG_INFO}
            });
        }

        private void LogMessageWithAttachmentUrl()
        {
            Embrace.Instance.LogMessage(AutomationConstants.AUTOMATION_LOG_INFO, EMBSeverity.Info, null, 
                AutomationConstants.AUTOMATION_ATTACHMENT_ID, AutomationConstants.AUTOMATION_ATTACHMENT_URL);
        }

        private void LogMessageWithAttachment()
        {
            byte[] attachmentBytes = System.Text.Encoding.UTF8.GetBytes(AutomationConstants.AUTOMATION_ATTACHMENT);
            sbyte[] attachmentSBytes = new sbyte[attachmentBytes.Length];
            
            for (int i = 0; i < attachmentBytes.Length; i++)
            {
                attachmentSBytes[i] = (sbyte) attachmentBytes[i];
            }
            
            Embrace.Instance.LogMessage(AutomationConstants.AUTOMATION_LOG_INFO, EMBSeverity.Info, null, attachmentSBytes);
        }

        private void LogWarning()
        {
            Embrace.Instance.LogWarning(AutomationConstants.AUTOMATION_LOG_WARNING);
        }
        
        private void LogError()
        {
            Embrace.Instance.LogError(AutomationConstants.AUTOMATION_LOG_ERROR);
        }
    }
}
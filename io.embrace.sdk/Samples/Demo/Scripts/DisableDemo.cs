using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    public class DisableDemo : DemoBase
    {
        [SerializeField] private Button DisableButton;
        [SerializeField] private Button LogMessageButton;
        
        private void Start()
        {
            DisableButton.onClick.AddListener(HandleDisable);
            LogMessageButton.onClick.AddListener(HandleLogMessage);
        }
        
        private void HandleDisable()
        {
            Embrace.Instance.Disable();
        }
        
        private void HandleLogMessage()
        {
            if (Embrace.Instance.IsEnabled == false)
            {
                EmbraceLogger.LogError("The Embrace SDK is disabled. Please enable it to log messages.");
                return;
            }
            
            EmbraceLogger.Log("SDK is enabled. Logging message...");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    public class DisableDemo : DemoBase
    {
        [SerializeField] private Button DisableButton;
        
        private void Start()
        {
            DisableButton.onClick.AddListener(HandleDisable);
        }
        
        private void HandleDisable()
        {
            Embrace.Instance.Disable();
        }
    }
}
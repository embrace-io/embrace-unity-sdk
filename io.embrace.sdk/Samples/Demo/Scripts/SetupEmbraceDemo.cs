using EmbraceSDK.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demonstrates how to initialize the EmbraceSDK. For more information please see our documentation.
    /// https://embrace.io/docs/unity/integration/session-reporting/
    /// </summary>
    public class SetupEmbraceDemo : MonoBehaviour
    {
        public string AppId = "abcde";
        #if DeveloperMode
        public string AppGroupId = "";
        public string BaseUrl = "http://your-url.com";
        public string DevBaseUrl = "http://your-url.com";
        public string ConfigBaseUrl = "http://your-url.com";
        #endif

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        
        void Start()
        {
            #if DeveloperMode && UNITY_IOS
            // This setup is for Embrace Developer Mode on iOS only.
            Embrace.Instance.StartSDK(new EmbraceStartupArgs(AppId, 
                EmbraceConfig.Default,
                AppGroupId.Length > 0 ? AppGroupId : null, 
                BaseUrl.Length > 0 ? BaseUrl : null, 
                DevBaseUrl.Length > 0 ? DevBaseUrl : null, 
                ConfigBaseUrl.Length > 0 ? ConfigBaseUrl : null));
            #elif UNITY_IOS
            // This setup is for Embrace on iOS only.
            Embrace.Instance.StartSDK(new EmbraceStartupArgs(AppId, EmbraceConfig.Default, null, null, null, null));
            #else
            // This setup is for Embrace on Android.
            Embrace.Instance.StartSDK();
            #endif
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using EmbraceSDK.Demo;
using EmbraceSDK.Internal;
using NSubstitute;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlaySetupTests : PlayTestBase
    {
        // These values need to match the DemoScene SetupEmbraceDemo.cs values.
        // Patching this in here for now. We should ideally pull from the scene, but that may be a bit complicated.
        public string AppId = "abcde";
        public string AppGroupId = "";
        public string BaseUrl = "http://127.0.0.1:8989/api";
        public string DevBaseUrl = "http://127.0.0.1:8989/api";
        public string ConfigBaseUrl = "http://127.0.0.1:8989/api";
        
        /// <summary>
        /// Tests the StartSDK() in the Demo Home scene.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestSetup()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);
            
#if DeveloperMode && UNITY_IOS
            // This setup is for Embrace Developer Mode on iOS only.
            embrace.provider.Received().StartSDK(new EmbraceStartupArgs(AppId, 
                EmbraceConfig.Default,
                AppGroupId.Length > 0 ? AppGroupId : null, 
                BaseUrl, 
                DevBaseUrl, 
                ConfigBaseUrl));
#elif UNITY_IOS
            // This setup is for Embrace on iOS only.
            embrace.StartSDK(new EmbraceStartupArgs(AppId, EmbraceConfig.Default, null, null, null, null));
#else
            // This setup is for Embrace on Android.
            embrace.StartSDK();
#endif

            Cleanup();
        }
    }
}
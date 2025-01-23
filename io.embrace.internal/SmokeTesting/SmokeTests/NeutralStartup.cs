using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

namespace Embrace.Internal.SmokeTests
{
    public class NeutralStartup 
    {
        /// <summary>
        /// Contains just a single function for the purpose of
        /// uploading crash reports to the mock api server
        /// </summary>
        [ExcludeFromCoverage]
        public class NeutralStartupFunctions
        {
            [Preserve, SmokeTest]
            public void StartSDKForReport()
            {
                EmbraceSDK.Embrace.Instance.StartSDK();
            }
        }
    }
}

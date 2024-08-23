using EmbraceSDK.Internal;
using UnityEngine;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demonstrates how to initialize and measure app startup times. For more information please see our documentation.
    /// https://embrace.io/docs/unity/integration/session-reporting/
    /// </summary>
    public class SetupEmbraceDemo : MonoBehaviour
    {
        void Start()
        {
            /*/ Old method
            // Required to initialize the Embrace SDK and make the API functional.
            Embrace.Instance.StartSDK();

            // Invoke your application startup methods here.

            // Call EndAppStartup if you'd like to measure how long your application takes to start up.
            Embrace.Instance.EndAppStartup();
            /*/
            var iOS6 = new Embrace_iOS6();
            var result = iOS6.EmbraceSDKIsStarted();
            Debug.Log($"Embrace is started call across iOS to Swift: {result}");
            //*/
        }
    }
}

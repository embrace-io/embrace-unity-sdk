using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates how to use the ANR API. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/
    /// </summary>
    public class AnrDemo : DemoBase
    {
        [Header("ANR Example")]
        public Button anrButton;

        private void Start()
        {
            anrButton.onClick.AddListener(HandleTriggerAnrClick);
        }

        /// <summary>
        /// Example code that triggers an ANR in an application.
        /// </summary>
        private void HandleTriggerAnrClick()
        {
            BlockAndroidMainThread();
            BlockUnityMainThread();
        }

        /// <summary>
        /// Blocks the Android main thread for 10s which helps trigger an ANR.
        /// This is _not_ the same thread as the main Unity thread.
        /// </summary>
        private void BlockAndroidMainThread() {
#if UNITY_ANDROID && !UNITY_EDITOR
            using(AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using(AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        Debug.Log("About to block Android main thread.");
                        Thread.Sleep(10000);
                        Debug.Log("No longer blocking Android main thread.");
                    }));
                }
            }
#endif
        }

        /// <summary>
        /// Blocks the Unity main thread for 10s.
        /// </summary>
        private void BlockUnityMainThread() {
            Debug.Log("About to block Unity Main thread.");
            Thread.Sleep(10000);
            Debug.Log("No longer blocking Unity Main thread.");
        }
    }
}

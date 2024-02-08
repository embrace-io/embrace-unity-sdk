using UnityEngine;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// Base class to help setup scenes and Embrace SDK.
    /// </summary>
    public class DemoBase : MonoBehaviour
    {
        protected void Awake()
        {
            Embrace.Instance.StartSDK();

            #if UNITY_2022_3_OR_NEWER
            var sceneSelector = FindAnyObjectByType<SceneSelector>();
            #else
            var sceneSelector = FindObjectOfType<SceneSelector>();
            #endif
            if (sceneSelector != null)
            {
                GameObject go = new GameObject("sceneSelector");
                go.AddComponent(typeof(SceneSelector));
            }

            Embrace.Instance.EndAppStartup();
        }
    }
}

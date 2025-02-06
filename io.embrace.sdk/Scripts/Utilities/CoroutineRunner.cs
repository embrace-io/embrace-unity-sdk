using System.Collections;
using UnityEngine;

namespace EmbraceSDK.Utilities
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;
        private static bool initialized = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (initialized)
            {
                Destroy(gameObject);
                return;
            }

            initialized = true;
        }

        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    instance = FindFirstObjectByType<CoroutineRunner>();
#else
                    instance = FindObjectOfType<CoroutineRunner>();
#endif

                    if (instance == null)
                    {
                        GameObject gameObject = new GameObject("CoroutineRunner");
                        instance = gameObject.AddComponent<CoroutineRunner>();
                    }
                }

                return instance;
            }
        }

        public Coroutine RunCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        public new void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}

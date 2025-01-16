using System.Collections;
using EmbraceSDK.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace EmbraceSDK.Tests
{
    public class PlayEmbraceTests : IEmbraceTest
    {
        /// <summary>
        /// Test if the provider is setup correctly.
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(1)]
        public IEnumerator ProviderSetup()
        {
            Embrace embrace = Embrace.Instance;
            yield return new WaitForSeconds(1f);

#if UNITY_ANDROID && !UNITY_EDITOR
            Assert.AreEqual(embrace.provider.GetType(), typeof(Embrace_Android));
#elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            Assert.AreEqual(embrace.provider.GetType(), typeof(Embrace_iOS));
#else
            Assert.AreEqual(embrace.provider.GetType(), typeof(Embrace_Stub));
#endif
            Cleanup();
        }

        /// <summary>
        /// Test if there is only one instance after Create() is called.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnlyOneInstanceAfterCreate()
        {
            GameObject tempGo = new GameObject();
            tempGo.AddComponent<Embrace>();

            yield return new WaitForFixedUpdate();

            Embrace embrace = Embrace.Create();

            Embrace[] components = GameObject.FindObjectsOfType<Embrace>();
            Assert.AreEqual(components.Length, 1);
            Cleanup();
        }

        /// <summary>
        /// Test if there is only one instance after calling Embrace.Instance.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnlyOneInstanceAfterEmbrace_Instance()
        {
            GameObject tempGo = new GameObject();
            tempGo.AddComponent<Embrace>();

            yield return new WaitForFixedUpdate();

            Embrace embrace = Embrace.Instance;

            Embrace[] components = GameObject.FindObjectsOfType<Embrace>();
            Assert.AreEqual(components.Length, 1);
            Cleanup();
        }

        /// <summary>
        /// Make sure that there is only one instance at any given time, if a scene has two Gameobjects with the Embrace Component.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnlyOneInstance()
        {
            GameObject tempGo = new GameObject();
            tempGo.AddComponent<Embrace>();

            GameObject tempGo2 = new GameObject();
            tempGo2.AddComponent<Embrace>();

            yield return new WaitForSeconds(1f);

            Embrace[] components = GameObject.FindObjectsOfType<Embrace>();
            Assert.AreEqual(components.Length, 1);
            Cleanup();
        }

        /// <summary>
        /// Test if start returns an instance.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator InstanceFromStart()
        {
            GameObject tempGo = new GameObject();
            Embrace embrace = tempGo.AddComponent<Embrace>();
            yield return new WaitForFixedUpdate();

            Assert.IsNotNull(Embrace.Instance);
            Cleanup();
        }

        /// <summary>
        /// Test if Embrace.Instance returns an instance.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator EmbraceInstance()
        {
            Embrace embrace = Embrace.Instance;
            yield return new WaitForFixedUpdate();
            Assert.IsNotNull(embrace);
            Cleanup();
        }

        /// <summary>
        /// Try Starting SDK before it has been Initialized. It should still work.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator StartSDKBeforeInitialize()
        {
            GameObject tempGo = new GameObject();
            Embrace embrace = tempGo.AddComponent<Embrace>();
            embrace.StartSDK();
            yield return new WaitForFixedUpdate();
            Assert.IsNotNull(embrace);
            Cleanup();
        }

        public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(Embrace.Instance.gameObject);
        }
    }
}
using System.Collections;
using EmbraceSDK.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace EmbraceSDK.Tests
{
    public class PlayEmbraceTests : IEmbraceTest
    {
        [SetUp]
        public void Setup()
        {
            Embrace.Stop();
        }
        
        /// <summary>
        /// Test if the provider is setup correctly.
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(1)]
        public IEnumerator ProviderSetup()
        {
            Embrace.Start();
            yield return new WaitForSeconds(1f);

#if UNITY_ANDROID && !UNITY_EDITOR
            Assert.AreEqual(embrace.provider.GetType(), typeof(Embrace_Android));
#elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            Assert.AreEqual(embrace.provider.GetType(), typeof(Embrace_iOS));
#else
            Assert.AreEqual(Embrace.Instance.provider.GetType(), typeof(Embrace_Stub));
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
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            EmbraceUnityListener[] components = Object.FindObjectsOfType<EmbraceUnityListener>();
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
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            EmbraceUnityListener[] components = Object.FindObjectsOfType<EmbraceUnityListener>();
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
            var embrace1 = new Embrace();
            embrace1.StartSDK();
            yield return new WaitForSeconds(1f);
            var embrace2 = new Embrace();
            embrace2.StartSDK();
            yield return new WaitForSeconds(1f);

            EmbraceUnityListener[] components = Object.FindObjectsOfType<EmbraceUnityListener>();
            Assert.AreEqual(1, components.Length);
            Cleanup();
        }

        /// <summary>
        /// Test if start returns an instance.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator InstanceFromStart()
        {
            Embrace.Start();
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
            Embrace.Start();
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
            Embrace embrace = new Embrace();
            embrace.StartSDK();
            yield return new WaitForFixedUpdate();
            Assert.IsNotNull(embrace.listener);
            Cleanup();
        }

        public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(Embrace.Instance.listener);
        }
    }
}
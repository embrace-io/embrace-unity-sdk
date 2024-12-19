using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests 
{
    #if UNITY_2020_2_OR_NEWER
    public class SceneManagerOverrideTests
    {
        [UnityTest, Order(1)]
        public IEnumerator SceneManagerOverrideMarksSafeAndUnsafe()
        {
            Action onSceneLoadStarted = Substitute.For<Action>();
            Action onSceneLoadFinished = Substitute.For<Action>();
            SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(onSceneLoadStarted, onSceneLoadFinished);
            
            yield return new WaitForSeconds(0.25f);
            
            SceneManager.LoadScene(1);
            
            yield return new WaitForSeconds(0.25f);
            
            onSceneLoadStarted.Received().Invoke();
            onSceneLoadFinished.Received().Invoke();
        }
    }
    #endif
}
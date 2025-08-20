using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK.Utilities
{
    public static class SceneLoadMeasurer
    {
        private static string _currentSceneLoadSpanId;
        
        private static void OnSceneLoadStarted(string sceneName)
        {
            if (Embrace.Instance.IsStarted == false)
            {
                Debug.LogWarning("Unable to start scene load span because Embrace is not started.");
                return;
            }

            _currentSceneLoadSpanId = Embrace.Instance.StartSpan($"Load Scene: {sceneName}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private static void OnSceneLoadFinished(string sceneName)
        {
            if (string.IsNullOrEmpty(_currentSceneLoadSpanId))
            {
                return;
            }
            
            Embrace.Instance.StopSpan(_currentSceneLoadSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            _currentSceneLoadSpanId = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnLoad()
        {
            SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(OnSceneLoadStarted, OnSceneLoadFinished);
        }
    }
}
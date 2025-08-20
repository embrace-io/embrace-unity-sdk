using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK.Utilities
{
    public class SceneLoadMeasurer : MonoBehaviour
    {
        private string _currentSceneLoadSpanId;
        
        private void Awake()
        {
            // TODO: Use weaving to determine if the client has already overridden the SceneManagerAPI.
            // if they have, then add our helper functions to their existing overrides.
            SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(OnSceneLoadStarted, OnSceneLoadFinished);
        }
        
        private void OnSceneLoadStarted(string sceneName)
        {
            if (Embrace.Instance.IsStarted == false)
            {
                Debug.LogWarning("Unable to start scene load span because Embrace is not started.");
                return;
            }

            _currentSceneLoadSpanId = Embrace.Instance.StartSpan($"Load Scene: {sceneName}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private void OnSceneLoadFinished(string sceneName)
        {
            if (string.IsNullOrEmpty(_currentSceneLoadSpanId))
            {
                return;
            }
            
            Embrace.Instance.StopSpan(_currentSceneLoadSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
}
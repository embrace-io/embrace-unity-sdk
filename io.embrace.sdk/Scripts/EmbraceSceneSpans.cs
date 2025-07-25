using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK
{
    /// <summary>
    /// Note: This class requires the EmbraceSceneManagerOverride to be set in the SceneManagerAPI.
    /// Example: SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(onSceneLoadStarted, onSceneLoadFinished);
    /// </summary>
    public static class EmbraceSceneSpans
    {
        private static readonly Dictionary<string, string> _sceneSpanIds = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RuntimeInitialize()
        {
            SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(StartSceneSpan, StopSceneSpan);
        }

        private static void StartSceneSpan(string sceneName)
        {
            if (Embrace.Instance.IsStarted && _sceneSpanIds.ContainsKey(sceneName) == false)
            {
                var spanId = Embrace.Instance.StartSpan($"SceneLoad", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Embrace.Instance.AddSpanAttribute(spanId, "SceneName", sceneName);
                _sceneSpanIds[sceneName] = spanId;
            }
        }

        private static void StopSceneSpan(string sceneName)
        {
            if (Embrace.Instance.IsStarted && _sceneSpanIds.TryGetValue(sceneName, out string spanId))
            {
                Embrace.Instance.StopSpan(spanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                _sceneSpanIds.Remove(sceneName);
            }
        }
    }
}
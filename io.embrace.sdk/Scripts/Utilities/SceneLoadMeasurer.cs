#if EMBRACE_SCENE_LOAD_SPANS

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK.Utilities
{
    /// <summary>
    /// Measures scene load times using Unity's SceneManagerAPI override.
    /// If you are already using a custom SceneManagerAPI override, this will not work or may conflict.
    /// </summary>
    public static class SceneLoadMeasurer
    {
        private static List<string> _sceneAllowList;
        private static string _currentSceneLoadSpanId;
        
        /// <summary>
        /// Call this function with a list of scenes you want to measure. If this is not called, all scenes will be measured.
        /// </summary>
        /// <param name="sceneAllowList"></param>
        public static void SetSceneAllowList(List<string> sceneAllowList)
        {
            _sceneAllowList = sceneAllowList;
        }
        
        private static void OnSceneLoadStarted(string sceneName)
        {
            if (Embrace.Instance.IsStarted == false)
            {
                Debug.LogWarning("Unable to start scene load span because Embrace is not started.");
                return;
            }

            if (_sceneAllowList is { Count: > 0 } && _sceneAllowList.Contains(sceneName) == false)
            {
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
#endif
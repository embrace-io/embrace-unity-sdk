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

            // If we have any scenes in the allow list, only measure those scenes.
            if (_sceneAllowList is { Count: > 0 } && !_sceneAllowList.Contains(sceneName))
            {
                return;
            }

            string spanName = $"scene-{sceneName}-loaded";
            
            if (EmbraceSpanIdTracker.HasSpanId(spanName))
            {
                Debug.LogWarning($"A scene load span for scene '{sceneName}' is already in progress. This may indicate that a previous scene of the same name load did not finish properly.");
                return;
            }
            
            string spanId = Embrace.Instance.StartSpan(spanName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            EmbraceSpanIdTracker.AddSpanId(spanName, spanId);
        }

        private static void OnSceneLoadFinished(string sceneName)
        {
            string spanId = EmbraceSpanIdTracker.GetSpanId($"scene-{sceneName}-loaded");
            
            if (string.IsNullOrEmpty(spanId))
            {
                return;
            }
            
            Embrace.Instance.StopSpan(spanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            EmbraceSpanIdTracker.RemoveSpanId($"scene-{sceneName}-loaded");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnLoad()
        {
            SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(OnSceneLoadStarted, OnSceneLoadFinished);
        }
    }
}
#endif
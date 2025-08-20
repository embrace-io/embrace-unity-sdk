using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmbraceSDK
{
#if UNITY_2020_2_OR_NEWER
    /// <summary>
    /// This current solution doesn't REALLY work because customers can override the SceneManagerAPI themselves. We need to find a way to
    /// weave this in at compile time. This is a temporary solution for now.
    /// </summary>
    public class EmbraceSceneManagerOverride : SceneManagerAPI
    {
        // This pattern is required to handle additive scene loading. It's overkill for single scene loads, unfortunately.
        private readonly List<(string sceneName, int sceneBuildIndex)> _scenesCurrentlyBeingLoaded =
            new List<(string sceneName, int sceneBuildIndex)>();

        private readonly Action<string> _onSceneLoadStarted;
        private readonly Action<string> _onSceneLoadFinished;
        public EmbraceSceneManagerOverride(Action<string> onSceneLoadStarted, Action<string> onSceneLoadFinished)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _onSceneLoadStarted = onSceneLoadStarted;
            _onSceneLoadFinished = onSceneLoadFinished;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // We need to remove the scene from the set of scenes currently being loaded.
            RemoveScene(scene.name, scene.buildIndex);

            if (_scenesCurrentlyBeingLoaded.Count == 0)
            {
                _onSceneLoadFinished?.Invoke(scene.name);
            }
        }

        private void RemoveScene(string name, int index)
        {
            for (int i = 0; i < _scenesCurrentlyBeingLoaded.Count; i++)
            {
                if (_scenesCurrentlyBeingLoaded[i].sceneName == name || _scenesCurrentlyBeingLoaded[i].sceneBuildIndex == index)
                {
                    _scenesCurrentlyBeingLoaded.RemoveAt(i);
                    return;
                }
            }
        }
        
        protected override AsyncOperation LoadSceneAsyncByNameOrIndex(string sceneName, int sceneBuildIndex, LoadSceneParameters parameters,
            bool mustCompleteNextFrame)
        {
            _scenesCurrentlyBeingLoaded.Add((sceneName, sceneBuildIndex));
            _onSceneLoadStarted?.Invoke(sceneName);
            return base.LoadSceneAsyncByNameOrIndex(sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame);
        }
    }
#endif
}
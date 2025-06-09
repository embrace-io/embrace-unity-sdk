using System;
using UnityEngine;

namespace EmbraceSDK
{
    public class EmbraceUnityListener : MonoBehaviour
    {
        private Action _onDestroyCallback;
        private Action<bool> _onApplicationPauseCallback;

        public void SetOnDestroyCallback(Action onDestroyCallback)
        {
            _onDestroyCallback = onDestroyCallback;
        }
        
        public void SetOnApplicationPauseCallback(Action<bool> onApplicationPauseCallback)
        {
            _onApplicationPauseCallback = onApplicationPauseCallback;
        }
        
        private void OnDestroy()
        {
            _onDestroyCallback?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            _onApplicationPauseCallback?.Invoke(pause);
        }
    }
}
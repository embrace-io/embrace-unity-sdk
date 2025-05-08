using System;
using UnityEngine;

namespace EmbraceSDK
{
    public class EmbraceUnityListener : MonoBehaviour
    {
        private Action _onDestroyCallback;

        public void SetOnDestroyCallback(Action onDestroyCallback)
        {
            _onDestroyCallback = onDestroyCallback;
        }
        
        private void OnDestroy()
        {
            _onDestroyCallback?.Invoke();
        }
    }
}
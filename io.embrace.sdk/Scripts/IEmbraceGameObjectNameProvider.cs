using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    public interface IEmbraceGameObjectNameProvider
    {
        /// <summary>
        /// Retrieves the name of a GameObject for Embrace instrumentation.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public string GetGameObjectName(GameObject gameObject);
    }
}
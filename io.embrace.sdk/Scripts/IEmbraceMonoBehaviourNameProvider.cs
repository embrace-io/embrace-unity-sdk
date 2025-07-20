using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    /// <summary>
    /// Interface for providing names from for Embrace instrumentation.
    /// </summary>
    public interface IEmbraceMonoBehaviourNameProvider
    {
        /// <summary>
        /// Returns the name of the game object from a monobehaviour for Embrace instrumentation.
        /// </summary>
        /// <returns></returns>
        public string GetName();
    }
}
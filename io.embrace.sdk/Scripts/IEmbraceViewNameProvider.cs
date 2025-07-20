using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    public interface IEmbraceViewNameProvider
    {
        /// <summary>
        /// Sets the name of the current view.
        /// </summary>
        void SetViewName(string viewName);
        
        /// <summary>
        /// Provides the name of the current view.
        /// </summary>
        /// <returns>The name of the view.</returns>
        string GetViewName();

        /// <summary>
        /// Provides the name of the current view, with a fallback to a default value if not available.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the view name is not available.</param>
        /// <returns>The name of the view or the default value.</returns>
        string GetViewName(string defaultValue);
    }
}
using UnityEngine;

namespace EmbraceSDK
{
    /// <summary>
    /// AutoViewCapture is a helper component that automatically starts and ends a view when the GameObject is enabled or disabled.
    /// Add it to your game objects that you want to capture as views in Embrace.
    /// </summary>
    public class AutoViewCapture : MonoBehaviour
    {
        [SerializeField] private string ViewName = string.Empty;

        /// <summary>
        /// Unity function called when the associated GameObject is enabled.
        /// </summary>
        private void OnEnable()
        {
            string viewName = string.IsNullOrEmpty(ViewName) ? gameObject.name : ViewName;
            Embrace.Instance.StartView(viewName);
        }

        /// <summary>
        /// Unity function called when the associated GameObject is disabled.
        /// </summary>
        private void OnDisable()
        {
            string viewName = string.IsNullOrEmpty(ViewName) ? gameObject.name : ViewName;
            Embrace.Instance.EndView(viewName);
        }

        /// <summary>
        /// Inherited classes can implement this method to hide the view. Useful for when you have a UI element that
        /// doesn't enable/disable but you still want to capture it's state. For example if you have a sliding view that
        /// only slides in and out, you can use this method to capture the state of the view when it is not visible.
        /// </summary>
        public virtual void HideView()
        {
            
        }
        
        /// <summary>
        /// Inherited classes can implement this method to show the view. Useful for when you have a UI element that
        /// doesn't enable/disable but you still want to capture it's state. For example if you have a sliding view that
        /// only slides in and out, you can use this method to capture the state of the view when it is visible.
        /// </summary>
        public virtual void ShowView()
        {
            
        }
    }
}
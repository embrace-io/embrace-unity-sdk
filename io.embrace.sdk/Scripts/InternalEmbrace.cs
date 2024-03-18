namespace EmbraceSDK.Internal
{
    /// <summary>
    /// This class is used to expose internal methods to the SDK. It should not be used by any other code.
    /// </summary>
    internal static class InternalEmbrace
    {
        /// <summary>
        /// The singleton instance of the Embrace class used for internal purposes only.
        /// </summary>
        private static Embrace _instance;
        
        internal static void SetInternalInstance(Embrace instance) => _instance = instance;
        
        /// <summary>
        /// An alternative to the Instance property which will not instantiate a new instance if the singleton is null.
        /// </summary>
        /// <returns>The singleton instance, or null if it does not exist</returns>
        internal static Embrace GetExistingInstance() => _instance;
    }
}
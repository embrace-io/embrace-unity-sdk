namespace EmbraceSDK
{
    /// <summary>
    /// These are used to configure the Unity Embrace SDK via code,
    /// as this is the supported method for initialization on iOS.
    ///
    /// As a result, this is REQUIRED for iOS.
    /// It is IGNORED for Android.
    /// </summary>
    public struct EmbraceStartupArgs
    {
        /// <summary>
        /// The AppId for the app you are integrating with Embrace
        /// This is the only item that is required.
        /// </summary>
        public string AppId;
        
        /// <summary>
        /// The AppGroupId for the app you are integrating with Embrace
        /// </summary>
        public string AppGroupId;
        
        /// <summary>
        /// The base url for the Embrace API for redirecting requests.
        /// This is primarily used for testing.
        /// </summary>
        public string BaseUrl;
        
        /// <summary>
        /// The dev base url for the Embrace API for redirecting requests.
        /// This is primarily used for testing.
        /// </summary>
        public string DevBaseUrl;
        
        /// <summary>
        /// The config base url for the Embrace API for redirecting requests.
        /// This is primarily used for testing.
        /// </summary>
        public string ConfigBaseUrl;
        
        /// <summary>
        /// The default constructor for the EmbraceStartupArgs.
        /// Null values are provided for all optional parameters.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appGroupId"></param>
        /// <param name="baseUrl"></param>
        /// <param name="devBaseUrl"></param>
        /// <param name="configBaseUrl"></param>
        public EmbraceStartupArgs(string appId, string appGroupId=null, string baseUrl=null, string devBaseUrl=null, string configBaseUrl=null)
        {
            AppId = appId;
            AppGroupId = appGroupId;
            BaseUrl = baseUrl;
            DevBaseUrl = devBaseUrl;
            ConfigBaseUrl = configBaseUrl;
        }
    }
}
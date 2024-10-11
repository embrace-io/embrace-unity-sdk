namespace EmbraceSDK
{
    /// <summary>
    /// These are used to configure the Unity Embrace SDK via code,
    /// as this is the supported method for initialization on iOS.
    ///
    /// As a result, this is REQUIRED for iOS.
    /// It is IGNORED for Android.
    /// </summary>
    public class EmbraceStartupArgs
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
        /// The native configuration for the Embrace SDK; currently only applies to iOS.
        /// </summary>
        public EmbraceConfig Config;
        
        /// <summary>
        /// Default constructor provided primarily for internal testing purposes.
        /// </summary>
        public EmbraceStartupArgs() {}
        
        /// <summary>
        /// The recommended constructor for the EmbraceStartupArgs.
        /// Null values are provided for all optional parameters.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="config"></param>
        /// <param name="appGroupId"></param>
        /// <param name="baseUrl"></param>
        /// <param name="devBaseUrl"></param>
        /// <param name="configBaseUrl"></param>
        public EmbraceStartupArgs(string appId,
            EmbraceConfig config = EmbraceConfig.Default,
            string appGroupId=null, 
            string baseUrl=null, 
            string devBaseUrl=null, 
            string configBaseUrl=null)
        {
            AppId = appId;
            AppGroupId = appGroupId;
            BaseUrl = baseUrl;
            DevBaseUrl = devBaseUrl;
            ConfigBaseUrl = configBaseUrl;
            Config = config;
        }

        /// <summary>
        /// Override equality to force comparison by data members.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EmbraceStartupArgs other = (EmbraceStartupArgs) obj;
            return AppId == other.AppId 
                   && AppGroupId == other.AppGroupId 
                   && BaseUrl == other.BaseUrl 
                   && DevBaseUrl == other.DevBaseUrl 
                   && ConfigBaseUrl == other.ConfigBaseUrl
                   && Config == other.Config;
            
        }
    }
}
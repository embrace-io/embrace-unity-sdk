namespace EmbraceSDK.Internal
{
    public struct EmbraceStartupArgs
    {
        public string AppId;
        public string AppGroupId;
        public string BaseUrl;
        public string DevBaseUrl;
        public string ConfigBaseUrl;
        
        public EmbraceStartupArgs(string appId, string appGroupId, string baseUrl, string devBaseUrl, string configBaseUrl)
        {
            AppId = appId;
            AppGroupId = appGroupId;
            BaseUrl = baseUrl;
            DevBaseUrl = devBaseUrl;
            ConfigBaseUrl = configBaseUrl;
        }
    }
}
namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Information on the current SDK version. Data is used when determining if SDK is being updated. Class is saved to the device as a JSON string at Application.persistentDataPath + "/DeviceSDKInfo.json"
    /// </summary>
    public class DeviceSDKInfo
    {
        public string version;
        public bool isManifestSetup;
    }
}
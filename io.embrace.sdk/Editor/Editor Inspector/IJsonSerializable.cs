namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Use this interface on serializable classes and structs that contain optional SDK fields that can be omitted at build time.
    /// </summary>
    public interface IJsonSerializable
    {
        bool ShouldSerialize();
    }
}
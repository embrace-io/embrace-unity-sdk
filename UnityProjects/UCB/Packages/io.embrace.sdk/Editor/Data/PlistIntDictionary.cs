using System;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Wrapper class that allows the underlying generic custom dictionary to be findable through
    /// the custom editor serializedObject.FindProperty() method (since generic properties aren't found).
    /// </summary>
    [Serializable]
    public class PlistIntDictionary : EmbracePlistDictionary<string, int>
    {
    }
}
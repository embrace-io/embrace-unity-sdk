using System.Collections.Generic;

namespace EmbraceSDK.EditorView
{
    public class EmbracePlistDictionary<TKey, TValue> : IJsonSerializable
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public bool ShouldSerialize()
        {
            return keys.Count > 0;
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }
    }
}
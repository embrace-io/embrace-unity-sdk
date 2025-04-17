using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Custom JSON converter that which reads and writes custom plist json into Editor serializable EmbracePlistDictionary subclasses.
    /// </summary>
    public class PlistDictionaryConverter<TKey, TValue> : KeyValuePairConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var pDictionary = (EmbracePlistDictionary<TKey, TValue>)value;
            writer.WriteStartObject();

            var min = Math.Min(pDictionary.keys.Count, pDictionary.values.Count);
            for (int i = 0; i < min; i++)
            {
                writer.WritePropertyName(pDictionary.keys[i].ToString());
                writer.WriteValue(pDictionary.values[i]);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var plistDictionary = (EmbracePlistDictionary<TKey, TValue>)existingValue;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    plistDictionary.keys.Add((TKey)reader.Value);
                }
                else
                {
                    plistDictionary.values.Add(serializer.Deserialize<TValue>(reader));
                }
            }

            return existingValue;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EmbracePlistDictionary<TKey, TValue>);
        }
    }
}
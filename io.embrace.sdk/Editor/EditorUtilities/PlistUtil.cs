#if UNITY_IOS || UNITY_TVOS
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace EmbraceSDK.EditorView
{
    public static class PlistUtil
    {
        /// <summary>
        /// Loads the specified plist file.
        /// </summary>
        /// <param name="filePath">Full path with unity project to the file.</param>
        /// <returns></returns>
        public static PlistDocument LoadPlist(string filePath)
        {
            var plist = new PlistDocument();
            plist.ReadFromFile(filePath);
            return plist;
        }

        /// <summary>
        /// Serializes a plist into JSON.
        /// </summary>
        /// <param name="plist"></param>
        /// <returns></returns>
        public static string ToJson(PlistDocument plist)
        {
            var sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    SerializePlistDict(plist.root, writer);
                    writer.WriteEndObject();
                }
            }

            return sb.ToString();
        }

        private static void SerializePlistDict(PlistElementDict pDict, JsonWriter writer)
        {
            foreach (var kvp in pDict.values)
            {
                writer.WritePropertyName(kvp.Key);

                if (kvp.Value.GetType() == typeof(PlistElementArray))
                {
                    writer.WriteStartArray();
                    SerializePlistArray(kvp.Value.AsArray(), writer);
                    writer.WriteEndArray();
                }
                else if (kvp.Value.GetType() == typeof(PlistElementDict))
                {
                    writer.WriteStartObject();
                    SerializePlistDict(kvp.Value.AsDict(), writer);
                    writer.WriteEndObject();
                }
                else
                {
                    SerializePListElement(kvp.Value, writer);
                }
            }
        }

        private static void SerializePlistArray(PlistElementArray pArray, JsonWriter writer)
        {
            foreach (var element in pArray.values)
            {
                SerializePListElement(element, writer);
            }
        }

        private static void SerializePListElement(PlistElement pElement, JsonWriter writer)
        {
            if (pElement.GetType() == typeof(PlistElementBoolean))
            {
                writer.WriteValue(pElement.AsBoolean().ToString());
            }
            else if (pElement.GetType() == typeof(PlistElementInteger))
            {
                writer.WriteValue(pElement.AsInteger());
            }
            else if (pElement.GetType() == typeof(PlistElementReal))
            {
                writer.WriteValue(pElement.AsReal());
            }
            else if (pElement.GetType() == typeof(PlistElementString))
            {
                writer.WriteValue(pElement.AsString());
            }
            else
            {
                throw new Exception($"Unhandled PlistElement Type:{pElement.GetType()}");
            }
        }

        /// <summary>
        /// Creates a plist document from a JSON representation of a plist.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static PlistDocument FromJson(string json)
        {
            var plist = new PlistDocument();

            using (StringReader sr = new StringReader(json))
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    reader.Read();
                    DeserializePlistDict(reader, plist.root);
                }
            }

            return plist;
        }

        private static void DeserializePlistDict(JsonTextReader reader, PlistElementDict pDict)
        {
            var key = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    key = reader.Value.ToString();
                    continue;
                }

                if (reader.TokenType == JsonToken.StartArray)
                {
                    DeserializeArray(reader, pDict.CreateArray(key));
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    DeserializePlistDict(reader, pDict.CreateDict(key));
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
                else
                {
                    DeserializePlistElement(reader, key, pDict);
                }
            }
        }

        private static void DeserializePlistElement(JsonTextReader reader, string key, PlistElementDict pDict)
        {
            if (reader.TokenType == JsonToken.Boolean)
            {
                bool.TryParse(reader.Value.ToString(), out var boolValue);
                pDict.SetBoolean(key, boolValue);
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int.TryParse(reader.Value.ToString(), out var intValue);
                pDict.SetInteger(key, intValue);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                pDict.SetString(key, reader.Value.ToString());
            }
            else
            {
                Debug.LogWarning($"Skipping unhandled PlistElement Value {reader.Value} of TokenType {reader.TokenType}");
            }
        }

        private static void DeserializeArray(JsonTextReader reader, PlistElementArray pArray)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.Boolean)
                {
                    bool.TryParse(reader.Value.ToString(), out var boolValue);
                    pArray.AddBoolean(boolValue);
                }
                else if (reader.TokenType == JsonToken.Integer)
                {
                    int.TryParse(reader.Value.ToString(), out var intValue);
                    pArray.AddInteger(intValue);
                }
                else if (reader.TokenType == JsonToken.String)
                {
                    pArray.AddString(reader.Value.ToString());
                }
                else if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
                else
                {
                    Debug.LogWarning($"Skipping PlistElementArray Value {reader.Value} of TokenType {reader.TokenType}");
                }
            }
        }
    }
}
#endif
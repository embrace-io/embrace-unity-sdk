using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// An implementation of ISettingsStore which stores settings in a JSON file.
    /// </summary>
    internal class JsonSettingsStore : ISettingsStore
    {
        private JObject _settings;
        private readonly string _path;
        private readonly string _directory;

        private bool _hasChanges;

        /// <summary>
        /// Creates a new instance of JsonSettingsStore.
        /// </summary>
        /// <param name="path">The file path where the JSON file will be read/written.</param>
        public JsonSettingsStore(string path)
        {
            _path = path;
            _directory = Path.GetDirectoryName(_path);
            _hasChanges = false;

            Load();
        }

        /// <summary>
        /// Prompts the settings store to reload the JSON file from disk.
        /// </summary>
        public void Load()
        {
            if (File.Exists(_path))
            {
                _settings = JObject.Parse(File.ReadAllText(_path));
            }
            else
            {
                _settings = new JObject();
            }
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _settings.ContainsKey(key);
        }

        /// <inheritdoc />
        public void DeleteKey(string key)
        {
            _settings.Remove(key);
        }

        /// <inheritdoc />
        public void Save()
        {
            if (!_hasChanges) return;

            if(!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            File.WriteAllText(_path, _settings.ToString());
            _hasChanges = false;
        }

        /// <inheritdoc />
        public void SetValue<T>(string key, T value, bool save = true)
        {
            _settings[key] = JToken.FromObject(value);
            _hasChanges = true;

            if (save)
            {
                Save();
            }
        }

        /// <inheritdoc />
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_settings.TryGetValue(key, out JToken jToken))
            {
                try
                {
                    return jToken.ToObject<T>();
                }
                catch (Exception e)
                {
                    EmbraceLogger.LogFormat(LogType.Error, "Embrace settings store encountered an error when getting value with key={0} : {1}", key, e);
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }
}
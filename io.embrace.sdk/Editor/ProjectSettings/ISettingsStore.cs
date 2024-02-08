namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Defines an interface for setting and getting values from a settings store. Embrace uses this interface internally
    /// to store and retrieve user and project settings.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>
        /// Persists the current set of values.
        /// </summary>
        void Save();

        /// <summary>
        /// Check if the settings store contains a value for the given key.
        /// </summary>
        bool ContainsKey(string key);

        /// <summary>
        /// Remove the value for the given key, if it exists.
        /// </summary>
        void DeleteKey(string key);

        /// <summary>
        /// Set a value for a given key.
        /// </summary>
        /// <param name="key">The key to set. If a value with this key already exists, it will be replaced.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="save">If true, the current values in the store will be persisted automatically once the new value is written.</param>
        void SetValue<T>(string key, T value, bool save = true);

        /// <summary>
        /// Gets a value for a given key.
        /// </summary>
        /// <param name="key">The key to retrieve. If the key is not present, the default value is returned.</param>
        /// <param name="defaultValue">The value to return if the key is not present.</param>
        /// <typeparam name="T">The type of the value to get. If this does not match the type stored in with the given key,
        /// default value will be returned.</typeparam>
        T GetValue<T>(string key, T defaultValue = default);
    }
}
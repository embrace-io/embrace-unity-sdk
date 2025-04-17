using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Utility for displaying scripting define symbols as settings inside the editor and applying those symbols
    /// to PlayerSettings.
    /// </summary>
    public class ScriptingDefineUtil
    {
        private readonly HashSet<string> _enabledSettingsCache;

        private readonly List<string> _iosDefinedSymbols;
        private readonly List<string> _androidDefinedSymbols;

        private readonly IScriptingDefineSymbolSource _source;

        private bool _hasModifiedSymbols;

        public ScriptingDefineUtil(IScriptingDefineSymbolSource source = null)
        {
            _enabledSettingsCache = new HashSet<string>();

            _iosDefinedSymbols = new List<string>();
            _androidDefinedSymbols = new List<string>();

            _source = source ?? new PlayerSettingsScriptingDefineSymbolSource();

            ReadFromProjectSettings();
        }

        /// <summary>
        /// Resets the internal state of this instance to match the current symbols defined in PlayerSettings
        /// </summary>
        public void ReadFromProjectSettings()
        {
            string iosSymbolString = _source.GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.iOS);
            string[] iosSymbols = iosSymbolString.Split(';');
            string androidSymbolString = _source.GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.Android);
            string[] androidSymbols = androidSymbolString.Split(';');

            _iosDefinedSymbols.Clear();
            _iosDefinedSymbols.AddRange(iosSymbols);

            _androidDefinedSymbols.Clear();
            _androidDefinedSymbols.AddRange(androidSymbols);

            _enabledSettingsCache.Clear();

            for (int i = 0; i < _iosDefinedSymbols.Count; ++i)
            {
                _enabledSettingsCache.Add(_iosDefinedSymbols[i]);
            }

            for (int i = 0; i < _androidDefinedSymbols.Count; ++i)
            {
                _enabledSettingsCache.Add(_androidDefinedSymbols[i]);
            }

            _hasModifiedSymbols = false;
        }

        /// <summary>
        /// Checks whether the symbol of the given setting is defined.
        /// </summary>
        /// <param name="settingsItem">The setting item whose symbol you'd like to check.</param>
        /// <returns>True if the symbol is defined, false otherwise.</returns>
        public bool CheckIfSettingIsEnabled(ScriptingDefineSettingsItem settingsItem) =>
            _enabledSettingsCache.Contains(settingsItem.symbol);

        /// <summary>
        /// Checkes whether the symbol is defined in Player Settings
        /// </summary>
        public bool CheckIfSettingIsEnabled(string symbol) => _enabledSettingsCache.Contains(symbol);

        /// <summary>
        /// Displays the given setting as a toggle in an EditorGUILayout context. NOTE: In order for changes to
        /// be applied to this setting, you must call ApplyModifiedSettings after all settings have been displayed.
        /// </summary>
        public void GUILayoutSetting(ScriptingDefineSettingsItem settingsItem)
        {
            bool isEnabled = CheckIfSettingIsEnabled(settingsItem);
            bool desiredState = EditorGUILayout.Toggle(settingsItem.guiContent, isEnabled);

            if (desiredState != isEnabled)
            {
                ToggleSymbol(settingsItem.symbol, desiredState);
            }
        }

        /// <summary>
        /// Gathers the values of item.guiContent.text into a string array for use with the GUILayoutSettingsAsFlags method.
        /// </summary>
        public string[] GetFlagNamesForSettingsItems(ScriptingDefineSettingsItem[] items)
        {
            string[] names = new string[items.Length];
            for (int i = 0; i < items.Length; ++i)
            {
                names[i] = items[i].guiContent.text;
            }

            return names;
        }

        /// <summary>
        /// Displays the settings items as a mask popup. NOTE: In order for changes to be applied to these settings, you
        /// must call ApplyModifiedSettings after all settings have been displayed.
        /// </summary>
        /// <param name="label">The GUIContent for the mask popup label.</param>
        /// <param name="items">The settings to include in the popup.</param>
        /// <param name="itemNames">The names of the settings items. Due to Unity API limitations, we need these
        /// as a separate string[].</param>
        public void GUILayoutSettingsAsFlags(GUIContent label, ScriptingDefineSettingsItem[] items, string[] itemNames)
        {
            int maskValue = 0;
            for (int i = 0; i < items.Length; ++i)
            {
                if (CheckIfSettingIsEnabled(items[i]))
                {
                    maskValue |= (1 << i);
                }
            }

            maskValue = EditorGUILayout.MaskField(label, maskValue, itemNames);

            for (int i = 0; i < items.Length; ++i)
            {
                bool desiredState = (maskValue & (1 << i)) > 0;
                if (desiredState != CheckIfSettingIsEnabled(items[i]))
                {
                    ToggleSymbol(items[i].symbol, desiredState);
                }
            }
        }

        /// <summary>
        /// Displays the settings items as a popup selection list.
        ///
        /// Additional selection options with no corresponding items can be included at the end of the itenNames array.
        /// If one of the extra items is selected, no symbols will be written. This is useful for adding a "Default" option.
        /// </summary>
        /// <param name="label">The label for the dropdown list.</param>
        /// <param name="items">The settings items that match the options in itemNames, starting at index 0.</param>
        /// <param name="itemNames">The names to select in the list.</param>
        public void GUILayoutSettingsAsSelectionList(GUIContent label, ScriptingDefineSettingsItem[] items, GUIContent[] itemNames)
        {
            int currentSelection = itemNames.Length > items.Length ? items.Length : -1;

            for (int i = 0; i < items.Length; ++i)
            {
                if (CheckIfSettingIsEnabled(items[i]))
                {
                    currentSelection = i;
                }
            }

            int newValue = EditorGUILayout.Popup(label, currentSelection, itemNames);

            if (currentSelection != newValue)
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    bool desiredState = i == newValue;
                    if (desiredState != CheckIfSettingIsEnabled(items[i]))
                    {
                        ToggleSymbol(items[i].symbol, desiredState);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the symbol to its default value. NOTE: In order for changes to be applied, you must call
        /// ApplyModifiedSettings afterwards.
        /// </summary>
        public void ApplyDefault(ScriptingDefineSettingsItem settingsItem)
        {
            if (settingsItem.defaultValue != CheckIfSettingIsEnabled(settingsItem))
            {
                ToggleSymbol(settingsItem.symbol, settingsItem.defaultValue);
            }
        }

        /// <summary>
        /// Writes all changed symbols to PlayerSettings.
        /// </summary>
        public void ApplyModifiedProperties()
        {
            if (!_hasModifiedSymbols)
            {
                return;
            }

            _hasModifiedSymbols = false;

            string iosSymbolString = string.Join(";", _iosDefinedSymbols);
            _source.SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.iOS, iosSymbolString);

            string androidSymbolString = string.Join(";", _androidDefinedSymbols);
            _source.SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.Android, androidSymbolString);
        }

        /// <summary>
        /// Sets the symbols state in the iOS and Android symbols lists as well as the cache.
        /// </summary>
        public void ToggleSymbol(string symbol, bool enable)
        {
            _hasModifiedSymbols = true;
            if (enable)
            {
                if (!_iosDefinedSymbols.Contains(symbol))
                {
                    _iosDefinedSymbols.Add(symbol);
                }
                if (!_androidDefinedSymbols.Contains(symbol))
                {
                    _androidDefinedSymbols.Add(symbol);
                }

                _enabledSettingsCache.Add(symbol);
            }
            else
            {
                while (_iosDefinedSymbols.Contains(symbol))
                {
                    _iosDefinedSymbols.Remove(symbol);
                }

                while (_androidDefinedSymbols.Contains(symbol))
                {
                    _androidDefinedSymbols.Remove(symbol);
                }

                if (_enabledSettingsCache.Contains(symbol))
                {
                    _enabledSettingsCache.Remove(symbol);
                }
            }
        }

        public enum BuildTargetSpecifier
        {
            Android,
            iOS
        }

        public interface IScriptingDefineSymbolSource
        {
            string GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier spec);
            void SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier group, string symbols);
        }

        private class PlayerSettingsScriptingDefineSymbolSource : IScriptingDefineSymbolSource
        {
            public string GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier specifier)
            {
                #if UNITY_2021_3_OR_NEWER
                return PlayerSettings.GetScriptingDefineSymbols(ConvertToUnityType(specifier));
                #else
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(ConvertToUnityType(specifier));
                #endif
            }

            public void SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier specifier, string symbols)
            {
                #if UNITY_2021_3_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(ConvertToUnityType(specifier), symbols);
                #else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(ConvertToUnityType(specifier), symbols);
                #endif
            }
            
            #if UNITY_2021_3_OR_NEWER
            private NamedBuildTarget ConvertToUnityType(BuildTargetSpecifier spec)
            {
                switch (spec)
                {
                    case BuildTargetSpecifier.Android:
                        return NamedBuildTarget.Android;
                    case BuildTargetSpecifier.iOS:
                        return NamedBuildTarget.iOS;
                    default:
                        throw new ArgumentException("Embrace::BuildTargetSpecifier has no mapping");
                }
            }
            #else
            private BuildTargetGroup ConvertToUnityType(BuildTargetSpecifier spec)
            {
                switch (spec)
                {
                    case BuildTargetSpecifier.Android:
                        return BuildTargetGroup.Android;
                    case BuildTargetSpecifier.iOS:
                        return BuildTargetGroup.iOS;
                    default:
                        throw new ArgumentException("Embrace::BuildTargetSpecifier has no mapping");
                }
            }
            #endif
        }
    }
}
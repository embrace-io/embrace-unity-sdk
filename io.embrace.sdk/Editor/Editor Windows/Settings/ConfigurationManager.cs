using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = System.Object;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Used in the Settings editor window to allow users to manage configurations and environments.
    /// Environments allow users to handle configurations based on their desired environment.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Configurations", 2)]
    [UnityEngine.TestTools.ExcludeFromCoverage]
    internal class ConfigurationManager : BaseSettingsManager
    {
        // environments
        private ReorderableList _rList;
        private bool _isDirty;

        private Vector2 _scrollPosition;

        private int _savedDeviceIndex;
        private UnityEditor.Editor _configEditor;

        private HashSet<string> _usedEnvNames = new HashSet<string>();

        public override void Initialize(MainSettingsEditor mainSettingsEditor)
        {
            base.Initialize(mainSettingsEditor);

            InitReorderableList();
            UpdateEmbraceSDKSettings();
            UpdateConfigEditor();

            if (environments != null)
                environments.EnvironmentsReset += OnEnvironmentsReset;
        }

        private void InitReorderableList()
        {
            if (environments == null) {
                return;
            }
            _rList = new ReorderableList(environments.environmentConfigurations, typeof(EnvironmentConfiguration), true, true, true, true);
            _rList.onAddCallback += OnAddCallback;
            _rList.onRemoveCallback += OnRemoveCallback;
            _rList.onSelectCallback += OnSelectCallback;
            _rList.drawElementCallback += DrawElement;
            _rList.drawHeaderCallback += DrawHeader;
            _rList.draggable = false;
        }

        public override void OnGUI()
        {
            if (environments == null)
            {
                RecoverEnvironments();
                return;
            }

            EditorGUILayout.HelpBox("Configurations allow you to use different app IDs or SDK settings in different environments. For instance, you might want to create a configuration to use in development builds, and another to use in production.", MessageType.Info);
            _rList.DoLayoutList();
            if (_isDirty && GUILayout.Button("Update Configurations"))
            {
                ValidateEnvironmentNames();
                UpdateConfigEditor();
                UpdateEmbraceSDKSettings();
                _isDirty = false;
            }

            GUILayout.Space(styleConfigs.space);

            // Compare the current active device index against the saved device index to determine if
            // a change has occured as a result of changes made from this window, or externally.
            var currentDeviceIndex = GUILayout.Toolbar(environments.activeDeviceIndex, Environments.DeviceStrings);
            if (_savedDeviceIndex != currentDeviceIndex)
            {
                // This redundancy ensures that any changes made to the active device index through this window
                // still set the right value on the environments object.
                environments.activeDeviceIndex = currentDeviceIndex;

                _savedDeviceIndex = currentDeviceIndex;
                UpdateConfigEditor();
            }

            GUILayout.Space(styleConfigs.space);

            CustomizeConfiguration();
        }

        #region Environments

        private const float SELECTION_COLUMN_WIDTH = 30f;
        private const float APP_ID_COLUMN_WIDTH = 50f;

        private void DrawHeader(Rect rect)
        {
            Rect nameFieldRect = new Rect(rect.x + SELECTION_COLUMN_WIDTH, rect.y, rect.width - SELECTION_COLUMN_WIDTH - APP_ID_COLUMN_WIDTH * 2f, rect.height);
            Rect androidAppIdRect = new Rect(rect.x + rect.width - APP_ID_COLUMN_WIDTH * 2f, rect.y, APP_ID_COLUMN_WIDTH, rect.height);
            Rect iosAppIdRect = new Rect(rect.x + rect.width - APP_ID_COLUMN_WIDTH, rect.y, APP_ID_COLUMN_WIDTH, rect.height);

            EditorGUI.LabelField(nameFieldRect, "Name", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(androidAppIdRect, "Android", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(iosAppIdRect, "iOS", EditorStyles.miniBoldLabel);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect selectionToggleRect = new Rect(rect.x, rect.y, SELECTION_COLUMN_WIDTH, rect.height);
            Rect nameFieldRect = new Rect(rect.x + SELECTION_COLUMN_WIDTH, rect.y, rect.width - SELECTION_COLUMN_WIDTH - APP_ID_COLUMN_WIDTH * 2f - 10f, rect.height);
            Rect androidAppIdRect = new Rect(rect.x + rect.width - APP_ID_COLUMN_WIDTH * 2f, rect.y, APP_ID_COLUMN_WIDTH, rect.height);
            Rect iosAppIdRect = new Rect(rect.x + rect.width - APP_ID_COLUMN_WIDTH, rect.y, APP_ID_COLUMN_WIDTH, rect.height);

            EnvironmentConfiguration env = environments.environmentConfigurations[index];

            if (EditorGUI.Toggle(selectionToggleRect, environments.activeEnvironmentIndex == index) && environments.activeEnvironmentIndex != index)
            {
                environments.activeEnvironmentIndex = index;
                UpdateConfigEditor();
            }

            EditorGUI.BeginChangeCheck();
            env.name = EditorGUI.TextField(nameFieldRect, env.name);
            if (EditorGUI.EndChangeCheck())
            {
                _isDirty = true;
            }

            string androidAppId = "";
            EmbraceConfiguration androidConfig = env[EmbraceDeviceType.Android];
            if(androidConfig != null)
            {
                androidAppId = androidConfig.AppId;
            }
            EditorGUI.SelectableLabel(androidAppIdRect, androidAppId, EditorStyles.miniLabel);

            string iosAppId = "";
            EmbraceConfiguration iosConfig = env[EmbraceDeviceType.IOS];
            if(iosConfig != null)
            {
                iosAppId = iosConfig.AppId;
            }
            EditorGUI.SelectableLabel(iosAppIdRect, iosAppId, EditorStyles.miniLabel);
        }

        private void OnAddCallback(ReorderableList list)
        {
            ValidateEnvironmentNames();

            var environmentGuid = Guid.NewGuid().ToString();
            environments.environmentConfigurations.Add(new EnvironmentConfiguration(environmentGuid));

            // The list.index property can have an unexpected value
            // if the list selection is not explicitly set.
            int index = environments.environmentConfigurations.Count - 1;
            list.index = index;

            var androidConfig = AssetDatabaseUtil.CreateConfiguration<AndroidConfiguration>(environmentGuid);
            var iosConfig = AssetDatabaseUtil.CreateConfiguration<IOSConfiguration>(environmentGuid);
            environments.environmentConfigurations[index].sdkConfigurations.Add(androidConfig);
            environments.environmentConfigurations[index].sdkConfigurations.Add(iosConfig);

            environments.activeEnvironmentIndex = index;
            environments.isDirty = true;

            _isDirty = true;

            UpdateConfigEditor();
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            foreach (var configItem in environments.environmentConfigurations[list.index].sdkConfigurations)
            {
                if (configItem != null)
                {
                    string configPath = AssetDatabase.GetAssetPath(configItem);
                    AssetDatabase.DeleteAsset(configPath);
                }
            }

            environments.environmentConfigurations.RemoveAt(list.index);

            environments.activeEnvironmentIndex = list.index - 1;

            // Ensure activeEnvironmentIndex doesn't equal -1 if the reorderable list
            // contains more elements, but the first element was both selected and deleted
            if (list.count > 0 && environments.activeEnvironmentIndex < 0)
            {
                environments.activeEnvironmentIndex = 0;
            }

            // explicitly set the list's current selected index, this can equal -1.
            list.index = environments.activeEnvironmentIndex;

            environments.isDirty = true;

            UpdateConfigEditor();
        }

        private void OnSelectCallback(ReorderableList list)
        {
            environments.activeEnvironmentIndex = list.index;
            UpdateConfigEditor();
        }

        private void UpdateEmbraceSDKSettings()
        {
            if (environments == null)
                return;
            for (int i = 0; i < environments.environmentConfigurations.Count; i++)
            {
                string name = environments.environmentConfigurations[i].name;
                foreach (var config in environments.environmentConfigurations[i].sdkConfigurations)
                {
                    string assetPath = AssetDatabase.GetAssetPath(config.GetInstanceID());
                    //rename only if necessary
                    if (config.EnvironmentName != name)
                    {
                        AssetDatabase.RenameAsset(assetPath, $"{name}{config.DeviceType}EnvironmentConfiguration.asset");
                        AssetDatabase.SaveAssets();

                        config.EnvironmentName = name;
                        EditorUtility.SetDirty(config);
                    }
                }
            }
        }

        #endregion

        private void RecoverEnvironments()
        {
            environments = AssetDatabaseUtil.LoadEnvironments();
            InitReorderableList();
            _rList.index = environments.activeEnvironmentIndex;
        }

        private void UpdateConfigEditor()
        {
            if (environments == null)
                return;
            if (environments.environmentConfigurations.Count != 0 &&
                environments.activeEnvironmentIndex > -1)
            {
                var environment = environments.environmentConfigurations[environments.activeEnvironmentIndex];
                androidConfiguration = environment.sdkConfigurations[0]; // If we were going to do this anyways, why have the reorderable list?
                iOSConfiguration = environment.sdkConfigurations[1];
            }
            else
            {
                androidConfiguration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>();
                iOSConfiguration = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>();
            }

            switch (environments.activeDeviceIndex)
            {
                case 0:
                    _configEditor = UnityEditor.Editor.CreateEditor(androidConfiguration);
                    break;
                case 1:
                    _configEditor = UnityEditor.Editor.CreateEditor(iOSConfiguration);
                    break;
            }
        }

        #region CustomConfigurations

        private void CustomizeConfiguration()
        {
            var device = Environments.DeviceStrings[environments.activeDeviceIndex];
            GUILayout.Label(new GUIContent($"Custom {device} Configurations", $"Overrides the default {device} configuration settings"));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // environment and _configEditor references can become null if the assets they reference are manually deleted from the project
            if (environments == null)
            {
                RecoverEnvironments();
            }

            if (_configEditor == null || _configEditor.target == null)
            {
                UpdateConfigEditor();
            }


            _configEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        public override void OnFocus()
        {
            // unused
        }

        public override void OnLostFocus()
        {
            if (_isDirty)
            {
                ValidateEnvironmentNames();
                UpdateConfigEditor();
                UpdateEmbraceSDKSettings();
                _isDirty = false;
            }

            EditorUtility.SetDirty(environments);
        }

        private void ValidateEnvironmentNames()
        {
            _usedEnvNames.Clear();

            for (int i = 0; i < _rList.count; i++)
            {
                EnvironmentConfiguration config = _rList.list[i] as EnvironmentConfiguration;

                if (config == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(config.name) || _usedEnvNames.Contains(config.name))
                {
                    config.name = environments.environmentConfigurations[i].guid;
                }
                else
                {
                    _usedEnvNames.Add(config.name);
                }
            }
        }

        private void OnEnvironmentsReset()
        {
            InitReorderableList();
        }

        public override void OnDestroy()
        {
            environments.EnvironmentsReset -= OnEnvironmentsReset;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Handles how the properties of EmbraceConfiguration objects are displayed.
    /// </summary>
    [CustomEditor(typeof(EmbraceConfiguration), true)]
    [UnityEngine.TestTools.ExcludeFromCoverage]
    public class EmbraceSDKConfigurationEditor : UnityEditor.Editor
    {
        private List<SerializedProperty> _serializedProperties;
        private float _longestLabelWidth;
        private string _envName;
        private string _envGuid;
        private string _deviceType;


        private void OnEnable()
        {
            // OnEnable can be invoked multiple times after a recompile,
            // and target can be null during some of those invocations.
            if (target != null)
            {

                var config = (EmbraceConfiguration)target;
                _envName = config.EnvironmentName;
                _envGuid = config.EnvironmentGuid;

                _deviceType = Enum.GetName(typeof(EmbraceDeviceType), config.DeviceType);

                _serializedProperties = new List<SerializedProperty>();
                InitSerializedProperties(0, target.GetType());
            }
        }

        private void InitSerializedProperties(int nestingDepth, Type type)
        {
            var fieldInfos = ReflectionUtil.GetDeclaredInstanceFields(type);
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.DeclaringType == target.GetType())
                {
                    if(fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                    {
                        continue;
                    }

                    SerializedProperty prop = serializedObject.FindProperty(fieldInfo.Name);
                    if (prop == null)
                    {
                        continue;
                    }
                    _serializedProperties.Add(prop);
                }

                var labelWidth = StaticStyleConfigs.DefaultToggleStyle.guiStyle.CalcSize(new GUIContent(fieldInfo.Name)).x;
                if (labelWidth > _longestLabelWidth)
                {
                    _longestLabelWidth = labelWidth + nestingDepth * StaticStyleConfigs.DefaultToggleStyle.guiStyle.padding.right;
                }

                if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(ITooltipPropertiesProvider)))
                {
                    InitSerializedProperties(nestingDepth + 1, fieldInfo.FieldType);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // If a config asset is deleted outside of the settings window's control
            // and is currently displayed in an inspector or editor window, this editor
            // needs to destroy itself as it's serialized object is no longer valid.
            if (serializedObject.targetObject == null)
            {
                DestroyImmediate(this);
                return;
            }

            serializedObject.Update();

            GUILayout.BeginVertical(StaticStyleConfigs.DarkBoxStyle.guiStyle);

            EditorGUILayout.TextField("Device Type", _deviceType, StaticStyleConfigs.BoldTextStyle.guiStyle);

            // Default configs do not have a name assigned.
            if (!string.IsNullOrEmpty(_envName))
            {
                GUILayout.Space(StaticStyleConfigs.Space);
                EditorGUILayout.TextField("Configuration Name", _envName, StaticStyleConfigs.BoldTextStyle.guiStyle);
            }

#if DeveloperMode
            EditorGUILayout.TextField("GUID", _envGuid);
#endif

            GUILayout.EndVertical();


            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);

            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = _longestLabelWidth;

            foreach (var property in _serializedProperties)
            {
                EditorGUILayout.PropertyField(property);
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
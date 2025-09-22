using System.Collections.Generic;
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EmbraceSDK.Editor
{
    internal static class AutoFPSSettingsIMGUI
    {
        private static bool _shouldApply = false;
        
        [SettingsProvider]
        public static SettingsProvider CreateAutoFPSSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Embrace/FPS", SettingsScope.Project)
            {
                label = "FPS Config",
                guiHandler = (searchContext) =>
                {
                    var settings = AutoFPSSettings.GetFPSSettings();
                    EditorGUILayout.LabelField("Embrace Auto-Instrumentation FPS Logging Configuration", EditorStyles.boldLabel);
                    #if EMBRACE_AUTO_INSTRUMENTATION_FPS_CAPTURE
                    // We use IntField here because it makes more sense to have whole numbers for framerate and interval.
                    var newFramerate = Mathf.Clamp(
                        EditorGUILayout.IntField("Target Framerate", (int) settings.targetFramerate), 
                        AutoFPSSettings.TargetFramerateRange.min, AutoFPSSettings.TargetFramerateRange.max);
                    var newInterval = Mathf.Clamp(
                        EditorGUILayout.IntField("Report Interval (seconds)", (int) settings.reportInterval),
                        AutoFPSSettings.ReportIntervalRange.min, AutoFPSSettings.ReportIntervalRange.max);

                    bool shouldSave = false;
                    
                    if (!Mathf.Approximately(newFramerate, settings.targetFramerate))
                    {
                        settings.targetFramerate = newFramerate;
                        shouldSave = true;
                    }

                    if (!Mathf.Approximately(newInterval, settings.reportInterval))
                    {
                        settings.reportInterval = newInterval;
                        shouldSave = true;
                    }
                    
                    if (shouldSave)
                    {
                        _shouldApply = true;
                        AutoFPSSettings.SaveFPSSettings(settings);
                    }

                    EditorGUILayout.HelpBox("Always apply changes when done editing configs.", MessageType.Info);
                    if (_shouldApply)
                    {
                        EditorGUILayout.HelpBox("You have unsaved changes. Please apply them to take effect.", MessageType.Warning);
                    }
                    if (GUILayout.Button("Apply Changes to Project"))
                    {
                        _shouldApply = false;
                        CompilationPipeline.RequestScriptCompilation();
                    }
                    #else
                    EditorGUILayout.HelpBox("Auto-Instrumentation of FPS capture is currently disabled. To enable it, opt in using the Embrace General Settings Menu.", MessageType.Warning);
                    #endif
                },

                keywords = new HashSet<string>(new[] { "Embrace", "FPS", "AutoFPS" })
            };

            return provider;
        }
        
    }
    
    internal class AutoFPSSettings
    {
        public float targetFramerate = 30f;
        public float reportInterval = 60f;

        public static readonly (float min, float max) TargetFramerateRange = (15f, 120f);
        public static readonly (float min, float max) ReportIntervalRange = (10f, 120f);

        public const string TargetFramerateKey = "Embrace_AutoFPS_TargetFramerate";
        public const string ReportIntervalKey = "Embrace_AutoFPS_ReportInterval";

        private AutoFPSSettings()
        {
            if (!EmbraceProjectSettings.Project.ContainsKey(TargetFramerateKey))
            {
                EmbraceProjectSettings.Project.SetValue(TargetFramerateKey, 30f);
            }
            
            if (!EmbraceProjectSettings.Project.ContainsKey(ReportIntervalKey))
            {
                EmbraceProjectSettings.Project.SetValue(ReportIntervalKey, 60f);
            }
        }
        
        public static AutoFPSSettings GetFPSSettings()
        {
            var settings = new AutoFPSSettings
            {
                targetFramerate = EmbraceProjectSettings.Project.GetValue(TargetFramerateKey, 30f),
                reportInterval = EmbraceProjectSettings.Project.GetValue(ReportIntervalKey, 60f)
            };
            return settings;
        }
        
        public static void SaveFPSSettings(AutoFPSSettings settings)
        {
            EmbraceProjectSettings.Project.SetValue(TargetFramerateKey, settings.targetFramerate, false);
            EmbraceProjectSettings.Project.SetValue(ReportIntervalKey, settings.reportInterval, false);
            EmbraceProjectSettings.Project.Save();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EmbraceSDK.Editor
{
    internal static class AutoMemoryMonitorSettingsIMGUI
    {
        private static bool _shouldApply = false;
        [SettingsProvider]
        public static SettingsProvider CreateAutoMemoryMonitorSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Embrace/Memory Monitor", SettingsScope.Project)
            {
                label = "Memory Monitor Config",
                guiHandler = (searchContext) =>
                {
                    var settings = AutoMemoryMonitorSettings.GetAutoMemoryMonitorSettings();
                    EditorGUILayout.LabelField("Embrace Auto-Instrumentation Memory Monitor Configuration", EditorStyles.boldLabel);
                    #if EMBRACE_AUTO_INSTRUMENTATION_MEMORY_MONITOR
                    var newGcReservedMb = Mathf.Clamp(
                        EditorGUILayout.IntField("GC Reserved Memory (MB)", settings.GCReservedMb),
                        AutoMemoryMonitorSettings.GCReservedMbRange.min, AutoMemoryMonitorSettings.GCReservedMbRange.max);
                    var newGcUsedMb = Mathf.Clamp(
                        EditorGUILayout.IntField("GC Used Memory (MB)", settings.GCUsedMb),
                        AutoMemoryMonitorSettings.GCUsedMbRange.min, AutoMemoryMonitorSettings.GCUsedMbRange.max);
                    var newSystemUsedMb = Mathf.Clamp(
                        EditorGUILayout.IntField("System Used Memory (MB)", settings.SystemUsedMb),
                        AutoMemoryMonitorSettings.SystemUsedMbRange.min, AutoMemoryMonitorSettings.SystemUsedMbRange.max);
                    var newTotalReservedMb = Mathf.Clamp(
                        EditorGUILayout.IntField("Total Reserved Memory (MB)", settings.TotalReservedMb),
                        AutoMemoryMonitorSettings.TotalReservedMbRange.min, AutoMemoryMonitorSettings.TotalReservedMbRange.max);
                    var newTotalUsedMb = Mathf.Clamp(
                        EditorGUILayout.IntField("Total Used Memory (MB)", settings.TotalUsedMb),
                        AutoMemoryMonitorSettings.TotalUsedMbRange.min, AutoMemoryMonitorSettings.TotalUsedMbRange.max);
                    var newGcCollectTimeMillis = Mathf.Clamp(
                        EditorGUILayout.IntField("GC Collection Time (ms)", settings.GCCollectTimeMillis),
                        AutoMemoryMonitorSettings.GCCollectTimeMillisRange.min, AutoMemoryMonitorSettings.GCCollectTimeMillisRange.max);
                    var newBatchIntervalSeconds = Mathf.Clamp(
                        EditorGUILayout.FloatField("Batch Interval (seconds)", settings.BatchIntervalSeconds),
                        AutoMemoryMonitorSettings.BatchIntervalSecondsRange.min, AutoMemoryMonitorSettings.BatchIntervalSecondsRange.max);
                    
                    EditorGUILayout.HelpBox("Always apply changes when done editing configs.", MessageType.Info);
                    if (_shouldApply)
                    {
                        EditorGUILayout.HelpBox("You have unsaved changes. Please apply them to take effect.", MessageType.Warning);
                    }
                    if (GUILayout.Button("Apply Changes to Project", GUILayout.Height(30)))
                    {
                        _shouldApply = false;
                        CompilationPipeline.RequestScriptCompilation();
                    }
                    
                    bool shouldSave = false;
                    
                    if (newGcReservedMb != settings.GCReservedMb)
                    {
                        settings.GCReservedMb = newGcReservedMb;
                        shouldSave = true;
                    }
                    
                    if (newGcUsedMb != settings.GCUsedMb)
                    {
                        settings.GCUsedMb = newGcUsedMb;
                        shouldSave = true;
                    }
                    
                    if (newSystemUsedMb != settings.SystemUsedMb)
                    {
                        settings.SystemUsedMb = newSystemUsedMb;
                        shouldSave = true;
                    }
                    
                    if (newTotalReservedMb != settings.TotalReservedMb)
                    {
                        settings.TotalReservedMb = newTotalReservedMb;
                        shouldSave = true;
                    }
                    
                    if (newTotalUsedMb != settings.TotalUsedMb)
                    {
                        settings.TotalUsedMb = newTotalUsedMb;
                        shouldSave = true;
                    }
                    
                    if (newGcCollectTimeMillis != settings.GCCollectTimeMillis)
                    {
                        settings.GCCollectTimeMillis = newGcCollectTimeMillis;
                        shouldSave = true;
                    }
                    
                    if (!Mathf.Approximately(newBatchIntervalSeconds, settings.BatchIntervalSeconds))
                    {
                        settings.BatchIntervalSeconds = newBatchIntervalSeconds;
                        shouldSave = true;
                    }
                    
                    if (shouldSave)
                    {
                        _shouldApply = true;
                        AutoMemoryMonitorSettings.SaveAutoMemoryMonitorSettings(settings);
                    }
                    #else
                    EditorGUILayout.HelpBox("Auto-Instrumentation of application memory usage is currently disabled. To enable it, opt in using the Embrace General Settings Menu.", MessageType.Warning);
                    #endif
                },
                
                keywords = new HashSet<string>(new[] { "Embrace", "Memory", "AutoMemoryMonitor", "Memory Monitor" })
            };
            
            return provider;
        }
    }

    internal class AutoMemoryMonitorSettings
    {
        public int GCReservedMb = 150;
        public int GCUsedMb = 100;
        public int SystemUsedMb = 400;
        public int TotalReservedMb = 600;
        public int TotalUsedMb = 450;
        public int GCCollectTimeMillis = 5;
        public float BatchIntervalSeconds = 10.0f;

        public static readonly (int min, int max) GCReservedMbRange = (20, 1000);
        public static readonly (int min, int max) GCUsedMbRange = (10, 1000);
        public static readonly (int min, int max) SystemUsedMbRange = (50, 1000);
        public static readonly (int min, int max) TotalReservedMbRange = (80, 1000);
        public static readonly (int min, int max) TotalUsedMbRange = (60, 1000);
        public static readonly (int min, int max) GCCollectTimeMillisRange = (1, 100);
        public static readonly (float min, float max) BatchIntervalSecondsRange = (1.0f, 1000.0f);

        public const string GCBytesReservedKey = "Embrace_AutoMemory_GCBytesReserved";
        public const string GCBytesUsedKey = "Embrace_AutoMemory_GCBytesUsed";
        public const string SystemBytesUsedKey = "Embrace_AutoMemory_SystemMemoryUsed";
        public const string TotalBytesReservedKey = "Embrace_AutoMemory_TotalBytesReserved";
        public const string TotalBytesUsedKey = "Embrace_AutoMemory_TotalBytesUsed";
        public const string GCCollectTimeNanosKey = "Embrace_AutoMemory_GCCollectTimeNanos";
        public const string BatchIntervalSecondsKey = "Embrace_AutoMemory_BatchIntervalSeconds";
        
        private AutoMemoryMonitorSettings()
        {
            if (!EmbraceProjectSettings.Project.ContainsKey(GCBytesReservedKey))
            {
                EmbraceProjectSettings.Project.SetValue(GCBytesReservedKey, 150 * (long) 1e6);
            }
            
            if (!EmbraceProjectSettings.Project.ContainsKey(GCBytesUsedKey))
            {
                EmbraceProjectSettings.Project.SetValue(GCBytesUsedKey, 100 * (long) 1e6);
            }

            if (!EmbraceProjectSettings.Project.ContainsKey(SystemBytesUsedKey))
            {
                EmbraceProjectSettings.Project.SetValue(SystemBytesUsedKey, 400 * (long) 1e6);
            }
            
            if (!EmbraceProjectSettings.Project.ContainsKey(TotalBytesReservedKey))
            {
                EmbraceProjectSettings.Project.SetValue(TotalBytesReservedKey, 600 * (long) 1e6);
            }

            if (!EmbraceProjectSettings.Project.ContainsKey(TotalBytesUsedKey))
            {
                EmbraceProjectSettings.Project.SetValue(TotalBytesUsedKey, 450 * (long) 1e6);
            }
            
            if (!EmbraceProjectSettings.Project.ContainsKey(GCCollectTimeNanosKey))
            {
                EmbraceProjectSettings.Project.SetValue(GCCollectTimeNanosKey, 5 * (long) 1e6);
            }
            
            if (!EmbraceProjectSettings.Project.ContainsKey(BatchIntervalSecondsKey))
            {
                EmbraceProjectSettings.Project.SetValue(BatchIntervalSecondsKey, 10.0f);
            }
        }

        public static AutoMemoryMonitorSettings GetAutoMemoryMonitorSettings()
        {
            var settings = new AutoMemoryMonitorSettings()
            {
                // We convert to the more user-friendly units here; factor of 1e6 for MB-bytes, and 1e6 for millis-nanos
                GCReservedMb = (int)(EmbraceProjectSettings.Project.GetValue<long>(GCBytesReservedKey) / (long) 1e6),
                GCUsedMb = (int)(EmbraceProjectSettings.Project.GetValue<long>(GCBytesUsedKey) / (long) 1e6),
                SystemUsedMb = (int)(EmbraceProjectSettings.Project.GetValue<long>(SystemBytesUsedKey) / (long) 1e6),
                TotalReservedMb = (int)(EmbraceProjectSettings.Project.GetValue<long>(TotalBytesReservedKey) / (long) 1e6),
                TotalUsedMb = (int)(EmbraceProjectSettings.Project.GetValue<long>(TotalBytesUsedKey) / (long) 1e6),
                GCCollectTimeMillis = (int)(EmbraceProjectSettings.Project.GetValue<long>(GCCollectTimeNanosKey) / (long) 1e6),
                BatchIntervalSeconds = EmbraceProjectSettings.Project.GetValue<float>(BatchIntervalSecondsKey)
            };

            return settings;
        }
        
        public static void SaveAutoMemoryMonitorSettings(AutoMemoryMonitorSettings settings)
        {
            // We convert to the actual units used in the SDK here; factor of 1e6 for MB-bytes, and 1e6 for millis-nanos
            EmbraceProjectSettings.Project.SetValue(GCBytesReservedKey, settings.GCReservedMb * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(GCBytesUsedKey, settings.GCUsedMb * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(SystemBytesUsedKey, settings.SystemUsedMb * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(TotalBytesReservedKey, settings.TotalReservedMb * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(TotalBytesUsedKey, settings.TotalUsedMb * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(GCCollectTimeNanosKey, settings.GCCollectTimeMillis * (long) 1e6, false);
            EmbraceProjectSettings.Project.SetValue(BatchIntervalSecondsKey, settings.BatchIntervalSeconds, false);
            EmbraceProjectSettings.Project.Save();
        }
    }
}
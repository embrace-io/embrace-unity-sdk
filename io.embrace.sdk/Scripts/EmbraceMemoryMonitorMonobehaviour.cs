using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    public class EmbraceMemoryMonitorMonobehaviour : MonoBehaviour
    {
        private EmbraceMemoryMonitor _embraceMemoryMonitor;
        /// <summary>
        /// This field is intentionally left public so that users are able to set thresholds at runtime based on changing conditions.
        /// HOWEVER, it is not recommended to change these values frequently as it may lead to inconsistent monitoring behavior.
        /// Additionally, these values should be set before calling StartMonitoring to ensure accurate tracking from the beginning.
        /// If necessary call StopMonitoring, adjust thresholds, then call StartMonitoring again.
        /// Finally, be cautious when modifying these values to have excessively low or high thresholds.
        /// Such settings may lead to an overwhelming number of violation logs or, conversely, a lack of meaningful monitoring data.
        /// The defaults provided are intended to offer a balanced starting point for most applications.
        /// </summary>
        public EmbraceMemorySnapshot thresholds;
        public bool markDontDestroyOnLoad = true;
        public bool autostartMonitoring = true;
        [Range(10f, 300f)] public float logBatchIntervalSeconds = 10.0f;
        private float _lastLogTime;
        private bool[] _hasViolations = new bool[(int) EmbraceMemoryMonitorId._EnumTypeCount];
        private int[] _violationCounts = new int[(int) EmbraceMemoryMonitorId._EnumTypeCount];
        private long[] _maxValues = new long[(int) EmbraceMemoryMonitorId._EnumTypeCount];

        private EmbraceMemorySnapshot _currentSnapshot = new EmbraceMemorySnapshot();
        
        private Dictionary<string, string> _logProperties = new Dictionary<string, string>();
        
        #if EMBRACE_AUTO_INSTRUMENTATION_MEMORY_MONITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnSceneLoad()
        {
            if (!FindFirstObjectByType<EmbraceMemoryMonitorMonobehaviour>())
            {
                var prefab = Resources.Load<EmbraceMemoryMonitorMonobehaviour>("EmbraceMemoryMonitor");
                
                if (prefab == null)
                {
                    Debug.LogError("EmbraceMemoryMonitor prefab not found in Resources folder.");
                    return;
                }
                
                var instance = Instantiate(prefab);
                instance.name = "EmbraceMemoryMonitor";
                
                if (instance.markDontDestroyOnLoad) instance.MarkExtendedLifetime();
                instance.InitializeMonitoring();
                if (instance.autostartMonitoring) instance.StartMonitoring();
            }
        }

        #endif // EMBRACE_MEMORY_MONITOR
        
        private static string IDViolationMap(EmbraceMemoryMonitorId id)
        {
            return id switch
            {
                EmbraceMemoryMonitorId.GCBytesReserved => "GCBytesReserved_violation_count",
                EmbraceMemoryMonitorId.GCBytesUsed => "GCBytesUsed_violation_count",
                EmbraceMemoryMonitorId.SystemBytesUsed => "SystemBytesUsed_violation_count",
                EmbraceMemoryMonitorId.TotalBytesReserved => "TotalBytesReserved_violation_count",
                EmbraceMemoryMonitorId.TotalBytesUsed => "TotalBytesUsed_violation_count",
                EmbraceMemoryMonitorId.GCCollectTimeNanos => "GCCollectTimeNanos_violation_count",
                _ => throw new ArgumentOutOfRangeException(nameof(id))
            };
        }
        
        private static string IDViolationMax(EmbraceMemoryMonitorId id)
        {
            return id switch
            {
                EmbraceMemoryMonitorId.GCBytesReserved => "GCBytesReserved_max_value",
                EmbraceMemoryMonitorId.GCBytesUsed => "GCBytesUsed_max_value",
                EmbraceMemoryMonitorId.SystemBytesUsed => "SystemBytesUsed_max_value",
                EmbraceMemoryMonitorId.TotalBytesReserved => "TotalBytesReserved_max_value",
                EmbraceMemoryMonitorId.TotalBytesUsed => "TotalBytesUsed_max_value",
                EmbraceMemoryMonitorId.GCCollectTimeNanos => "GCCollectTimeNanos_max_value",
                _ => throw new ArgumentOutOfRangeException(nameof(id))
            };
        }
        
        public void InitializeMonitoring()
        {
            _embraceMemoryMonitor = new EmbraceMemoryMonitor();
            _lastLogTime = Time.time;
        }

        public void MarkExtendedLifetime()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_embraceMemoryMonitor == null) return;
            
            _embraceMemoryMonitor.GetSnapshotCurrent(_currentSnapshot);
            
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.SystemBytesUsed);
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.TotalBytesReserved);
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.TotalBytesUsed);
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.GCBytesReserved);
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.GCBytesUsed);
            CheckViolation(_currentSnapshot, EmbraceMemoryMonitorId.GCCollectTimeNanos);
            
            if (Time.time - _lastLogTime >= Mathf.Clamp(10f, 300f, logBatchIntervalSeconds))
            {
                LogBatchedViolations();
                ResetViolationTracking();
                _lastLogTime = Time.time;
            }
        }

        public void StartMonitoring()
        {
            if (_embraceMemoryMonitor == null)
            {
                InitializeMonitoring();
            }
            
            _embraceMemoryMonitor?.Start();
        }

        public void StopMonitoring()
        {
            _embraceMemoryMonitor?.Stop();
        }
        
        private void CheckViolation(EmbraceMemorySnapshot snapshot, EmbraceMemoryMonitorId id)
        {
            var index = (int)id;
            var currentValue = snapshot[id];
            
            if (currentValue >= thresholds[id])
            {
                _hasViolations[index] = true;
                _violationCounts[index]++;
                if (currentValue > _maxValues[index])
                {
                    _maxValues[index] = currentValue;
                }
            }
        }
        
        private void LogBatchedViolations()
        {
            bool anyViolations = false;
            for (int i = 0; i < _hasViolations.Length && !anyViolations; i++)
            {
                anyViolations |= _hasViolations[i];
            }
            
            if (!anyViolations) return;
            
            _logProperties.Clear();
            
            for (int i = 0; i < _hasViolations.Length; i++)
            {
                if (!_hasViolations[i]) continue;
                
                var id = (EmbraceMemoryMonitorId)i;
                _logProperties[IDViolationMap(id)] = _violationCounts[i].ToString();
                _logProperties[IDViolationMax(id)] = _maxValues[i].ToString();
            }

            var curGCCollectBytes = _embraceMemoryMonitor.GetCurrentGCCollectBytes();
            if (curGCCollectBytes != 0)
            {
                _logProperties.Add("GCCollectBytes", curGCCollectBytes.ToString());
            }
            
            Embrace.Instance.LogMessage("Memory pressure violations detected in batch", EMBSeverity.Warning, _logProperties);
        }
        
        private void ResetViolationTracking()
        {
            for (int i = 0; i < _hasViolations.Length; i++)
            {
                _hasViolations[i] = false;
                _violationCounts[i] = 0;
                _maxValues[i] = 0;
            }
        }

        private void OnDestroy()
        {
            _embraceMemoryMonitor?.Dispose();
        }
    }
    
    public class EmbraceMemoryMonitor : IDisposable
    {
        private ProfilerRecorder _gcReservedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 60);
        private ProfilerRecorder _gcUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 60);
        private ProfilerRecorder _systemUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 60);
        private ProfilerRecorder _gcCollectTimeMonitor = ProfilerRecorder.StartNew(new ProfilerCategory("GC"), "GC.Collect", 60);
        private ProfilerRecorder _totalReservedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 60);
        private ProfilerRecorder _totalUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 60);
        private ProfilerRecorder _gcCollectMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Collect");

        public void Start()
        {
            _gcReservedMonitor.Start();
            _gcUsedMonitor.Start();
            _systemUsedMonitor.Start();
            _gcCollectTimeMonitor.Start();
            _totalReservedMonitor.Start();
            _totalUsedMonitor.Start();
        }

        public void Stop()
        {
            _gcReservedMonitor.Stop();
            _gcUsedMonitor.Stop();
            _systemUsedMonitor.Stop();
            _gcCollectTimeMonitor.Stop();
            _totalReservedMonitor.Stop();
            _totalUsedMonitor.Stop();
        }

        public EmbraceMemorySnapshot GetSnapshotCurrent(EmbraceMemorySnapshot toReuse = null)
        {
            if (toReuse == null)
                return new EmbraceMemorySnapshot()
                {
                    GCBytesReserved = _gcReservedMonitor.CurrentValue,
                    GCBytesUsed = _gcUsedMonitor.CurrentValue,
                    SystemBytesUsed = _systemUsedMonitor.CurrentValue,
                    TotalBytesReserved = _totalReservedMonitor.CurrentValue,
                    TotalBytesUsed = _totalUsedMonitor.CurrentValue,
                    GCCollectTimeNanos = _gcCollectTimeMonitor.CurrentValue
                };
            
            toReuse.GCBytesReserved = _gcReservedMonitor.CurrentValue;
            toReuse.GCBytesUsed = _gcUsedMonitor.CurrentValue;
            toReuse.SystemBytesUsed = _systemUsedMonitor.CurrentValue;
            toReuse.TotalBytesReserved = _totalReservedMonitor.CurrentValue;
            toReuse.TotalBytesUsed = _totalUsedMonitor.CurrentValue;
            toReuse.GCCollectTimeNanos = _gcCollectTimeMonitor.CurrentValue;
            return toReuse;
        }

        public long GetCurrentGCCollectBytes() => _gcCollectMonitor.CurrentValue;

        public EmbraceMemorySnapshot GetSnapshotLast()
        {
            return new EmbraceMemorySnapshot()
            {
                GCBytesReserved = _gcReservedMonitor.LastValue,
                GCBytesUsed = _gcUsedMonitor.LastValue,
                SystemBytesUsed = _systemUsedMonitor.LastValue,
                TotalBytesReserved = _totalReservedMonitor.LastValue,
                TotalBytesUsed = _totalUsedMonitor.LastValue,
                GCCollectTimeNanos = _gcCollectTimeMonitor.LastValue
            };
        }
        
        public void Dispose()
        {
            _gcReservedMonitor.Dispose();
            _gcUsedMonitor.Dispose();
            _systemUsedMonitor.Dispose();
            _gcCollectTimeMonitor.Dispose();
            _totalReservedMonitor.Dispose();
            _totalUsedMonitor.Dispose();
        }
    }
    
    [Serializable]
    public class EmbraceMemorySnapshot
    {
        public long this[EmbraceMemoryMonitorId index]
        {
            get
            {
                return index switch
                {
                    EmbraceMemoryMonitorId.GCBytesReserved => GCBytesReserved,
                    EmbraceMemoryMonitorId.GCBytesUsed => GCBytesUsed,
                    EmbraceMemoryMonitorId.SystemBytesUsed => SystemBytesUsed,
                    EmbraceMemoryMonitorId.TotalBytesReserved => TotalBytesReserved,
                    EmbraceMemoryMonitorId.TotalBytesUsed => TotalBytesUsed,
                    EmbraceMemoryMonitorId.GCCollectTimeNanos => GCCollectTimeNanos,
                    _ => throw new ArgumentException("Illegal EmbraceMemoryMonitorID given")
                };
            }
        }
        
        public static (long min, long max) GetValidRange(EmbraceMemoryMonitorId id)
        {
            return id switch
            {
                EmbraceMemoryMonitorId.GCBytesReserved => (20000000L, 1000000000L), // 20MB to 1GB
                EmbraceMemoryMonitorId.GCBytesUsed => (10000000L, 1000000000L), // 10MB to 1GB
                EmbraceMemoryMonitorId.SystemBytesUsed => (50000000L, 1000000000L), // 50MB to 1GB
                EmbraceMemoryMonitorId.TotalBytesReserved => (80000000L, 1000000000L), // 80MB to 1GB
                EmbraceMemoryMonitorId.TotalBytesUsed => (60000000L, 1000000000L), // 60MB to 1GB
                EmbraceMemoryMonitorId.GCCollectTimeNanos => (1000000L, 1000000000L), // 1ms to 1s
                _ => throw new ArgumentOutOfRangeException(nameof(id), "Illegal EmbraceMemoryMonitorID given")
            };
        }

        [Range(0f, 1000000000L)] public long GCBytesReserved = 150000000L; // 150MB
        [Range(0f, 1000000000L)] public long GCBytesUsed = 100000000L; // 100MB
        [Range(0f, 1000000000L)] public long SystemBytesUsed = 400000000L; // 400MB
        [Range(0f, 1000000000L)] public long TotalBytesReserved = 600000000L; // 600MB
        [Range(0f, 1000000000L)] public long TotalBytesUsed = 450000000L; // 450MB
        [Range(1, 1000000000L)] public long GCCollectTimeNanos = 5000000L; // 5ms
    }

    public enum EmbraceMemoryMonitorId
    {
        GCBytesReserved,
        GCBytesUsed,
        SystemBytesUsed,
        TotalBytesReserved,
        TotalBytesUsed,
        GCCollectTimeNanos,
        _EnumTypeCount // This is used to determine the number of enum values
    }
}
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    public class EmbraceMemoryMonobehaviour : MonoBehaviour
    {
        private EmbraceMemoryMonitor _embraceMemoryMonitor;
        public EmbraceMemorySnapshot thresholds;
        public float logBatchIntervalSeconds = 10.0f;
        private float _lastLogTime;
        private bool[] _hasViolations = new bool[(int) EmbraceMemoryMonitorId._EnumTypeCount];
        private int[] _violationCounts = new int[(int) EmbraceMemoryMonitorId._EnumTypeCount];
        private long[] _maxValues = new long[(int) EmbraceMemoryMonitorId._EnumTypeCount];
        
        private Dictionary<string, string> _logProperties = new Dictionary<string, string>();

        #if EMBRACE_MEMORY_MONITOR
        
        private void Awake()
        {
            InitializeMonitoring();
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
            var currentSnapshot = _embraceMemoryMonitor.GetSnapshotCurrent();
            
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.SystemBytesUsed);
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.TotalBytesReserved);
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.TotalBytesUsed);
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.GCBytesReserved);
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.GCBytesUsed);
            CheckViolation(currentSnapshot, EmbraceMemoryMonitorId.GCCollectTimeNanos);
            
            if (Time.time - _lastLogTime >= logBatchIntervalSeconds)
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
            else
            {
                _embraceMemoryMonitor?.Start();
            }
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

        public EmbraceMemorySnapshot GetSnapshotCurrent()
        {
            return new EmbraceMemorySnapshot()
            {
                GCBytesReserved = _gcReservedMonitor.CurrentValue,
                GCBytesUsed = _gcUsedMonitor.CurrentValue,
                SystemBytesUsed = _systemUsedMonitor.CurrentValue,
                TotalBytesReserved = _totalReservedMonitor.CurrentValue,
                TotalBytesUsed = _totalUsedMonitor.CurrentValue,
                GCCollectTimeNanos = _gcCollectTimeMonitor.CurrentValue
            };
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
    public struct EmbraceMemorySnapshot
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
        
        public long GCBytesReserved;
        public long GCBytesUsed;
        public long SystemBytesUsed;
        public long TotalBytesReserved;
        public long TotalBytesUsed;
        public long GCCollectTimeNanos;
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
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
        
        private Dictionary<string, string> logProperties = new Dictionary<string, string>();

        private static string IDViolationMap(EmbraceMemoryMonitorId id)
        {
            switch (id)
            {
                case EmbraceMemoryMonitorId.GCBytesReserved:
                    return "GCBytesReserved_violation_count";
                case EmbraceMemoryMonitorId.GCBytesUsed:
                    return "GCBytesUsed_violation_count";
                case EmbraceMemoryMonitorId.SystemBytesUsed:
                    return "SystemBytesUsed_violation_count";
                case EmbraceMemoryMonitorId.TotalBytesReserved:
                    return "TotalBytesReserved_violation_count";
                case EmbraceMemoryMonitorId.TotalBytesUsed:
                    return "TotalBytesUsed_violation_count";
                case EmbraceMemoryMonitorId.GCCollectTimeNanos:
                    return "GCCollectTimeNanos_violation_count";
                default:
                    throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        private static string IDViolationMax(EmbraceMemoryMonitorId id)
        {
            switch (id)
            {
                case EmbraceMemoryMonitorId.GCBytesReserved:
                    return "GCBytesReserved_max_value";
                case EmbraceMemoryMonitorId.GCBytesUsed:
                    return "GCBytesUsed_max_value";
                case EmbraceMemoryMonitorId.SystemBytesUsed:
                    return "SystemBytesUsed_max_value";
                case EmbraceMemoryMonitorId.TotalBytesReserved:
                    return "TotalBytesReserved_max_value";
                case EmbraceMemoryMonitorId.TotalBytesUsed:
                    return "TotalBytesUsed_max_value";
                case EmbraceMemoryMonitorId.GCCollectTimeNanos:
                    return "GCCollectTimeNanos_max_value";
                default:
                    throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
        
        public void InitializeMonitoring()
        {
            _embraceMemoryMonitor = new EmbraceMemoryMonitor();
            _lastLogTime = Time.time;
        }

        public void MarkExtendedLifetime()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Awake()
        {
            InitializeMonitoring();
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
                _embraceMemoryMonitor.Start();
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
            
            logProperties.Clear();
            
            for (int i = 0; i < _hasViolations.Length; i++)
            {
                if (!_hasViolations[i]) continue;
                
                var id = (EmbraceMemoryMonitorId)i;
                logProperties[IDViolationMap(id)] = _violationCounts[i].ToString();
                logProperties[IDViolationMax(id)] = _maxValues[i].ToString();
            }
            
            Embrace.Instance.LogMessage("Memory pressure violations detected in batch", EMBSeverity.Warning, logProperties);
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
            _embraceMemoryMonitor.Dispose();
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
                switch (index)
                {
                    case EmbraceMemoryMonitorId.SystemBytesUsed:
                        return SystemBytesUsed;
                    case EmbraceMemoryMonitorId.TotalBytesReserved:
                        return TotalBytesReserved;
                    case EmbraceMemoryMonitorId.TotalBytesUsed:
                        return TotalBytesUsed;
                    case EmbraceMemoryMonitorId.GCBytesReserved:
                        return GCBytesReserved;
                    case EmbraceMemoryMonitorId.GCBytesUsed:
                        return GCBytesUsed;
                    case EmbraceMemoryMonitorId.GCCollectTimeNanos:
                        return GCCollectTimeNanos;
                    default:
                        throw new ArgumentException("Illegal EmbraceMemoryMonitorID given");
                }
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
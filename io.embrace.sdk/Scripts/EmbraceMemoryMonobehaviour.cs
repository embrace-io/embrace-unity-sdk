using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace EmbraceSDK.Instrumentation
{
    public class EmbraceMemoryMonobehaviour : MonoBehaviour
    {
        private EmbraceMemoryMonitor _embraceMemoryMonitor;
        public EmbraceMemorySnapshot _thresholds;
        public Dictionary<EmbraceMemoryMonitorId, string> _spanIdDict;
        
        public void InitializeMonitoring()
        {
            _embraceMemoryMonitor = new EmbraceMemoryMonitor();
        }

        public void MarkExtendedLifetime()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Awake()
        {
            InitializeMonitoring();
        }

        void Update()
        {
            UpdateSpan(EmbraceMemoryMonitorId.SystemBytesUsed, "Embrace Unity System Bytes Used Over Threshold");
            UpdateSpan(EmbraceMemoryMonitorId.TotalBytesReserved, "Embrace Unity Total Bytes Reserved Over Threshold");
            UpdateSpan(EmbraceMemoryMonitorId.TotalBytesUsed, "Embrace Unity Total Bytes Used Over Threshold");
            UpdateSpan(EmbraceMemoryMonitorId.GCBytesReserved, "Embrace Unity GC Bytes Reserved Over Threshold");
            UpdateSpan(EmbraceMemoryMonitorId.GCBytesUsed, "Embrace Unity GC Bytes Used Over Threshold");
            UpdateSpan(EmbraceMemoryMonitorId.GCCollectTimeNanos, "Embrace Unity GC Collect Time Nanoseconds Over Threshold");
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
        
        private void UpdateSpan(EmbraceMemoryMonitorId id, string spanName)
        {
            
            var currentSnapshot = _embraceMemoryMonitor.GetSnapshotCurrent();
            
            if (currentSnapshot[id] >= _thresholds[id] &&
                !_spanIdDict.ContainsKey(id))
            {
                _spanIdDict[id] = Embrace.Instance.StartSpan(spanName, 
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            } else if (currentSnapshot[id] < _thresholds[id] && _spanIdDict.ContainsKey(id))
            {
                Embrace.Instance.StopSpan(_spanIdDict[id], 
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); 
                    _spanIdDict.Remove(id);
            }
        }

        private void OnDestroy()
        {
            _embraceMemoryMonitor.Dispose();
        }
    }

    public class EmbraceMemoryMonitor : IDisposable
    {
        private ProfilerRecorder gcReservedMonitor;
        private ProfilerRecorder gcUsedMonitor;
        private ProfilerRecorder systemUsedMonitor;
        private ProfilerRecorder gcCollectTimeMonitor;
        private ProfilerRecorder totalReservedMonitor;
        private ProfilerRecorder totalUsedMonitor;
        public EmbraceMemoryMonitor()
        {
            gcReservedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 60);
            gcUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 60);
            systemUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 60);
            gcCollectTimeMonitor = ProfilerRecorder.StartNew(new ProfilerCategory("GC"), "GC.Collect", 60);
            totalReservedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 60);
            totalUsedMonitor = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 60);
        }

        public void Start()
        {
            gcReservedMonitor.Start();
            gcUsedMonitor.Start();
            systemUsedMonitor.Start();
            gcCollectTimeMonitor.Start();
            totalReservedMonitor.Start();
            totalUsedMonitor.Start();
        }

        public void Stop()
        {
            gcReservedMonitor.Stop();
            gcUsedMonitor.Stop();
            systemUsedMonitor.Stop();
            gcCollectTimeMonitor.Stop();
            totalReservedMonitor.Stop();
            totalUsedMonitor.Stop();
        }

        public EmbraceMemorySnapshot GetSnapshotCurrent()
        {
            return new EmbraceMemorySnapshot()
            {
                GCBytesReserved = gcReservedMonitor.CurrentValue,
                GCBytesUsed = gcUsedMonitor.CurrentValue,
                SystemBytesUsed = systemUsedMonitor.CurrentValue,
                TotalBytesReserved = totalReservedMonitor.CurrentValue,
                TotalBytesUsed = totalUsedMonitor.CurrentValue,
                GCCollectTimeNanos = gcCollectTimeMonitor.CurrentValue
            };
        }

        public EmbraceMemorySnapshot GetSnapshotLast()
        {
            return new EmbraceMemorySnapshot()
            {
                GCBytesReserved = gcReservedMonitor.LastValue,
                GCBytesUsed = gcUsedMonitor.LastValue,
                SystemBytesUsed = systemUsedMonitor.LastValue,
                TotalBytesReserved = totalReservedMonitor.LastValue,
                TotalBytesUsed = totalUsedMonitor.LastValue,
                GCCollectTimeNanos = gcCollectTimeMonitor.LastValue
            };
        }
        
        public void Dispose()
        {
            gcReservedMonitor.Dispose();
            gcUsedMonitor.Dispose();
            systemUsedMonitor.Dispose();
            gcCollectTimeMonitor.Dispose();
            totalReservedMonitor.Dispose();
            totalUsedMonitor.Dispose();
        }
    }
    
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
    }
}
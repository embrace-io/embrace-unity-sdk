using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

namespace EmbraceSDK.Utilities
{
    internal class EmbraceProfilerRecorderHelper
    {
        private readonly Dictionary<string, ProfilerRecorder> _profileRecords = new();

        private List<(string, string)> _profileRecordNames = new()
        {
            ("PlayerLoop", "PlayerLoop"),
            ("GC", "GC.Collect"),
            ("Gui", "GUI.Repaint"),
            ("Render", "Camera.Render"),
            ("Render", "Canvas.RenderSubBatch")
        };

        public EmbraceProfilerRecorderHelper()
        {
            foreach(var (categoryName, statName) in _profileRecordNames)
            {
                ProfilerCategory category = new ProfilerCategory(categoryName);
                var recorder = ProfilerRecorder.StartNew(category, statName, 15);
                _profileRecords.Add(statName, recorder);
            }
        }
        
        public Dictionary<string, float> GenerateAttributes()
        {
            Dictionary<string, float> attributes = new();
            
            foreach ((string name, var recorder) in _profileRecords)
            {
                if (recorder.Count > 0)
                {
                    long sum = 0;
                    for (int i = 0; i < recorder.Count; i++)
                    {
                        var sample = recorder.GetSample(i);
                        sum += sample.Value;
                    }
                    
                    float average = sum / (float)recorder.Count / 1000000f; // convert to milliseconds

                    if (average == 0)
                    {
                        // Skip attributes with an average of 0 to avoid cluttering the span
                        continue;
                    }
                    
                    attributes[$"profiler-marker-{name}-ms"] = average;
                }
            }
            
            var sortedByValue = attributes.OrderBy(kvp => kvp.Value);
            return sortedByValue.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        public void Dispose()
        {
            foreach (var recorder in _profileRecords.Values)
            {
                recorder.Dispose();
            }
            
            _profileRecords.Clear();
        }
        
        public void Reset()
        {
            foreach (var recorder in _profileRecords.Values)
            {
                recorder.Reset();
            }
        }
        
        public void Start()
        {
            foreach (var recorder in _profileRecords.Values)
            {
                recorder.Start();
            }
        }
    }
    
    public class EmbraceFrameMeasurer : MonoBehaviour
    {
        public class EmbraceLowFrameRateReport
        {
            public float FrameTime;
            public float AverageFPS => 1f / (FrameTime / FrameCount);
            public int FrameCount;
            public int LowFrameRateCount;

            public void AddFrameTime(float frameTime)
            {
                FrameTime += frameTime;
                FrameCount++;
            }

            public void Reset()
            {
                FrameTime = 0f;
                FrameCount = 0;
                LowFrameRateCount = 0;
            }
            
            public void AddLowFrameRate()
            {
                LowFrameRateCount++;
            }
        }
        
        private readonly EmbraceProfilerRecorderHelper _profilerRecorderHelper = new();
        private readonly EmbraceLowFrameRateReport _frameRateReport = new();
        private float _totalSessionTime;
        [SerializeField] private float _targetFrameRate = 30f;
        [SerializeField] private float _reportInterval = 60f;
        private float _reportIntervalRemaining = 60f;
        private float _previousFrameTime = 0f;
        private int _totalSessionFrames = 0;
        
        public float SessionAverageFPS => 1 / (_totalSessionTime / _totalSessionFrames);
        
#if EMBRACE_AUTO_INSTRUMENTATION_FPS_CAPTURE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnSceneLoad()
        {
            if (!FindFirstObjectByType<EmbraceFrameMeasurer>())
            {
                var prefab = Resources.Load<EmbraceFrameMeasurer>("EmbraceFrameMeasurer");
                
                if (prefab == null)
                {
                    Debug.LogError("EmbraceFrameMeasurer prefab not found in Resources folder.");
                    return;
                }
                
                var instance = Instantiate(prefab);
                instance.name = "EmbraceFrameMeasurer";
            }
        }
#endif

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _previousFrameTime = Time.unscaledDeltaTime;
            _reportIntervalRemaining = _reportInterval;
            _profilerRecorderHelper.Reset();
            _profilerRecorderHelper.Start();
        }

        private void OnDestroy()
        {
            _profilerRecorderHelper.Dispose();
        }
        
        private void Update()
        {
            if (Embrace.Instance.IsStarted == false)
            {
                return;
            }
            
            _frameRateReport.AddFrameTime(Time.unscaledDeltaTime);
            _reportIntervalRemaining -= Time.unscaledDeltaTime;
            _totalSessionTime += Time.unscaledDeltaTime;
            _totalSessionFrames++;

            float difference = Time.unscaledDeltaTime - _previousFrameTime;
            
            if (difference > 1f / _targetFrameRate)
            {
                _frameRateReport.AddLowFrameRate();
            }
            
            if (_reportIntervalRemaining <= 0)
            {
                ReportFrameRate();
                _reportIntervalRemaining = _reportInterval;
                _profilerRecorderHelper.Reset();
                _profilerRecorderHelper.Start();
            }
        }

        private void ReportFrameRate()
        {
            Dictionary<string, string> properties = new()
            {
                { "record-average-fps", _frameRateReport.AverageFPS.ToString("F2") },
                { "low-frame-rate-count", $"{_frameRateReport.LowFrameRateCount} / {_frameRateReport.FrameCount} ({(float)_frameRateReport.LowFrameRateCount / _frameRateReport.FrameCount}%)" }
            };
            
            var recordedAttributes = _profilerRecorderHelper.GenerateAttributes();
            
            foreach ((string key, float value) in recordedAttributes)
            {
                properties[key] = value.ToString("F2");
            }
            
            Embrace.Instance.LogMessage("frame-rate-report", EMBSeverity.Info, properties);
            Embrace.Instance.AddSessionProperty("session-average-fps", SessionAverageFPS.ToString("F2"), false);
            _frameRateReport.Reset();
        }
    }
}

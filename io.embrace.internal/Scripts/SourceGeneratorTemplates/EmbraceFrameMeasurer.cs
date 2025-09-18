using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.Utilities
{
    /// <summary>
    /// The Frame Measurer is a utility that allows you to measure frame rate over a specified interval.
    /// It will also report low frame rates based on a target frame rate.
    /// Optionally you can also set profiler markers to look for specific performance issues.
    /// </summary>
    public class EmbraceFrameMeasurer : MonoBehaviour
    {
        /// <summary>
        /// Tracks frame rates and low frame rates over a specified interval.
        /// </summary>
        public class EmbraceFrameRateReport
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
        private readonly EmbraceFrameRateReport _frameRateReport = new();
        private float _totalSessionTime;
        private float _targetFrameRate = 30f;
        private float _reportInterval = 60f;
        private float _reportIntervalRemaining = 60f;
        private float _previousFrameTime = 0f;
        private int _totalSessionFrames = 0;
        
        public float SessionAverageFPS => 1 / (_totalSessionTime / _totalSessionFrames);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnSceneLoad()
        {
            if (!FindFirstObjectByType<EmbraceFrameMeasurer>())
            {
                var go = new GameObject("EmbraceFrameMeasurer");
                go.AddComponent<EmbraceFrameMeasurer>();
            }
            else
            {
                Debug.LogWarning("EmbraceFrameMeasurer is already loaded");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _reportInterval = Mathf.Max(_reportInterval, 10f); // Minimum of 10 seconds per report
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

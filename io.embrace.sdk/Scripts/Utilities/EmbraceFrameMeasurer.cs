using System;
using System.Collections.Generic;
using System.Globalization;
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
                var recorder = ProfilerRecorder.StartNew(category, statName, 40);
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
                    
                    attributes[name] = average;
                }
            }

            return attributes;
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
    
    /// <summary>
    /// Measures the difference between the current frame and the previous frame.
    /// Keeps track of the DateTimeOffset of the previous frame.
    /// If the frame rate drops below a certain threshold, it will enter a "low frame rate" state.
    /// Once the low frame state ends we create span that starts with the first frame measured and ends with the last frame measured.
    /// Note: Hook into OnDestroy to create a span in case the user closes the app before the low frame rate state ends.
    /// Note2: Crash Handling? Reason for span creation (recovery, app closed by user, crash)
    /// Profiler markers are grabbed from the Profiler Reporter API and added to the span attributes.
    /// Note3: Create child spans for GPU and CPU frame time if they are also in low frame rate state.
    /// </summary>
    public class EmbraceFrameMeasurer : MonoBehaviour
    {
        private EmbraceProfilerRecorderHelper _profilerRecorderHelper = new();
        private DateTimeOffset _lowFrameStopTime;
        private const string _spanName = "emb-unity-frame-rate-alert";
        private float _targetFrameRate = 30f; // TODO: Pull this from settings
        private float _lowFrameTime = 0;
        private float _cooldownTime = 3.0f;
        private int _lowFrameCount = 0;
        private bool _isLowFrameRateState;
        private string _spanId = null;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnSceneLoad()
        {
            if (!FindFirstObjectByType<EmbraceFrameMeasurer>())
            {
                _ = new GameObject("EmbraceFrameMeasurer", typeof(EmbraceFrameMeasurer));
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ProfilerCategory category = new ProfilerCategory("PlayerLoop");
        }

        private void OnDestroy()
        {
            if (_isLowFrameRateState)
            {
                RecordLowFrameState();
            }
            
            _profilerRecorderHelper.Dispose();
        }
        
        private void Update()
        {
            float currentFrameTime = Time.unscaledDeltaTime;
            float frameRate = 1f / currentFrameTime;

            if (_isLowFrameRateState && frameRate >= _targetFrameRate && _cooldownTime > 0)
            {
                if (_lowFrameStopTime == default)
                {
                    _lowFrameStopTime = DateTimeOffset.UtcNow;
                }
                
                _cooldownTime -= Time.unscaledDeltaTime;
            }
            else if (_isLowFrameRateState && frameRate >= _targetFrameRate && _cooldownTime <= 0)
            {
                RecordLowFrameState();
            }
            else if (!_isLowFrameRateState && frameRate < _targetFrameRate)
            {
                _spanId = Embrace.Instance.StartSpan(_spanName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                _profilerRecorderHelper.Reset();
                _profilerRecorderHelper.Start();
                _isLowFrameRateState = true;
                _lowFrameCount = 0;
                _lowFrameTime = 0;
                _cooldownTime = 3.0f;
                _lowFrameStopTime = default;
            }

            if (_isLowFrameRateState)
            {
                _lowFrameCount++;
                _lowFrameTime += currentFrameTime;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                _targetFrameRate = float.MaxValue;
            }
            else
            {
                _targetFrameRate = 30f;
            }
        }

        private void RecordLowFrameState()
        {
            _isLowFrameRateState = false;
            Embrace.Instance.AddSpanAttribute(_spanId, "AverageFPS", (1 / (_lowFrameTime / _lowFrameCount)).ToString(CultureInfo.InvariantCulture));
            var attributes = _profilerRecorderHelper.GenerateAttributes();
            
            // sort attributes by value (highest to lowest)
            var sortedAttributes = new SortedDictionary<float, string>(Comparer<float>.Create((x, y) => y.CompareTo(x)));
            
            foreach (var kvp in attributes)
            {
                sortedAttributes.Add(kvp.Value, kvp.Key);
            }
            
            foreach (var kvp in sortedAttributes)
            {
                Embrace.Instance.AddSpanAttribute(_spanId, kvp.Key.ToString(CultureInfo.InvariantCulture), kvp.Value);
            }

            Embrace.Instance.StopSpan(_spanId, _lowFrameStopTime.ToUnixTimeMilliseconds());
        }
    }
}

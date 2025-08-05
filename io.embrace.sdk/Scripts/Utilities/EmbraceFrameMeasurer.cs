using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Profiling;
using UnityEngine;

namespace EmbraceSDK.Utilities
{
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
        private ProfilerRecorder _mainThreadRecorder;
        private const string _spanName = "emb-unity-frame-rate-alert";
        private float _targetFrameRate = 30f; // TODO: Pull this from settings
        private DateTimeOffset _previousFrameTimeOffset;
        private float _badFrameTime = 0;
        private int _badFrameCount = 0;
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
            _mainThreadRecorder = ProfilerRecorder.StartNew(category, "PlayerLoop", 40);
        }

        private void OnDestroy()
        {
            if (_isLowFrameRateState)
            {
                DateTimeOffset currentTime = DateTimeOffset.Now;
                RecordLowFrameState(currentTime);
            }
            
            _mainThreadRecorder.Dispose();
        }
        
        private void Update()
        {
            DateTimeOffset currentTime = DateTimeOffset.Now;
            float currentFrameTime = Time.unscaledDeltaTime;
            float frameRate = 1f / currentFrameTime;
            
            if (_isLowFrameRateState && frameRate >= _targetFrameRate)
            {
                RecordLowFrameState(currentTime);
            }
            else if (!_isLowFrameRateState && frameRate < _targetFrameRate)
            {
                _spanId = Embrace.Instance.StartSpan(_spanName, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                _mainThreadRecorder.Reset();
                _mainThreadRecorder.Start();
                _isLowFrameRateState = true;
                _badFrameCount = 0;
                _badFrameTime = 0;
                _previousFrameTimeOffset = currentTime;
            }

            if (_isLowFrameRateState)
            {
                _badFrameCount++;
                _badFrameTime += currentFrameTime;
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

        private void RecordLowFrameState(DateTimeOffset currentTime)
        {
            _isLowFrameRateState = false;
                
            Embrace.Instance.AddSpanAttribute(_spanId, "AverageFPS", (1 / (_badFrameTime / _badFrameCount)).ToString(CultureInfo.InvariantCulture));

            if (_mainThreadRecorder.Count > 0)
            {
                float averagePlayerLoopTime = GetAverageSample();
                averagePlayerLoopTime /= 1000000f; // convert to milliseconds
                Embrace.Instance.AddSpanAttribute(_spanId, "AveragePlayerLoopTimeMS", averagePlayerLoopTime.ToString(CultureInfo.InvariantCulture));
            }

            Embrace.Instance.StopSpan(_spanId, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }

        private long GetAverageSample()
        {
            long sum = 0;
            
            for(int i = 0; i < _mainThreadRecorder.Count; i++)
            {
                var sample = _mainThreadRecorder.GetSample(i);
                sum += sample.Value;
            }
            
            return sum / _mainThreadRecorder.Count;
        }
    }
}

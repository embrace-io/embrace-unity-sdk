using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;

namespace EmbraceSDK.Utilities
{
    /// <summary>
    /// Helps manage profiler recorder markers
    /// </summary>
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
}
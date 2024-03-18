using System.Diagnostics;

namespace EmbraceSDK.Utilities
{

    /// <summary>
    /// Utility class for helping manage Time.
    /// </summary>
    public class TimeUtil
    {
        // We are unable to use Time.time due to it being required by the main thread.
        // So we implemented our own time using the Stopwatch class.
        private static Stopwatch stopWatch;

        public static void InitStopWatch()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        /// <summary>
        /// The time that has elapsed (Read Only).
        /// </summary>
        public static float time
        {
            get
            {
                if (mockTime != -1) return mockTime;
                return (float)stopWatch.Elapsed.TotalSeconds;
            }
        }

        private static float mockTime = -1;

        /// <summary>
        /// Set a mock time that can be used for testing.
        /// </summary>
        /// <param name="time"></param>
        public static void SetMockTime(float time)
        {
            mockTime = time;
        }

        /// <summary>
        /// Clean up time.
        /// </summary>
        public static void Clean()
        {
            mockTime = -1;
        }
    }
}
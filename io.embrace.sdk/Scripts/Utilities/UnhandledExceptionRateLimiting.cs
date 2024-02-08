using System.Collections.Generic;

namespace EmbraceSDK
{
    /// <summary>
    /// UnhandledExceptionRateLimiting prevents too many UnhandledException being called in a given moment.
    /// </summary>
    public class UnhandledExceptionRateLimiting
    {
        public float uniqueExceptionTimePeriodSec = 5.0f;
        public int uniqueExceptionMaxCount = 2; // 100;
        public float uniqueExceptionLastTrimTimeSec = 0.0f;
        public float uniqueExceptionMinTrimPeriodSec = 5.0f;
        public float exceptionsWindowTimeSec = 30.0f; // 5.0f;
        public int exceptionsWindowCount = 3; // 20;

        private readonly object allowLock = new object();

        private List<float> sendTime = new List<float>();

        private Dictionary<UnhandledException, float> uniqueExceptions = new Dictionary<UnhandledException, float>(new UnhandledExceptionEqualityComparer());


        public bool IsAllowed(UnhandledException exception)
        {
            lock (allowLock)
            {
                float now = TimeUtil.time;

                // Check if we have exceeded the count for the given time window.
                if (sendTime.Count >= exceptionsWindowCount)
                {
                    if (now - sendTime[0] < exceptionsWindowTimeSec)
                    {
                        EmbraceLogger.LogWarning($"Rejecting unhandled exception. Hit max count {exceptionsWindowCount} and oldest is only {now - sendTime[0]}.");
                        return false;
                    }
                }

                // Check if the the last time this exception was sent was longer ago than its cool-off period.
                if (uniqueExceptions.ContainsKey(exception))
                {
                    float lastTime = uniqueExceptions[exception];
                    if (now - lastTime < uniqueExceptionTimePeriodSec)
                    {
                        EmbraceLogger.LogWarning($"Rejecting unhandled exception. Duplicate unique last sent {now - lastTime} seconds ago");
                        return false;
                    }
                }

                // Track time for each unique exception
                uniqueExceptions[exception] = now;
                // Trim unique exception count tracked and if it's been long enough seen we last trimmed
                if ((uniqueExceptions.Count > uniqueExceptionMaxCount) && (now - uniqueExceptionLastTrimTimeSec > uniqueExceptionMinTrimPeriodSec))
                {
                    List<UnhandledException> removals = new List<UnhandledException>();
                    // Find all unique exceptions that occurred more than UniqueExceptionTimePeriod seconds ago and remove them since they would not block
                    foreach (UnhandledException ue in uniqueExceptions.Keys)
                    {
                        if (now - uniqueExceptions[ue] > uniqueExceptionTimePeriodSec)
                        {
                            removals.Add(ue);
                        }
                    }
                    foreach (UnhandledException ue in removals)
                    {
                        uniqueExceptions.Remove(ue);

                    }

                    uniqueExceptionLastTrimTimeSec = now;
                }

                // Track time for all sent exceptions
                sendTime.Add(now);
                // Trim time list of expired sent exceptions.
                while (sendTime.Count > 0)
                {
                    if (now > sendTime[0] + uniqueExceptionTimePeriodSec)
                    {
                        sendTime.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }

                return true;
            }

        }

        public int GetExceptionsCount()
        {
            return uniqueExceptions.Count;
        }
    }
}
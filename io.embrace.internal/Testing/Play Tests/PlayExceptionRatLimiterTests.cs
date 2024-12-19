using System;
using System.Collections;
using System.Collections.Generic;
using EmbraceSDK;
using EmbraceSDK.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayExceptionRatLimiterTests
    {
        [SetUp]
        public void SetUp()
        {
            TimeUtil.Clean();
            TimeUtil.InitStopWatch();
        }

        [UnityTest]
        public IEnumerator UnhandledExceptionRateLimiting_DuplicateExceptions()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            UnhandledException ue = new UnhandledException("Test", Environment.StackTrace);

            // Shorten window so test runs faster
            rateLimiter.uniqueExceptionTimePeriodSec = 1f;

            // First instance of the exception is allowed
            Assert.IsTrue(rateLimiter.IsAllowed(ue));
            Debug.Log("Completed first exception");

            // Second instance of the exception is not allowed
            Assert.IsFalse(rateLimiter.IsAllowed(ue));
            Debug.Log("Completed second exception");

            // After cooldown period, the exception is allowed again
            var before = DateTime.Now;

            while (true)
            {
                var delta = DateTime.Now - before;
                if (delta.TotalSeconds > (rateLimiter.uniqueExceptionTimePeriodSec + 0.1f))
                {
                    break;
                }

                yield return null;
            }

            Assert.IsTrue(rateLimiter.IsAllowed(ue));
            Debug.Log("Completed third exception");
        }

        [UnityTest]
        public IEnumerator UnhandledExceptionRateLimiting_MaxRateWindow()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            // Shorten window so test runs faster
            rateLimiter.exceptionsWindowTimeSec = 1f;

            // Unique exception up to max window count are allowed
            string stackTrace = Environment.StackTrace;
            for (int i = 0; i < rateLimiter.exceptionsWindowCount; ++i)
            {
                UnhandledException exception = new UnhandledException($"TestException{i}", stackTrace);
                Assert.IsTrue(rateLimiter.IsAllowed(exception));
                yield return null;
            }

            // 4th unique exception is not allowed
            UnhandledException blockedException = new UnhandledException("BlockedException", Environment.StackTrace);
            Assert.IsFalse(rateLimiter.IsAllowed(blockedException));
            yield return null;

            // After cooldown, another unique exception is allowed
            yield return new WaitForSeconds(rateLimiter.exceptionsWindowTimeSec);
            UnhandledException allowedException = new UnhandledException("AllowedException", Environment.StackTrace);
            Assert.IsTrue(rateLimiter.IsAllowed(allowedException));
        }
    }
}
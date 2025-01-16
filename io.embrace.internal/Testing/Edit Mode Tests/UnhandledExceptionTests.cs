using NSubstitute;
using NUnit.Framework;
using System;
using EmbraceSDK.Internal;
using EmbraceSDK.Utilities;
using UnityEngine;
using UnityEngine.TestTools;


namespace EmbraceSDK.Tests
{
    /// <summary>
    /// Provides test for Unhandled Exception and the Unhandled Exception Rate Limiting.
    /// </summary>
    public class UnhandledExceptionTests
    {
        [Test]
        public void UnhandledException()
        {
            string message = "Test message";
            string stackTrace = Environment.StackTrace;
            UnhandledException ue = new UnhandledException(message, stackTrace);

            Assert.AreEqual(ue.Message, message);
            Assert.AreEqual(ue.StackTrace, stackTrace);
        }

        [Test]
        public void UnhandledExceptionRateLimiting_NotAllowed()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            UnhandledException ue = new UnhandledException("Test", Environment.StackTrace);

            TimeUtil.SetMockTime(0);
            rateLimiter.IsAllowed(ue);
            TimeUtil.SetMockTime(rateLimiter.uniqueExceptionTimePeriodSec - 0.01f);

            Assert.IsFalse(rateLimiter.IsAllowed(ue));

            //clean up
            TimeUtil.Clean();
        }

        [Test]
        public void UnhandledExceptionRateLimiting_Allowed()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            UnhandledException ue = new UnhandledException("Test", Environment.StackTrace);

            TimeUtil.SetMockTime(0);
            rateLimiter.IsAllowed(ue);
            TimeUtil.SetMockTime(rateLimiter.uniqueExceptionTimePeriodSec + 1);

            Assert.IsTrue(rateLimiter.IsAllowed(ue));

            //clean up
            TimeUtil.Clean();
        }

        [Test]
        public void UnhandledExceptionRateLimitingExceptions_WindowCountExceeded()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            int startTime = 1;

            UnhandledException ue = new UnhandledException("Test0", Environment.StackTrace);
            TimeUtil.SetMockTime(startTime);
            rateLimiter.IsAllowed(ue);


            UnhandledException ue1 = new UnhandledException("Test1", Environment.StackTrace);
            TimeUtil.SetMockTime(2);
            rateLimiter.IsAllowed(ue1);

            UnhandledException ue2 = new UnhandledException("Test2", Environment.StackTrace);
            TimeUtil.SetMockTime(3);
            rateLimiter.IsAllowed(ue2);

            UnhandledException ue3 = new UnhandledException("Test3", Environment.StackTrace);
            TimeUtil.SetMockTime(rateLimiter.exceptionsWindowTimeSec + startTime);
            Assert.IsTrue(rateLimiter.IsAllowed(ue3));

            //clean up
            TimeUtil.Clean();
        }

        [Test]
        public void UnhandledExceptionRateLimitingExceptions_WindowCountExceeded_ExceptionsWindowTimeSecExceeded()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();

            UnhandledException ue = new UnhandledException("Test0", Environment.StackTrace);
            TimeUtil.SetMockTime(0);
            rateLimiter.IsAllowed(ue);


            UnhandledException ue1 = new UnhandledException("Test1", Environment.StackTrace);
            TimeUtil.SetMockTime(1);
            rateLimiter.IsAllowed(ue1);

            UnhandledException ue2 = new UnhandledException("Test2", Environment.StackTrace);
            TimeUtil.SetMockTime(2);
            rateLimiter.IsAllowed(ue2);

            UnhandledException ue3 = new UnhandledException("Test3", Environment.StackTrace);
            TimeUtil.SetMockTime(rateLimiter.exceptionsWindowTimeSec - 1);
            Assert.IsFalse(rateLimiter.IsAllowed(ue3));

            //clean up
            TimeUtil.Clean();
        }

        [Test]
        public void UnhandledExceptionRateLimiting_CoolOffPeriod()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            string message = "Test";
            string stackTrace = Environment.StackTrace;

            UnhandledException ue = new UnhandledException(message, stackTrace);
            TimeUtil.SetMockTime(0);
            rateLimiter.IsAllowed(ue);

            UnhandledException ue2 = new UnhandledException(message, stackTrace);
            TimeUtil.SetMockTime(rateLimiter.uniqueExceptionTimePeriodSec - 1);
            Assert.IsFalse(rateLimiter.IsAllowed(ue2));
        }

        [Test]
        public void UnhandledExceptionRateLimiting_TrimUniqueException()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            int trimTime = 7;

            // This exception should be removed
            UnhandledException ue = new UnhandledException("Test0", Environment.StackTrace);
            TimeUtil.SetMockTime(1);
            rateLimiter.IsAllowed(ue);

            UnhandledException ue1 = new UnhandledException("Test1", Environment.StackTrace);
            TimeUtil.SetMockTime(6);
            rateLimiter.IsAllowed(ue1);

            // exception that triggers trim
            UnhandledException ue2 = new UnhandledException("Test2", Environment.StackTrace);
            TimeUtil.SetMockTime(trimTime);
            rateLimiter.IsAllowed(ue2);

            Assert.AreEqual(2, rateLimiter.GetExceptionsCount());
            Assert.AreEqual(trimTime, rateLimiter.uniqueExceptionLastTrimTimeSec);
        }

        [Test]
        [TestMustExpectAllLogs]
        public void LogUnhandledException_RejectsNullExceptionInstance()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            string stack = Environment.StackTrace;

            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null exception is not allowed through the Embrace SDK.");

            embrace.LogUnhandledUnityException(null, stack);

            embrace.provider.DidNotReceiveWithAnyArgs().LogUnhandledUnityException("", "", "");
        }

        [Test]
        [TestMustExpectAllLogs]
        public void LogUnhandledException_RejectsNullExceptionName()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string message = "exception message";
            string stack = Environment.StackTrace;

            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null exception name is not allowed through the Embrace SDK.");

            embrace.LogUnhandledUnityException(null, message, stack);

            embrace.provider.DidNotReceiveWithAnyArgs().LogUnhandledUnityException("", "", "");
        }

        [Test]
        public void LogUnhandledException_ReplacesNullMessage_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionName = "Exception";
            string stack = Environment.StackTrace;

            embrace.LogUnhandledUnityException(exceptionName, null, stack);

            embrace.provider.Received(1).LogUnhandledUnityException(exceptionName, "", stack);
        }

        [Test]
        public void LogUnhandledException_ReplacesNullMessageProperty_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            Exception exception = Substitute.For<Exception>();
            exception.Message.Returns((_) => null);

            string stack = Environment.StackTrace;

            embrace.LogUnhandledUnityException(exception, stack);

            embrace.provider.Received(1).LogUnhandledUnityException(exception.GetType().Name, "", stack);
        }

        [Test]
        public void LogUnhandledException_ReplacesNullStackTrace_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionName = "Exception";
            const string exceptionMessage = "test exception message";
            Exception exception = new Exception(exceptionMessage);

            embrace.LogUnhandledUnityException(exceptionName, exceptionMessage, null);
            embrace.LogUnhandledUnityException(exception, null);

            embrace.provider.Received(2).LogUnhandledUnityException(exceptionName, exceptionMessage, "");
        }

        [Test]
        public void LogUnhandledException_ReplacesNullStackTraceParameter_WithStackTraceProperty()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionMessage = "test exception message";
            string stack = Environment.StackTrace;

            Exception exception = Substitute.For<Exception>();
            exception.Message.Returns((_) => exceptionMessage);
            exception.StackTrace.Returns((_) => stack);

            embrace.LogUnhandledUnityException(exception);

            embrace.provider.Received(1).LogUnhandledUnityException(exception.GetType().Name, exceptionMessage, stack);
        }

        [Test]
        [TestMustExpectAllLogs]
        public void LogHandledException_RejectsNullExceptionInstance()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            string stack = Environment.StackTrace;

            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null exception is not allowed through the Embrace SDK.");

            embrace.LogHandledUnityException(null, stack);

            embrace.provider.DidNotReceiveWithAnyArgs().LogHandledUnityException("", "", "");
        }

        [Test]
        [TestMustExpectAllLogs]
        public void LogHandledException_RejectsNullExceptionName()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string message = "exception message";
            string stack = Environment.StackTrace;

            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null exception name is not allowed through the Embrace SDK.");

            embrace.LogHandledUnityException(null, message, stack);

            embrace.provider.DidNotReceiveWithAnyArgs().LogHandledUnityException("", "", "");
        }

        [Test]
        public void LogHandledException_ReplacesNullMessage_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionName = "Exception";
            string stack = Environment.StackTrace;

            embrace.LogHandledUnityException(exceptionName, null, stack);

            embrace.provider.Received(1).LogHandledUnityException(exceptionName, "", stack);
        }

        [Test]
        public void LogHandledException_ReplacesNullMessageProperty_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            Exception exception = Substitute.For<Exception>();
            exception.Message.Returns((_) => null);

            string stack = Environment.StackTrace;

            embrace.LogHandledUnityException(exception, stack);

            embrace.provider.Received(1).LogHandledUnityException(exception.GetType().Name, "", stack);
        }

        [Test]
        public void LogHandledException_ReplacesNullStackTrace_WithEmptyString()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionName = "Exception";
            const string exceptionMessage = "test exception message";
            Exception exception = new Exception(exceptionMessage);

            embrace.LogHandledUnityException(exceptionName, exceptionMessage, null);
            embrace.LogHandledUnityException(exception, null);

            embrace.provider.Received(2).LogHandledUnityException(exceptionName, exceptionMessage, "");
        }

        [Test]
        public void LogHandledException_ReplacesNullStackTraceParameter_WithStackTraceProperty()
        {
            Embrace embrace = Embrace.Create();
            embrace.provider = Substitute.For<IEmbraceProvider>();

            const string exceptionMessage = "test exception message";
            string stack = Environment.StackTrace;

            Exception exception = Substitute.For<Exception>();
            exception.Message.Returns((_) => exceptionMessage);
            exception.StackTrace.Returns((_) => stack);

            embrace.LogHandledUnityException(exception);

            embrace.provider.Received(1).LogHandledUnityException(exception.GetType().Name, exceptionMessage, stack);
        }
    }
}
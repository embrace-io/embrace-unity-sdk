using NSubstitute;
using NSubstitute.Core;
using UnityEngine;
using NUnit.Framework;

namespace EmbraceSDK.Tests
{
    public class EmbraceLoggerTests
    {
        private const string TEST_LOG_MESSAGE = "testing log stripping";
        private const string TEST_LOG_MESSAGE_WITH_TAG = EmbraceLogger.LOG_TAG + " : " + TEST_LOG_MESSAGE;

        [SetUp]
        public void SetUp()
        {
            EmbraceLogger.WrappedLogger = Substitute.For<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            EmbraceLogger.WrappedLogger = null;
        }

        [Test]
        public void WrappedLoggerCanBeSubstituted()
        {
            Assert.IsTrue(EmbraceLogger.WrappedLogger is ICallRouterProvider);
        }

        [Test]
        public void WrappedLoggerCanNotBeSetToNull()
        {
            EmbraceLogger.WrappedLogger = null;
            Assert.IsNotNull(EmbraceLogger.WrappedLogger);
        }

        [Test]
        public void LogMessageIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogMessageWithTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogMessageWithTypeIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Log, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogWarningWithTypeIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Warning, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogErrorWithTypeIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Error, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogMessageWithTypeAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();

            EmbraceLogger.Log(LogType.Log, TEST_LOG_MESSAGE, context: context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }
        
        [Test]
        public void LogWarningWithTypeAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();

            EmbraceLogger.Log(LogType.Warning, TEST_LOG_MESSAGE, context: context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }
        
        [Test]
        public void LogErrorWithTypeAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();

            EmbraceLogger.Log(LogType.Error, TEST_LOG_MESSAGE, context: context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }

        [Test]
        public void LogMessageWithTypeAndTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogWarningWithTypeAndTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogErrorWithTypeAndTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogMessageWithTypeAndTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Log, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }
        
        [Test]
        public void LogWarningWithTypeAndTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Warning, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }
        
        [Test]
        public void LogErrorWithTypeAndTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(LogType.Error, EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }

        [Test]
        public void LogMessageWithTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }

        [Test]
        public void LogWarningIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogWarning(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogWarningIsOmittedWhenSpecificallySilenced()
        {
            EmbraceLogger.LogWarning(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogsOmittedWhenSpecificallySilenced()
        {
            EmbraceLogger.Log(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().Log(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogErrorIsOmittedWhenSpecificallySilenced()
        {
            EmbraceLogger.LogError(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogWarningWithTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogWarningWithTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogWarning(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }

        [Test]
        public void LogErrorIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogError(TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogErrorWithTagIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogErrorWithTagAndContextIsOmittedWhenSilenced()
        {
            Object context = Substitute.For<Object>();
            EmbraceLogger.LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogError(EmbraceLogger.LOG_TAG, TEST_LOG_MESSAGE, context);
            }
        }

        [Test]
        public void LogFormatMessageIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogFormat(LogType.Log, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.LogsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogFormat(LogType.Log, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogFormat(LogType.Log, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogFormatWarningIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogFormat(LogType.Warning, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.WarningsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogFormat(LogType.Warning, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogFormat(LogType.Warning, TEST_LOG_MESSAGE);
            }
        }
        
        [Test]
        public void LogFormatErrorIsOmittedWhenSilenced()
        {
            EmbraceLogger.LogFormat(LogType.Error, TEST_LOG_MESSAGE);

            if (EmbraceLogger.IsSilenced || EmbraceLogger.ErrorsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogFormat(LogType.Error, TEST_LOG_MESSAGE);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogFormat(LogType.Error, TEST_LOG_MESSAGE);
            }
        }

        [Test]
        public void LogExceptionIsOmittedWhenSilenced()
        {
            System.Exception exception = new System.Exception();
            EmbraceLogger.LogException(exception);

            if (EmbraceLogger.IsSilenced)
            {
                EmbraceLogger.WrappedLogger.DidNotReceiveWithAnyArgs().LogException(exception);
            }
            else
            {
                EmbraceLogger.WrappedLogger.Received().LogException(exception);
            }
        }
    }
}
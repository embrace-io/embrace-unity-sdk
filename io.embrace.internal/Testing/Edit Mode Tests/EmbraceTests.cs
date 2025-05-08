using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using EmbraceSDK;
using EmbraceSDK.Internal;
using EmbraceSDK.Utilities;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class EmbraceTests
    {
        /// <summary>
        /// Test if Embrace.Create() is actually creating an object of type Embrace.
        /// </summary>
        [Test]
        public void EmbraceCreate()
        {
            Embrace embrace = new Embrace();
            Assert.IsInstanceOf<Embrace>(embrace);
        }

        [Test]
        public void EmbraceIsStarted()
        {
            Embrace embrace = new Embrace();
            Assert.IsFalse(embrace.IsStarted);
            embrace.StartSDK(null, false);
            Assert.IsTrue(embrace.IsStarted);
        }

        // Test if calls to SDK are passed down to provider.
        // Todo: We need to find a way to run these tests on Android and iOS.
        #region ProviderTests

        [Test]
        public void StartSDKProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
            embrace.StartSDK(null, false);
            embrace.provider.Received().StartSDK();
            embrace.provider.Received().SetMetaData(Application.unityVersion, Application.buildGUID, sdkInfo.version);
        }

        [Test]
        public void StartSDKiOSTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);

            EmbraceStartupArgs startupArgs = new EmbraceStartupArgs("AppId",
                EmbraceConfig.Default,
                "GroupId",
                "baseUrl",
                "devBaseUrl",
                "configBaseUrl");
            
            embrace.StartSDK(startupArgs, false);
            embrace.provider.Received().StartSDK(startupArgs);
        }

        [Test]
        public void StartSDKWithIntegrationTestingProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
            EmbraceStartupArgs args = new EmbraceStartupArgs();
            embrace.StartSDK(args, false);
            embrace.provider.Received().StartSDK(args);
            embrace.provider.Received().SetMetaData(Application.unityVersion, Application.buildGUID, sdkInfo.version);
        }

        [Test]
        public void StartSDKMultipleTimesTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.StartSDK(null, false);
            embrace.StartSDK(null, false);
            embrace.provider.Received(1).StartSDK();
        }

        [Test]
        public void SetUserIdentifierProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            string identifier = "TestIdentifier";
            embrace.SetUserIdentifier(identifier);
            embrace.provider.Received().SetUserIdentifier(identifier);
        }

        [Test]
        public void ClearUserIdentifierProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.ClearUserIdentifier();
            embrace.provider.Received().ClearUserIdentifier();
        }

        [Test]
        public void SetUsernameProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.SetUsername("test name");
            embrace.provider.Received().SetUsername("test name");
        }

        [Test]
        public void ClearUsernameProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.ClearUsername();
            embrace.provider.Received().ClearUsername();
        }

        [Test]
        public void SetUserEmailProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string email = "test@email.com";
            embrace.SetUserEmail(email);
            embrace.provider.Received().SetUserEmail(email);
        }

        [Test]
        public void ClearUserEmailProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.ClearUserEmail();
            embrace.provider.Received().ClearUserEmail();
        }

        [Test]
        public void SetUserAsPayerProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.SetUserAsPayer();
            embrace.provider.Received().SetUserAsPayer();
        }

        [Test]
        public void ClearUserAsPayerProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.ClearUserAsPayer();
            embrace.provider.Received().ClearUserAsPayer();
        }

        [Test]
        public void AddUserPersonaProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string persona = "Test Persona";
            embrace.AddUserPersona(persona);
            embrace.provider.Received().AddUserPersona(persona);
        }

        [Test]
        public void ClearUserPersonaProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string persona = "Test Persona";
            embrace.ClearUserPersona(persona);
            embrace.provider.Received().ClearUserPersona(persona);
        }

        [Test]
        public void ClearAllUserPersonasProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.ClearAllUserPersonas();
            embrace.provider.Received().ClearAllUserPersonas();
        }

        [Test]
        public void AddSessionPropertyProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string key = "testKey";
            string value = "testValue";
            bool permanent = true;
            embrace.AddSessionProperty(key, value, permanent);
            embrace.provider.Received().AddSessionProperty(key, value, permanent);
        }

        [Test]
        public void RemoveSessionPropertyProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string key = "testKey";
            embrace.RemoveSessionProperty(key);
            embrace.provider.Received().RemoveSessionProperty(key);
        }

        [Test]
        public void GetSessionPropertiesProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.GetSessionProperties();
            embrace.provider.Received().GetSessionProperties();
        }
        
        [Test]
        public void LogMessage_WithArgs_ProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            EMBSeverity severity = EMBSeverity.Error;
            Dictionary<string, string> properties = new Dictionary<string, string>();
            embrace.LogMessage(message, severity, properties);
            embrace.provider.Received().LogMessage(message, severity, Arg.Is<Dictionary<string, string>>(d => d.Count == 0));
        }

        [Test]
        public void LogMessageProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            EMBSeverity severity = EMBSeverity.Error;
            embrace.LogMessage(message, severity);
            embrace.provider.Received().LogMessage(message, severity, Arg.Is<Dictionary<string, string>>(d => d.Count == 0));
        }
        
        [Test]
        public void LogMessage_Returns_WhenMessageIsNull_Test()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            EMBSeverity severity = EMBSeverity.Error;
            Embrace.Instance.LogMessage(null, severity);
            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null log message is not allowed through the Embrace SDK.");
        }

        [Test]
        public void LogMessage_With_Attachment()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            EMBSeverity severity = EMBSeverity.Info;
#if UNITY_IOS || UNITY_TVOS
            byte[] attachment = new byte[1024 * 1024]; // > 1 MiB
            Embrace.Instance.LogMessage(message, severity, null, attachment);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Info, Arg.Any<Dictionary<string, string>>(), attachment);
#elif UNITY_ANDROID
            sbyte[] attachment = new sbyte[1024 * 1024]; // > 1 MiB
            embrace.LogMessage(message, severity, null, attachment);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Info, Arg.Any<Dictionary<string, string>>(), attachment);
#endif
        }

        [Test]
        public void LogMessage_With_Attachment_Rejects_Large_Size()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            EMBSeverity severity = EMBSeverity.Info;
#if UNITY_IOS || UNITY_TVOS
            byte[] attachment = new byte[1024 * 1025]; // > 1 MiB
            Embrace.Instance.LogMessage(message, severity, null, attachment);
            embrace.provider.Received().AddBreadcrumb($"Embrace Attachment failure. Attachment size too large. Message: {message}");
#elif UNITY_ANDROID
            sbyte[] attachment = new sbyte[1024 * 1025]; // > 1 MiB
            embrace.LogMessage(message, severity, null, attachment);
            embrace.provider.Received().AddBreadcrumb($"Embrace Attachment failure. Attachment size too large. Message: {message}");
#endif
        }

        [Test]
        public void LogMessageWithAttachmentUrl()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            EMBSeverity severity = EMBSeverity.Info;
            string attachmentId = new Guid().ToString();
            string attachmentUrl = "http://www.example.com";
            embrace.LogMessage(message, severity, null, attachmentId, attachmentUrl);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Info, Arg.Any<Dictionary<string, string>>(), attachmentId, attachmentUrl);
        }

        [Test]
        public void LogInfoProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            embrace.LogInfo(message);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Info, Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void LogWarningProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            embrace.LogWarning(message);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Warning, Arg.Any<Dictionary<string, string>>());

        }

        [Test]
        public void LogErrorProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            embrace.LogError(message);
            embrace.provider.Received().LogMessage(message, EMBSeverity.Error, Arg.Any<Dictionary<string, string>>());

        }

        [Test]
        public void AddBreadcrumbProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            embrace.AddBreadcrumb(message);
            embrace.provider.Received().AddBreadcrumb(message);
        }

        [Test]
        public void EndSessionProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.EndSession();
            embrace.provider.Received().EndSession(false);
        }

        [Test]
        public void GetDeviceIdProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.GetDeviceId();
            embrace.provider.Received().GetDeviceId();
        }
        
        [Test]
        public void GetCurrentSessionIdProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.GetCurrentSessionId();
            embrace.provider.Received().GetCurrentSessionId();
        }
        
        [Test]
        public void GetCurrentSessionId_IsNotNull_IfSDKStarted_Test()
        {
            Embrace embrace = new Embrace();
            embrace.StartSDK(null, false);
            var sessionId = Embrace.Instance.GetCurrentSessionId();
            Assert.NotNull(sessionId);
        }
        
        [Test]
        public void GetCurrentSessionId_IsNotNull_IfSDKNotStarted_Test()
        {
            Embrace embrace = new Embrace();
            var sessionId = Embrace.Instance.GetCurrentSessionId();
            Assert.NotNull(sessionId);
            Assert.IsEmpty(sessionId);
        }

        [Test]
        public void StartViewProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string message = "test message";
            embrace.StartView(message);
            embrace.provider.Received().StartView(message);
        }

        [Test]
        public void EndViewProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string name = "test name";
            embrace.EndView(name);
            embrace.provider.Received().EndView(name);
        }
        
        [Test]
        public void RecordCompletedNetworkRequestTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string url = "url";
            HTTPMethod method = HTTPMethod.GET;
            long startms = 1;
            long endms = 3;
            int bytesSent = 100;
            int bytesReceived = 150;
            int code = 200;
            embrace.RecordCompleteNetworkRequest(url, method, startms, endms, bytesReceived, bytesSent, code);
            embrace.provider.Received().RecordCompletedNetworkRequest(url, method, startms, endms, bytesReceived, bytesSent, code);
        }
        
        [Test]
        public void RecordCompletedNetworkRequest_Return_WhenUrlIsNull_Test()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            HTTPMethod method = HTTPMethod.GET;
            long startms = 1;
            long endms = 3;
            int bytesSent = 100;
            int bytesReceived = 150;
            int code = 200;
            Embrace.Instance.RecordCompleteNetworkRequest(null, method, startms, endms, bytesReceived, bytesSent, code);
            LogAssert.Expect(LogType.Error, $"[Embrace Unity SDK] : null network url is not allowed through the Embrace SDK.");
        }

        [Test]
        public void RecordIncompleteNetworkRequestTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string url = "url";
            HTTPMethod method = HTTPMethod.GET;
            long startms = 1;
            long endms = 2;
            string error = "error";
            embrace.RecordIncompleteNetworkRequest(url, method, startms, endms, error);
            embrace.provider.Received().RecordIncompleteNetworkRequest(url, method, startms, endms, error);
        }
        
                
        [Test]
        public void RecordIncompleteNetworkRequestTest_Return_WhenUrlIsNull_Test()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            HTTPMethod method = HTTPMethod.GET;
            long startms = 1;
            long endms = 2;
            string error = "error";
            Embrace.Instance.RecordIncompleteNetworkRequest(null, method, startms, endms, error);
            LogAssert.Expect(LogType.Error, $"[Embrace Unity SDK] : null network url is not allowed through the Embrace SDK.");
        }
        
        [Test]
        public void RecordIncompleteNetworkRequestTest_Return_WhenErrorIsNull_Test()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            string url = "url";
            HTTPMethod method = HTTPMethod.GET;
            long startms = 1;
            long endms = 2;
            Embrace.Instance.RecordIncompleteNetworkRequest(url, method, startms, endms, null);
            LogAssert.Expect(LogType.Error, $"[Embrace Unity SDK] : null network error is not allowed through the Embrace SDK.");
        }

        public void RecordPushNotificationProviderTest()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
#if UNITY_IOS
            var iosArgs = new iOSPushNotificationArgs("title", "body", 
                "subtitle", "category", 0);
            Embrace.Instance.RecordPushNotification(iosArgs);
            embrace.provider.Received().RecordPushNotification(iosArgs);
#elif UNITY_ANDROID
            var androidArgs = new AndroidPushNotificationArgs("title", "body", "topic", 
                "id", 0, 0, false, false);
            Embrace.Instance.RecordPushNotification(androidArgs);
            embrace.provider.Received().RecordPushNotification(androidArgs);
#endif
        }

        [Test]
        public void LogUnhandledUnityExceptionProviderTest_WithStringParams()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            const string exceptionName = "Exception";
            const string exceptionMessage = "exception Message";
            string stack = Environment.StackTrace;
            embrace.LogUnhandledUnityException(exceptionName, exceptionMessage, stack);
            embrace.provider.Received(1).LogUnhandledUnityException(exceptionName, exceptionMessage, stack);
        }

        [Test]
        public void LogUnhandledUnityExceptionProviderTest_WithExceptionInstance()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            const string exceptionMessage = "exception Message";
            Exception exception = new Exception(exceptionMessage);
            string stack = Environment.StackTrace;
            embrace.LogUnhandledUnityException(exception, stack);
            embrace.provider.Received(1).LogUnhandledUnityException("Exception", exceptionMessage, stack);
        }

        [Test]
        public void LogHandledUnityExceptionProviderTest_WithStringParams()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            const string exceptionName = "Exception";
            const string exceptionMessage = "exception Message";
            string stack = Environment.StackTrace;
            embrace.LogHandledUnityException(exceptionName, exceptionMessage, stack);
            embrace.provider.Received(1).LogHandledUnityException(exceptionName, exceptionMessage, stack);
        }

        [Test]
        public void LogHandledUnityExceptionProviderTest_WithExceptionInstance()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            const string exceptionMessage = "exception Message";
            Exception exception = new Exception(exceptionMessage);
            string stack = Environment.StackTrace;
            embrace.LogHandledUnityException(exception, stack);
            embrace.provider.Received(1).LogHandledUnityException("Exception", exceptionMessage, stack);
        }
        
        #endregion

        /// <summary>
        /// Test NoNullsError
        /// </summary>
        [Test]
        [TestMustExpectAllLogs]
        public void NoNullsError()
        {
            LogAssert.Expect(LogType.Error, "[Embrace Unity SDK] : null username is not allowed through the Embrace SDK.");
            Embrace embrace = new Embrace();
            embrace.SetUsername(null);
        }

        [Test]
        public void UnhandledExceptionRateLimiting_NotAllowed()
        {
            UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
            UnhandledException ue = new UnhandledException("Test", "stackTrace test");
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
            UnhandledException ue = new UnhandledException("Test", "stackTrace test");
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
            UnhandledException ue = new UnhandledException("Test0", "stackTrace test0");
            TimeUtil.SetMockTime(0);
            rateLimiter.IsAllowed(ue);
            UnhandledException ue1 = new UnhandledException("Test1", "stackTrace test1");
            TimeUtil.SetMockTime(1);
            rateLimiter.IsAllowed(ue1);
            UnhandledException ue2 = new UnhandledException("Test2", "stackTrace test2");
            TimeUtil.SetMockTime(2);
            rateLimiter.IsAllowed(ue2);
            UnhandledException ue3 = new UnhandledException("Test3", "stackTrace test3");
            TimeUtil.SetMockTime(3);
            Assert.IsFalse(rateLimiter.IsAllowed(ue3));
            //clean up
            TimeUtil.Clean();
        }

        [Test]
        public void EmbraceLogHandlerTest()
        {
            const string exceptionMessage = "This is an exception";
            LogAssert.Expect(LogType.Exception, $"Exception: {exceptionMessage}");
            
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.StartSDK(null, false);
            Exception exception = new Exception(exceptionMessage);
            Debug.LogException(exception);
            embrace.provider.ReceivedWithAnyArgs().LogUnhandledUnityException("Exception", exceptionMessage, Arg.Any<string>());
        }

        [Test]
        public void BridgedHTTPMethodTest()
        {
            List<int> values = new List<int>();
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.OTHER));
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.GET));
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.POST));
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.PUT));
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.DELETE));
            values.Add(Embrace.__BridgedHTTPMethod(HTTPMethod.PATCH));
            Assert.AreEqual(new List<int> {0,1,2,3,4,5},values);
        }

        [Test]
        public void GetLastRunEndState_CallsProvider_AfterSdkStarted()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            embrace.StartSDK(null, false);
            var state = embrace.GetLastRunEndState();
            embrace.provider.Received(1).GetLastRunEndState();
        }

        [Test]
        public void GetLastRunEndState_DoesNotCallProvider_BeforeSdkStarted()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            var state = embrace.GetLastRunEndState();
            embrace.provider.DidNotReceive().GetLastRunEndState();
            Assert.AreEqual(LastRunEndState.Invalid, state);
        }
    }
}
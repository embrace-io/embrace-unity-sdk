using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System;
using EmbraceSDK.Internal;

namespace EmbraceSDK.Tests
{

    public class PlayStubTests
    {
        [SetUp]
        public void Setup()
        {
            Embrace.Stop();
        }
        
        [UnityTest]
        public IEnumerator InitializeSDK()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: InitializeSDK");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.provider.InitializeSDK();
        }

        [UnityTest]
        public IEnumerator SetUserIdentifier()
        {
            string identifier = "test";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: SetUserIdentifier {identifier}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.SetUserIdentifier(identifier);
        }

        [UnityTest]
        public IEnumerator ClearUserIdentifier()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearUserIdentifier");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearUserIdentifier();
        }

        [UnityTest]
        public IEnumerator SetUsername()
        {
            string username = "username";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: SetUsername {username}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.SetUsername(username);
        }

        [UnityTest]
        public IEnumerator ClearUsername()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearUsername");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearUsername();
        }

        [UnityTest]
        public IEnumerator SetUserEmail()
        {
            string email = "email@test.com";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: SetUserEmail {email}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.SetUserEmail(email);
        }

        [UnityTest]
        public IEnumerator ClearUserEmail()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearUserEmail");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearUserEmail();
        }

        [UnityTest]
        public IEnumerator SetUserAsPayer()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: SetUserAsPayer");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.SetUserAsPayer();
        }

        [UnityTest]
        public IEnumerator ClearUserAsPayer()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearUserAsPayer");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearUserAsPayer();
        }

        [UnityTest]
        public IEnumerator AddUserPersona()
        {
            string persona = "test persona";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: AddUserPersona {persona}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.AddUserPersona(persona);
        }

        [UnityTest]
        public IEnumerator ClearUserPersona()
        {
            string persona = "test persona";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearUserPersona {persona}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearUserPersona(persona);
        }

        [UnityTest]
        public IEnumerator ClearAllUserPersonas()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: ClearAllUserPersonas");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.ClearAllUserPersonas();
        }

        [UnityTest]
        public IEnumerator AddSessionProperty()
        {
            string key = "Test Key";
            string value = "Test Value";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: AddSessionProperty key: {key} value: {value}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.AddSessionProperty(key, value, false);
        }

        [UnityTest]
        public IEnumerator RemoveSessionProperty()
        {
            string key = "test key";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: RemoveSessionProperty key: {key}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.RemoveSessionProperty(key);
        }

        [UnityTest]
        public IEnumerator GetSessionProperties()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: GetSessionProperties");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Dictionary<string, string> dictionary;
            dictionary = Embrace.Instance.GetSessionProperties();

            Assert.IsNotNull(dictionary);
        }

        [UnityTest]
        public IEnumerator LogMessage_Info()
        {
            string severityString = "info";
            string message = "Test Message";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: LogMessage severity: {severityString} message: {message}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.LogMessage(message, EMBSeverity.Info);
        }

        [UnityTest]
        public IEnumerator LogMessage_Warning()
        {
            string severityString = "warning";
            string message = "Test Message";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: LogMessage severity: {severityString} message: {message}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.LogMessage(message, EMBSeverity.Warning);
        }

        [UnityTest]
        public IEnumerator LogMessage_Error()
        {
            string severityString = "error";
            string message = "Test Message";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: LogMessage severity: {severityString} message: {message}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.LogMessage(message, EMBSeverity.Error);
        }

        [UnityTest]
        public IEnumerator AddBreadcrumb()
        {
            string message = "Test Message";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: AddBreadcrumb {message}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.AddBreadcrumb(message);
        }

        [UnityTest]
        public IEnumerator EndSession()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: EndSession");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.EndSession();
        }

        [UnityTest]
        public IEnumerator GetDeviceId()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: GetDeviceId");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.GetDeviceId();
        }
        
        [UnityTest]
        public IEnumerator GetCurrentSessionId()
        {
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: GetCurrentSessionId");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.GetCurrentSessionId();
        }

        [UnityTest]
        public IEnumerator StartView()
        {
            string name = "Test Name";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: StartView {name}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.StartView(name);
        }

        [UnityTest]
        public IEnumerator EndView()
        {
            string name = "Test Name";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: EndView {name}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.EndView(name);
        }

        [UnityTest]
        public IEnumerator SetMetaData()
        {
            string unityVersion = "1.2.1";
            string guid = System.Guid.NewGuid().ToString();
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);

            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: Unity Version = {unityVersion} GUID = {guid} Unity-SDK Version= {sdkInfo.version}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.provider.SetMetaData(unityVersion, guid, sdkInfo.version);
        }
        
        [UnityTest]
        public IEnumerator RecordCompleteNetworkRequest()
        {
            string url = "https://www.test.com";
            HTTPMethod method = HTTPMethod.GET;
            long startms = 0;
            long endms = 0;
            int bytesin = 0;
            int bytesout = 0;
            int code = 0;
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: Network Request: {url} method: {method} start: {startms} end: {endms} bytesin: {bytesin} bytesout: {bytesout}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.RecordCompleteNetworkRequest(url, method, startms, endms, bytesin, bytesout, code);
        }
        
        [UnityTest]
        public IEnumerator RecordIncompleteNetworkRequest()
        {
            string url = "https://www.test.com";
            HTTPMethod method = HTTPMethod.GET;
            long startms = 0;
            long endms = 0;
            string error = "Test Error";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: Network Request: {url} method: {method} start: {startms} end: {endms} error: {error}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.RecordIncompleteNetworkRequest(url, method, startms, endms, error);
        }

        [UnityTest]
        public IEnumerator RecordPushNotification()
        {
#if UNITY_IOS
            var iosArgs = new iOSPushNotificationArgs("title", "body", "subtitle", "category", 0);
            var expected = $"Push Notification: title: {iosArgs.title} subtitle: {iosArgs.subtitle} body: {iosArgs.body} category: {iosArgs.category} badge: {iosArgs.badge}";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: {expected}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.RecordPushNotification(iosArgs);
#elif UNITY_ANDROID
            var androidArgs = new AndroidPushNotificationArgs("title", "body","topic", 
                "id", 0, 0, false, false);
            var expected = $"Push Notification: title: {androidArgs.title} body: {androidArgs.body} topic: {androidArgs.topic} id: {androidArgs.id} notificationPriority: {androidArgs.notificationPriority} messageDeliveredPriority: {androidArgs.messageDeliveredPriority} isNotification: {androidArgs.isNotification} hasData: {androidArgs.hasData}";
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: {expected}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.RecordPushNotification(androidArgs);
#else
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator LogUnhandledUnityException()
        {
            string exceptionMessage = "Test Exception Message";
            string stack = Environment.StackTrace;
            LogAssert.Expect(LogType.Log, $"{EmbraceLogger.LOG_TAG}: Unhandled Exception: Exception : {exceptionMessage} : stack : {stack}");
            Embrace.Start();
            yield return new WaitForFixedUpdate();
            Embrace.Instance.provider.LogUnhandledUnityException("Exception", exceptionMessage, stack);
        }
    }
}
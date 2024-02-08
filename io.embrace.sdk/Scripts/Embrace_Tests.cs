using System.Collections.Generic;

namespace EmbraceSDK
{
    [UnityEngine.TestTools.ExcludeFromCoverage]
    public class Embrace_Tests
    {
        public void RunTests()
        {
            Embrace.Instance.StartSDK();
            Embrace.Instance.SetUserIdentifier("embrace_test_user");
            Embrace.Instance.SetUserIdentifier(null);
            Embrace.Instance.ClearUserIdentifier();
            Embrace.Instance.SetUserEmail("embrace_test_email");
            Embrace.Instance.SetUserEmail(null);
            Embrace.Instance.ClearUserEmail();
            Embrace.Instance.SetUserAsPayer();
            Embrace.Instance.ClearUserAsPayer();

#pragma warning disable 0618
            Embrace.Instance.SetUserPersona("embrace_test_persona");
#pragma warning restore 0618

            Embrace.Instance.AddUserPersona("embrace_test_persona");
            EmbraceLogger.Log("running set b");

#pragma warning disable 0618
            Embrace.Instance.SetUserPersona(null);
#pragma warning restore 0618

            Embrace.Instance.AddUserPersona(null);
            Embrace.Instance.ClearUserPersona("embrace_test_persona");
            Embrace.Instance.ClearUserPersona(null);
            Embrace.Instance.ClearAllUserPersonas();
            Embrace.Instance.AddSessionProperty("test_key", "test_value", true);
            Embrace.Instance.AddSessionProperty("test_key", "test_value", false);
            Embrace.Instance.AddSessionProperty("test_key", null, false);
            Embrace.Instance.AddSessionProperty(null, "test_value", false);
            Embrace.Instance.AddSessionProperty(null, null, false);
            Embrace.Instance.RemoveSessionProperty("test_key");
            Embrace.Instance.RemoveSessionProperty("test_key_doesnt_exist");
            Embrace.Instance.RemoveSessionProperty(null);
            EmbraceLogger.Log("running set c");
            Embrace.Instance.AddSessionProperty("test_key", "test_value", true);
            Dictionary<string, string> sessionProperties = Embrace.Instance.GetSessionProperties();
            foreach (var item in sessionProperties.Keys)
            {
                string value = sessionProperties[item];
                EmbraceLogger.Log("session properties: " + item + " = " + value);
            }
            Embrace.Instance.SetUsername("embrace_test_user");
            Embrace.Instance.SetUsername(null);
            Embrace.Instance.ClearUsername();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("test_key", "test_value");
            Embrace.Instance.StartMoment("test_name", "test_id", true, properties);
            Embrace.Instance.StartMoment("test_name", "test_id", false, properties);
            Embrace.Instance.StartMoment("test_name", "test_id", true, null);
            Embrace.Instance.StartMoment("test_name", null, true, properties);
            Embrace.Instance.StartMoment(null, "test_id", true, properties);
            Embrace.Instance.StartMoment(null, null, true, properties);
            Embrace.Instance.EndMoment("test_name", "test_id", properties);
            Embrace.Instance.EndMoment("test_name", "test_id", null);
            Embrace.Instance.EndMoment("test_name", null, properties);
            Embrace.Instance.EndMoment(null, "test_id", properties);
            Embrace.Instance.EndMoment(null, null, properties);
            Embrace.Instance.EndAppStartup(null);
            #pragma warning disable CS0618
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Info, properties, true);
            #pragma warning restore CS0618
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Info, properties);
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Info, null);
            Embrace.Instance.LogMessage(null, EMBSeverity.Info, properties);
            Embrace.Instance.LogMessage(null, EMBSeverity.Info, null);
            EmbraceLogger.Log("running set d");
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Warning, properties);
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Warning, null);
            Embrace.Instance.LogMessage(null, EMBSeverity.Warning, properties);
            Embrace.Instance.LogMessage(null, EMBSeverity.Warning, null);
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Error, properties);
            Embrace.Instance.LogMessage("test_message", EMBSeverity.Error, null);
            Embrace.Instance.LogMessage(null, EMBSeverity.Error, properties);
            Embrace.Instance.LogMessage(null, EMBSeverity.Error, null);

#pragma warning disable 0618
            Embrace.Instance.LogBreadcrumb("test_message");
            Embrace.Instance.LogBreadcrumb(null);
#pragma warning restore 0618

            Embrace.Instance.AddBreadcrumb("test_message");
            Embrace.Instance.AddBreadcrumb(null);
            Embrace.Instance.EndSession(true);
            Embrace.Instance.EndSession(false);
            string deviceId = Embrace.Instance.GetDeviceId();
            EmbraceLogger.Log("deviceid: " + deviceId);
            string currentSessionId = Embrace.Instance.GetCurrentSessionId();
            EmbraceLogger.Log("currentSessionId: " + currentSessionId);
            Embrace.Instance.StartView("test_view");
            Embrace.Instance.StartView(null);
            Embrace.Instance.EndView("test_view");
            Embrace.Instance.EndView(null);
            EmbraceLogger.Log("running set e");
        }
    }
}
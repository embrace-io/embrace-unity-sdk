using NUnit.Framework;
using AltTester.AltTesterUnitySDK.Driver;
using EmbraceSDK;
using EmbraceSDK.Automation;

public class EmbraceAltTests
{
    private AltDriver _altDriver;
    
    //Before any test it connects with the socket
    [OneTimeSetUp]
    public void SetUp()
    {
        _altDriver = new AltDriver();
        string appId = System.Environment.GetEnvironmentVariable("EMBRACE_APP_ID");
        EmbraceStartupArgs args = new EmbraceStartupArgs("");
        EmbraceSDK.Embrace.Instance.StartSDK(args);
        EmbraceSDK.Embrace.Instance.SetUsername(AutomationConstants.AUTOMATION_USERNAME);
        EmbraceSDK.Embrace.Instance.SetUserEmail(AutomationConstants.AUTOMATION_USERNAME);
        EmbraceSDK.Embrace.Instance.SetUserIdentifier(AutomationConstants.AUTOMATION_USERNAME);
        EmbraceSDK.Embrace.Instance.AddUserPersona(AutomationConstants.AUTOMATION_USERNAME);
        EmbraceSDK.Embrace.Instance.SetUserAsPayer();
    }

    //At the end of the test closes the connection with the socket
    [OneTimeTearDown]
    public void TearDown()
    {
        _altDriver.Stop();
    }

    [Test]
    public void AddBreadcrumbTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_AddBreadcrumb");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogInfoTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogInfo");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogInfoWithPropertiesTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogInfoWithProperties");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogMessageWithAttachmentUrlTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogMessageWithAttachmentUrl");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogMessageWithAttachmentTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogMessageWithAttachment");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogWarningTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogWarning");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
    
    [Test]
    public void LogErrorTest()
    {
        var buttonObject = _altDriver.FindObject(By.NAME, "Button_LogError");
        Assert.IsNotNull(buttonObject);
        buttonObject.Click();
    }
}
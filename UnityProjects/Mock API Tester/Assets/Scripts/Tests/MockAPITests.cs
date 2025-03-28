using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Embrace.MockAPI;
using EmbraceSDK;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MockAPITests
{
    private MockAPIClient _mockAPIClient;
    
    [OneTimeSetUp]
    public void Setup()
    {
        _mockAPIClient = new MockAPIClient();
    }
    
    [Test]
    public async Task LogMessageTests()
    {
        var response = await _mockAPIClient.LogMessage("Test Log", EMBSeverity.Info);
        Assert.IsNotNull(response);
        Debug.Log(response);
    }
}

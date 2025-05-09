using System.Collections;
using System.Collections.Generic;
using EmbraceSDK;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Embrace.Internal.SmokeTests
{
    [ExcludeFromCoverage]
    public class MiscSmokeTests
    {
        // Start the SDK twice to confirm that it doesn't crash
        [Preserve, SmokeTest]
        public IEnumerator StartSDKTwice()
        {
            EmbraceSDK.Embrace.Start();
            yield return null;
            EmbraceSDK.Embrace.Start();
        }

        // Open and close the keyboard multiple times to confirm that the native SDK is not logging internal errors
        // every time it closes.
        [Preserve, SmokeTest]
        public IEnumerator OpenAndCloseKeyboard()
        {
            const int iterations = 5;

            GameObject canvasPrefab = Resources.Load<GameObject>("SmokeTest_InputField");
            GameObject canvasInstance = GameObject.Instantiate(canvasPrefab);
            InputField inputField = canvasInstance.GetComponentInChildren<InputField>();

            EmbraceSDK.Embrace.Start();
            yield return null;

            for (int i = 0; i < iterations; ++i)
            {
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);

                yield return new WaitForSeconds(0.1f);

                EventSystem.current.SetSelectedGameObject(null, new BaseEventData(EventSystem.current));

                yield return new WaitForSeconds(0.1f);
            }
        }

        // Log an error immediately after starting the SDK to confirm that it is safe to access the API during
        // a potentially async native SDK startup.
        [Preserve, SmokeTest]
        public void LogErrorImmediatelyAfterStart()
        {
            EmbraceSDK.Embrace.Start();
            EmbraceSDK.Embrace.Instance.LogMessage("Error message", EMBSeverity.Error);
        }

        // Log an error one second after starting the SDK. Written to try and surface/diagnose an issue with
        // the smoke test framework on CI. Payload Validation checks are expected to pass on CI, but were
        // failing at the time of writing. They were passing locally, however. This may no longer be an issue.
        [Preserve, SmokeTest]
        public IEnumerator LogErrorOneSecondAfterStart()
        {
            EmbraceSDK.Embrace.Start();
            yield return new WaitForSeconds(1f);
            EmbraceSDK.Embrace.Instance.LogMessage("Error message", EMBSeverity.Error);
        }
    }
}
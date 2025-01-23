using System.Collections;
using EmbraceSDK.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EmbraceSDK.Tests
{
    public class PlaySceneSelectorTests : PlayTestBase
    {
        /// <summary>
        /// Tests the scene selector and scene buttons for correct transitions
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestSceneTransitions()
        {
            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            var totalScenes = GameObject.FindObjectOfType<SceneSelector>().SceneCount;

            for (int i = 0; i < totalScenes; i++)
            {
                // Re-acquire scene button references since they are destroyed every time Demo Home is unloaded.
                SceneButton[] sceneButtons = GameObject.FindObjectsOfType<SceneButton>();

                SceneButton sceneButton = sceneButtons[i];
                sceneButton.GetComponent<Button>().onClick.Invoke();

                yield return new WaitForSeconds(.25f);

                Assert.AreEqual(sceneButton.SceneName, SceneManager.GetActiveScene().name);

                Button backButton = GameObject.Find(DemoConstants.BUTTON_NAME_BACK).GetComponent<Button>();
                backButton.onClick.Invoke();

                yield return new WaitForSeconds(.25f);

                Assert.AreEqual(SceneManager.GetActiveScene().name, DemoConstants.SCENE_NAME_DEMO_HOME);
            }

            Cleanup();
        }
    }
}
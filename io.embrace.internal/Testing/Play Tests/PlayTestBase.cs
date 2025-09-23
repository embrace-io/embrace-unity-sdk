using System.Collections;
using EmbraceSDK.Internal;
using NSubstitute;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EmbraceSDK.Tests
{
    public class PlayTestBase : IEmbraceTest
    {
        protected void ProviderSetup(IEmbraceProvider provider = null)
        {
            Embrace.Instance.provider = provider;

            if (provider == null)
            {
                var substitute = Substitute.For<IEmbraceProvider>();
                substitute.StartSpan(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>()).Returns("spanId");
            }
        }

        protected IEnumerator LoadScene(string sceneName, float waitSeconds = 0f)
        {
            SceneManager.LoadScene(sceneName);
            yield return new WaitForSeconds(waitSeconds);
        }

        protected IEnumerator PressButton(string buttonName, float waitSeconds = 0f)
        {
            Button sceneButton = GameObject.Find(buttonName).GetComponent<Button>();
            sceneButton.onClick.Invoke();

            if (waitSeconds > 0f)
            {
                yield return new WaitForSeconds(waitSeconds);
            }
            else
            {
                yield return null;
            }
        }

        public void Cleanup()
        {
            GameObject.DestroyImmediate(Embrace.Instance.gameObject);
        }
    }
}

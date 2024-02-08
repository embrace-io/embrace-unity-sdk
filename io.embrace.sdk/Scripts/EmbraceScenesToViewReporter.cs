using UnityEngine.SceneManagement;

namespace EmbraceSDK
{
    internal class EmbraceScenesToViewReporter : System.IDisposable
    {
        private string activeSceneName;
        public EmbraceScenesToViewReporter()
        {
            SceneManager.activeSceneChanged += EmbraceActiveSceneChangedHandler;
        }

        public void StartViewFromScene(Scene scene)
        {
            activeSceneName = scene.name;
            Embrace.Instance.StartView(activeSceneName);
        }

        public void EndViewFromScene(Scene scene)
        {
            Embrace.Instance.EndView(scene.name);
        }

        private void EmbraceActiveSceneChangedHandler(Scene current, Scene next)
        {
            if (activeSceneName != null)
            {
                Embrace.Instance.EndView(activeSceneName);
            }

            // It is important to note that current is only initialized in the case of additive scenes changing which one is active.
            if (current.name != null && !current.name.Equals(activeSceneName))
            {
                EndViewFromScene(current);
            }

            StartViewFromScene(next);
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= EmbraceActiveSceneChangedHandler;
        }
    }
}
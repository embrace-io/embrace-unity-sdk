using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// Allows you to select scenes from the demo UI.
    /// </summary>
    public class SceneSelector : MonoBehaviour
    {
        public GameObject sceneButtonPrefab;
        public Transform content;

        private Dictionary<string, string> scenes = new Dictionary<string, string>
        {
            { "Integration", "Integrate" },
            { "Logs", "Logs" },
            { "Crashes", "Crashes" },
            { "Breadcrumb", "Breadcrumb" },
            { "Moments", "Moments" },
            { "User Data", "Users" },
            { "Network Capture", "Network" }
        };

        public int SceneCount
        {
            get { return scenes.Count; }
        }

        private static SceneSelector _instance;

        public static SceneSelector instance
        {
            get { return _instance; }
        }


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            if (content != null)
            {
                SetupUI();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            DontDestroyOnLoad(this.gameObject);
        }

        public void LoadScene(string scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        private void SetupUI()
        {
            foreach (var pair in scenes)
            {
                GameObject go = Instantiate(sceneButtonPrefab, content);
                go.name = pair.Value;
                SceneButton sceneButton = go.GetComponent<SceneButton>();
                sceneButton.SceneName = pair.Value;
                sceneButton.buttonTitle.text = pair.Key;

                if(EventSystem.current.firstSelectedGameObject == null)
                {
                    EventSystem.current.firstSelectedGameObject = go;
                    EventSystem.current.SetSelectedGameObject(go);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Demo Home")
            {
                if (content != null && content.childCount == 0)
                {
                    SetupUI();
                }
            }
        }
    }
}